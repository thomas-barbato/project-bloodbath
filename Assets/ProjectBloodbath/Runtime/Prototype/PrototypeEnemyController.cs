using System.Collections;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(NavMeshAgent))]
    public sealed class PrototypeEnemyController : MonoBehaviour
    {
        private enum AttackPhase
        {
            Idle,
            Windup,
            Recovery
        }

        [SerializeField] private Transform target;
        [SerializeField] private Renderer[] visuals;
        [SerializeField] private EnemyBehaviorProfile behaviorProfile;
        [SerializeField] private EnemyAttackProfile attackProfile;

        [Header("Cycle de vie")]
        [SerializeField] private EnemyRespawnProfile respawnProfile;

        [Header("Réactions")]
        [SerializeField] private Color hitColor = new(1f, 0.72f, 0.4f, 1f);
        [SerializeField, Min(0f)] private float hitStaggerDuration = 0.14f;
        [SerializeField, Min(0f)] private float maximumHitPush = 0.16f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private Health health;
        private Health targetHealth;
        private ICombatTarget combatTarget;
        private NavMeshAgent agent;
        private Collider[] hitColliders;
        private RaycastHit[] sightHits;
        private MaterialPropertyBlock propertyBlock;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private Vector3 spawnScale;
        private Quaternion[] visualRestRotations;
        private Color baseColor;
        private float nextAttackTime;
        private float flashUntil;
        private float staggerUntil;
        private float attackPhaseStartedAt;
        private AttackPhase attackPhase;
        private EnemyBehaviorState behaviorState;
        private bool alerted;
        private bool resetting;

        public EnemyBehaviorProfile BehaviorProfile => behaviorProfile;
        public EnemyAttackProfile AttackProfile => attackProfile;
        public EnemyRespawnProfile RespawnProfile => respawnProfile;
        public EnemyBehaviorState BehaviorState => behaviorState;
        public bool IsAlerted => alerted;
        public bool IsPreparingAttack => attackPhase == AttackPhase.Windup;
        public bool IsRecoveringAttack => attackPhase == AttackPhase.Recovery;
        public float AttackCooldownRemaining =>
            Mathf.Max(0f, nextAttackTime - Time.time);

        public void Configure(
            Transform chaseTarget,
            Renderer[] renderers,
            EnemyAttackProfile profile = null,
            EnemyBehaviorProfile movementProfile = null,
            EnemyRespawnProfile lifeCycleProfile = null)
        {
            target = chaseTarget;
            combatTarget = target != null
                ? target.GetComponent<ICombatTarget>()
                : null;
            visuals = renderers;
            if (profile != null)
            {
                attackProfile = profile;
            }

            if (movementProfile != null)
            {
                behaviorProfile = movementProfile;
                ApplyBehaviorProfile();
            }

            if (lifeCycleProfile != null)
            {
                respawnProfile = lifeCycleProfile;
            }
        }

        public void SetRespawnProfile(EnemyRespawnProfile profile)
        {
            respawnProfile = profile;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            agent = GetComponent<NavMeshAgent>();
            targetHealth = target != null ? target.GetComponent<Health>() : null;
            combatTarget = target != null
                ? target.GetComponent<ICombatTarget>()
                : null;
            hitColliders = GetComponentsInChildren<Collider>();
            sightHits = new RaycastHit[8];
            propertyBlock = new MaterialPropertyBlock();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            spawnScale = transform.localScale;
            behaviorState = EnemyBehaviorState.Idle;
            ApplyBehaviorProfile();
            CacheVisualPose();

            if (visuals != null && visuals.Length > 0 && visuals[0].sharedMaterial != null &&
                visuals[0].sharedMaterial.HasProperty(BaseColorId))
            {
                baseColor = visuals[0].sharedMaterial.GetColor(BaseColorId);
            }
            else
            {
                baseColor = new Color(0.2f, 0.35f, 0.24f, 1f);
            }
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
            RestoreVisualPose();
        }

        private void Update()
        {
            SetColor(Time.time < flashUntil ? hitColor : baseColor);

            if (
                resetting ||
                target == null ||
                behaviorProfile == null ||
                attackProfile == null ||
                !health.IsAlive ||
                !agent.isOnNavMesh)
            {
                return;
            }

            if (targetHealth == null)
            {
                targetHealth = target.GetComponent<Health>();
            }

            if (combatTarget == null)
            {
                combatTarget = target.GetComponent<ICombatTarget>();
            }

            if (
                targetHealth == null ||
                !targetHealth.IsAlive ||
                (combatTarget != null && !combatTarget.CanBeTargeted))
            {
                alerted = false;
                CancelAttack();
                UpdateReturnToSpawn();
                return;
            }

            if (Time.time < staggerUntil)
            {
                agent.isStopped = true;
                return;
            }

            if (UpdateAttackSequence())
            {
                behaviorState = EnemyBehaviorState.Combat;
                return;
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            Vector3 targetFromSpawn = target.position - spawnPosition;
            targetFromSpawn.y = 0f;

            bool targetWithinLeash =
                targetFromSpawn.sqrMagnitude <=
                behaviorProfile.LeashRange * behaviorProfile.LeashRange;
            if (!targetWithinLeash)
            {
                alerted = false;
                CancelAttack();
            }
            else if (
                !alerted &&
                CanInitiallyDetectTarget(offset))
            {
                alerted = true;
            }

            if (!alerted)
            {
                UpdateReturnToSpawn();
                return;
            }

            float distance = offset.magnitude;
            if (
                behaviorProfile.MovementStyle ==
                    EnemyMovementStyle.MaintainDistance &&
                distance < behaviorProfile.PreferredMinimumDistance)
            {
                RepositionAwayFromTarget(offset);
            }
            else if (
                behaviorProfile.MovementStyle ==
                    EnemyMovementStyle.MaintainDistance &&
                distance > behaviorProfile.PreferredMaximumDistance)
            {
                PursueTarget();
            }
            else if (distance <= attackProfile.Range)
            {
                behaviorState = EnemyBehaviorState.Combat;
                agent.isStopped = true;
                FaceTarget(offset);
                if (
                    Time.time >= nextAttackTime &&
                    CanAttackTarget(offset))
                {
                    BeginAttack();
                }
            }
            else if (
                behaviorProfile.MovementStyle ==
                EnemyMovementStyle.Stationary)
            {
                behaviorState = EnemyBehaviorState.Combat;
                agent.isStopped = true;
                FaceTarget(offset);
            }
            else
            {
                PursueTarget();
            }
        }

        private void PursueTarget()
        {
            behaviorState = EnemyBehaviorState.Pursuing;
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }

        private void RepositionAwayFromTarget(Vector3 targetOffset)
        {
            behaviorState = EnemyBehaviorState.Repositioning;
            agent.isStopped = false;
            FaceTarget(targetOffset);

            Vector3 retreatDirection = -targetOffset.normalized;
            Vector3 destination =
                transform.position +
                retreatDirection * behaviorProfile.RetreatStepDistance;
            agent.SetDestination(destination);
        }

        private bool CanAttackTarget(Vector3 planarOffset)
        {
            return
                planarOffset.sqrMagnitude > 0.001f &&
                planarOffset.sqrMagnitude <=
                    attackProfile.Range * attackProfile.Range &&
                Vector3.Angle(transform.forward, planarOffset) <=
                    attackProfile.ArcDegrees * 0.5f &&
                (!attackProfile.RequiresLineOfSight ||
                 HasClearLineOfSight());
        }

        private bool CanInitiallyDetectTarget(Vector3 planarOffset)
        {
            if (
                planarOffset.sqrMagnitude >
                behaviorProfile.DetectionRange *
                behaviorProfile.DetectionRange ||
                planarOffset.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            if (
                Vector3.Angle(transform.forward, planarOffset) >
                behaviorProfile.ViewAngle * 0.5f)
            {
                return false;
            }

            return !behaviorProfile.RequiresLineOfSight ||
                HasClearLineOfSight();
        }

        private bool HasClearLineOfSight()
        {
            Vector3 origin =
                transform.position + Vector3.up * behaviorProfile.EyeHeight;
            Vector3 destination =
                target.position + Vector3.up * behaviorProfile.TargetHeight;
            Vector3 direction = destination - origin;
            float distance = direction.magnitude;
            if (distance <= 0.001f)
            {
                return true;
            }

            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction / distance,
                sightHits,
                distance,
                behaviorProfile.LineOfSightMask,
                QueryTriggerInteraction.Ignore);
            Transform closestTransform = null;
            float closestDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                Transform hitTransform = sightHits[index].transform;
                if (
                    hitTransform == null ||
                    hitTransform == transform ||
                    hitTransform.IsChildOf(transform) ||
                    sightHits[index].distance >= closestDistance)
                {
                    continue;
                }

                closestTransform = hitTransform;
                closestDistance = sightHits[index].distance;
            }

            return
                closestTransform == null ||
                closestTransform == target ||
                closestTransform.IsChildOf(target);
        }

        private void ApplyBehaviorProfile()
        {
            if (agent == null || behaviorProfile == null)
            {
                return;
            }

            agent.speed = behaviorProfile.MovementSpeed;
            agent.acceleration = behaviorProfile.Acceleration;
            agent.angularSpeed = behaviorProfile.AngularSpeed;
            agent.stoppingDistance = behaviorProfile.StoppingDistance;
        }

        private void UpdateReturnToSpawn()
        {
            Vector3 offset = spawnPosition - transform.position;
            offset.y = 0f;
            if (
                offset.sqrMagnitude <=
                behaviorProfile.ReturnTolerance *
                behaviorProfile.ReturnTolerance)
            {
                behaviorState = EnemyBehaviorState.Idle;
                agent.isStopped = true;
                return;
            }

            behaviorState = EnemyBehaviorState.Returning;
            agent.isStopped = false;
            agent.SetDestination(spawnPosition);
        }

        private void FaceTarget(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion desired = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desired,
                agent.angularSpeed * Time.deltaTime);
        }

        private void BeginAttack()
        {
            attackPhase = AttackPhase.Windup;
            attackPhaseStartedAt = Time.time;
            behaviorState = EnemyBehaviorState.Combat;
            agent.isStopped = true;
        }

        private bool UpdateAttackSequence()
        {
            if (attackPhase == AttackPhase.Idle)
            {
                return false;
            }

            agent.isStopped = true;
            if (attackPhase == AttackPhase.Windup)
            {
                float progress = Mathf.Clamp01(
                    (Time.time - attackPhaseStartedAt) /
                    attackProfile.WindupDuration);
                ApplyAttackPose(
                    -Mathf.SmoothStep(0f, attackProfile.LeanAngle, progress));
                if (progress >= 1f)
                {
                    ResolveAttack();
                    attackPhase = AttackPhase.Recovery;
                    attackPhaseStartedAt = Time.time;
                }

                return true;
            }

            float recoveryProgress = Mathf.Clamp01(
                (Time.time - attackPhaseStartedAt) /
                attackProfile.RecoveryDuration);
            float recoveryAngle = Mathf.Lerp(
                attackProfile.LeanAngle * 0.65f,
                0f,
                Mathf.SmoothStep(0f, 1f, recoveryProgress));
            ApplyAttackPose(recoveryAngle);
            if (recoveryProgress >= 1f)
            {
                attackPhase = AttackPhase.Idle;
                nextAttackTime = Time.time + attackProfile.CooldownDuration;
                RestoreVisualPose();
            }

            return true;
        }

        private void ResolveAttack()
        {
            if (target == null || targetHealth == null || !targetHealth.IsAlive)
            {
                return;
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            if (!CanAttackTarget(offset))
            {
                return;
            }

            Vector3 direction = offset.normalized;
            if (
                attackProfile.Delivery == EnemyAttackDelivery.Projectile &&
                attackProfile.ProjectilePrefab != null)
            {
                Vector3 origin =
                    transform.position +
                    Vector3.up * attackProfile.LaunchHeight +
                    direction * 0.65f;
                Vector3 aimPoint =
                    target.position +
                    Vector3.up * behaviorProfile.TargetHeight;
                EnemyProjectile projectile = Instantiate(
                    attackProfile.ProjectilePrefab,
                    origin,
                    Quaternion.LookRotation(aimPoint - origin));
                projectile.Initialize(
                    gameObject,
                    aimPoint - origin,
                    attackProfile.ProjectileSpeed,
                    attackProfile.ProjectileRadius,
                    attackProfile.ProjectileLifetime,
                    attackProfile.Damage,
                    attackProfile.DamageType,
                    attackProfile.ImpactForce);
                return;
            }

            DamageInfo damage = new(
                attackProfile.Damage,
                attackProfile.DamageType,
                target.position + Vector3.up,
                -direction,
                direction,
                attackProfile.ImpactForce,
                gameObject);
            targetHealth.ApplyDamage(damage);
        }

        private void OnDamaged(DamageInfo damage)
        {
            alerted = true;
            CancelAttack();
            flashUntil = Time.time + 0.09f;
            staggerUntil = Time.time + hitStaggerDuration;
            nextAttackTime = Mathf.Max(nextAttackTime, staggerUntil + 0.08f);

            Vector3 pushDirection = damage.Direction;
            pushDirection.y = 0f;
            if (
                maximumHitPush > 0f &&
                pushDirection.sqrMagnitude > 0.001f &&
                agent.enabled &&
                agent.isOnNavMesh)
            {
                float pushDistance = Mathf.Min(
                    maximumHitPush,
                    damage.Force * 0.01f);
                agent.Move(pushDirection.normalized * pushDistance);
            }
        }

        private void OnDied(DamageInfo damage)
        {
            CancelAttack();
            if (!resetting)
            {
                StartCoroutine(HandleDeath());
            }
        }

        private IEnumerator HandleDeath()
        {
            resetting = true;
            agent.isStopped = true;
            agent.enabled = false;

            foreach (Collider hitCollider in hitColliders)
            {
                hitCollider.enabled = false;
            }

            float elapsed = 0f;
            const float collapseDuration = 0.2f;
            while (elapsed < collapseDuration)
            {
                elapsed += Time.deltaTime;
                float ratio = Mathf.Clamp01(elapsed / collapseDuration);
                transform.localScale = Vector3.Lerp(
                    spawnScale,
                    new Vector3(spawnScale.x, spawnScale.y * 0.12f, spawnScale.z),
                    ratio);
                yield return null;
            }

            if (respawnProfile == null || !respawnProfile.RespawnsDuringSession)
            {
                yield break;
            }

            yield return new WaitForSeconds(respawnProfile.Delay);

            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            transform.localScale = spawnScale;
            foreach (Collider hitCollider in hitColliders)
            {
                hitCollider.enabled = true;
            }

            health.RestoreFull();
            flashUntil = 0f;
            staggerUntil = 0f;
            attackPhase = AttackPhase.Idle;
            behaviorState = EnemyBehaviorState.Idle;
            alerted = false;
            RestoreVisualPose();
            SetColor(baseColor);
            agent.enabled = true;
            agent.Warp(spawnPosition);
            ApplyBehaviorProfile();
            nextAttackTime = Time.time + 0.5f;
            resetting = false;
        }

        private void SetColor(Color color)
        {
            if (visuals == null)
            {
                return;
            }

            foreach (Renderer visual in visuals)
            {
                if (visual == null)
                {
                    continue;
                }

                visual.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                visual.SetPropertyBlock(propertyBlock);
            }
        }

        private void CacheVisualPose()
        {
            if (visuals == null)
            {
                visualRestRotations = null;
                return;
            }

            visualRestRotations = new Quaternion[visuals.Length];
            for (int index = 0; index < visuals.Length; index++)
            {
                if (visuals[index] != null)
                {
                    visualRestRotations[index] =
                        visuals[index].transform.localRotation;
                }
            }
        }

        private void ApplyAttackPose(float angle)
        {
            if (visuals == null || visualRestRotations == null)
            {
                return;
            }

            int count = Mathf.Min(visuals.Length, visualRestRotations.Length);
            for (int index = 0; index < count; index++)
            {
                if (visuals[index] != null)
                {
                    visuals[index].transform.localRotation =
                        visualRestRotations[index] * Quaternion.Euler(angle, 0f, 0f);
                }
            }
        }

        private void RestoreVisualPose()
        {
            if (visuals == null || visualRestRotations == null)
            {
                return;
            }

            int count = Mathf.Min(visuals.Length, visualRestRotations.Length);
            for (int index = 0; index < count; index++)
            {
                if (visuals[index] != null)
                {
                    visuals[index].transform.localRotation =
                        visualRestRotations[index];
                }
            }
        }

        private void CancelAttack()
        {
            if (attackPhase == AttackPhase.Idle)
            {
                return;
            }

            attackPhase = AttackPhase.Idle;
            nextAttackTime = Mathf.Max(nextAttackTime, Time.time + 0.1f);
            RestoreVisualPose();
        }
    }
}
