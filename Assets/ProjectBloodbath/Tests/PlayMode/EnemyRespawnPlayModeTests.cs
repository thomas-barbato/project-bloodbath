using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Enemies;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class EnemyRespawnPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PrototypeEnemyController enemy;
        private Health enemyHealth;
        private EnemyRespawnProfile temporaryProfile;

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

            GameObject enemyObject = GameObject.Find("PrototypeEnemy");
            Assert.That(enemyObject, Is.Not.Null);

            enemy = enemyObject.GetComponent<PrototypeEnemyController>();
            enemyHealth = enemyObject.GetComponent<Health>();
            Assert.That(enemy, Is.Not.Null);
            Assert.That(enemyHealth, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (temporaryProfile != null)
            {
                Object.Destroy(temporaryProfile);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator MovementLabEnemiesUseTestOnlyTimedRespawn()
        {
            GameObject skirmisherObject = GameObject.Find(
                "PrototypeSkirmisher");
            Assert.That(skirmisherObject, Is.Not.Null);
            PrototypeEnemyController skirmisher =
                skirmisherObject.GetComponent<PrototypeEnemyController>();

            Assert.That(enemy.RespawnProfile, Is.Not.Null);
            Assert.That(enemy.RespawnProfile.RespawnsDuringSession, Is.True);
            Assert.That(enemy.RespawnProfile.Delay, Is.EqualTo(2.5f));
            Assert.That(skirmisher, Is.Not.Null);
            Assert.That(skirmisher.RespawnProfile, Is.SameAs(enemy.RespawnProfile));
            yield break;
        }

        [UnityTest]
        public IEnumerator DefaultEnemyStaysDeadDuringSession()
        {
            enemy.SetRespawnProfile(null);
            KillEnemy();

            yield return new WaitForSeconds(0.45f);

            Assert.That(enemyHealth.IsAlive, Is.False);
        }

        [UnityTest]
        public IEnumerator TimedProfileAllowsExplicitRespawn()
        {
            temporaryProfile = ScriptableObject.CreateInstance<
                EnemyRespawnProfile>();
            temporaryProfile.Configure(EnemyRespawnMode.Timed, 0.1f);
            enemy.SetRespawnProfile(temporaryProfile);
            KillEnemy();

            yield return new WaitForSeconds(0.45f);

            Assert.That(enemyHealth.IsAlive, Is.True);
        }

        private void KillEnemy()
        {
            DamageInfo lethalDamage = new(
                enemyHealth.Maximum,
                DamageType.Ballistic,
                enemy.transform.position,
                Vector3.up,
                Vector3.zero,
                0f,
                null);
            enemyHealth.ApplyDamage(lethalDamage);
            Assert.That(enemyHealth.IsAlive, Is.False);
        }
    }
}
