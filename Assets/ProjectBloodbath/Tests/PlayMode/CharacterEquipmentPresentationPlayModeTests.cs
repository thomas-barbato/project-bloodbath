using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Player;
using ProjectBloodbath.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class CharacterEquipmentPresentationPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private CharacterInventory inventory;
        private CharacterEquipment equipment;
        private CharacterEquipmentPresentation presentation;
        private EquipmentVisualBinding implantBinding;
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
            Assert.That(player, Is.Not.Null);
            inventory = player.GetComponent<CharacterInventory>();
            equipment = player.GetComponent<CharacterEquipment>();
            presentation = player.GetComponent<CharacterEquipmentPresentation>();
            implantBinding = player.GetComponentInChildren<
                EquipmentVisualBinding>(true);
            implantPickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();

            Assert.That(inventory, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(presentation, Is.Not.Null);
            Assert.That(implantBinding, Is.Not.Null);
            Assert.That(implantPickup, Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EquippingAndRemovingImplantUpdatesBodyVisual()
        {
            Assert.That(implantBinding.IsVisible, Is.False);
            Assert.That(presentation.VisibleBindingCount, Is.Zero);
            Assert.That(implantPickup.TryCollect(inventory), Is.True);

            WorldPickupDefinition implant = implantPickup.Definition;
            Assert.That(equipment.TryEquip(implant), Is.True);
            Assert.That(implantBinding.IsVisible, Is.True);
            Assert.That(presentation.VisibleBindingCount, Is.EqualTo(1));

            Assert.That(equipment.TryUnequip(EquipmentSlot.Implant), Is.True);
            Assert.That(implantBinding.IsVisible, Is.False);
            Assert.That(presentation.VisibleBindingCount, Is.Zero);
            yield break;
        }
    }
}
