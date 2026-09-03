using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class HitscanWeaponAmmunitionPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private HitscanWeapon weapon;
        private GameObject selfBlocker;
        private GameObject externalTarget;

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

            GameObject weaponObject = GameObject.Find("PrototypeWeapon");
            Assert.That(weaponObject, Is.Not.Null);
            weapon = weaponObject.GetComponent<HitscanWeapon>();
            Assert.That(weapon, Is.Not.Null);
            Assert.That(weapon.Settings, Is.Not.Null);

            SetEnemyActive("PrototypeEnemy", false);
            SetEnemyActive("PrototypeSkirmisher", false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (selfBlocker != null)
            {
                Object.Destroy(selfBlocker);
            }

            if (externalTarget != null)
            {
                Object.Destroy(externalTarget);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator MagazineEmptiesThenReloadConsumesReserve()
        {
            HitscanWeaponSettings settings = weapon.Settings;
            Assert.That(weapon.CurrentMagazine, Is.EqualTo(settings.MagazineSize));
            Assert.That(weapon.ReserveAmmo, Is.EqualTo(settings.InitialReserveAmmo));

            for (int shot = 0; shot < settings.MagazineSize; shot++)
            {
                Assert.That(weapon.TryFire(), Is.True);
                if (shot < settings.MagazineSize - 1)
                {
                    yield return new WaitForSeconds(settings.SecondsPerShot);
                }
            }

            Assert.That(weapon.CurrentMagazine, Is.Zero);
            Assert.That(weapon.TryFire(), Is.False);
            Assert.That(weapon.TryStartReload(), Is.True);
            Assert.That(weapon.IsReloading, Is.True);

            yield return new WaitForSeconds(settings.ReloadDuration + 0.05f);

            Assert.That(weapon.IsReloading, Is.False);
            Assert.That(weapon.CurrentMagazine, Is.EqualTo(settings.MagazineSize));
            Assert.That(
                weapon.ReserveAmmo,
                Is.EqualTo(settings.InitialReserveAmmo - settings.MagazineSize));
        }

        [UnityTest]
        public IEnumerator ShotIgnoresOwnerCollidersAndHitsExternalTarget()
        {
            Health playerHealth = weapon.GetComponentInParent<Health>();
            Camera aimCamera = weapon.GetComponentInParent<Camera>();
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(aimCamera, Is.Not.Null);

            selfBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            selfBlocker.name = "SelfHitTestBlocker";
            selfBlocker.transform.SetParent(playerHealth.transform);
            selfBlocker.transform.SetPositionAndRotation(
                aimCamera.transform.position + aimCamera.transform.forward * 0.75f,
                aimCamera.transform.rotation);
            selfBlocker.transform.localScale = Vector3.one;

            externalTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
            externalTarget.name = "ExternalHitTestTarget";
            externalTarget.transform.SetPositionAndRotation(
                aimCamera.transform.position + aimCamera.transform.forward * 4f,
                aimCamera.transform.rotation);
            externalTarget.transform.localScale = Vector3.one * 2f;
            Health targetHealth = externalTarget.AddComponent<Health>();
            targetHealth.Configure(100f);

            Physics.SyncTransforms();
            float initialPlayerHealth = playerHealth.Current;
            float initialTargetHealth = targetHealth.Current;

            Assert.That(weapon.TryFire(), Is.True);
            yield return null;

            Assert.That(playerHealth.Current, Is.EqualTo(initialPlayerHealth));
            Assert.That(targetHealth.Current, Is.LessThan(initialTargetHealth));
            Assert.That(weapon.Settings.AppliedMarkEffect, Is.Not.Null);
            WeaponMarkState markState =
                externalTarget.GetComponent<WeaponMarkState>();
            Assert.That(markState, Is.Not.Null);
            Assert.That(
                markState.GetStacks(weapon.Settings.AppliedMarkEffect),
                Is.EqualTo(1),
                "Un tir infligeant des dégâts doit appliquer une charge de rupture.");
        }

        private static void SetEnemyActive(string enemyName, bool active)
        {
            GameObject enemy = GameObject.Find(enemyName);
            if (enemy != null)
            {
                enemy.SetActive(active);
            }
        }
    }
}
