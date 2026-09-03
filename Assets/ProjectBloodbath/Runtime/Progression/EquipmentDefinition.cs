using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    public enum EquipmentSlot
    {
        Head = 0,
        Torso = 1,
        Hands = 2,
        Legs = 3,
        Feet = 4,
        Implant = 5,
        ImplantSecondary = 6,
        ImplantTertiary = 7,
        PrimaryRightHand = 8,
        PrimaryLeftHand = 9,
        SecondaryRightHand = 10,
        SecondaryLeftHand = 11
    }

    public enum HandEquipmentType
    {
        None,
        RangedWeapon,
        MeleeWeapon,
        Shield
    }

    [CreateAssetMenu(
        fileName = "EquipmentDefinition",
        menuName = "Project Bloodbath/Progression/Equipment")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "equipment";
        [SerializeField] private string displayName = "Équipement";
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private HandEquipmentType handEquipmentType;
        [SerializeField, Min(0f)] private float damageMultiplierBonus;
        [SerializeField] private List<EquipmentStatRequirement> requirements =
            new();
        [SerializeField] private List<SecondaryStatModifier>
            secondaryStatModifiers = new();

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public EquipmentSlot Slot => slot;
        public HandEquipmentType HandEquipmentType => handEquipmentType;
        public bool IsHandEquipment =>
            handEquipmentType != HandEquipmentType.None;
        public bool IsImplantEquipment => IsImplantSlot(slot);
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
            IReadOnlyList<SecondaryStatModifier> statModifiers = null,
            HandEquipmentType equipmentHandType = HandEquipmentType.None)
        {
            identifier = string.IsNullOrWhiteSpace(equipmentIdentifier)
                ? "equipment"
                : equipmentIdentifier.Trim();
            displayName = string.IsNullOrWhiteSpace(equipmentDisplayName)
                ? "Équipement"
                : equipmentDisplayName.Trim();
            slot = equipmentSlot;
            handEquipmentType = equipmentHandType;
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

        public bool CanEquipIn(EquipmentSlot targetSlot)
        {
            if (!IsHandEquipment)
            {
                if (IsImplantEquipment)
                {
                    return IsImplantSlot(targetSlot);
                }

                return targetSlot == slot;
            }

            if (!IsHandSlot(targetSlot))
            {
                return false;
            }

            return handEquipmentType != HandEquipmentType.Shield ||
                IsLeftHandSlot(targetSlot);
        }

        public static bool IsHandSlot(EquipmentSlot targetSlot)
        {
            return targetSlot == EquipmentSlot.PrimaryRightHand ||
                targetSlot == EquipmentSlot.PrimaryLeftHand ||
                targetSlot == EquipmentSlot.SecondaryRightHand ||
                targetSlot == EquipmentSlot.SecondaryLeftHand;
        }

        public static bool IsImplantSlot(EquipmentSlot targetSlot)
        {
            return targetSlot == EquipmentSlot.Implant ||
                targetSlot == EquipmentSlot.ImplantSecondary ||
                targetSlot == EquipmentSlot.ImplantTertiary;
        }

        public static bool IsLeftHandSlot(EquipmentSlot targetSlot)
        {
            return targetSlot == EquipmentSlot.PrimaryLeftHand ||
                targetSlot == EquipmentSlot.SecondaryLeftHand;
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
