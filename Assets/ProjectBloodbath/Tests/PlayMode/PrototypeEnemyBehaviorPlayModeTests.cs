using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Enemies;
using ProjectBloodbath.Player;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeEnemyBehaviorPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PrototypeEnemyController enemy;
        private NavMeshAgent agent;
        private Transform playerTransform;
        private CharacterController playerController;
        private FpsPlayerController playerMovement;
        private Vector3 spawnPosition;

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
            agent = enemyObject.GetComponent<NavMeshAgent>();
            playerTransform = playerObject.transform;
            playerController = playerObject.GetComponent<CharacterController>();
            playerMovement = playerObject.GetComponent<FpsPlayerController>();
            spawnPosition = enemyObject.transform.position;

            Assert.That(enemy, Is.Not.Null);
            Assert.That(enemy.BehaviorProfile, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);

            for (int frame = 0; frame < 10 && !agent.isOnNavMesh; frame++)
            {
                yield return null;
            }
            Assert.That(agent.isOnNavMesh, Is.True);
        }

        [UnityTest]
        public IEnumerator PlayerOutsideDetectionRangeKeepsEnemyIdle()
        {
            float distance = Vector3.Distance(
                playerTransform.position,
                spawnPosition);
            Assert.That(
                distance,
                Is.GreaterThan(enemy.BehaviorProfile.DetectionRange));

            yield return null;
            yield return null;

            Assert.That(enemy.IsAlerted, Is.False);
            Assert.That(enemy.BehaviorState, Is.EqualTo(EnemyBehaviorState.Idle));
            Assert.That(agent.isStopped, Is.True);
            Assert.That(
                agent.speed,
                Is.EqualTo(enemy.BehaviorProfile.MovementSpeed));
            Assert.That(
                agent.stoppingDistance,
                Is.EqualTo(enemy.BehaviorProfile.StoppingDistance));
        }

        [UnityTest]
        public IEnumerator EnteringDetectionRangeStartsPursuit()
        {
            PlacePlayer(spawnPosition + Vector3.back * 5f);

            for (
                int frame = 0;
                frame < 10 && enemy.BehaviorState !=
                    EnemyBehaviorState.Pursuing;
                frame++)
            {
                yield return null;
            }

            Assert.That(enemy.IsAlerted, Is.True);
            Assert.That(
                enemy.BehaviorState,
                Is.EqualTo(EnemyBehaviorState.Pursuing));
            Assert.That(agent.isStopped, Is.False);
        }

        [UnityTest]
        public IEnumerator ObstacleBlocksInitialDetection()
        {
            PlacePlayer(spawnPosition + Vector3.back * 5f);
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "PerceptionTestBlocker";
            blocker.transform.position =
                spawnPosition + Vector3.back * 2.5f + Vector3.up * 1.25f;
            blocker.transform.localScale = new Vector3(2f, 2.5f, 0.4f);
            Physics.SyncTransforms();

            yield return null;
            yield return null;

            Assert.That(enemy.IsAlerted, Is.False);
            Assert.That(
                enemy.BehaviorState,
                Is.EqualTo(EnemyBehaviorState.Idle));

            Object.Destroy(blocker);
            for (
                int frame = 0;
                frame < 10 && !enemy.IsAlerted;
                frame++)
            {
                yield return null;
            }

            Assert.That(enemy.IsAlerted, Is.True);
            Assert.That(
                enemy.BehaviorState,
                Is.EqualTo(EnemyBehaviorState.Pursuing));
        }

        [UnityTest]
        public IEnumerator LeavingLeashMakesEnemyReturnToSpawn()
        {
            PlacePlayer(spawnPosition + Vector3.back * 5f);

            float movementTimeout = Time.time + 1f;
            while (
                Vector3.Distance(enemy.transform.position, spawnPosition) < 0.25f &&
                Time.time < movementTimeout)
            {
                yield return null;
            }
            Assert.That(
                Vector3.Distance(enemy.transform.position, spawnPosition),
                Is.GreaterThanOrEqualTo(0.25f));

            PlacePlayer(
                spawnPosition +
                Vector3.right * (enemy.BehaviorProfile.LeashRange + 2f));
            yield return null;

            Assert.That(enemy.IsAlerted, Is.False);
            Assert.That(
                enemy.BehaviorState,
                Is.EqualTo(EnemyBehaviorState.Returning));
            Assert.That(
                Vector3.Distance(agent.destination, spawnPosition),
                Is.LessThan(0.1f));
        }

        private void PlacePlayer(Vector3 position)
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            if (playerController != null)
            {
                playerController.enabled = false;
            }

            playerTransform.position = position;
        }
    }
}
