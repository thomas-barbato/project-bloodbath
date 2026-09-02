using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    public enum EquipmentSlot
    {
        Head,
        Torso,
        Hands,
        Legs,
        Feet,
        Implant,
        Amulet,
        Ring
    }

    [CreateAssetMenu(
        fileName = "EquipmentDefinition",
        menuName = "Project Bloodbath/Progression/Equipment")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "equipment";
        [SerializeField] private string displayName = "Équipement";
        [SerializeField] private EquipmentSlot slot;
        [SerializeField, Min(0f)] private float damageMultiplierBonus;
        [SerializeField] private List<EquipmentStatRequirement> requirements =
            new();
        [SerializeField] private List<SecondaryStatModifier>
            secondaryStatModifiers = new();

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public EquipmentSlot Slot => slot;
        public float DamageMultiplierBonus => damageMultiplierBonus;
        public IReadOnlyList<EquipmentStatRequirement> Requirements =>
            requirements;
        public IReadOnlyList<SecondaryStatModifier> SecondaryStatModifiers =>
            secondaryStatModifiers;

        public void Configure(
            string equipmentIdentifier,
            string equipmentDisplayName,
            EquipmentSlot equipmentSlot,
            float outgoingDamageBonus,
            IReadOnlyList<EquipmentStatRequirement> statRequirements = null,
            IReadOnlyList<SecondaryStatModifier> statModifiers = null)
        {
            identifier = string.IsNullOrWhiteSpace(equipmentIdentifier)
                ? "equipment"
                : equipmentIdentifier.Trim();
            displayName = string.IsNullOrWhiteSpace(equipmentDisplayName)
                ? "Équipement"
                : equipmentDisplayName.Trim();
            slot = equipmentSlot;
            damageMultiplierBonus = Mathf.Max(0f, outgoingDamageBonus);
            requirements ??= new List<EquipmentStatRequirement>();
            requirements.Clear();
            if (statRequirements != null)
            {
                foreach (EquipmentStatRequirement requirement in
                    statRequirements)
                {
                    if (requirement?.Statistic != null)
                    {
                        requirements.Add(requirement);
                    }
                }
            }

            secondaryStatModifiers ??= new List<SecondaryStatModifier>();
            secondaryStatModifiers.Clear();
            if (statModifiers != null)
            {
                foreach (SecondaryStatModifier modifier in statModifiers)
                {
                    if (modifier?.Statistic != null)
                    {
                        secondaryStatModifiers.Add(modifier);
                    }
                }
            }
        }

        public bool MeetsRequirements(
            CharacterStatistics statistics,
            out EquipmentStatRequirement firstUnmetRequirement)
        {
            if (requirements == null)
            {
                firstUnmetRequirement = null;
                return true;
            }

            foreach (EquipmentStatRequirement requirement in requirements)
            {
                if (requirement != null && !requirement.IsMetBy(statistics))
                {
                    firstUnmetRequirement = requirement;
                    return false;
                }
            }

            firstUnmetRequirement = null;
            return true;
        }

        private void OnValidate()
        {
            damageMultiplierBonus = Mathf.Max(0f, damageMultiplierBonus);
        }
    }
}
