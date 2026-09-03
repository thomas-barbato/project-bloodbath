using System.Collections.Generic;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterEquipment))]
    public sealed class CharacterEquipmentPresentation : MonoBehaviour
    {
        [SerializeField] private CharacterEquipment equipment;

        private readonly List<EquipmentVisualBinding> bindings = new();

        public IReadOnlyList<EquipmentVisualBinding> Bindings => bindings;
        public int VisibleBindingCount
        {
            get
            {
                int visibleCount = 0;
                for (int index = 0; index < bindings.Count; index++)
                {
                    if (bindings[index] != null && bindings[index].IsVisible)
                    {
                        visibleCount++;
                    }
                }

                return visibleCount;
            }
        }

        public void RefreshBindings()
        {
            bindings.Clear();
            GetComponentsInChildren(true, bindings);
            SyncAll();
        }

        private void Awake()
        {
            equipment ??= GetComponent<CharacterEquipment>();
            RefreshBindings();
        }

        private void OnEnable()
        {
            equipment ??= GetComponent<CharacterEquipment>();
            if (equipment != null)
            {
                equipment.EquipmentChanged += OnEquipmentChanged;
            }

            RefreshBindings();
        }

        private void OnDisable()
        {
            if (equipment != null)
            {
                equipment.EquipmentChanged -= OnEquipmentChanged;
            }
        }

        private void OnEquipmentChanged(
            EquipmentSlot changedSlot,
            WorldPickupDefinition item)
        {
            for (int index = 0; index < bindings.Count; index++)
            {
                EquipmentVisualBinding binding = bindings[index];
                if (binding != null && binding.Slot == changedSlot)
                {
                    binding.SetVisible(binding.Matches(item));
                }
            }
        }

        private void SyncAll()
        {
            if (equipment == null)
            {
                return;
            }

            for (int index = 0; index < bindings.Count; index++)
            {
                EquipmentVisualBinding binding = bindings[index];
                if (binding == null)
                {
                    continue;
                }

                binding.SetVisible(binding.Matches(
                    equipment.GetEquippedItem(binding.Slot)));
            }
        }
    }
}
