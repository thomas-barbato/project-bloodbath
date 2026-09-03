using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeShockwaveAbilityPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PrototypeShockwaveAbility ability;
        private AbilityResource resource;
        private Health targetHealth;
        private GameObject target;
        private readonly List<GameObject> additionalTargets = new();

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
            ability = player.GetComponent<PrototypeShockwaveAbility>();
            resource = player.GetComponent<AbilityResource>();
            Camera cameraComponent = player.GetComponentInChildren<Camera>();

            Assert.That(ability, Is.Not.Null);
            Assert.That(ability.Settings, Is.Not.Null);
            Assert.That(resource, Is.Not.Null);
            Assert.That(cameraComponent, Is.Not.Null);

            target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target.name = "AbilityTestTarget";
            target.transform.position = cameraComponent.transform.position +
                cameraComponent.transform.forward * 3f;
            targetHealth = target.AddComponent<Health>();
            Rigidbody targetBody = target.AddComponent<Rigidbody>();
            targetBody.isKinematic = true;
            Physics.SyncTransforms();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (target != null)
            {
                Object.Destroy(target);
            }

            for (int index = 0; index < additionalTargets.Count; index++)
            {
                Object.Destroy(additionalTargets[index]);
            }

            additionalTargets.Clear();

            yield return null;
        }

        [UnityTest]
        public IEnumerator ActivationDamagesTargetSpendsResourceAndStartsCooldown()
        {
            float startingHealth = targetHealth.Current;
            float startingResource = resource.Current;

            Assert.That(
                ability.TryActivate(),
                Is.True,
                "La première activation devrait être disponible.");
            Assert.That(
                targetHealth.Current,
                Is.EqualTo(startingHealth - ability.Settings.Damage));
            Assert.That(
                resource.Current,
                Is.EqualTo(startingResource - ability.Settings.ResourceCost));
            Assert.That(ability.LastHitCount, Is.EqualTo(1));
            Assert.That(ability.CooldownRemaining, Is.GreaterThan(0f));

            Assert.That(
                ability.TryActivate(),
                Is.False,
                "Le temps de recharge devrait bloquer une activation immédiate.");
            Assert.That(
                resource.Current,
                Is.EqualTo(startingResource - ability.Settings.ResourceCost));

            yield return new WaitForSeconds(
                ability.Settings.CooldownDuration + 0.05f);
            Assert.That(
                ability.TryActivate(),
                Is.True,
                "La compétence devrait redevenir disponible après son délai.");
        }

        [UnityTest]
        public IEnumerator ActivationHitsTargetsNearBothOuterEdgesOfCone()
        {
            Camera cameraComponent = ability.GetComponentInChildren<Camera>();
            Health leftTarget = CreateSmallTarget(
                cameraComponent,
                "AbilityLeftEdgeTarget",
                -44f,
                5.8f);
            Health rightTarget = CreateSmallTarget(
                cameraComponent,
                "AbilityRightEdgeTarget",
                44f,
                5.8f);
            Physics.SyncTransforms();
            yield return null;

            Assert.That(ability.TryActivate(), Is.True);
            Assert.That(
                leftTarget.Current,
                Is.EqualTo(leftTarget.Maximum - ability.Settings.Damage));
            Assert.That(
                rightTarget.Current,
                Is.EqualTo(rightTarget.Maximum - ability.Settings.Damage));
            Assert.That(ability.LastHitCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator MarkedTargetDetonatesForBonusDamage()
        {
            WeaponMarkEffectSettings markEffect =
                ability.Settings.ConsumedMarkEffect;
            Assert.That(markEffect, Is.Not.Null);
            WeaponMarkState markState = target.AddComponent<WeaponMarkState>();
            Assert.That(markState.ApplyMark(markEffect, 2), Is.EqualTo(2));
            float startingHealth = targetHealth.Current;
            float expectedDamage =
                ability.Settings.Damage +
                markEffect.DetonationDamagePerStack * 2f;

            Assert.That(ability.TryActivate(), Is.True);

            Assert.That(
                targetHealth.Current,
                Is.EqualTo(startingHealth - expectedDamage).Within(0.001f));
            Assert.That(markState.GetStacks(markEffect), Is.Zero);
            Assert.That(ability.LastDetonatedMarkCount, Is.EqualTo(2));
            Assert.That(ability.ShowsSynergyFeedback, Is.True);
            yield break;
        }

        private Health CreateSmallTarget(
            Camera cameraComponent,
            string targetName,
            float angle,
            float distance)
        {
            GameObject edgeTarget = new(targetName);
            Vector3 direction = Quaternion.AngleAxis(
                angle,
                cameraComponent.transform.up) *
                cameraComponent.transform.forward;
            edgeTarget.transform.position =
                cameraComponent.transform.position + direction * distance;
            SphereCollider collider = edgeTarget.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
            Health health = edgeTarget.AddComponent<Health>();
            additionalTargets.Add(edgeTarget);
            return health;
        }
    }
}
