using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectBloodbath.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class CharacterStatisticsPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private CharacterProgression progression;
        private CharacterStatistics statistics;
        private CharacterInventory inventory;
        private CharacterEquipment equipment;
        private EquipmentDefinition temporaryEquipment;
        private WorldPickupDefinition temporaryItem;

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

            progression = player.GetComponent<CharacterProgression>();
            statistics = player.GetComponent<CharacterStatistics>();
            inventory = player.GetComponent<CharacterInventory>();
            equipment = player.GetComponent<CharacterEquipment>();
            Assert.That(progression, Is.Not.Null);
            Assert.That(statistics, Is.Not.Null);
            Assert.That(inventory, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(temporaryItem);
            Object.Destroy(temporaryEquipment);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FiveBaseStatsReceivePointsAndGateEquipment()
        {
            Assert.That(statistics.Statistics.Count, Is.EqualTo(5));
            CharacterStatDefinition strength = FindStat("strength");
            Assert.That(strength, Is.Not.Null);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(10));

            EquipmentStatRequirement requirement =
                new(strength, 12);
            temporaryEquipment = ScriptableObject.CreateInstance<
                EquipmentDefinition>();
            temporaryEquipment.Configure(
                "test_strength_helmet",
                "Casque de test",
                EquipmentSlot.Head,
                0f,
                new List<EquipmentStatRequirement> { requirement });

            temporaryItem = ScriptableObject.CreateInstance<
                WorldPickupDefinition>();
            temporaryItem.Configure(
                "test_strength_helmet_pickup",
                "Casque de test",
                WorldPickupKind.Item,
                WorldPickupMode.Manual,
                null,
                Color.gray,
                temporaryEquipment);
            Assert.That(inventory.AddItem(temporaryItem), Is.True);

            Assert.That(equipment.TryEquip(temporaryItem), Is.False);
            Assert.That(equipment.LastFailedRequirement, Is.SameAs(requirement));
            Assert.That(inventory.ContainsItem(temporaryItem), Is.True);

            progression.AddExperience(
                progression.ExperienceRequiredForNextLevel);
            Assert.That(
                statistics.UnspentAttributePoints,
                Is.EqualTo(progression.Settings.AttributePointsPerLevel));
            Assert.That(
                statistics.TrySpendAttributePoints(strength, 2),
                Is.True);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(12));
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(3));

            Assert.That(equipment.TryEquip(temporaryItem), Is.True);
            Assert.That(inventory.ContainsItem(temporaryItem), Is.False);
            yield break;
        }

        [UnityTest]
        public IEnumerator PendingAttributePointsCanBeCancelledOrCommitted()
        {
            CharacterStatDefinition strength = FindStat("strength");
            Assert.That(strength, Is.Not.Null);
            progression.AddExperience(
                progression.ExperienceRequiredForNextLevel);

            Assert.That(
                statistics.TrySpendAttributePoints(strength, 2),
                Is.True);
            Assert.That(statistics.GetPendingIncrease(strength), Is.EqualTo(2));
            Assert.That(statistics.PendingAttributePointCount, Is.EqualTo(2));
            Assert.That(statistics.HasPendingAttributeChanges, Is.True);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(12));
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(3));

            Assert.That(statistics.CancelPendingAttributePoints(), Is.True);
            Assert.That(statistics.GetPendingIncrease(strength), Is.Zero);
            Assert.That(statistics.HasPendingAttributeChanges, Is.False);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(10));
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(5));

            Assert.That(
                statistics.TrySpendAttributePoints(strength, 2),
                Is.True);
            Assert.That(statistics.CommitPendingAttributePoints(), Is.True);
            Assert.That(statistics.GetPendingIncrease(strength), Is.Zero);
            Assert.That(statistics.HasPendingAttributeChanges, Is.False);
            Assert.That(statistics.GetValue(strength), Is.EqualTo(12));
            Assert.That(statistics.UnspentAttributePoints, Is.EqualTo(3));
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
    }
}
