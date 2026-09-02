using System.Collections;
using NUnit.Framework;
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
            playerLife = player.GetComponent<PrototypePlayerLife>();
            implantPickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();

            Assert.That(inventory, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
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
            yield break;
        }
    }
}
