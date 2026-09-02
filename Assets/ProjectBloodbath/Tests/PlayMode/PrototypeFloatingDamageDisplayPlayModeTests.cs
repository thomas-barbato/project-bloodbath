using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeFloatingDamageDisplayPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private GameObject player;
        private GameObject playerWeaponSource;
        private GameObject enemy;
        private Health enemyHealth;
        private PrototypeFloatingDamageDisplay display;

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

            player = GameObject.Find("Player");
            enemy = GameObject.Find("PrototypeEnemy");
            Assert.That(player, Is.Not.Null);
            Assert.That(enemy, Is.Not.Null);

            GameObject.Find("PrototypeSkirmisher")?.SetActive(false);
            enemy.GetComponent<PrototypeEnemyController>().enabled = false;
            display = player.GetComponent<PrototypeFloatingDamageDisplay>();
            enemyHealth = enemy.GetComponent<Health>();
            playerWeaponSource = new GameObject("FloatingDamagePlayerWeapon");
            playerWeaponSource.transform.SetParent(player.transform);

            Assert.That(display, Is.Not.Null);
            Assert.That(display.Settings, Is.Not.Null);
            Assert.That(enemyHealth, Is.Not.Null);
            display.SetDamageNumbersVisible(true, false);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(playerWeaponSource);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerDamageAppearsAboveHeadAndCanBeHidden()
        {
            Renderer headRenderer = enemy.transform.Find("Head")
                .GetComponent<Renderer>();
            int initialSpawnCount = display.SpawnCount;

            ApplyDamage(25f);

            Assert.That(display.ActiveNumberCount, Is.EqualTo(1));
            Assert.That(display.SpawnCount, Is.EqualTo(initialSpawnCount + 1));
            Assert.That(
                display.LastSpawnWorldPosition.y,
                Is.GreaterThan(headRenderer.bounds.max.y));

            display.SetDamageNumbersVisible(false, false);
            enemyHealth.RestoreFull();
            ApplyDamage(10f);

            Assert.That(display.ActiveNumberCount, Is.Zero);
            Assert.That(display.SpawnCount, Is.EqualTo(initialSpawnCount + 1));
            yield break;
        }

        private void ApplyDamage(float amount)
        {
            enemyHealth.ApplyDamage(new DamageInfo(
                amount,
                DamageType.Ballistic,
                enemy.transform.position,
                Vector3.up,
                Vector3.forward,
                0f,
                playerWeaponSource));
        }
    }
}
