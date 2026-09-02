using UnityEngine;

namespace ProjectBloodbath.Combat
{
    public enum EnemyAttackDelivery
    {
        Direct,
        Projectile
    }

    [CreateAssetMenu(
        fileName = "EnemyAttackProfile",
        menuName = "Project Bloodbath/Combat/Enemy Attack Profile")]
    public sealed class EnemyAttackProfile : ScriptableObject
    {
        [Header("Impact")]
        [SerializeField, Min(0f)] private float damage = 12f;
        [SerializeField] private DamageType damageType = DamageType.Melee;
        [SerializeField, Min(0f)] private float impactForce;
        [SerializeField] private EnemyAttackDelivery delivery =
            EnemyAttackDelivery.Direct;
        [SerializeField] private bool requiresLineOfSight = true;

        [Header("Portée")]
        [SerializeField, Min(0.1f)] private float range = 1.65f;
        [SerializeField, Range(1f, 180f)] private float arcDegrees = 105f;

        [Header("Rythme")]
        [SerializeField, Min(0.05f)] private float windupDuration = 0.34f;
        [SerializeField, Min(0.05f)] private float recoveryDuration = 0.38f;
        [SerializeField, Min(0.1f)] private float cooldownDuration = 0.85f;

        [Header("Télégraphe prototype")]
        [SerializeField, Range(0f, 35f)] private float leanAngle = 16f;

        [Header("Projectile")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 12f;
        [SerializeField, Min(0.01f)] private float projectileRadius = 0.12f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 4f;
        [SerializeField, Min(0f)] private float launchHeight = 1.15f;

        public float Damage => damage;
        public DamageType DamageType => damageType;
        public float ImpactForce => impactForce;
        public EnemyAttackDelivery Delivery => delivery;
        public bool RequiresLineOfSight => requiresLineOfSight;
        public float Range => range;
        public float ArcDegrees => arcDegrees;
        public float WindupDuration => windupDuration;
        public float RecoveryDuration => recoveryDuration;
        public float CooldownDuration => cooldownDuration;
        public float LeanAngle => leanAngle;
        public EnemyProjectile ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileRadius => projectileRadius;
        public float ProjectileLifetime => projectileLifetime;
        public float LaunchHeight => launchHeight;

        private void OnValidate()
        {
            damage = Mathf.Max(0f, damage);
            impactForce = Mathf.Max(0f, impactForce);
            range = Mathf.Max(0.1f, range);
            arcDegrees = Mathf.Clamp(arcDegrees, 1f, 180f);
            windupDuration = Mathf.Max(0.05f, windupDuration);
            recoveryDuration = Mathf.Max(0.05f, recoveryDuration);
            cooldownDuration = Mathf.Max(0.1f, cooldownDuration);
            leanAngle = Mathf.Clamp(leanAngle, 0f, 35f);
            projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
            projectileRadius = Mathf.Max(0.01f, projectileRadius);
            projectileLifetime = Mathf.Max(0.1f, projectileLifetime);
            launchHeight = Mathf.Max(0f, launchHeight);
        }
    }
}
