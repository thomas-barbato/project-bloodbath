using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [DisallowMultipleComponent]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        private readonly RaycastHit[] hits = new RaycastHit[8];

        private GameObject source;
        private Vector3 direction;
        private float speed;
        private float radius;
        private float expiresAt;
        private float damage;
        private DamageType damageType;
        private float impactForce;
        private bool initialized;

        public void Initialize(
            GameObject attackSource,
            Vector3 travelDirection,
            float travelSpeed,
            float collisionRadius,
            float lifetime,
            float damageAmount,
            DamageType type,
            float force)
        {
            source = attackSource;
            direction = travelDirection.normalized;
            speed = Mathf.Max(0.1f, travelSpeed);
            radius = Mathf.Max(0.01f, collisionRadius);
            expiresAt = Time.time + Mathf.Max(0.1f, lifetime);
            damage = Mathf.Max(0f, damageAmount);
            damageType = type;
            impactForce = Mathf.Max(0f, force);
            initialized = direction.sqrMagnitude > 0.001f;
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (Time.time >= expiresAt)
            {
                Destroy(gameObject);
                return;
            }

            float travelDistance = speed * Time.deltaTime;
            if (TryResolveImpact(travelDistance))
            {
                return;
            }

            transform.position += direction * travelDistance;
        }

        private bool TryResolveImpact(float travelDistance)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                radius,
                direction,
                hits,
                travelDistance,
                ~0,
                QueryTriggerInteraction.Ignore);
            RaycastHit closestHit = default;
            float closestDistance = float.PositiveInfinity;
            bool foundImpact = false;
            for (int index = 0; index < hitCount; index++)
            {
                Transform hitTransform = hits[index].transform;
                if (
                    hitTransform == null ||
                    IsPartOfSource(hitTransform) ||
                    hits[index].distance >= closestDistance)
                {
                    continue;
                }

                closestHit = hits[index];
                closestDistance = hits[index].distance;
                foundImpact = true;
            }

            if (!foundImpact)
            {
                return false;
            }

            Health health = closestHit.collider.GetComponentInParent<Health>();
            if (health != null && health.IsAlive)
            {
                health.ApplyDamage(new DamageInfo(
                    damage,
                    damageType,
                    closestHit.point,
                    closestHit.normal,
                    direction,
                    impactForce,
                    source));
            }

            initialized = false;
            Destroy(gameObject);
            return true;
        }

        private bool IsPartOfSource(Transform candidate)
        {
            return
                source != null &&
                (candidate == source.transform ||
                 candidate.IsChildOf(source.transform));
        }
    }
}
