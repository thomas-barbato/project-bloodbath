using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class DualWeaponLoadoutPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PrototypeWeaponLoadout loadout;
        private GameObject rightWeaponObject;
        private GameObject leftWeaponObject;
        private GameObject meleeWeaponObject;
        private Keyboard keyboard;

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

            GameObject player = GameObject.Find("Player");
            rightWeaponObject = GameObject.Find("PrototypeWeapon");
            leftWeaponObject = GameObject.Find("PrototypeLeftWeapon");
            meleeWeaponObject = FindInactiveObject("PrototypeMeleeWeapon");

            Assert.That(player, Is.Not.Null);
            Assert.That(rightWeaponObject, Is.Not.Null);
            Assert.That(leftWeaponObject, Is.Not.Null);
            Assert.That(meleeWeaponObject, Is.Not.Null);

            loadout = player.GetComponent<PrototypeWeaponLoadout>();
            Assert.That(loadout, Is.Not.Null);

            SetEnemyActive("PrototypeEnemy", false);
            SetEnemyActive("PrototypeSkirmisher", false);
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (keyboard != null && keyboard.added)
            {
                SetKeys();
                InputSystem.RemoveDevice(keyboard);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SwappingChangesTheCompleteEquippedHandSet()
        {
            Assert.That(loadout.HasTwoActiveRangedWeapons, Is.True);
            Assert.That(loadout.ActiveRightRangedWeapon.gameObject,
                Is.SameAs(rightWeaponObject));
            Assert.That(loadout.ActiveLeftRangedWeapon.gameObject,
                Is.SameAs(leftWeaponObject));
            Assert.That(rightWeaponObject.activeInHierarchy, Is.True);
            Assert.That(leftWeaponObject.activeInHierarchy, Is.True);
            Assert.That(meleeWeaponObject.activeInHierarchy, Is.False);

            loadout.SwapHandSet();
            yield return null;

            Assert.That(
                loadout.ActiveHandSet,
                Is.EqualTo(PrototypeHandSetSlot.Secondary));
            Assert.That(rightWeaponObject.activeInHierarchy, Is.False);
            Assert.That(leftWeaponObject.activeInHierarchy, Is.False);
            Assert.That(meleeWeaponObject.activeInHierarchy, Is.True);

            loadout.SwapHandSet();
            yield return null;

            Assert.That(
                loadout.ActiveHandSet,
                Is.EqualTo(PrototypeHandSetSlot.Primary));
            Assert.That(rightWeaponObject.activeInHierarchy, Is.True);
            Assert.That(leftWeaponObject.activeInHierarchy, Is.True);
            Assert.That(meleeWeaponObject.activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator BothRangedHandsCanFireDuringTheSameFrame()
        {
            HitscanWeapon rightWeapon = loadout.ActiveRightRangedWeapon;
            HitscanWeapon leftWeapon = loadout.ActiveLeftRangedWeapon;
            int rightMagazine = rightWeapon.CurrentMagazine;
            int leftMagazine = leftWeapon.CurrentMagazine;

            Assert.That(loadout.TryUseHand(CombatHand.Right), Is.True);
            Assert.That(loadout.TryUseHand(CombatHand.Left), Is.True);

            Assert.That(rightWeapon.CurrentMagazine,
                Is.EqualTo(rightMagazine - 1));
            Assert.That(leftWeapon.CurrentMagazine,
                Is.EqualTo(leftMagazine - 1));
            Assert.That(loadout.TryReloadActiveWeapons(), Is.True);
            Assert.That(rightWeapon.IsReloading, Is.True);
            Assert.That(leftWeapon.IsReloading, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RebindableSwapActionChangesTheEquippedSet()
        {
            Assert.That(
                loadout.ActiveHandSet,
                Is.EqualTo(PrototypeHandSetSlot.Primary));

            SetKeys(Key.Z);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(
                loadout.ActiveHandSet,
                Is.EqualTo(PrototypeHandSetSlot.Secondary));
            Assert.That(meleeWeaponObject.activeInHierarchy, Is.True);
            Assert.That(rightWeaponObject.activeInHierarchy, Is.False);
            Assert.That(leftWeaponObject.activeInHierarchy, Is.False);
        }

        private static void SetEnemyActive(string enemyName, bool active)
        {
            GameObject enemy = GameObject.Find(enemyName);
            if (enemy != null)
            {
                enemy.SetActive(active);
            }
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

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
