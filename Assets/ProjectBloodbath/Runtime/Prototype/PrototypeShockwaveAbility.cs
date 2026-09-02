using System;
using System.Collections.Generic;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader), typeof(AbilityResource))]
    public sealed class PrototypeShockwaveAbility : MonoBehaviour
    {
        private const int HitBufferSize = 48;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private AbilityResource abilityResource;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private ActiveAbilitySettings settings;
        [SerializeField] private PrototypePlayerLife playerLife;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<IDamageable> hitTargets = new();
        private float readyAt;
        private float feedbackUntil;

        public event Action<int> Activated;

        public ActiveAbilitySettings Settings => settings;
        public int LastHitCount { get; private set; }
        public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
        public float CooldownProgress => settings == null ||
            settings.CooldownDuration <= 0f
                ? 0f
                : Mathf.Clamp01(CooldownRemaining / settings.CooldownDuration);
        public bool IsReady => settings != null && CooldownRemaining <= 0f;
        public bool HasEnoughResource => settings != null &&
            abilityResource != null &&
            abilityResource.Current >= settings.ResourceCost;
        public float ActivationFeedbackRemaining => Mathf.Max(
            0f,
            feedbackUntil - Time.time);

        public void Configure(
            PlayerInputReader reader,
            AbilityResource resource,
            Camera cameraComponent,
            ActiveAbilitySettings abilitySettings,
            PrototypePlayerLife life)
        {
            inputReader = reader;
            abilityResource = resource;
            aimCamera = cameraComponent;
            settings = abilitySettings;
            playerLife = life;
        }

        public bool TryActivate()
        {
            CacheReferences();
            if (
                !isActiveAndEnabled ||
                settings == null ||
                abilityResource == null ||
                aimCamera == null ||
                (playerLife != null && playerLife.IsSoul) ||
                Time.time < readyAt ||
                !abilityResource.TrySpend(settings.ResourceCost))
            {
                return false;
            }

            readyAt = Time.time + settings.CooldownDuration;
            feedbackUntil = Time.time + 0.16f;
            LastHitCount = ResolveHits();
            Activated?.Invoke(LastHitCount);
            return true;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (abilityResource == null)
            {
                abilityResource = GetComponent<AbilityResource>();
            }

            if (playerLife == null)
            {
                playerLife = GetComponent<PrototypePlayerLife>();
            }

            if (aimCamera == null)
            {
                aimCamera = GetComponentInChildren<Camera>(true);
            }

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (
                inputReader != null &&
                inputReader.ConsumeAbility1Pressed() &&
                Cursor.lockState == CursorLockMode.Locked)
            {
                TryActivate();
            }
        }

        private int ResolveHits()
        {
            Vector3 origin = aimCamera.transform.position;
            Vector3 forward = aimCamera.transform.forward;
            int colliderCount = Physics.OverlapSphereNonAlloc(
                origin,
                settings.Range,
                hitBuffer,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            hitTargets.Clear();
            int hitCount = 0;
            for (int index = 0; index < colliderCount; index++)
            {
                Collider candidate = hitBuffer[index];
                if (candidate == null || candidate.transform.root == transform.root)
                {
                    continue;
                }

                IDamageable damageable =
                    candidate.GetComponentInParent<IDamageable>();
                if (
                    damageable == null ||
                    hitTargets.Contains(damageable))
                {
                    continue;
                }

                Vector3 hitPoint = candidate.ClosestPoint(origin);
                Vector3 toTarget = hitPoint - origin;
                float distance = toTarget.magnitude;
                if (distance <= 0.001f)
                {
                    hitPoint = candidate.bounds.center;
                    toTarget = hitPoint - origin;
                    distance = toTarget.magnitude;
                }

                if (
                    distance <= 0.001f ||
                    distance > settings.Range ||
                    Vector3.Angle(forward, toTarget) >
                        settings.ArcDegrees * 0.5f)
                {
                    continue;
                }

                if (
                    !HasLineOfSight(origin, toTarget, damageable))
                {
                    continue;
                }

                hitTargets.Add(damageable);

                Vector3 direction = toTarget / distance;
                DamageInfo damage = new(
                    settings.Damage *
                        (playerLife?.OutgoingDamageMultiplier ?? 1f),
                    settings.DamageType,
                    hitPoint,
                    -direction,
                    direction,
                    settings.ImpactForce,
                    gameObject);
                damageable.ApplyDamage(damage);

                Rigidbody body = candidate.attachedRigidbody;
                if (body != null && !body.isKinematic)
                {
                    body.AddForceAtPosition(
                        direction * settings.ImpactForce,
                        hitPoint,
                        ForceMode.Impulse);
                }

                hitCount++;
                if (hitCount >= settings.MaximumTargets)
                {
                    break;
                }
            }

            return hitCount;
        }

        private static bool HasLineOfSight(
            Vector3 origin,
            Vector3 toTarget,
            IDamageable intendedTarget)
        {
            float distance = toTarget.magnitude;
            if (!Physics.Raycast(
                origin,
                toTarget / distance,
                out RaycastHit obstruction,
                distance + 0.05f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            IDamageable obstructionTarget =
                obstruction.collider.GetComponentInParent<IDamageable>();
            return ReferenceEquals(obstructionTarget, intendedTarget);
        }
    }
}
