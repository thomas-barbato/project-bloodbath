using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [Serializable]
    public sealed class StartingEquipmentEntry
    {
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private WorldPickupDefinition item;

        public EquipmentSlot Slot => slot;
        public WorldPickupDefinition Item => item;
    }

    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class CharacterEquipment : MonoBehaviour
    {
        private readonly Dictionary<EquipmentSlot, WorldPickupDefinition>
            equippedItems = new();

        [SerializeField] private List<StartingEquipmentEntry>
            startingEquipment = new();

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
            return item?.Equipment != null &&
                TryEquip(item, item.Equipment.Slot);
        }

        public bool TryEquip(
            WorldPickupDefinition item,
            EquipmentSlot targetSlot)
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
            if (
                item?.Equipment == null ||
                inventory == null ||
                !item.Equipment.CanEquipIn(targetSlot))
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

            EquipmentSlot slot = targetSlot;
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
            ApplyStartingEquipment();
        }

        private void ApplyStartingEquipment()
        {
            if (inventory == null || startingEquipment == null)
            {
                return;
            }

            foreach (StartingEquipmentEntry entry in startingEquipment)
            {
                if (
                    entry?.Item == null ||
                    equippedItems.ContainsKey(entry.Slot))
                {
                    continue;
                }

                inventory.AddItem(entry.Item);
                TryEquip(entry.Item, entry.Slot);
            }
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
