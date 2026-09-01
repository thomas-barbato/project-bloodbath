using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [CreateAssetMenu(
        fileName = "HitscanWeaponSettings",
        menuName = "Project Bloodbath/Combat/Hitscan Weapon Settings")]
    public sealed class HitscanWeaponSettings : ScriptableObject
    {
        [Header("Tir")]
        [SerializeField, Min(1f)] private float damage = 34f;
        [SerializeField, Min(1f)] private float range = 80f;
        [SerializeField, Min(1f)] private float roundsPerMinute = 480f;
        [SerializeField, Min(0f)] private float spreadDegrees = 0.25f;
        [SerializeField] private bool automatic = true;

        [Header("Impact")]
        [SerializeField, Min(0f)] private float impactForce = 18f;
        [SerializeField] private DamageType damageType = DamageType.Ballistic;

        [Header("Recul")]
        [SerializeField, Min(0f)] private float pitchRecoil = 0.85f;
        [SerializeField, Min(0f)] private float yawRecoil = 0.3f;
        [SerializeField, Min(0f)] private float visualKick = 0.08f;
        [SerializeField, Min(0.01f)] private float visualRecovery = 14f;

        public float Damage => damage;
        public float Range => range;
        public float SecondsPerShot => 60f / roundsPerMinute;
        public float SpreadDegrees => spreadDegrees;
        public bool Automatic => automatic;
        public float ImpactForce => impactForce;
        public DamageType DamageType => damageType;
        public float PitchRecoil => pitchRecoil;
        public float YawRecoil => yawRecoil;
        public float VisualKick => visualKick;
        public float VisualRecovery => visualRecovery;

        private void OnValidate()
        {
            damage = Mathf.Max(1f, damage);
            range = Mathf.Max(1f, range);
            roundsPerMinute = Mathf.Max(1f, roundsPerMinute);
        }
    }
}
