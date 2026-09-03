using System;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectBloodbath.Combat
{
    public enum PrototypeHandSetSlot
    {
        Primary,
        Secondary
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterEquipment))]
    public sealed class PrototypeWeaponLoadout : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private CharacterEquipment equipment;
        [SerializeField, FormerlySerializedAs("primaryRightWeapon")]
        private GameObject rightRangedWeapon;
        [SerializeField, FormerlySerializedAs("primaryLeftWeapon")]
        private GameObject leftRangedWeapon;
        [SerializeField, FormerlySerializedAs("secondaryRightWeapon")]
        private GameObject rightMeleeWeapon;
        [SerializeField, FormerlySerializedAs("secondaryLeftWeapon")]
        private GameObject leftMeleeWeapon;
        [SerializeField, FormerlySerializedAs("initialSlot")]
        private PrototypeHandSetSlot initialHandSet =
            PrototypeHandSetSlot.Primary;

        public event Action<PrototypeHandSetSlot> HandSetChanged;

        public PrototypeHandSetSlot ActiveHandSet { get; private set; }
        public bool CombatEnabled { get; private set; } = true;
        public GameObject ActiveRightWeapon { get; private set; }
        public GameObject ActiveLeftWeapon { get; private set; }
        public HitscanWeapon ActiveRightRangedWeapon { get; private set; }
        public HitscanWeapon ActiveLeftRangedWeapon { get; private set; }
        public MeleeWeapon ActiveRightMeleeWeapon { get; private set; }
        public MeleeWeapon ActiveLeftMeleeWeapon { get; private set; }
        public bool HasTwoActiveRangedWeapons =>
            ActiveRightRangedWeapon != null &&
            ActiveLeftRangedWeapon != null;

        public void ConfigureHandSets(
            PlayerInputReader reader,
            GameObject primaryRight,
            GameObject primaryLeft,
            GameObject secondaryRight,
            GameObject secondaryLeft,
            PrototypeHandSetSlot startingSet = PrototypeHandSetSlot.Primary)
        {
            inputReader = reader;
            rightRangedWeapon = primaryRight;
            leftRangedWeapon = primaryLeft;
            rightMeleeWeapon = secondaryRight;
            leftMeleeWeapon = secondaryLeft;
            initialHandSet = startingSet;
            SelectHandSet(startingSet, false);
        }

        public void SelectHandSet(PrototypeHandSetSlot handSet)
        {
            SelectHandSet(handSet, true);
        }

        public void SwapHandSet()
        {
            SelectHandSet(
                ActiveHandSet == PrototypeHandSetSlot.Primary
                    ? PrototypeHandSetSlot.Secondary
                    : PrototypeHandSetSlot.Primary,
                true);
        }

        public void SetCombatEnabled(bool enabled)
        {
            CombatEnabled = enabled;
            ApplyWeaponVisibility();
        }

        public bool TryUseHand(CombatHand hand)
        {
            if (!CombatEnabled)
            {
                return false;
            }

            GameObject weaponObject = hand == CombatHand.Right
                ? ActiveRightWeapon
                : ActiveLeftWeapon;
            if (weaponObject == null || !weaponObject.activeInHierarchy)
            {
                return false;
            }

            HitscanWeapon rangedWeapon =
                weaponObject.GetComponent<HitscanWeapon>();
            if (rangedWeapon != null)
            {
                bool fired = rangedWeapon.TryFire();
                if (!fired && rangedWeapon.CurrentMagazine <= 0)
                {
                    rangedWeapon.TryStartReload();
                }

                return fired;
            }

            MeleeWeapon meleeWeapon = weaponObject.GetComponent<MeleeWeapon>();
            return meleeWeapon != null && meleeWeapon.TryAttack();
        }

        public bool TryReloadActiveWeapons()
        {
            if (!CombatEnabled)
            {
                return false;
            }

            bool reloadStarted = false;
            if (ActiveRightRangedWeapon != null)
            {
                reloadStarted |= ActiveRightRangedWeapon.TryStartReload();
            }

            if (
                ActiveLeftRangedWeapon != null &&
                ActiveLeftRangedWeapon != ActiveRightRangedWeapon)
            {
                reloadStarted |= ActiveLeftRangedWeapon.TryStartReload();
            }

            return reloadStarted;
        }

        private void Awake()
        {
            equipment ??= GetComponent<CharacterEquipment>();
            SelectHandSet(initialHandSet, false);
        }

        private void OnEnable()
        {
            equipment ??= GetComponent<CharacterEquipment>();
            if (equipment != null)
            {
                equipment.EquipmentChanged += OnEquipmentChanged;
            }

            CacheActiveWeapons();
            ApplyWeaponVisibility();
        }

        private void OnDisable()
        {
            if (equipment != null)
            {
                equipment.EquipmentChanged -= OnEquipmentChanged;
            }
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            if (inputReader.SwapHandSetPressedThisFrame)
            {
                SwapHandSet();
            }

            if (!CombatEnabled || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            if (inputReader.ConsumeReloadPressed())
            {
                TryReloadActiveWeapons();
            }

            HandleHandInput(
                CombatHand.Right,
                inputReader.RightHandHeld,
                inputReader.RightHandPressedThisFrame);
            HandleHandInput(
                CombatHand.Left,
                inputReader.LeftHandHeld,
                inputReader.LeftHandPressedThisFrame);
        }

        private void HandleHandInput(
            CombatHand hand,
            bool held,
            bool pressed)
        {
            GameObject weaponObject = hand == CombatHand.Right
                ? ActiveRightWeapon
                : ActiveLeftWeapon;
            if (weaponObject == null)
            {
                return;
            }

            HitscanWeapon rangedWeapon =
                weaponObject.GetComponent<HitscanWeapon>();
            bool wantsToUse = rangedWeapon != null &&
                rangedWeapon.Settings != null
                    ? rangedWeapon.Settings.Automatic ? held : pressed
                    : pressed;
            if (wantsToUse)
            {
                TryUseHand(hand);
            }
        }

        private void SelectHandSet(
            PrototypeHandSetSlot handSet,
            bool notify)
        {
            ActiveHandSet = handSet;
            CacheActiveWeapons();
            ApplyWeaponVisibility();

            if (notify)
            {
                HandSetChanged?.Invoke(handSet);
            }
        }

        private void CacheActiveWeapons()
        {
            EquipmentSlot rightSlot = GetHandSlot(
                ActiveHandSet,
                CombatHand.Right);
            EquipmentSlot leftSlot = GetHandSlot(
                ActiveHandSet,
                CombatHand.Left);
            ActiveRightWeapon = ResolveWeaponVisual(
                equipment?.GetEquippedItem(rightSlot),
                CombatHand.Right);
            ActiveLeftWeapon = ResolveWeaponVisual(
                equipment?.GetEquippedItem(leftSlot),
                CombatHand.Left);
            ActiveRightRangedWeapon =
                GetWeaponComponent<HitscanWeapon>(ActiveRightWeapon);
            ActiveLeftRangedWeapon =
                GetWeaponComponent<HitscanWeapon>(ActiveLeftWeapon);
            ActiveRightMeleeWeapon =
                GetWeaponComponent<MeleeWeapon>(ActiveRightWeapon);
            ActiveLeftMeleeWeapon =
                GetWeaponComponent<MeleeWeapon>(ActiveLeftWeapon);
        }

        private static T GetWeaponComponent<T>(GameObject weaponObject)
            where T : Component
        {
            return weaponObject == null
                ? null
                : weaponObject.GetComponent<T>();
        }

        private void ApplyWeaponVisibility()
        {
            ApplyWeaponVisibility(rightRangedWeapon);
            ApplyWeaponVisibility(leftRangedWeapon);
            ApplyWeaponVisibility(rightMeleeWeapon);
            ApplyWeaponVisibility(leftMeleeWeapon);
        }

        private void ApplyWeaponVisibility(GameObject weaponObject)
        {
            if (weaponObject == null)
            {
                return;
            }

            bool belongsToActiveSet =
                weaponObject == ActiveRightWeapon ||
                weaponObject == ActiveLeftWeapon;
            weaponObject.SetActive(CombatEnabled && belongsToActiveSet);
        }

        private GameObject ResolveWeaponVisual(
            WorldPickupDefinition item,
            CombatHand hand)
        {
            HandEquipmentType handType = item?.Equipment == null
                ? HandEquipmentType.None
                : item.Equipment.HandEquipmentType;
            return handType switch
            {
                HandEquipmentType.RangedWeapon =>
                    hand == CombatHand.Right
                        ? rightRangedWeapon
                        : leftRangedWeapon,
                HandEquipmentType.MeleeWeapon =>
                    hand == CombatHand.Right
                        ? rightMeleeWeapon
                        : leftMeleeWeapon,
                _ => null
            };
        }

        private void OnEquipmentChanged(
            EquipmentSlot changedSlot,
            WorldPickupDefinition item)
        {
            if (!EquipmentDefinition.IsHandSlot(changedSlot))
            {
                return;
            }

            CacheActiveWeapons();
            ApplyWeaponVisibility();
            HandSetChanged?.Invoke(ActiveHandSet);
        }

        public static EquipmentSlot GetHandSlot(
            PrototypeHandSetSlot handSet,
            CombatHand hand)
        {
            if (handSet == PrototypeHandSetSlot.Primary)
            {
                return hand == CombatHand.Right
                    ? EquipmentSlot.PrimaryRightHand
                    : EquipmentSlot.PrimaryLeftHand;
            }

            return hand == CombatHand.Right
                ? EquipmentSlot.SecondaryRightHand
                : EquipmentSlot.SecondaryLeftHand;
        }
    }
}
