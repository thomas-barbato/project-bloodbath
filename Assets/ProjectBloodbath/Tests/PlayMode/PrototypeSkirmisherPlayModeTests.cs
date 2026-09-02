using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Enemies;
using ProjectBloodbath.Player;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeSkirmisherPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PrototypeEnemyController skirmisher;
        private NavMeshAgent agent;
        private Transform playerTransform;
        private CharacterController playerController;
        private FpsPlayerController playerMovement;
        private Health playerHealth;

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

            GameObject enemyObject = GameObject.Find("PrototypeSkirmisher");
            GameObject playerObject = GameObject.Find("Player");
            Assert.That(enemyObject, Is.Not.Null);
            Assert.That(playerObject, Is.Not.Null);

            skirmisher =
                enemyObject.GetComponent<PrototypeEnemyController>();
            agent = enemyObject.GetComponent<NavMeshAgent>();
            playerTransform = playerObject.transform;
            playerController = playerObject.GetComponent<CharacterController>();
            playerMovement = playerObject.GetComponent<FpsPlayerController>();
            playerHealth = playerObject.GetComponent<Health>();

            Assert.That(skirmisher, Is.Not.Null);
            Assert.That(skirmisher.BehaviorProfile, Is.Not.Null);
            Assert.That(skirmisher.AttackProfile, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);
            Assert.That(playerHealth, Is.Not.Null);

            for (int frame = 0; frame < 10 && !agent.isOnNavMesh; frame++)
            {
                yield return null;
            }
            Assert.That(agent.isOnNavMesh, Is.True);
        }

        [UnityTest]
        public IEnumerator UsesDistanceKeepingAndProjectileProfiles()
        {
            Assert.That(
                skirmisher.BehaviorProfile.MovementStyle,
                Is.EqualTo(EnemyMovementStyle.MaintainDistance));
            Assert.That(
                skirmisher.AttackProfile.Delivery,
                Is.EqualTo(EnemyAttackDelivery.Projectile));
            Assert.That(skirmisher.AttackProfile.ProjectilePrefab, Is.Not.Null);
            yield break;
        }

        [UnityTest]
        public IEnumerator PlayerTooCloseMakesSkirmisherReposition()
        {
            PlacePlayer(
                skirmisher.transform.position +
                skirmisher.transform.forward * 2.5f);

            for (
                int frame = 0;
                frame < 10 && skirmisher.BehaviorState !=
                    EnemyBehaviorState.Repositioning;
                frame++)
            {
                yield return null;
            }

            Assert.That(skirmisher.IsAlerted, Is.True);
            Assert.That(
                skirmisher.BehaviorState,
                Is.EqualTo(EnemyBehaviorState.Repositioning));
            Assert.That(agent.isStopped, Is.False);
        }

        [UnityTest]
        public IEnumerator ProjectileAttackDamagesPlayer()
        {
            playerHealth.RestoreFull();
            PlacePlayer(
                skirmisher.transform.position +
                skirmisher.transform.forward * 6f);

            yield return WaitForProjectileLaunch();
            Assert.That(
                Object.FindFirstObjectByType<EnemyProjectile>(),
                Is.Not.Null);

            yield return new WaitForSeconds(0.8f);
            Assert.That(
                playerHealth.Current,
                Is.EqualTo(100f - skirmisher.AttackProfile.Damage));
        }

        [UnityTest]
        public IEnumerator SidestepAfterLaunchAvoidsProjectile()
        {
            playerHealth.RestoreFull();
            PlacePlayer(
                skirmisher.transform.position +
                skirmisher.transform.forward * 6f);

            yield return WaitForProjectileLaunch();
            PlacePlayer(playerTransform.position + Vector3.right * 3f);
            yield return new WaitForSeconds(0.8f);

            Assert.That(playerHealth.Current, Is.EqualTo(100f));
        }

        private IEnumerator WaitForProjectileLaunch()
        {
            float timeout = Time.time + 1.5f;
            while (!skirmisher.IsRecoveringAttack && Time.time < timeout)
            {
                yield return null;
            }

            Assert.That(skirmisher.IsRecoveringAttack, Is.True);
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

            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
}
