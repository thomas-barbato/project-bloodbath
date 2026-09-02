using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [CreateAssetMenu(
        fileName = "MeleeWeaponSettings",
        menuName = "Project Bloodbath/Combat/Melee Weapon Settings")]
    public sealed class MeleeWeaponSettings : ScriptableObject
    {
        [Header("Attaque")]
        [SerializeField, Min(1f)] private float damage = 58f;
        [SerializeField, Min(0.25f)] private float range = 2.4f;
        [SerializeField, Range(1f, 180f)] private float arcDegrees = 95f;
        [SerializeField, Min(1f)] private float attacksPerMinute = 92f;
        [SerializeField, Min(1)] private int maximumTargets = 3;

        [Header("Impact")]
        [SerializeField, Min(0f)] private float impactForce = 14f;
        [SerializeField] private DamageType damageType = DamageType.Melee;

        [Header("Animation prototype")]
        [SerializeField, Min(0.05f)] private float animationDuration = 0.56f;
        [SerializeField, Range(0.1f, 0.8f)] private float hitNormalizedTime = 0.43f;
        [SerializeField] private float windupAngle = -28f;
        [SerializeField] private float followThroughAngle = 105f;

        [Header("Retour de frappe")]
        [SerializeField, Min(0f)] private float cameraPitchKick = 0.3f;
        [SerializeField, Min(0f)] private float cameraYawKick = 0.65f;
        [SerializeField, Min(0f)] private float hitCameraKick = 0.35f;

        public float Damage => damage;
        public float Range => range;
        public float ArcDegrees => arcDegrees;
        public float SecondsPerAttack => 60f / attacksPerMinute;
        public int MaximumTargets => maximumTargets;
        public float ImpactForce => impactForce;
        public DamageType DamageType => damageType;
        public float AnimationDuration => animationDuration;
        public float HitNormalizedTime => hitNormalizedTime;
        public float WindupAngle => windupAngle;
        public float FollowThroughAngle => followThroughAngle;
        public float CameraPitchKick => cameraPitchKick;
        public float CameraYawKick => cameraYawKick;
        public float HitCameraKick => hitCameraKick;

        private void OnValidate()
        {
            damage = Mathf.Max(1f, damage);
            range = Mathf.Max(0.25f, range);
            arcDegrees = Mathf.Clamp(arcDegrees, 1f, 180f);
            attacksPerMinute = Mathf.Max(1f, attacksPerMinute);
            maximumTargets = Mathf.Max(1, maximumTargets);
            animationDuration = Mathf.Max(0.05f, animationDuration);
            hitNormalizedTime = Mathf.Clamp(hitNormalizedTime, 0.1f, 0.8f);
            cameraPitchKick = Mathf.Max(0f, cameraPitchKick);
            cameraYawKick = Mathf.Max(0f, cameraYawKick);
            hitCameraKick = Mathf.Max(0f, hitCameraKick);
        }
    }
}
