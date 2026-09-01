using System.Collections;
using ProjectBloodbath.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(NavMeshAgent))]
    public sealed class PrototypeEnemyController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Renderer[] visuals;
        [SerializeField, Min(0.1f)] private float attackRange = 1.65f;
        [SerializeField, Min(0f)] private float attackDamage = 12f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 0.85f;
        [SerializeField, Min(0.1f)] private float respawnDelay = 2.5f;
        [SerializeField] private Color hitColor = new(1f, 0.72f, 0.4f, 1f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private Health health;
        private Health targetHealth;
        private NavMeshAgent agent;
        private Collider[] hitColliders;
        private MaterialPropertyBlock propertyBlock;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private Vector3 spawnScale;
        private Color baseColor;
        private float nextAttackTime;
        private float flashUntil;
        private bool resetting;

        public void Configure(Transform chaseTarget, Renderer[] renderers)
        {
            target = chaseTarget;
            visuals = renderers;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            agent = GetComponent<NavMeshAgent>();
            targetHealth = target != null ? target.GetComponent<Health>() : null;
            hitColliders = GetComponentsInChildren<Collider>();
            propertyBlock = new MaterialPropertyBlock();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            spawnScale = transform.localScale;

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
        }

        private void Update()
        {
            SetColor(Time.time < flashUntil ? hitColor : baseColor);

            if (resetting || target == null || !health.IsAlive || !agent.isOnNavMesh)
            {
                return;
            }

            if (targetHealth == null)
            {
                targetHealth = target.GetComponent<Health>();
            }

            if (targetHealth == null || !targetHealth.IsAlive)
            {
                agent.isStopped = true;
                return;
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= attackRange * attackRange)
            {
                agent.isStopped = true;
                FaceTarget(offset);
                if (Time.time >= nextAttackTime)
                {
                    Attack(offset.normalized);
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
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

        private void Attack(Vector3 direction)
        {
            nextAttackTime = Time.time + attackCooldown;
            DamageInfo damage = new(
                attackDamage,
                DamageType.Melee,
                target.position + Vector3.up,
                -direction,
                direction,
                0f,
                gameObject);
            targetHealth.ApplyDamage(damage);
        }

        private void OnDamaged(DamageInfo damage)
        {
            flashUntil = Time.time + 0.09f;
        }

        private void OnDied(DamageInfo damage)
        {
            if (!resetting)
            {
                StartCoroutine(ResetEnemy());
            }
        }

        private IEnumerator ResetEnemy()
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

            yield return new WaitForSeconds(respawnDelay);

            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            transform.localScale = spawnScale;
            foreach (Collider hitCollider in hitColliders)
            {
                hitCollider.enabled = true;
            }

            health.RestoreFull();
            flashUntil = 0f;
            SetColor(baseColor);
            agent.enabled = true;
            agent.Warp(spawnPosition);
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
    }
}
