using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class CharacterEquipment : MonoBehaviour
    {
        private readonly Dictionary<EquipmentSlot, WorldPickupDefinition>
            equippedItems = new();

        private CharacterInventory inventory;
        private CharacterStatistics statistics;
        private CharacterSecondaryStatistics secondaryStatistics;

        public event Action<EquipmentSlot, WorldPickupDefinition>
            EquipmentChanged;
        public IReadOnlyDictionary<EquipmentSlot, WorldPickupDefinition>
            EquippedItems => equippedItems;
        public EquipmentStatRequirement LastFailedRequirement { get; private set; }

        public float OutgoingDamageMultiplier
        {
            get
            {
                if (secondaryStatistics != null)
                {
                    return secondaryStatistics.GetValue(
                        "outgoing_damage_multiplier",
                        1f);
                }

                float multiplier = 1f;
                foreach (WorldPickupDefinition item in equippedItems.Values)
                {
                    if (item?.Equipment != null)
                    {
                        multiplier += item.Equipment.DamageMultiplierBonus;
                    }
                }

                return multiplier;
            }
        }

        public bool TryEquip(WorldPickupDefinition item)
        {
            if (inventory == null)
            {
                inventory = GetComponent<CharacterInventory>();
            }

            if (statistics == null)
            {
                statistics = GetComponent<CharacterStatistics>();
            }

            LastFailedRequirement = null;
            if (item?.Equipment == null || inventory == null)
            {
                return false;
            }

            if (!item.Equipment.MeetsRequirements(
                statistics,
                out EquipmentStatRequirement failedRequirement))
            {
                LastFailedRequirement = failedRequirement;
                return false;
            }

            if (!inventory.RemoveItem(item))
            {
                return false;
            }

            EquipmentSlot slot = item.Equipment.Slot;
            if (equippedItems.TryGetValue(
                slot,
                out WorldPickupDefinition previousItem))
            {
                inventory.AddItem(previousItem);
            }

            equippedItems[slot] = item;
            ApplyEquipmentModifiers(slot, item.Equipment);
            EquipmentChanged?.Invoke(slot, item);
            return true;
        }

        public bool TryUnequip(EquipmentSlot slot)
        {
            if (inventory == null)
            {
                inventory = GetComponent<CharacterInventory>();
            }

            if (
                inventory == null ||
                !equippedItems.TryGetValue(
                    slot,
                    out WorldPickupDefinition item))
            {
                return false;
            }

            equippedItems.Remove(slot);
            secondaryStatistics?.RemoveModifiers(GetModifierSource(slot));
            inventory.AddItem(item);
            EquipmentChanged?.Invoke(slot, null);
            return true;
        }

        public WorldPickupDefinition GetEquippedItem(EquipmentSlot slot)
        {
            return equippedItems.TryGetValue(slot, out WorldPickupDefinition item)
                ? item
                : null;
        }

        private void Awake()
        {
            inventory = GetComponent<CharacterInventory>();
            statistics = GetComponent<CharacterStatistics>();
            secondaryStatistics = GetComponent<CharacterSecondaryStatistics>();
        }

        private void ApplyEquipmentModifiers(
            EquipmentSlot slot,
            EquipmentDefinition definition)
        {
            if (secondaryStatistics == null)
            {
                secondaryStatistics = GetComponent<
                    CharacterSecondaryStatistics>();
            }

            secondaryStatistics?.SetModifiers(
                GetModifierSource(slot),
                definition.SecondaryStatModifiers);
        }

        private static string GetModifierSource(EquipmentSlot slot)
        {
            return $"equipment:{slot}";
        }
    }
}
