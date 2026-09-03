using UnityEngine;

namespace ProjectBloodbath.Progression
{
    public enum WorldPickupKind
    {
        StackableResource,
        Item
    }

    public enum WorldPickupMode
    {
        Automatic,
        Manual
    }

    public enum InventoryItemCategory
    {
        Miscellaneous,
        Weapon,
        Armor,
        Implant,
        QuestItem
    }

    [CreateAssetMenu(
        fileName = "WorldPickupDefinition",
        menuName = "Project Bloodbath/Progression/World Pickup")]
    public sealed class WorldPickupDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "pickup";
        [SerializeField] private string displayName = "Objet";
        [SerializeField] private WorldPickupKind kind;
        [SerializeField] private WorldPickupMode pickupMode;
        [SerializeField] private InventoryResourceDefinition resource;
        [SerializeField] private EquipmentDefinition equipment;
        [SerializeField] private InventoryItemCategory inventoryCategory;
        [SerializeField] private Color prototypeColor =
            new(0.75f, 0.18f, 0.08f, 1f);

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public WorldPickupKind Kind => kind;
        public WorldPickupMode PickupMode => pickupMode;
        public InventoryResourceDefinition Resource => resource;
        public EquipmentDefinition Equipment => equipment;
        public InventoryItemCategory InventoryCategory =>
            ResolveInventoryCategory();
        public Color PrototypeColor => prototypeColor;

        public void Configure(
            string pickupIdentifier,
            string pickupDisplayName,
            WorldPickupKind pickupKind,
            WorldPickupMode mode,
            InventoryResourceDefinition resourceDefinition,
            Color color,
            EquipmentDefinition equipmentDefinition = null,
            InventoryItemCategory itemCategory =
                InventoryItemCategory.Miscellaneous)
        {
            identifier = pickupIdentifier;
            displayName = pickupDisplayName;
            kind = pickupKind;
            pickupMode = mode;
            resource = resourceDefinition;
            equipment = equipmentDefinition;
            inventoryCategory = itemCategory;
            prototypeColor = color;
            ValidateValues();
        }

        private InventoryItemCategory ResolveInventoryCategory()
        {
            if (inventoryCategory != InventoryItemCategory.Miscellaneous)
            {
                return inventoryCategory;
            }

            if (equipment == null)
            {
                return InventoryItemCategory.Miscellaneous;
            }

            if (equipment.IsHandEquipment)
            {
                return InventoryItemCategory.Weapon;
            }

            return EquipmentDefinition.IsImplantSlot(equipment.Slot)
                ? InventoryItemCategory.Implant
                : InventoryItemCategory.Armor;
        }

        private void OnValidate()
        {
            ValidateValues();
        }

        private void ValidateValues()
        {
            identifier = string.IsNullOrWhiteSpace(identifier)
                ? "pickup"
                : identifier.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Objet"
                : displayName.Trim();
        }
    }
}
