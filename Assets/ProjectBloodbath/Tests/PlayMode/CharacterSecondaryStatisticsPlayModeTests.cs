using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectBloodbath.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class CharacterSecondaryStatisticsPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private CharacterSecondaryStatistics statistics;
        private SecondaryStatDefinition outgoingDamage;

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
            statistics = player.GetComponent<CharacterSecondaryStatistics>();
            Assert.That(statistics, Is.Not.Null);

            foreach (SecondaryStatDefinition definition in
                statistics.Definitions)
            {
                if (definition.Identifier == "outgoing_damage_multiplier")
                {
                    outgoingDamage = definition;
                    break;
                }
            }

            Assert.That(outgoingDamage, Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator IndependentSourcesCombineReplaceAndExpireCleanly()
        {
            SecondaryStatModifier equipmentBonus = new(
                outgoingDamage,
                SecondaryStatModifierOperation.AdditivePercent,
                0.25f);
            SecondaryStatModifier temporaryBuff = new(
                outgoingDamage,
                SecondaryStatModifierOperation.MultiplicativePercent,
                0.2f);

            Assert.That(statistics.GetValue(outgoingDamage), Is.EqualTo(1f));
            Assert.That(statistics.SetModifiers(
                "equipment:test",
                new List<SecondaryStatModifier> { equipmentBonus }), Is.True);
            Assert.That(
                statistics.SetModifiers(
                    "buff:test",
                    new List<SecondaryStatModifier> { temporaryBuff }),
                Is.True);
            Assert.That(
                statistics.GetValue(outgoingDamage),
                Is.EqualTo(1.5f).Within(0.001f));

            Assert.That(statistics.RemoveModifiers("buff:test"), Is.True);
            Assert.That(
                statistics.GetValue(outgoingDamage),
                Is.EqualTo(1.25f).Within(0.001f));

            SecondaryStatModifier replacementBonus = new(
                outgoingDamage,
                SecondaryStatModifierOperation.AdditivePercent,
                0.1f);
            statistics.SetModifiers(
                "equipment:test",
                new List<SecondaryStatModifier> { replacementBonus });
            Assert.That(
                statistics.GetValue(outgoingDamage),
                Is.EqualTo(1.1f).Within(0.001f));

            Assert.That(
                statistics.RemoveModifiers("equipment:test"),
                Is.True);
            Assert.That(statistics.GetValue(outgoingDamage), Is.EqualTo(1f));
            yield break;
        }
    }
}
