using System;
using ProjectBloodbath.Player;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [DisallowMultipleComponent]
    public sealed class HitscanWeapon : MonoBehaviour
    {
        private const int HitBufferSize = 64;

        [SerializeField] private FpsPlayerController playerController;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private HitscanWeaponSettings settings;
        [SerializeField] private Transform weaponVisual;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Light muzzleFlash;
        [SerializeField] private LineRenderer tracer;

        private Vector3 visualRestPosition;
        private IDamageOutputProvider damageOutputProvider;
        private CharacterInventory inventory;
        private int fallbackReserveAmmo;
        private float nextShotTime;
        private float feedbackUntil;
        private float reloadStartedAt;
        private float reloadCompletesAt;
        private bool ammunitionInitialized;
        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];

        public event Action<int, int> AmmunitionChanged;
        public event Action ReloadStarted;
        public event Action ReloadFinished;

        public HitscanWeaponSettings Settings => settings;
        public int CurrentMagazine { get; private set; }
        public int ReserveAmmo =>
            inventory != null && settings?.AmmunitionType != null
                ? inventory.GetResourceQuantity(settings.AmmunitionType)
                : fallbackReserveAmmo;
        public bool IsReloading { get; private set; }
        public float ReloadProgress => !IsReloading
            ? 0f
            : Mathf.InverseLerp(
                reloadStartedAt,
                reloadCompletesAt,
                Time.time);

        public void Configure(
            FpsPlayerController controller,
            Camera cameraComponent,
            HitscanWeaponSettings weaponSettings,
            Transform visual,
            Transform muzzleTransform,
            Light flash,
            LineRenderer tracerRenderer)
        {
            playerController = controller;
            aimCamera = cameraComponent;
            settings = weaponSettings;
            weaponVisual = visual;
            muzzle = muzzleTransform;
            muzzleFlash = flash;
            tracer = tracerRenderer;
            InitializeAmmunition(true);
            CacheDamageOutputProvider();
            CacheVisualState();
            SetFeedbackVisible(false);
        }

        private void Awake()
        {
            InitializeAmmunition(false);
            CacheDamageOutputProvider();
            CacheVisualState();
            SetFeedbackVisible(false);
        }

        private void OnDisable()
        {
            feedbackUntil = 0f;
            SetFeedbackVisible(false);
            if (weaponVisual != null)
            {
                weaponVisual.localPosition = visualRestPosition;
            }
        }

        private void Update()
        {
            RecoverWeaponVisual();
            UpdateShotFeedback();
            UpdateReload();

            if (
                settings == null ||
                aimCamera == null)
            {
                return;
            }
        }

        public bool TryFire()
        {
            if (
                settings == null ||
                IsReloading ||
                CurrentMagazine <= 0 ||
                Time.time < nextShotTime)
            {
                return false;
            }

            CurrentMagazine--;
            AmmunitionChanged?.Invoke(CurrentMagazine, ReserveAmmo);
            nextShotTime = Time.time + settings.SecondsPerShot;

            Quaternion spread = Quaternion.Euler(
                UnityEngine.Random.Range(
                    -settings.SpreadDegrees,
                    settings.SpreadDegrees),
                UnityEngine.Random.Range(
                    -settings.SpreadDegrees,
                    settings.SpreadDegrees),
                0f);
            Vector3 direction = spread * aimCamera.transform.forward;
            Ray ray = new(aimCamera.transform.position, direction);
            Vector3 tracerEnd = ray.GetPoint(settings.Range);

            if (TryGetClosestExternalHit(ray, out RaycastHit hit))
            {
                tracerEnd = hit.point;
                DamageInfo damage = new(
                    settings.Damage * GetDamageMultiplier(),
                    settings.DamageType,
                    hit.point,
                    hit.normal,
                    direction,
                    settings.ImpactForce,
                    gameObject);

                IDamageable damageable =
                    hit.collider.GetComponentInParent<IDamageable>();
                Health targetHealth = hit.collider.GetComponentInParent<Health>();
                float previousHealth = targetHealth?.Current ?? 0f;
                damageable?.ApplyDamage(damage);

                if (
                    targetHealth != null &&
                    targetHealth.IsAlive &&
                    targetHealth.Current < previousHealth &&
                    settings.AppliedMarkEffect != null)
                {
                    WeaponMarkState.GetOrAdd(targetHealth).ApplyMark(
                        settings.AppliedMarkEffect);
                }

                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                {
                    hit.rigidbody.AddForceAtPosition(
                        direction * settings.ImpactForce,
                        hit.point,
                        ForceMode.Impulse);
                }
            }

            ShowShotFeedback(tracerEnd);
            ApplyRecoil();
            return true;
        }

        private bool TryGetClosestExternalHit(
            Ray ray,
            out RaycastHit closestHit)
        {
            int hitCount = Physics.RaycastNonAlloc(
                ray,
                hitBuffer,
                settings.Range,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            Transform ownerRoot = transform.root;
            float closestDistance = float.PositiveInfinity;
            bool foundHit = false;
            closestHit = default;

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit candidate = hitBuffer[index];
                if (
                    candidate.collider == null ||
                    candidate.collider.transform.root == ownerRoot ||
                    candidate.distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = candidate.distance;
                closestHit = candidate;
                foundHit = true;
            }

            return foundHit;
        }

        public bool TryStartReload()
        {
            if (
                settings == null ||
                IsReloading ||
                CurrentMagazine >= settings.MagazineSize ||
                ReserveAmmo <= 0)
            {
                return false;
            }

            IsReloading = true;
            reloadStartedAt = Time.time;
            reloadCompletesAt = Time.time + settings.ReloadDuration;
            ReloadStarted?.Invoke();
            return true;
        }

        private void ShowShotFeedback(Vector3 tracerEnd)
        {
            feedbackUntil = Time.time + 0.045f;
            if (muzzleFlash != null)
            {
                muzzleFlash.enabled = true;
            }

            if (tracer != null && muzzle != null)
            {
                tracer.enabled = true;
                tracer.SetPosition(0, muzzle.position);
                tracer.SetPosition(1, tracerEnd);
            }
        }

        private void ApplyRecoil()
        {
            if (playerController != null)
            {
                playerController.AddLookImpulse(
                    settings.PitchRecoil,
                    UnityEngine.Random.Range(
                        -settings.YawRecoil,
                        settings.YawRecoil));
            }

            if (weaponVisual != null)
            {
                weaponVisual.localPosition = visualRestPosition + Vector3.back * settings.VisualKick;
            }
        }

        private void UpdateShotFeedback()
        {
            if (feedbackUntil > 0f && Time.time >= feedbackUntil)
            {
                feedbackUntil = 0f;
                SetFeedbackVisible(false);
            }
        }

        private void UpdateReload()
        {
            if (!IsReloading || Time.time < reloadCompletesAt)
            {
                return;
            }

            int missingRounds = settings.MagazineSize - CurrentMagazine;
            int loadedRounds = Mathf.Min(missingRounds, ReserveAmmo);
            CurrentMagazine += loadedRounds;
            if (inventory != null && settings.AmmunitionType != null)
            {
                inventory.RemoveResource(settings.AmmunitionType, loadedRounds);
            }
            else
            {
                fallbackReserveAmmo -= loadedRounds;
            }
            IsReloading = false;
            AmmunitionChanged?.Invoke(CurrentMagazine, ReserveAmmo);
            ReloadFinished?.Invoke();
        }

        private void RecoverWeaponVisual()
        {
            if (weaponVisual != null && settings != null)
            {
                weaponVisual.localPosition = Vector3.Lerp(
                    weaponVisual.localPosition,
                    visualRestPosition,
                    1f - Mathf.Exp(-settings.VisualRecovery * Time.deltaTime));
            }
        }

        private void CacheVisualState()
        {
            if (weaponVisual != null)
            {
                visualRestPosition = weaponVisual.localPosition;
            }
        }

        private void CacheDamageOutputProvider()
        {
            damageOutputProvider =
                GetComponentInParent<IDamageOutputProvider>();
        }

        private void InitializeAmmunition(bool force)
        {
            if (settings == null || (ammunitionInitialized && !force))
            {
                return;
            }

            CurrentMagazine = settings.MagazineSize;
            inventory = GetComponentInParent<CharacterInventory>();
            if (inventory != null && settings.AmmunitionType != null)
            {
                inventory.EnsureAtLeast(
                    settings.AmmunitionType,
                    settings.InitialReserveAmmo);
            }
            else
            {
                fallbackReserveAmmo = settings.InitialReserveAmmo;
            }
            IsReloading = false;
            ammunitionInitialized = true;
            AmmunitionChanged?.Invoke(CurrentMagazine, ReserveAmmo);
        }

        private float GetDamageMultiplier()
        {
            return damageOutputProvider?.OutgoingDamageMultiplier ?? 1f;
        }

        private void SetFeedbackVisible(bool visible)
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.enabled = visible;
            }

            if (tracer != null)
            {
                tracer.enabled = visible;
            }
        }
    }
}
