using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectBloodbath.Input;
using ProjectBloodbath.Prototype;
using ProjectBloodbath.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeControlsSettingsPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private PlayerInputReader inputReader;
        private ControlSettingsManager settings;
        private PrototypeSystemMenu systemMenu;
        private PrototypeControlsSettingsPanel panel;
        private bool originalGamepadEnabled;

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
            Assert.That(player, Is.Not.Null);
            inputReader = player.GetComponent<PlayerInputReader>();
            settings = player.GetComponent<ControlSettingsManager>();
            systemMenu = player.GetComponent<PrototypeSystemMenu>();
            panel = player.GetComponent<PrototypeControlsSettingsPanel>();
            Assert.That(inputReader, Is.Not.Null);
            Assert.That(settings, Is.Not.Null);
            Assert.That(systemMenu, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            originalGamepadEnabled = settings.AppliedGamepadEnabled;

            player.GetComponent<ProjectBloodbath.Player.FpsPlayerController>()
                .enabled = false;
            foreach (PrototypeEnemyController enemy in
                     Object.FindObjectsByType<PrototypeEnemyController>(
                         FindObjectsSortMode.None))
            {
                enemy.enabled = false;
            }

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            panel?.CancelAndClose();
            systemMenu?.SetOpen(false);
            if (settings != null &&
                settings.AppliedGamepadEnabled != originalGamepadEnabled)
            {
                settings.BeginEditing();
                settings.ToggleGamepadEnabled();
                settings.ApplyPending();
            }
            Time.timeScale = 1f;
            yield return null;
        }

        [Test]
        public void FrenchKeyboardLayoutsSelectAzertyDefaults()
        {
            Assert.That(
                ControlSettingsManager.DetectKeyboardLayout(
                    "French",
                    "fr-FR"),
                Is.EqualTo(KeyboardLayoutProfile.Azerty));
            Assert.That(
                ControlSettingsManager.DetectKeyboardLayout(
                    "English (United States)",
                    "en-US"),
                Is.EqualTo(KeyboardLayoutProfile.Qwerty));

            Assert.That(
                ControlSettingsManager.ResolveMovementBindingPath(
                    KeyboardLayoutProfile.Azerty,
                    KeyboardLayoutProfile.Azerty,
                    "up"),
                Is.EqualTo("<Keyboard>/w"));
            Assert.That(
                ControlSettingsManager.ResolveMovementBindingPath(
                    KeyboardLayoutProfile.Azerty,
                    KeyboardLayoutProfile.Qwerty,
                    "up"),
                Is.EqualTo("<Keyboard>/z"));
            Assert.That(
                ControlSettingsManager.ResolveMovementBindingPath(
                    KeyboardLayoutProfile.Qwerty,
                    KeyboardLayoutProfile.Qwerty,
                    "left"),
                Is.EqualTo("<Keyboard>/a"));
            Assert.That(
                ControlSettingsManager.ResolveMovementBindingPath(
                    KeyboardLayoutProfile.Qwerty,
                    KeyboardLayoutProfile.Azerty,
                    "left"),
                Is.EqualTo("<Keyboard>/q"));

            settings.BeginEditing();
            settings.ResetPendingToDefaults();
            Assert.That(settings.PendingKeyboardLayoutIsAutomatic, Is.True);
            Assert.That(
                settings.PendingKeyboardLayout,
                Is.EqualTo(settings.DetectedKeyboardLayout));
            if (settings.PendingKeyboardLayout != KeyboardLayoutProfile.Azerty)
            {
                settings.CycleKeyboardLayout(1);
            }
            Assert.That(
                settings.GetBindingLabel(
                    "Move",
                    "up",
                    ControlDeviceProfile.KeyboardMouse),
                Is.EqualTo("Z"));
            Assert.That(
                settings.GetBindingLabel(
                    "Move",
                    "left",
                    ControlDeviceProfile.KeyboardMouse),
                Is.EqualTo("Q"));

            settings.CycleKeyboardLayout(1);
            Assert.That(settings.PendingKeyboardLayoutIsAutomatic, Is.False);
            Assert.That(
                settings.PendingKeyboardLayout,
                Is.EqualTo(KeyboardLayoutProfile.Qwerty));
            Assert.That(
                settings.GetBindingLabel(
                    "Move",
                    "up",
                    ControlDeviceProfile.KeyboardMouse),
                Is.EqualTo("W"));
            Assert.That(
                settings.GetBindingLabel(
                    "Move",
                    "left",
                    ControlDeviceProfile.KeyboardMouse),
                Is.EqualTo("A"));
            settings.CancelPending();
        }

        [Test]
        public void GamepadTriggersRepresentMatchingHands()
        {
            InputAction rightHand =
                settings.InputActions.FindAction("Player/Attack", true);
            InputAction leftHand =
                settings.InputActions.FindAction("Player/UseLeftHand", true);

            Assert.That(
                rightHand.bindings.Any(binding =>
                    binding.path == "<Gamepad>/rightTrigger"),
                Is.True);
            Assert.That(
                leftHand.bindings.Any(binding =>
                    binding.path == "<Gamepad>/leftTrigger"),
                Is.True);
            Assert.That(
                rightHand.bindings.Any(binding =>
                    binding.path == "<Mouse>/leftButton"),
                Is.True);
            Assert.That(
                leftHand.bindings.Any(binding =>
                    binding.path == "<Mouse>/rightButton"),
                Is.True);
        }

        [Test]
        public void GamepadCanBeDisabledWithoutDisablingKeyboardAndMouse()
        {
            settings.BeginEditing();
            if (settings.PendingGamepadEnabled)
            {
                settings.ToggleGamepadEnabled();
            }
            settings.ApplyPending();

            Assert.That(settings.AppliedGamepadEnabled, Is.False);
            Assert.That(ControlSettingsManager.GamepadPromptsEnabled, Is.False);
            Assert.That(
                ControlSettingsManager.FormatShortcut("E", "X"),
                Is.EqualTo("E"));
            Assert.That(settings.InputActions.bindingMask.HasValue, Is.True);
            Assert.That(
                settings.InputActions.bindingMask.Value.groups,
                Does.Contain("Keyboard&Mouse"));
            InputAction rightHand =
                settings.InputActions.FindAction("Player/Attack", true);
            Assert.That(
                rightHand.controls.Any(control => control.device is Mouse),
                Is.True);
            Assert.That(
                rightHand.controls.Any(control => control.device is Gamepad),
                Is.False);
        }

        [UnityTest]
        public IEnumerator BindingOverrideCanBeCancelled()
        {
            string original = settings.GetBindingLabel(
                "Attack",
                string.Empty,
                ControlDeviceProfile.KeyboardMouse);
            settings.BeginEditing();

            Assert.That(
                settings.ApplyBindingOverride(
                    "Attack",
                    string.Empty,
                    ControlDeviceProfile.KeyboardMouse,
                    "<Mouse>/middleButton"),
                Is.True);
            Assert.That(
                settings.GetBindingLabel(
                    "Attack",
                    string.Empty,
                    ControlDeviceProfile.KeyboardMouse),
                Is.EqualTo("CLIC MOLETTE"));

            settings.CancelPending();
            Assert.That(
                settings.GetBindingLabel(
                    "Attack",
                    string.Empty,
                    ControlDeviceProfile.KeyboardMouse),
                Is.EqualTo(original));
            yield break;
        }

        [UnityTest]
        public IEnumerator ControlsSubmenuReturnsToSystemMenuWithoutPausing()
        {
            float timeScale = Time.timeScale;
            systemMenu.SetOpen(true);
            systemMenu.MoveSelection(4);
            Assert.That(systemMenu.SelectedIndex, Is.EqualTo(4));

            systemMenu.ActivateSelected();
            Assert.That(systemMenu.IsOpen, Is.False);
            Assert.That(panel.IsOpen, Is.True);
            Assert.That(inputReader.GameplaySuppressed, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(timeScale));

            panel.CancelAndClose();
            Assert.That(panel.IsOpen, Is.False);
            Assert.That(systemMenu.IsOpen, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(timeScale));
            yield break;
        }
    }
}
