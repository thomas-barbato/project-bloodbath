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
    public sealed class PrototypeBloodHarvestPassivePlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private GameObject player;
        private AbilityResource resource;
        private PrototypeBloodHarvestPassive passive;
        private PrototypeCombatHud hud;
        private GameObject playerWeaponSource;
        private GameObject foreignSource;
        private GameObject target;
        private Health targetHealth;

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

            player = GameObject.Find("Player");
            Assert.That(player, Is.Not.Null);
            resource = player.GetComponent<AbilityResource>();
            passive = player.GetComponent<PrototypeBloodHarvestPassive>();
            hud = player.GetComponent<PrototypeCombatHud>();
            Assert.That(resource, Is.Not.Null);
            Assert.That(passive, Is.Not.Null);
            Assert.That(passive.Settings, Is.Not.Null);
            Assert.That(hud, Is.Not.Null);

            playerWeaponSource = new GameObject("PlayerWeaponSource");
            playerWeaponSource.transform.SetParent(player.transform);
            foreignSource = new GameObject("ForeignSource");
            target = new GameObject("PassiveTestTarget");
            targetHealth = target.AddComponent<Health>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(playerWeaponSource);
            Object.Destroy(foreignSource);
            Object.Destroy(target);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerKillRestoresEnergyButForeignKillDoesNot()
        {
            Assert.That(resource.TrySpend(50f), Is.True);
            float expectedRestore = passive.Settings.ResourceRestoredPerKill;

            KillTarget(playerWeaponSource);

            Assert.That(
                resource.Current,
                Is.EqualTo(50f + expectedRestore));
            Assert.That(passive.TriggerCount, Is.EqualTo(1));
            Assert.That(passive.LastRestoredAmount, Is.EqualTo(expectedRestore));
            Assert.That(passive.FeedbackRemaining, Is.GreaterThan(0f));
            Assert.That(hud.ShowsPassiveFeedback, Is.True);
            Assert.That(
                hud.PassiveFeedbackLabel,
                Is.EqualTo(
                    $"MOISSON SANGLANTE  •  +{expectedRestore:0} ÉNERGIE"));

            targetHealth.RestoreFull();
            KillTarget(foreignSource);

            Assert.That(
                resource.Current,
                Is.EqualTo(50f + expectedRestore));
            Assert.That(passive.TriggerCount, Is.EqualTo(1));
            yield break;
        }

        private void KillTarget(GameObject source)
        {
            targetHealth.ApplyDamage(new DamageInfo(
                targetHealth.Maximum,
                DamageType.Ballistic,
                target.transform.position,
                Vector3.up,
                Vector3.forward,
                0f,
                source));
            Assert.That(targetHealth.IsAlive, Is.False);
        }
    }
}
