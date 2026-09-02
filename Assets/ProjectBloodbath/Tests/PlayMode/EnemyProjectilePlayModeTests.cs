using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class EnemyProjectilePlayModeTests
    {
        private static readonly Vector3 PathOrigin = Vector3.up * 100f;

        private Scene testScene;
        private GameObject source;
        private GameObject target;
        private Health targetHealth;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            testScene = SceneManager.CreateScene("EnemyProjectileTestScene");
            SceneManager.SetActiveScene(testScene);

            source = new GameObject("ProjectileSource");
            source.transform.position = PathOrigin;
            target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "ProjectileTarget";
            target.transform.position = PathOrigin + Vector3.forward * 3f;
            targetHealth = target.AddComponent<Health>();
            targetHealth.Configure(100f);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (testScene.IsValid() && testScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(testScene);
            }
        }

        [UnityTest]
        public IEnumerator ProjectileDamagesTargetOnImpact()
        {
            SpawnProjectile();

            yield return new WaitForSeconds(0.5f);

            Assert.That(targetHealth.Current, Is.EqualTo(93f));
        }

        [UnityTest]
        public IEnumerator MovingAsideAvoidsProjectile()
        {
            SpawnProjectile();

            yield return new WaitForSeconds(0.1f);
            target.transform.position += Vector3.right * 3f;
            yield return new WaitForSeconds(0.4f);

            Assert.That(targetHealth.Current, Is.EqualTo(100f));
        }

        private void SpawnProjectile()
        {
            GameObject projectileObject = new("TestEnemyProjectile");
            projectileObject.transform.position = PathOrigin;
            EnemyProjectile projectile =
                projectileObject.AddComponent<EnemyProjectile>();
            projectile.Initialize(
                source,
                Vector3.forward,
                10f,
                0.1f,
                1f,
                7f,
                DamageType.Ballistic,
                1f);
        }
    }
}
