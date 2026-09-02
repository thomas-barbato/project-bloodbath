using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    public sealed class CharacterInventory : MonoBehaviour
    {
        private readonly Dictionary<InventoryResourceDefinition, int>
            resourceQuantities = new();
        private readonly List<WorldPickupDefinition> items = new();

        public event Action<InventoryResourceDefinition, int> ResourceChanged;
        public event Action<string, int> PickupCollected;

        public IReadOnlyList<WorldPickupDefinition> Items => items;

        public int GetResourceQuantity(InventoryResourceDefinition resource)
        {
            return resource != null &&
                resourceQuantities.TryGetValue(resource, out int quantity)
                    ? quantity
                    : 0;
        }

        public int AddResource(
            InventoryResourceDefinition resource,
            int requestedQuantity)
        {
            if (resource == null || requestedQuantity <= 0)
            {
                return 0;
            }

            int current = GetResourceQuantity(resource);
            int accepted = Mathf.Min(
                requestedQuantity,
                resource.MaximumCarried - current);
            if (accepted <= 0)
            {
                return 0;
            }

            int updated = current + accepted;
            resourceQuantities[resource] = updated;
            ResourceChanged?.Invoke(resource, updated);
            return accepted;
        }

        public bool RemoveResource(
            InventoryResourceDefinition resource,
            int requestedQuantity)
        {
            if (
                resource == null ||
                requestedQuantity <= 0 ||
                GetResourceQuantity(resource) < requestedQuantity)
            {
                return false;
            }

            int updated = GetResourceQuantity(resource) - requestedQuantity;
            resourceQuantities[resource] = updated;
            ResourceChanged?.Invoke(resource, updated);
            return true;
        }

        public void EnsureAtLeast(
            InventoryResourceDefinition resource,
            int minimumQuantity)
        {
            if (resource == null || minimumQuantity <= 0)
            {
                return;
            }

            int current = GetResourceQuantity(resource);
            AddResource(resource, minimumQuantity - current);
        }

        public void SetResourceQuantity(
            InventoryResourceDefinition resource,
            int quantity)
        {
            if (resource == null)
            {
                return;
            }

            int updated = Mathf.Clamp(quantity, 0, resource.MaximumCarried);
            resourceQuantities[resource] = updated;
            ResourceChanged?.Invoke(resource, updated);
        }

        public bool AddItem(WorldPickupDefinition item)
        {
            if (item == null || item.Kind != WorldPickupKind.Item)
            {
                return false;
            }

            items.Add(item);
            return true;
        }

        public bool RemoveItem(WorldPickupDefinition item)
        {
            return item != null && items.Remove(item);
        }

        public bool ContainsItem(WorldPickupDefinition item)
        {
            return item != null && items.Contains(item);
        }

        public void NotifyPickupCollected(string displayName, int quantity)
        {
            if (string.IsNullOrWhiteSpace(displayName) || quantity <= 0)
            {
                return;
            }

            PickupCollected?.Invoke(displayName, quantity);
        }
    }
}
