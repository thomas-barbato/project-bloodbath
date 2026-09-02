using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class LootPickupPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private CharacterInventory inventory;
        private PrototypeLootInteraction interaction;
        private GameObject temporaryBlocker;

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

            GameObject player = GameObject.Find("Player");
            Assert.That(player, Is.Not.Null);
            inventory = player.GetComponent<CharacterInventory>();
            interaction = player.GetComponent<PrototypeLootInteraction>();
            Assert.That(inventory, Is.Not.Null);
            Assert.That(interaction, Is.Not.Null);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (temporaryBlocker != null)
            {
                Object.Destroy(temporaryBlocker);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator FullAmmoLeavesPickupOnGroundAndPartialSpaceLeavesRemainder()
        {
            WorldPickup pickup = GameObject.Find("AmmoPickup_Test")
                .GetComponent<WorldPickup>();
            InventoryResourceDefinition ammunition = pickup.Definition.Resource;
            Assert.That(pickup.PickupMode, Is.EqualTo(WorldPickupMode.Automatic));

            inventory.SetResourceQuantity(
                ammunition,
                ammunition.MaximumCarried - 5);
            Assert.That(pickup.TryCollect(inventory), Is.True);
            Assert.That(
                inventory.GetResourceQuantity(ammunition),
                Is.EqualTo(ammunition.MaximumCarried));
            Assert.That(pickup.RemainingQuantity, Is.EqualTo(7));
            Assert.That(pickup.gameObject.activeSelf, Is.True);
            Assert.That(interaction.NotificationVisible, Is.True);
            Assert.That(
                interaction.LastNotificationText,
                Does.Contain(pickup.DisplayName));

            Assert.That(pickup.TryCollect(inventory), Is.False);
            Assert.That(pickup.RemainingQuantity, Is.EqualTo(7));
            yield break;
        }

        [UnityTest]
        public IEnumerator AimedManualItemShowsItsNameThenEntersInventory()
        {
            WorldPickup pickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();
            Camera cameraComponent = Camera.main;
            pickup.transform.position =
                cameraComponent.transform.position +
                cameraComponent.transform.forward * 2f;
            Physics.SyncTransforms();

            interaction.RefreshHoveredPickup();

            Assert.That(interaction.HoveredPickup, Is.SameAs(pickup));
            Assert.That(interaction.HoverLabel, Does.Contain(pickup.DisplayName));
            Assert.That(pickup.TryCollect(inventory), Is.True);
            Assert.That(inventory.Items, Does.Contain(pickup.Definition));
            Assert.That(interaction.NotificationVisible, Is.True);
            Assert.That(
                interaction.LastNotificationText,
                Does.Contain(pickup.DisplayName));
            yield break;
        }

        [UnityTest]
        public IEnumerator NearbyOffCenterItemStillReceivesAimAssistance()
        {
            WorldPickup pickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();
            Camera cameraComponent = Camera.main;
            Vector3 assistedDirection = Quaternion.AngleAxis(
                8f,
                cameraComponent.transform.up) *
                cameraComponent.transform.forward;
            pickup.transform.position =
                cameraComponent.transform.position + assistedDirection * 3f;
            Physics.SyncTransforms();

            interaction.RefreshHoveredPickup();

            Assert.That(
                interaction.HoveredPickup,
                Is.SameAs(pickup),
                "Un petit objet proche de la visée ne doit pas exiger un ciblage au pixel près.");
            Assert.That(interaction.CanCollectHoveredPickup, Is.True);
            Assert.That(interaction.HoverLabel, Does.Contain(pickup.DisplayName));
            yield break;
        }

        [UnityTest]
        public IEnumerator LocalPlayerBodyCannotHideAVisiblePickup()
        {
            WorldPickup pickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();
            Camera cameraComponent = Camera.main;
            pickup.transform.position =
                cameraComponent.transform.position +
                cameraComponent.transform.forward * 2f;

            temporaryBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            temporaryBlocker.name = "LocalBodyInteractionBlocker";
            temporaryBlocker.transform.SetParent(inventory.transform);
            temporaryBlocker.transform.position =
                cameraComponent.transform.position +
                cameraComponent.transform.forward * 1f;
            temporaryBlocker.transform.localScale = Vector3.one * 0.5f;
            Physics.SyncTransforms();

            interaction.RefreshHoveredPickup();

            Assert.That(
                interaction.HoveredPickup,
                Is.SameAs(pickup),
                "Le corps physique du joueur local ne doit jamais masquer son propre loot.");
            yield break;
        }
    }
}
