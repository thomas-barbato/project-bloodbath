using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectBloodbath.Combat;
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
        private CharacterIdentity identity;
        private CharacterStatistics statistics;
        private CharacterSecondaryStatistics secondaryStatistics;
        private PrototypeWeaponLoadout weaponLoadout;
        private WorldPickup implantPickup;
        private readonly List<WorldPickupDefinition> temporaryItems = new();

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
            identity = player.GetComponent<CharacterIdentity>();
            statistics = player.GetComponent<CharacterStatistics>();
            secondaryStatistics = player.GetComponent<
                CharacterSecondaryStatistics>();
            weaponLoadout = player.GetComponent<PrototypeWeaponLoadout>();
            implantPickup = GameObject.Find("ManualItemPickup_Test")
                .GetComponent<WorldPickup>();

            Assert.That(inputReader, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            Assert.That(inventory, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(progression, Is.Not.Null);
            Assert.That(identity, Is.Not.Null);
            Assert.That(statistics, Is.Not.Null);
            Assert.That(secondaryStatistics, Is.Not.Null);
            Assert.That(weaponLoadout, Is.Not.Null);
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

            foreach (WorldPickupDefinition item in temporaryItems)
            {
                Object.Destroy(item);
            }
            temporaryItems.Clear();

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
            Assert.That(statistics.HasPendingAttributeChanges, Is.True);
            Assert.That(panel.AttributeActionsVisible, Is.True);

            Assert.That(panel.CancelAttributeDistribution(), Is.True);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(10));
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(5));
            Assert.That(statistics.HasPendingAttributeChanges, Is.False);
            Assert.That(panel.AttributeActionsVisible, Is.False);

            Assert.That(panel.TryIncreaseStat(strength), Is.True);
            Assert.That(panel.CommitAttributeDistribution(), Is.True);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(11));
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(4));
            Assert.That(statistics.HasPendingAttributeChanges, Is.False);
            Assert.That(panel.AttributeActionsVisible, Is.False);

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

        [UnityTest]
        public IEnumerator PanelMovesAWeaponIntoTheChosenLoadoutHand()
        {
            WorldPickupDefinition rifle = equipment.GetEquippedItem(
                EquipmentSlot.PrimaryLeftHand);
            Assert.That(rifle, Is.Not.Null);

            Assert.That(
                panel.TryUnequip(EquipmentSlot.PrimaryLeftHand),
                Is.True);
            Assert.That(inventory.ContainsItem(rifle), Is.True);

            Assert.That(
                panel.TryEquipToHand(
                    rifle,
                    EquipmentSlot.SecondaryLeftHand),
                Is.True);
            Assert.That(inventory.ContainsItem(rifle), Is.False);
            Assert.That(
                equipment.GetEquippedItem(EquipmentSlot.SecondaryLeftHand),
                Is.SameAs(rifle));
            yield break;
        }

        [UnityTest]
        public IEnumerator IdentityAndButtonControlTheLiveLoadout()
        {
            Assert.That(panel.DisplayedCharacterName, Is.EqualTo("Mara Voss"));
            Assert.That(
                panel.DisplayedClassName,
                Is.EqualTo("Classe prototype"));
            Assert.That(
                weaponLoadout.ActiveHandSet,
                Is.EqualTo(PrototypeHandSetSlot.Primary));

            panel.SwapActiveHandSet();
            yield return null;

            Assert.That(
                weaponLoadout.ActiveHandSet,
                Is.EqualTo(PrototypeHandSetSlot.Secondary));
            Assert.That(weaponLoadout.ActiveRightMeleeWeapon, Is.Not.Null);
            Assert.That(weaponLoadout.ActiveLeftWeapon, Is.Null);
        }

        [UnityTest]
        public IEnumerator InventoryFiltersSearchSortAndFourPages()
        {
            WorldPickupDefinition questItem = CreateItem(
                "quest_sample",
                "Échantillon oublié",
                InventoryItemCategory.QuestItem);
            WorldPickupDefinition alphaItem = CreateItem(
                "alpha_item",
                "Alpha",
                InventoryItemCategory.Miscellaneous);
            WorldPickupDefinition zuluItem = CreateItem(
                "zulu_item",
                "Zulu",
                InventoryItemCategory.Miscellaneous);
            Assert.That(inventory.AddItem(questItem), Is.True);
            Assert.That(inventory.AddItem(zuluItem), Is.True);
            Assert.That(inventory.AddItem(alphaItem), Is.True);

            WorldPickupDefinition rifle = equipment.GetEquippedItem(
                EquipmentSlot.PrimaryLeftHand);
            Assert.That(panel.TryUnequip(EquipmentSlot.PrimaryLeftHand), Is.True);

            panel.SetInventoryFilter(CharacterInventoryFilter.QuestItems);
            Assert.That(panel.FilteredInventoryCount, Is.EqualTo(1));
            panel.SetInventorySearch("chant");
            Assert.That(panel.FilteredInventoryCount, Is.EqualTo(1));
            panel.SetInventorySearch("ch");
            Assert.That(
                panel.FilteredInventoryCount,
                Is.EqualTo(1),
                "Moins de trois caractères ne doivent pas filtrer.");

            panel.SetInventoryFilter(CharacterInventoryFilter.Weapons);
            panel.SetInventorySearch("fus");
            Assert.That(panel.FilteredInventoryCount, Is.EqualTo(1));
            Assert.That(inventory.ContainsItem(rifle), Is.True);

            panel.SetInventoryFilter(CharacterInventoryFilter.All);
            panel.SetInventorySearch(string.Empty);
            panel.SortInventory(InventorySortMode.Name);
            for (int index = 1; index < inventory.Items.Count; index++)
            {
                Assert.That(string.Compare(
                    inventory.Items[index - 1].DisplayName,
                    inventory.Items[index].DisplayName,
                    System.StringComparison.CurrentCultureIgnoreCase),
                    Is.LessThanOrEqualTo(0));
            }

            panel.SetInventoryPage(3);
            Assert.That(panel.InventoryPage, Is.EqualTo(3));
            yield break;
        }

        private WorldPickupDefinition CreateItem(
            string identifier,
            string displayName,
            InventoryItemCategory category)
        {
            WorldPickupDefinition item =
                ScriptableObject.CreateInstance<WorldPickupDefinition>();
            item.Configure(
                identifier,
                displayName,
                WorldPickupKind.Item,
                WorldPickupMode.Manual,
                null,
                Color.white,
                null,
                category);
            temporaryItems.Add(item);
            return item;
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
