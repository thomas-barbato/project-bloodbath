using System;
using ProjectBloodbath.Input;
using UnityEngine;

namespace ProjectBloodbath.Combat
{
    public enum PrototypeWeaponSlot
    {
        Ranged,
        Melee
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeWeaponLoadout : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private GameObject rangedWeapon;
        [SerializeField] private GameObject meleeWeapon;
        [SerializeField] private PrototypeWeaponSlot initialSlot =
            PrototypeWeaponSlot.Ranged;

        public event Action<PrototypeWeaponSlot> WeaponChanged;

        public PrototypeWeaponSlot CurrentSlot { get; private set; }

        public void Configure(
            PlayerInputReader reader,
            GameObject rangedWeaponObject,
            GameObject meleeWeaponObject,
            PrototypeWeaponSlot startingSlot = PrototypeWeaponSlot.Ranged)
        {
            inputReader = reader;
            rangedWeapon = rangedWeaponObject;
            meleeWeapon = meleeWeaponObject;
            initialSlot = startingSlot;
            Select(startingSlot, false);
        }

        public void Select(PrototypeWeaponSlot slot)
        {
            Select(slot, true);
        }

        private void Awake()
        {
            Select(initialSlot, false);
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            if (inputReader.SelectRangedPressedThisFrame)
            {
                Select(PrototypeWeaponSlot.Ranged);
            }
            else if (inputReader.SelectMeleePressedThisFrame)
            {
                Select(PrototypeWeaponSlot.Melee);
            }
        }

        private void Select(PrototypeWeaponSlot slot, bool notify)
        {
            CurrentSlot = slot;
            if (rangedWeapon != null)
            {
                rangedWeapon.SetActive(slot == PrototypeWeaponSlot.Ranged);
            }

            if (meleeWeapon != null)
            {
                meleeWeapon.SetActive(slot == PrototypeWeaponSlot.Melee);
            }

            if (notify)
            {
                WeaponChanged?.Invoke(slot);
            }
        }
    }
}
