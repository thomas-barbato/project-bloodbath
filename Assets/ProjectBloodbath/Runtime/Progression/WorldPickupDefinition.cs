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
        [SerializeField] private Color prototypeColor =
            new(0.75f, 0.18f, 0.08f, 1f);

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public WorldPickupKind Kind => kind;
        public WorldPickupMode PickupMode => pickupMode;
        public InventoryResourceDefinition Resource => resource;
        public EquipmentDefinition Equipment => equipment;
        public Color PrototypeColor => prototypeColor;

        public void Configure(
            string pickupIdentifier,
            string pickupDisplayName,
            WorldPickupKind pickupKind,
            WorldPickupMode mode,
            InventoryResourceDefinition resourceDefinition,
            Color color,
            EquipmentDefinition equipmentDefinition = null)
        {
            identifier = pickupIdentifier;
            displayName = pickupDisplayName;
            kind = pickupKind;
            pickupMode = mode;
            resource = resourceDefinition;
            equipment = equipmentDefinition;
            prototypeColor = color;
            ValidateValues();
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
