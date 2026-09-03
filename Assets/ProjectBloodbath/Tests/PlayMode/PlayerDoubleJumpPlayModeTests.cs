using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PlayerDoubleJumpPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Keyboard keyboard;
        private FpsPlayerController playerController;
        private CharacterController characterController;

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
            playerController = player.GetComponent<FpsPlayerController>();
            characterController = player.GetComponent<CharacterController>();
            Assert.That(playerController, Is.Not.Null);
            Assert.That(characterController, Is.Not.Null);

            keyboard = InputSystem.AddDevice<Keyboard>();
            for (int frame = 0; frame < 5; frame++)
            {
                yield return null;
            }
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
        public IEnumerator SecondJumpWorksInAirAndThirdWaitsForLanding()
        {
            Assert.That(characterController.isGrounded, Is.True);
            Assert.That(playerController.RemainingJumps, Is.EqualTo(2));

            SetKeys(Key.Space);
            yield return null;
            SetKeys();

            float airborneTimeout = Time.time + 1f;
            while (
                characterController.isGrounded &&
                Time.time < airborneTimeout)
            {
                yield return null;
            }

            Assert.That(characterController.isGrounded, Is.False);
            Assert.That(playerController.JumpsPerformed, Is.EqualTo(1));
            Assert.That(playerController.RemainingJumps, Is.EqualTo(1));

            yield return new WaitForSeconds(0.12f);
            float velocityBeforeSecondJump = playerController.Velocity.y;
            SetKeys(Key.Space);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(playerController.JumpsPerformed, Is.EqualTo(2));
            Assert.That(playerController.RemainingJumps, Is.Zero);
            Assert.That(
                playerController.Velocity.y,
                Is.GreaterThan(velocityBeforeSecondJump + 1f),
                "Le second saut doit réinjecter une impulsion verticale.");

            float velocityBeforeThirdPress = playerController.Velocity.y;
            SetKeys(Key.Space);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(playerController.JumpsPerformed, Is.EqualTo(2));
            Assert.That(
                playerController.Velocity.y,
                Is.LessThan(velocityBeforeThirdPress),
                "Une troisième pression aérienne ne doit pas relancer le joueur.");

            float landingTimeout = Time.time + 4f;
            while (
                !characterController.isGrounded &&
                Time.time < landingTimeout)
            {
                yield return null;
            }
            yield return null;

            Assert.That(characterController.isGrounded, Is.True);
            Assert.That(playerController.JumpsPerformed, Is.Zero);
            Assert.That(playerController.RemainingJumps, Is.EqualTo(2));
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
