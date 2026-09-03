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
    public sealed class PrototypeCharacterPanelPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Keyboard keyboard;
        private bool ownsKeyboard;
        private PlayerInputReader inputReader;
        private PrototypeCharacterPanel panel;
        private CharacterInventory inventory;
        private CharacterEquipment equipment;
        private CharacterProgression progression;
        private CharacterStatistics statistics;
        private CharacterSecondaryStatistics secondaryStatistics;
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
            inputReader = player.GetComponent<PlayerInputReader>();
            panel = player.GetComponent<PrototypeCharacterPanel>();
            inventory = player.GetComponent<CharacterInventory>();
            equipment = player.GetComponent<CharacterEquipment>();
            progression = player.GetComponent<CharacterProgression>();
            statistics = player.GetComponent<CharacterStatistics>();
            secondaryStatistics = player.GetComponent<
                CharacterSecondaryStatistics>();
            implantPickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();

            Assert.That(inputReader, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            Assert.That(inventory, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(progression, Is.Not.Null);
            Assert.That(statistics, Is.Not.Null);
            Assert.That(secondaryStatistics, Is.Not.Null);
            Assert.That(implantPickup, Is.Not.Null);

            keyboard = InputSystem.AddDevice<Keyboard>();
            ownsKeyboard = true;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            panel?.SetOpen(false);
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
        public IEnumerator TabTogglesPanelAndSuppressesGameplay()
        {
            Assert.That(inputReader.enabled, Is.True);
            Assert.That(panel.enabled, Is.True);

            SetKeys(Key.Tab);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(panel.IsOpen, Is.True);
            Assert.That(inputReader.GameplaySuppressed, Is.True);
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));

            SetKeys(Key.Tab);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(panel.IsOpen, Is.False);
            Assert.That(inputReader.GameplaySuppressed, Is.False);
        }

        [UnityTest]
        public IEnumerator PanelSpendsPointsAndManagesImplant()
        {
            CharacterStatDefinition strength = FindStat("strength");
            Assert.That(strength, Is.Not.Null);
            Assert.That(implantPickup.TryCollect(inventory), Is.True);

            progression.AddExperience(
                progression.ExperienceRequiredForNextLevel);
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(5));

            panel.SetOpen(true);
            Assert.That(panel.TryIncreaseStat(strength), Is.True);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(11));
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(4));

            WorldPickupDefinition implant = implantPickup.Definition;
            Assert.That(panel.TryEquip(implant), Is.True);
            Assert.That(
                equipment.GetEquippedItem(EquipmentSlot.Implant),
                Is.SameAs(implant));
            Assert.That(
                secondaryStatistics.GetValue(
                    "outgoing_damage_multiplier",
                    1f),
                Is.EqualTo(1.15f).Within(0.001f));

            Assert.That(panel.TryUnequip(EquipmentSlot.Implant), Is.True);
            Assert.That(inventory.ContainsItem(implant), Is.True);
            Assert.That(
                secondaryStatistics.GetValue(
                    "outgoing_damage_multiplier",
                    1f),
                Is.EqualTo(1f).Within(0.001f));
            yield break;
        }

        private CharacterStatDefinition FindStat(string identifier)
        {
            foreach (CharacterStatValue value in statistics.Statistics)
            {
                if (value.Definition.Identifier == identifier)
                {
                    return value.Definition;
                }
            }

            return null;
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
