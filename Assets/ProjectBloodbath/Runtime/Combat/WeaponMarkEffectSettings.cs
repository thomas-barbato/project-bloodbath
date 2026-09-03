using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [CreateAssetMenu(
        fileName = "WeaponMarkEffectSettings",
        menuName = "Project Bloodbath/Combat/Weapon Mark Effect Settings")]
    public sealed class WeaponMarkEffectSettings : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField] private string identifier = "weapon_mark";
        [SerializeField] private string displayName = "Marque d'arme";

        [Header("Accumulation")]
        [SerializeField, Min(1)] private int maximumStacks = 3;
        [SerializeField, Min(0.1f)] private float duration = 6f;

        [Header("Détonation")]
        [SerializeField, Min(0f)] private float detonationDamagePerStack = 14f;

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public int MaximumStacks => maximumStacks;
        public float Duration => duration;
        public float DetonationDamagePerStack => detonationDamagePerStack;

        public void Configure(
            string effectIdentifier,
            string effectDisplayName,
            int stacks,
            float effectDuration,
            float damagePerStack)
        {
            identifier = string.IsNullOrWhiteSpace(effectIdentifier)
                ? "weapon_mark"
                : effectIdentifier.Trim();
            displayName = string.IsNullOrWhiteSpace(effectDisplayName)
                ? "Marque d'arme"
                : effectDisplayName.Trim();
            maximumStacks = Mathf.Max(1, stacks);
            duration = Mathf.Max(0.1f, effectDuration);
            detonationDamagePerStack = Mathf.Max(0f, damagePerStack);
        }

        private void OnValidate()
        {
            Configure(
                identifier,
                displayName,
                maximumStacks,
                duration,
                detonationDamagePerStack);
        }
    }
}
