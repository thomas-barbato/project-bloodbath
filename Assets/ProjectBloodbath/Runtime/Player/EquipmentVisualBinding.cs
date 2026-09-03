using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Player
{
    [DisallowMultipleComponent]
    public sealed class EquipmentVisualBinding : MonoBehaviour
    {
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private string equipmentIdentifier;
        [SerializeField] private GameObject visualRoot;

        public EquipmentSlot Slot => slot;
        public string EquipmentIdentifier => equipmentIdentifier;
        public bool IsVisible => ResolveVisualRoot().activeSelf;

        public void Configure(
            EquipmentSlot equipmentSlot,
            string identifier,
            GameObject root = null)
        {
            slot = equipmentSlot;
            equipmentIdentifier = identifier?.Trim() ?? string.Empty;
            visualRoot = root;
        }

        public bool Matches(WorldPickupDefinition item)
        {
            return
                item?.Equipment != null &&
                item.Equipment.Slot == slot &&
                string.Equals(
                    item.Equipment.Identifier,
                    equipmentIdentifier,
                    System.StringComparison.Ordinal);
        }

        public void SetVisible(bool visible)
        {
            GameObject root = ResolveVisualRoot();
            if (root.activeSelf != visible)
            {
                root.SetActive(visible);
            }
        }

        private GameObject ResolveVisualRoot()
        {
            return visualRoot != null ? visualRoot : gameObject;
        }
    }
}
