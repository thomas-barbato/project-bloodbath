using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Input;
using ProjectBloodbath.Player;
using ProjectBloodbath.Prototype;
using ProjectBloodbath.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypePlayerLifePlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private GameObject player;
        private Health health;
        private AbilityResource abilityResource;
        private PrototypePlayerLife playerLife;
        private PlayerInputReader inputReader;
        private FpsPlayerController playerController;
        private CharacterController characterController;
        private PrototypeWeaponLoadout weaponLoadout;
        private GameObject rangedWeapon;
        private GameObject meleeWeapon;
        private Transform cameraPivot;
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

            player = GameObject.Find("Player");
            rangedWeapon = GameObject.Find("PrototypeWeapon");
            meleeWeapon = FindInactiveObject("PrototypeMeleeWeapon");
            cameraPivot = player?.transform.Find("CameraPivot");
            Assert.That(player, Is.Not.Null);
            Assert.That(rangedWeapon, Is.Not.Null);
            Assert.That(meleeWeapon, Is.Not.Null);
            Assert.That(cameraPivot, Is.Not.Null);

            health = player.GetComponent<Health>();
            abilityResource = player.GetComponent<AbilityResource>();
            playerLife = player.GetComponent<PrototypePlayerLife>();
            inputReader = player.GetComponent<PlayerInputReader>();
            playerController = player.GetComponent<FpsPlayerController>();
            characterController = player.GetComponent<CharacterController>();
            weaponLoadout = player.GetComponent<PrototypeWeaponLoadout>();
            spawnPosition = player.transform.position;

            Assert.That(health, Is.Not.Null);
            Assert.That(abilityResource, Is.Not.Null);
            Assert.That(playerLife, Is.Not.Null);
            Assert.That(inputReader, Is.Not.Null);
            Assert.That(playerController, Is.Not.Null);
            Assert.That(characterController, Is.Not.Null);
            Assert.That(weaponLoadout, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator NonFatalDamageReducesHealthWithoutRespawn()
        {
            health.ApplyDamage(CreateDamage(25f));

            Assert.That(health.Current, Is.EqualTo(75f));
            Assert.That(playerLife.IsRespawning, Is.False);
            yield break;
        }

        [UnityTest]
        public IEnumerator DeathCreatesSoulThenCorpseRecoveryRestoresCombat()
        {
            playerController.AddLookImpulse(18f, 12f);
            Assert.That(abilityResource.TrySpend(40f), Is.True);
            player.transform.position += Vector3.forward * 2f;
            Vector3 deathPosition = player.transform.position;
            health.ApplyDamage(CreateDamage(health.Maximum + 1f));

            yield return null;

            Assert.That(playerLife.IsRespawning, Is.True);
            Assert.That(playerLife.ActiveCorpse, Is.Not.Null);
            Assert.That(
                Vector3.Distance(
                    playerLife.ActiveCorpse.transform.position,
                    deathPosition),
                Is.LessThan(0.01f));
            Assert.That(health.IsAlive, Is.False);
            Assert.That(inputReader.enabled, Is.False);
            Assert.That(playerController.enabled, Is.False);
            Assert.That(characterController.enabled, Is.False);
            Assert.That(weaponLoadout.CombatEnabled, Is.False);
            Assert.That(rangedWeapon.activeInHierarchy, Is.False);
            Assert.That(meleeWeapon.activeInHierarchy, Is.False);

            yield return new WaitForSeconds(playerLife.RespawnDelay + 0.1f);

            Assert.That(playerLife.IsRespawning, Is.False);
            Assert.That(playerLife.IsSoul, Is.True);
            Assert.That(health.Current, Is.EqualTo(health.Maximum));
            Assert.That(abilityResource.Current, Is.EqualTo(60f));
            Assert.That(health.IsInvulnerable, Is.True);
            Assert.That(inputReader.enabled, Is.True);
            Assert.That(playerController.enabled, Is.True);
            Assert.That(characterController.enabled, Is.True);
            Assert.That(weaponLoadout.CombatEnabled, Is.False);
            Assert.That(rangedWeapon.activeInHierarchy, Is.False);
            Assert.That(meleeWeapon.activeInHierarchy, Is.False);
            Assert.That(
                Vector3.Distance(player.transform.position, spawnPosition),
                Is.LessThan(0.01f));
            Assert.That(
                Quaternion.Angle(cameraPivot.localRotation, Quaternion.identity),
                Is.LessThan(0.01f));
            Vector2 horizontalVelocity = new(
                playerController.Velocity.x,
                playerController.Velocity.z);
            Assert.That(horizontalVelocity.sqrMagnitude, Is.LessThan(0.001f));

            health.ApplyDamage(CreateDamage(health.Maximum));
            Assert.That(health.Current, Is.EqualTo(health.Maximum));
            Assert.That(abilityResource.Current, Is.EqualTo(60f));

            PrototypeCorpseRecovery corpse = playerLife.ActiveCorpse;
            float recoveryTimeout = Time.time + 2f;
            while (playerLife.IsSoul && Time.time < recoveryTimeout)
            {
                Vector3 toCorpse =
                    corpse.transform.position - player.transform.position;
                toCorpse.y = 0f;
                characterController.Move(
                    Vector3.ClampMagnitude(toCorpse, 0.25f));
                yield return null;
            }

            Assert.That(playerLife.IsSoul, Is.False);
            Assert.That(playerLife.ActiveCorpse, Is.Null);
            Assert.That(health.IsInvulnerable, Is.False);
            Assert.That(health.Current, Is.EqualTo(health.Maximum));
            Assert.That(
                abilityResource.Current,
                Is.EqualTo(abilityResource.Maximum));
            Assert.That(weaponLoadout.CombatEnabled, Is.True);
            Assert.That(rangedWeapon.activeInHierarchy, Is.True);
            Assert.That(
                playerLife.OutgoingDamageMultiplier,
                Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(playerLife.ResurrectionPenaltyRemaining, Is.GreaterThan(0f));
        }

        private static DamageInfo CreateDamage(float amount)
        {
            return new DamageInfo(
                amount,
                DamageType.Ballistic,
                Vector3.zero,
                Vector3.back,
                Vector3.forward,
                0f,
                null);
        }

        private static GameObject FindInactiveObject(string objectName)
        {
            foreach (Transform candidate in
                Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (
                    candidate.name == objectName &&
                    candidate.gameObject.scene.IsValid())
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }
    }
}
