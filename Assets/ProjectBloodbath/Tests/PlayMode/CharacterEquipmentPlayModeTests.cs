using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class CharacterEquipmentPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private CharacterInventory inventory;
        private CharacterEquipment equipment;
        private PrototypeWeaponLoadout weaponLoadout;
        private PrototypePlayerLife playerLife;
        private WorldPickup implantPickup;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                ScenePath,
                LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null);
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            GameObject.Find("PrototypeEnemy")?.SetActive(false);
            GameObject.Find("PrototypeSkirmisher")?.SetActive(false);

            GameObject player = GameObject.Find("Player");
            inventory = player.GetComponent<CharacterInventory>();
            equipment = player.GetComponent<CharacterEquipment>();
            weaponLoadout = player.GetComponent<PrototypeWeaponLoadout>();
            playerLife = player.GetComponent<PrototypePlayerLife>();
            implantPickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();

            Assert.That(inventory, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(weaponLoadout, Is.Not.Null);
            Assert.That(playerLife, Is.Not.Null);
            Assert.That(implantPickup, Is.Not.Null);
            Assert.That(implantPickup.Definition.Equipment, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator CollectedImplantCanBeEquippedAndUnequipped()
        {
            WorldPickupDefinition item = implantPickup.Definition;
            Assert.That(implantPickup.TryCollect(inventory), Is.True);
            Assert.That(inventory.ContainsItem(item), Is.True);

            Assert.That(equipment.TryEquip(item), Is.True);
            Assert.That(inventory.ContainsItem(item), Is.False);
            Assert.That(
                equipment.GetEquippedItem(EquipmentSlot.Implant),
                Is.SameAs(item));
            Assert.That(
                playerLife.OutgoingDamageMultiplier,
                Is.EqualTo(1.15f).Within(0.001f));

            Assert.That(equipment.TryUnequip(EquipmentSlot.Implant), Is.True);
            Assert.That(inventory.ContainsItem(item), Is.True);
            Assert.That(playerLife.OutgoingDamageMultiplier,
                Is.EqualTo(1f).Within(0.001f));

            Assert.That(
                equipment.TryEquip(item, EquipmentSlot.ImplantSecondary),
                Is.True);
            Assert.That(
                equipment.GetEquippedItem(EquipmentSlot.ImplantSecondary),
                Is.SameAs(item));
            Assert.That(playerLife.OutgoingDamageMultiplier,
                Is.EqualTo(1.15f).Within(0.001f));
            Assert.That(
                equipment.TryUnequip(EquipmentSlot.ImplantSecondary),
                Is.True);
            Assert.That(inventory.ContainsItem(item), Is.True);

            Assert.That(
                equipment.TryEquip(item, EquipmentSlot.ImplantTertiary),
                Is.True);
            Assert.That(
                equipment.GetEquippedItem(EquipmentSlot.ImplantTertiary),
                Is.SameAs(item));
            Assert.That(
                equipment.TryUnequip(EquipmentSlot.ImplantTertiary),
                Is.True);
            yield break;
        }

        [UnityTest]
        public IEnumerator StartingWeaponsOccupyTheTwoVisibleHandSets()
        {
            WorldPickupDefinition primaryRight = equipment.GetEquippedItem(
                EquipmentSlot.PrimaryRightHand);
            WorldPickupDefinition primaryLeft = equipment.GetEquippedItem(
                EquipmentSlot.PrimaryLeftHand);
            WorldPickupDefinition secondaryRight = equipment.GetEquippedItem(
                EquipmentSlot.SecondaryRightHand);

            Assert.That(primaryRight, Is.Not.Null);
            Assert.That(primaryLeft, Is.SameAs(primaryRight));
            Assert.That(
                primaryRight.Equipment.HandEquipmentType,
                Is.EqualTo(HandEquipmentType.RangedWeapon));
            Assert.That(secondaryRight, Is.Not.Null);
            Assert.That(
                secondaryRight.Equipment.HandEquipmentType,
                Is.EqualTo(HandEquipmentType.MeleeWeapon));
            Assert.That(
                equipment.GetEquippedItem(EquipmentSlot.SecondaryLeftHand),
                Is.Null);
            Assert.That(weaponLoadout.HasTwoActiveRangedWeapons, Is.True);
            yield break;
        }

        [UnityTest]
        public IEnumerator UnequippedWeaponCanBePlacedInAnotherHandSet()
        {
            WorldPickupDefinition rifle = equipment.GetEquippedItem(
                EquipmentSlot.PrimaryLeftHand);

            Assert.That(
                equipment.TryUnequip(EquipmentSlot.PrimaryLeftHand),
                Is.True);
            Assert.That(inventory.ContainsItem(rifle), Is.True);
            Assert.That(weaponLoadout.ActiveLeftWeapon, Is.Null);

            Assert.That(
                equipment.TryEquip(rifle, EquipmentSlot.SecondaryLeftHand),
                Is.True);
            Assert.That(inventory.ContainsItem(rifle), Is.False);
            weaponLoadout.SwapHandSet();
            yield return null;

            Assert.That(weaponLoadout.ActiveLeftRangedWeapon, Is.Not.Null);
            Assert.That(
                equipment.GetEquippedItem(EquipmentSlot.SecondaryLeftHand),
                Is.SameAs(rifle));
        }
    }
}
