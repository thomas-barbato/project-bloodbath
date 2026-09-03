using ProjectBloodbath.Combat;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "ActiveAbilitySettings",
        menuName = "Project Bloodbath/Progression/Active Ability Settings")]
    public sealed class ActiveAbilitySettings : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField] private string displayName = "Onde de rupture";

        [Header("Utilisation")]
        [SerializeField, Min(0f)] private float resourceCost = 30f;
        [SerializeField, Min(0f)] private float cooldownDuration = 4f;

        [Header("Effet")]
        [SerializeField, Min(0.1f)] private float range = 6f;
        [SerializeField, Range(1f, 360f)] private float arcDegrees = 90f;
        [SerializeField, Min(0f)] private float damage = 25f;
        [SerializeField, Min(0f)] private float impactForce = 16f;
        [SerializeField, Min(1)] private int maximumTargets = 8;
        [SerializeField] private DamageType damageType = DamageType.Extradimensional;

        [Header("Synergie")]
        [SerializeField] private WeaponMarkEffectSettings consumedMarkEffect;

        public string DisplayName => displayName;
        public float ResourceCost => resourceCost;
        public float CooldownDuration => cooldownDuration;
        public float Range => range;
        public float ArcDegrees => arcDegrees;
        public float Damage => damage;
        public float ImpactForce => impactForce;
        public int MaximumTargets => maximumTargets;
        public DamageType DamageType => damageType;
        public WeaponMarkEffectSettings ConsumedMarkEffect => consumedMarkEffect;

        public void SetConsumedMarkEffect(WeaponMarkEffectSettings effect)
        {
            consumedMarkEffect = effect;
        }

        private void OnValidate()
        {
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Compétence"
                : displayName.Trim();
            resourceCost = Mathf.Max(0f, resourceCost);
            cooldownDuration = Mathf.Max(0f, cooldownDuration);
            range = Mathf.Max(0.1f, range);
            damage = Mathf.Max(0f, damage);
            impactForce = Mathf.Max(0f, impactForce);
            maximumTargets = Mathf.Max(1, maximumTargets);
        }
    }
}
