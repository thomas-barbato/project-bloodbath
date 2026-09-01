using ProjectBloodbath.Input;
using ProjectBloodbath.Player;
using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [DisallowMultipleComponent]
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private FpsPlayerController playerController;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private HitscanWeaponSettings settings;
        [SerializeField] private Transform weaponVisual;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Light muzzleFlash;
        [SerializeField] private LineRenderer tracer;

        private Vector3 visualRestPosition;
        private float nextShotTime;
        private float feedbackUntil;

        public void Configure(
            PlayerInputReader reader,
            FpsPlayerController controller,
            Camera cameraComponent,
            HitscanWeaponSettings weaponSettings,
            Transform visual,
            Transform muzzleTransform,
            Light flash,
            LineRenderer tracerRenderer)
        {
            inputReader = reader;
            playerController = controller;
            aimCamera = cameraComponent;
            settings = weaponSettings;
            weaponVisual = visual;
            muzzle = muzzleTransform;
            muzzleFlash = flash;
            tracer = tracerRenderer;
            CacheVisualState();
            SetFeedbackVisible(false);
        }

        private void Awake()
        {
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

            if (
                settings == null ||
                inputReader == null ||
                aimCamera == null ||
                Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            bool wantsToFire = settings.Automatic
                ? inputReader.AttackHeld
                : inputReader.AttackPressedThisFrame;
            if (wantsToFire && Time.time >= nextShotTime)
            {
                Fire();
            }
        }

        private void Fire()
        {
            nextShotTime = Time.time + settings.SecondsPerShot;

            Quaternion spread = Quaternion.Euler(
                Random.Range(-settings.SpreadDegrees, settings.SpreadDegrees),
                Random.Range(-settings.SpreadDegrees, settings.SpreadDegrees),
                0f);
            Vector3 direction = spread * aimCamera.transform.forward;
            Ray ray = new(aimCamera.transform.position, direction);
            Vector3 tracerEnd = ray.GetPoint(settings.Range);

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                settings.Range,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            {
                tracerEnd = hit.point;
                DamageInfo damage = new(
                    settings.Damage,
                    settings.DamageType,
                    hit.point,
                    hit.normal,
                    direction,
                    settings.ImpactForce,
                    gameObject);

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.ApplyDamage(damage);

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
                    Random.Range(-settings.YawRecoil, settings.YawRecoil));
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
