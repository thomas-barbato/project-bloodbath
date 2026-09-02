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
    public sealed class EnemyLootDropPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PrototypeEnemyLootDropper dropper;
        private Health enemyHealth;

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

            GameObject enemy = GameObject.Find("PrototypeEnemy");
            Assert.That(enemy, Is.Not.Null);
            dropper = enemy.GetComponent<PrototypeEnemyLootDropper>();
            enemyHealth = enemy.GetComponent<Health>();
            Assert.That(dropper, Is.Not.Null);
            Assert.That(enemyHealth, Is.Not.Null);
            Assert.That(dropper.LootProfile, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator EnemyDeathSpawnsConfiguredWorldPickup()
        {
            DamageInfo lethalDamage = new(
                enemyHealth.Maximum,
                DamageType.Ballistic,
                enemyHealth.transform.position,
                Vector3.up,
                Vector3.forward,
                0f,
                GameObject.Find("Player"));

            enemyHealth.ApplyDamage(lethalDamage);
            yield return null;

            Assert.That(dropper.LastDropCount, Is.GreaterThan(0));
            Assert.That(dropper.LastSpawnedPickup, Is.Not.Null);
            Assert.That(dropper.LastSpawnedPickup.Definition, Is.Not.Null);
            Assert.That(dropper.LastSpawnedPickup.RemainingQuantity, Is.GreaterThan(0));
            Assert.That(dropper.LastSpawnedPickup.gameObject.activeSelf, Is.True);
        }
    }
}
