using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeEnemyAttackPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PrototypeEnemyController enemy;
        private Transform enemyTransform;
        private Transform playerTransform;
        private Health playerHealth;
        private CharacterController playerController;

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
            GameObject playerObject = GameObject.Find("Player");
            Assert.That(enemyObject, Is.Not.Null);
            Assert.That(playerObject, Is.Not.Null);

            enemy = enemyObject.GetComponent<PrototypeEnemyController>();
            enemyTransform = enemyObject.transform;
            playerTransform = playerObject.transform;
            playerHealth = playerObject.GetComponent<Health>();
            playerController = playerObject.GetComponent<CharacterController>();
            NavMeshAgent agent = enemyObject.GetComponent<NavMeshAgent>();

            Assert.That(enemy, Is.Not.Null);
            Assert.That(enemy.BehaviorProfile, Is.Not.Null);
            Assert.That(enemy.AttackProfile, Is.Not.Null);
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);

            for (int frame = 0; frame < 10 && !agent.isOnNavMesh; frame++)
            {
                yield return null;
            }
            Assert.That(agent.isOnNavMesh, Is.True);

            playerHealth.RestoreFull();
            PlacePlayer(enemyTransform.position + Vector3.back * 1.35f);
            enemyTransform.rotation =
                Quaternion.LookRotation(Vector3.back, Vector3.up);

            yield return null;
            Assert.That(enemy.IsPreparingAttack, Is.True);
        }

        [UnityTest]
        public IEnumerator AttackDealsDamageOnlyAfterWindup()
        {
            float startingHealth = playerHealth.Current;

            yield return new WaitForSeconds(
                enemy.AttackProfile.WindupDuration * 0.5f);
            Assert.That(playerHealth.Current, Is.EqualTo(startingHealth));

            float timeout = Time.time +
                enemy.AttackProfile.WindupDuration + 0.5f;
            while (!enemy.IsRecoveringAttack && Time.time < timeout)
            {
                yield return null;
            }
            Assert.That(enemy.IsRecoveringAttack, Is.True);
            Assert.That(
                playerHealth.Current,
                Is.EqualTo(startingHealth - enemy.AttackProfile.Damage));
        }

        [UnityTest]
        public IEnumerator LeavingRangeDuringWindupAvoidsDamage()
        {
            float startingHealth = playerHealth.Current;
            PlacePlayer(enemyTransform.position + Vector3.right * 4f);

            yield return new WaitForSeconds(
                enemy.AttackProfile.WindupDuration + 0.1f);

            Assert.That(playerHealth.Current, Is.EqualTo(startingHealth));
        }

        private void PlacePlayer(Vector3 position)
        {
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            playerTransform.position = position;

            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
}
