using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class LootPickupPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private CharacterInventory inventory;
        private PlayerInputReader inputReader;
        private PrototypeLootInteraction interaction;
        private PrototypeCharacterPanel characterPanel;
        private GameObject temporaryBlocker;
        private Keyboard keyboard;
        private bool ownsKeyboard;

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
            inputReader = player.GetComponent<PlayerInputReader>();
            interaction = player.GetComponent<PrototypeLootInteraction>();
            characterPanel = player.GetComponent<PrototypeCharacterPanel>();
            Assert.That(inventory, Is.Not.Null);
            Assert.That(inputReader, Is.Not.Null);
            Assert.That(interaction, Is.Not.Null);
            Assert.That(characterPanel, Is.Not.Null);

            keyboard = InputSystem.AddDevice<Keyboard>();
            ownsKeyboard = true;

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            characterPanel?.SetOpen(false);
            if (temporaryBlocker != null)
            {
                Object.Destroy(temporaryBlocker);
            }

            if (keyboard != null && keyboard.added)
            {
                SetKeys();
            }

            if (ownsKeyboard && keyboard != null && keyboard.added)
            {
                InputSystem.RemoveDevice(keyboard);
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
        public IEnumerator DistantManualItemShowsOnlyItsName()
        {
            WorldPickup pickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();
            GameObject narrativeTerminal =
                GameObject.Find("QuarantineLoreTerminal");
            Assert.That(narrativeTerminal, Is.Not.Null);
            narrativeTerminal.SetActive(false);
            Camera cameraComponent = Camera.main;
            pickup.transform.position =
                cameraComponent.transform.position +
                cameraComponent.transform.forward * 5f;
            Physics.SyncTransforms();

            interaction.RefreshHoveredPickup();

            Assert.That(interaction.HoveredPickup, Is.SameAs(pickup));
            Assert.That(interaction.CanCollectHoveredPickup, Is.False);
            Assert.That(interaction.HoverLabel, Is.EqualTo(pickup.DisplayName));
            Assert.That(interaction.HoverLabel, Does.Not.Contain("APPROCHEZ"));
            Assert.That(interaction.HoverLabel, Does.Not.Contain("INTERAGIR"));
            yield break;
        }

        [UnityTest]
        public IEnumerator BriefInteractPressCollectsAimedManualItem()
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

            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(
                inventory.Items,
                Does.Contain(pickup.Definition),
                "Une simple pression sur E doit ramasser l'objet sans maintien.");
        }

        [UnityTest]
        public IEnumerator OpenMenuHidesTargetedLootWithoutPausingWorld()
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
            Assert.That(interaction.HoverPromptVisible, Is.True);
            float timeScaleBeforeOpening = Time.timeScale;

            characterPanel.SetOpen(true);
            interaction.RefreshHoveredPickup();

            Assert.That(inputReader.GameplaySuppressed, Is.True);
            Assert.That(interaction.HoveredPickup, Is.Null);
            Assert.That(interaction.HoverPromptVisible, Is.False);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(timeScaleBeforeOpening),
                "Un menu local ne doit jamais mettre le monde en pause.");
            yield return null;
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

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
