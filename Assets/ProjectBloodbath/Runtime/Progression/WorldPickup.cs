using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    public sealed class WorldPickup : MonoBehaviour
    {
        [SerializeField] private WorldPickupDefinition definition;
        [SerializeField, Min(1)] private int remainingQuantity = 1;

        public WorldPickupDefinition Definition => definition;
        public int RemainingQuantity => remainingQuantity;
        public string DisplayName => definition == null
            ? string.Empty
            : definition.DisplayName;
        public WorldPickupMode PickupMode => definition == null
            ? WorldPickupMode.Manual
            : definition.PickupMode;

        public void Configure(
            WorldPickupDefinition pickupDefinition,
            int quantity)
        {
            definition = pickupDefinition;
            remainingQuantity = Mathf.Max(1, quantity);
        }

        public bool TryCollect(CharacterInventory inventory)
        {
            if (
                inventory == null ||
                definition == null ||
                remainingQuantity <= 0)
            {
                return false;
            }

            int accepted = 0;
            if (
                definition.Kind == WorldPickupKind.StackableResource &&
                definition.Resource != null)
            {
                accepted = inventory.AddResource(
                    definition.Resource,
                    remainingQuantity);
            }
            else if (
                definition.Kind == WorldPickupKind.Item &&
                inventory.AddItem(definition))
            {
                accepted = 1;
            }

            if (accepted <= 0)
            {
                return false;
            }

            remainingQuantity -= accepted;
            inventory.NotifyPickupCollected(definition.DisplayName, accepted);
            if (remainingQuantity <= 0)
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryCollectAutomatically(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryCollectAutomatically(other);
        }

        private void TryCollectAutomatically(Collider other)
        {
            if (
                definition == null ||
                definition.PickupMode != WorldPickupMode.Automatic)
            {
                return;
            }

            CharacterInventory inventory =
                other.GetComponentInParent<CharacterInventory>();
            TryCollect(inventory);
        }

        private void OnValidate()
        {
            remainingQuantity = Mathf.Max(1, remainingQuantity);
        }
    }
}
