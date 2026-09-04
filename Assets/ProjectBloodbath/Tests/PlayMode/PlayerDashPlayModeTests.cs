using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Input;
using ProjectBloodbath.Player;
using ProjectBloodbath.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PlayerDashPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Keyboard keyboard;
        private Gamepad gamepad;
        private FpsPlayerController playerController;
        private ControlSettingsManager controlSettings;
        private InputActionAsset inputActions;
        private InputBinding? originalBindingMask;

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
            controlSettings = player.GetComponent<ControlSettingsManager>();
            PlayerInputReader inputReader =
                player.GetComponent<PlayerInputReader>();
            Assert.That(playerController, Is.Not.Null);
            Assert.That(controlSettings, Is.Not.Null);
            Assert.That(inputReader, Is.Not.Null);

            inputActions = inputReader.InputActions;
            originalBindingMask = inputActions.bindingMask;
            inputActions.bindingMask = null;
            controlSettings.BeginEditing();
            Assert.That(
                controlSettings.ApplyBindingOverride(
                    "Move",
                    "up",
                    ControlDeviceProfile.KeyboardMouse,
                    "<Keyboard>/w"),
                Is.True);
            Assert.That(
                controlSettings.ApplyBindingOverride(
                    "Dash",
                    string.Empty,
                    ControlDeviceProfile.KeyboardMouse,
                    "<Keyboard>/leftAlt"),
                Is.True);
            Assert.That(
                controlSettings.ApplyBindingOverride(
                    "Dash",
                    string.Empty,
                    ControlDeviceProfile.Gamepad,
                    "<Gamepad>/rightStickPress"),
                Is.True);

            keyboard = InputSystem.AddDevice<Keyboard>();
            gamepad = InputSystem.AddDevice<Gamepad>();
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
            if (gamepad != null && gamepad.added)
            {
                SetGamepad(Vector2.zero, false);
                InputSystem.RemoveDevice(gamepad);
            }

            controlSettings?.CancelPending();
            if (inputActions != null)
            {
                inputActions.bindingMask = originalBindingMask;
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator KeyboardDoubleTapStartsDirectionalDash()
        {
            yield return DoubleTap(Key.W);

            Assert.That(playerController.IsDashing, Is.True);
            Assert.That(
                Vector3.Dot(
                    playerController.Velocity,
                    playerController.transform.forward),
                Is.GreaterThan(15f));
        }

        [UnityTest]
        public IEnumerator DedicatedGamepadButtonUsesLeftStickDirection()
        {
            SetGamepad(Vector2.right, false);
            yield return null;
            SetGamepad(Vector2.right, true);
            yield return null;

            Assert.That(playerController.IsDashing, Is.True);
            Assert.That(
                Vector3.Dot(
                    playerController.Velocity,
                    playerController.transform.right),
                Is.GreaterThan(15f));
        }

        [UnityTest]
        public IEnumerator DoubleStickFlickDoesNotTriggerDash()
        {
            SetGamepad(Vector2.right, false);
            yield return null;
            SetGamepad(Vector2.zero, false);
            yield return new WaitForSeconds(0.06f);
            SetGamepad(Vector2.right, false);
            yield return null;

            Assert.That(
                playerController.IsDashing,
                Is.False,
                "Le stick seul ne doit pas produire de dash involontaire.");
        }

        [UnityTest]
        public IEnumerator CooldownRejectsAnImmediateSecondDash()
        {
            yield return DoubleTap(Key.W);
            Assert.That(playerController.IsDashing, Is.True);

            yield return new WaitForSeconds(0.22f);
            Assert.That(playerController.IsDashing, Is.False);

            yield return DoubleTap(Key.W);
            Assert.That(
                playerController.IsDashing,
                Is.False,
                "Le délai doit empêcher la répétition immédiate du dash.");
        }

        private IEnumerator DoubleTap(Key key)
        {
            SetKeys(key);
            yield return null;
            SetKeys();
            yield return new WaitForSeconds(0.06f);
            SetKeys(key);
            yield return null;
            SetKeys();
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private void SetGamepad(Vector2 leftStick, bool dashPressed)
        {
            GamepadState state = new()
            {
                leftStick = leftStick
            };
            if (dashPressed)
            {
                state = state.WithButton(GamepadButton.RightStick);
            }
            InputSystem.QueueStateEvent(gamepad, state);
        }
    }
}
