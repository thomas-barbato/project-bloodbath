using System;
using System.Collections.Generic;
using ProjectBloodbath.Input;
using ProjectBloodbath.Player;
using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [DisallowMultipleComponent]
    public sealed class MeleeWeapon : MonoBehaviour
    {
        private const int HitBufferSize = 32;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private FpsPlayerController playerController;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private MeleeWeaponSettings settings;
        [SerializeField] private Transform weaponVisual;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<IDamageable> hitTargets = new();
        private Quaternion visualRestRotation;
        private IDamageOutputProvider damageOutputProvider;
        private float attackStartedAt;
        private float nextAttackTime;
        private bool hitResolved;
        private int attackDirection = 1;

        public event Action<int> AttackResolved;

        public bool IsAttacking { get; private set; }
        public int LastAttackHitCount { get; private set; }

        public void Configure(
            PlayerInputReader reader,
            FpsPlayerController controller,
            Camera cameraComponent,
            MeleeWeaponSettings weaponSettings,
            Transform visual)
        {
            inputReader = reader;
            playerController = controller;
            aimCamera = cameraComponent;
            settings = weaponSettings;
            weaponVisual = visual;
            CacheDamageOutputProvider();
            CacheVisualState();
        }

        public bool TryAttack()
        {
            if (
                !isActiveAndEnabled ||
                settings == null ||
                aimCamera == null ||
                Time.time < nextAttackTime)
            {
                return false;
            }

            attackStartedAt = Time.time;
            nextAttackTime = Time.time + settings.SecondsPerAttack;
            hitResolved = false;
            IsAttacking = true;
            LastAttackHitCount = 0;
            attackDirection *= -1;
            playerController?.AddLookImpulse(
                settings.CameraPitchKick,
                settings.CameraYawKick * attackDirection);
            return true;
        }

        private void Awake()
        {
            CacheDamageOutputProvider();
            CacheVisualState();
        }

        private void OnDisable()
        {
            IsAttacking = false;
            hitResolved = false;
            RestoreVisual();
        }

        private void Update()
        {
            UpdateAttackAnimation();

            if (
                inputReader != null &&
                inputReader.AttackPressedThisFrame &&
                Cursor.lockState == CursorLockMode.Locked)
            {
                TryAttack();
            }
        }

        private void UpdateAttackAnimation()
        {
            if (!IsAttacking || settings == null)
            {
                return;
            }

            float normalizedTime = Mathf.Clamp01(
                (Time.time - attackStartedAt) / settings.AnimationDuration);

            if (!hitResolved && normalizedTime >= settings.HitNormalizedTime)
            {
                ResolveHits();
                hitResolved = true;
            }

            AnimateVisual(normalizedTime);
            if (normalizedTime >= 1f)
            {
                IsAttacking = false;
                RestoreVisual();
            }
        }

        private void ResolveHits()
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
                if (
                    candidate == null ||
                    candidate.transform.root == transform.root)
                {
                    continue;
                }

                Vector3 samplePoint = origin + forward * (settings.Range * 0.65f);
                Vector3 hitPoint = candidate.ClosestPoint(samplePoint);
                Vector3 toTarget = hitPoint - origin;
                float distance = toTarget.magnitude;
                if (
                    distance <= 0.001f ||
                    distance > settings.Range ||
                    Vector3.Angle(forward, toTarget) > settings.ArcDegrees * 0.5f)
                {
                    continue;
                }

                IDamageable damageable = candidate.GetComponentInParent<IDamageable>();
                if (damageable == null || !HasLineOfSight(origin, toTarget, damageable))
                {
                    continue;
                }

                if (!hitTargets.Add(damageable))
                {
                    continue;
                }

                Vector3 direction = toTarget / distance;
                DamageInfo damage = new(
                    settings.Damage * GetDamageMultiplier(),
                    settings.DamageType,
                    hitPoint,
                    -direction,
                    direction,
                    settings.ImpactForce,
                    transform.root.gameObject);
                damageable.ApplyDamage(damage);

                Rigidbody targetBody = candidate.attachedRigidbody;
                if (targetBody != null && !targetBody.isKinematic)
                {
                    targetBody.AddForceAtPosition(
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

            LastAttackHitCount = hitCount;
            if (hitCount > 0)
            {
                playerController?.AddLookImpulse(
                    settings.HitCameraKick,
                    -settings.CameraYawKick * attackDirection * 0.2f);
            }

            AttackResolved?.Invoke(hitCount);
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

        private void AnimateVisual(float normalizedTime)
        {
            if (weaponVisual == null)
            {
                return;
            }

            float hitTime = settings.HitNormalizedTime;
            float recoveryStart = Mathf.Lerp(hitTime, 1f, 0.55f);
            float angle;

            if (normalizedTime < hitTime)
            {
                float phase = Mathf.SmoothStep(0f, 1f, normalizedTime / hitTime);
                angle = Mathf.Lerp(0f, settings.WindupAngle, phase);
            }
            else if (normalizedTime < recoveryStart)
            {
                float phase = Mathf.SmoothStep(
                    0f,
                    1f,
                    (normalizedTime - hitTime) / (recoveryStart - hitTime));
                angle = Mathf.Lerp(
                    settings.WindupAngle,
                    settings.FollowThroughAngle,
                    phase);
            }
            else
            {
                float phase = Mathf.SmoothStep(
                    0f,
                    1f,
                    (normalizedTime - recoveryStart) / (1f - recoveryStart));
                angle = Mathf.Lerp(settings.FollowThroughAngle, 0f, phase);
            }

            angle *= attackDirection;
            weaponVisual.localRotation = visualRestRotation *
                Quaternion.Euler(angle * 0.12f, angle, -angle * 0.32f);
        }

        private void CacheVisualState()
        {
            if (weaponVisual != null)
            {
                visualRestRotation = weaponVisual.localRotation;
            }
        }

        private void CacheDamageOutputProvider()
        {
            damageOutputProvider =
                GetComponentInParent<IDamageOutputProvider>();
        }

        private float GetDamageMultiplier()
        {
            return damageOutputProvider?.OutgoingDamageMultiplier ?? 1f;
        }

        private void RestoreVisual()
        {
            if (weaponVisual != null)
            {
                weaponVisual.localRotation = visualRestRotation;
            }
        }

    }
}
