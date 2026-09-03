using System;
using System.Globalization;
using ProjectBloodbath.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectBloodbath.Settings
{
    public enum ControlDeviceProfile
    {
        KeyboardMouse,
        Gamepad
    }

    public enum KeyboardLayoutProfile
    {
        Qwerty,
        Azerty
    }

    [DefaultExecutionOrder(-1150)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class ControlSettingsManager : MonoBehaviour
    {
        public const float MinimumMouseSensitivity = 0.02f;
        public const float MaximumMouseSensitivity = 0.5f;
        public const float MouseSensitivityStep = 0.01f;
        public const float MinimumGamepadLookSpeed = 60f;
        public const float MaximumGamepadLookSpeed = 540f;
        public const float GamepadLookSpeedStep = 15f;

        private const string BindingOverridesPreferenceKey =
            "project_bloodbath.controls.binding_overrides";
        private const string KeyboardLayoutPreferenceKey =
            "project_bloodbath.controls.keyboard_layout";
        private const string MouseSensitivityPreferenceKey =
            "project_bloodbath.controls.mouse_sensitivity";
        private const string GamepadLookSpeedPreferenceKey =
            "project_bloodbath.controls.gamepad_look_speed";
        private const string InvertMouseYPreferenceKey =
            "project_bloodbath.controls.invert_mouse_y";
        private const string InvertGamepadYPreferenceKey =
            "project_bloodbath.controls.invert_gamepad_y";
        private const string GamepadEnabledPreferenceKey =
            "project_bloodbath.controls.gamepad_enabled";
        private const string KeyboardLayoutMappingVersionPreferenceKey =
            "project_bloodbath.controls.keyboard_layout_mapping_version";
        private const string KeyboardLayoutExplicitPreferenceKey =
            "project_bloodbath.controls.keyboard_layout_explicit";
        private const string LastDetectedKeyboardLayoutPreferenceKey =
            "project_bloodbath.controls.last_detected_keyboard_layout";
        private const int CurrentKeyboardLayoutMappingVersion = 2;
        private const string KeyboardMouseBindingGroup = "Keyboard&Mouse";
        private const string GamepadBindingGroup = "Gamepad";

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private float defaultMouseSensitivity = 0.1f;
        [SerializeField] private float defaultGamepadLookSpeed = 180f;
        [SerializeField] private bool defaultGamepadEnabled = true;

        private InputActionRebindingExtensions.RebindingOperation
            rebindOperation;
        private InputAction rebindAction;
        private bool rebindActionWasEnabled;
        private string editingBindingSnapshot = string.Empty;
        private bool bindingChangesPending;
        private InputBinding? bindingMaskBeforeRebind;

        public float AppliedMouseSensitivity { get; private set; }
        public float PendingMouseSensitivity { get; private set; }
        public float AppliedGamepadLookSpeed { get; private set; }
        public float PendingGamepadLookSpeed { get; private set; }
        public bool AppliedInvertMouseY { get; private set; }
        public bool PendingInvertMouseY { get; private set; }
        public bool AppliedInvertGamepadY { get; private set; }
        public bool PendingInvertGamepadY { get; private set; }
        public bool AppliedGamepadEnabled { get; private set; }
        public bool PendingGamepadEnabled { get; private set; }
        public KeyboardLayoutProfile AppliedKeyboardLayout { get; private set; }
        public KeyboardLayoutProfile PendingKeyboardLayout { get; private set; }
        public bool AppliedKeyboardLayoutIsAutomatic { get; private set; }
        public bool PendingKeyboardLayoutIsAutomatic { get; private set; }
        public KeyboardLayoutProfile DetectedKeyboardLayout { get; private set; }
        public bool IsRebinding => rebindOperation != null;
        public bool IsEditing { get; private set; }
        public static bool GamepadPromptsEnabled { get; private set; } = true;
        public bool HasPendingChanges =>
            !Mathf.Approximately(
                PendingMouseSensitivity,
                AppliedMouseSensitivity) ||
            !Mathf.Approximately(
                PendingGamepadLookSpeed,
                AppliedGamepadLookSpeed) ||
            PendingInvertMouseY != AppliedInvertMouseY ||
            PendingInvertGamepadY != AppliedInvertGamepadY ||
            PendingGamepadEnabled != AppliedGamepadEnabled ||
            PendingKeyboardLayout != AppliedKeyboardLayout ||
            PendingKeyboardLayoutIsAutomatic !=
                AppliedKeyboardLayoutIsAutomatic ||
            bindingChangesPending;

        public InputActionAsset InputActions => inputReader?.InputActions;

        public static string FormatShortcut(string keyboard, string gamepad)
        {
            return GamepadPromptsEnabled
                ? $"{keyboard} / {gamepad}"
                : keyboard;
        }

        private void Awake()
        {
            inputReader ??= GetComponent<PlayerInputReader>();
            DetectedKeyboardLayout = DetectKeyboardLayout(
                Keyboard.current?.keyboardLayout,
                CultureInfo.CurrentCulture.Name);
            LoadSavedSettings();
            ApplyGamepadInputState(AppliedGamepadEnabled);
            CopyAppliedSettingsToPending();
            editingBindingSnapshot = SaveBindingOverrides();
            bindingChangesPending = false;
        }

        private void OnDestroy()
        {
            CancelInteractiveRebind();
        }

        public static KeyboardLayoutProfile DetectKeyboardLayout(
            string keyboardLayout,
            string cultureName)
        {
            string layout = (keyboardLayout ?? string.Empty).ToLowerInvariant();
            string culture = (cultureName ?? string.Empty).ToLowerInvariant();
            bool azertyLayout =
                layout.Contains("azerty") ||
                layout.Contains("french") ||
                layout.Contains("belgian") ||
                layout.Contains("0000040c") ||
                layout.Contains("0000080c") ||
                layout.Contains("0000100c");
            bool azertyCulture =
                culture.StartsWith("fr-fr") ||
                culture.StartsWith("fr-be") ||
                culture.StartsWith("fr-lu") ||
                culture.StartsWith("fr-mc");
            return azertyLayout || azertyCulture
                ? KeyboardLayoutProfile.Azerty
                : KeyboardLayoutProfile.Qwerty;
        }

        public static string ResolveMovementBindingPath(
            KeyboardLayoutProfile detectedLayout,
            KeyboardLayoutProfile requestedLayout,
            string compositePart)
        {
            bool useNativePhysicalPositions =
                detectedLayout == requestedLayout;
            return compositePart?.ToLowerInvariant() switch
            {
                "up" => useNativePhysicalPositions
                    ? "<Keyboard>/w"
                    : "<Keyboard>/z",
                "left" => useNativePhysicalPositions
                    ? "<Keyboard>/a"
                    : "<Keyboard>/q",
                "down" => "<Keyboard>/s",
                "right" => "<Keyboard>/d",
                _ => string.Empty
            };
        }

        public void BeginEditing()
        {
            CancelInteractiveRebind();
            IsEditing = true;
            CopyAppliedSettingsToPending();
            editingBindingSnapshot = SaveBindingOverrides();
            bindingChangesPending = false;
        }

        public void ChangeMouseSensitivity(int direction)
        {
            PendingMouseSensitivity = StepAndClamp(
                PendingMouseSensitivity,
                direction,
                MouseSensitivityStep,
                MinimumMouseSensitivity,
                MaximumMouseSensitivity);
        }

        public void ChangeGamepadLookSpeed(int direction)
        {
            PendingGamepadLookSpeed = StepAndClamp(
                PendingGamepadLookSpeed,
                direction,
                GamepadLookSpeedStep,
                MinimumGamepadLookSpeed,
                MaximumGamepadLookSpeed);
        }

        public void ToggleInvertY(ControlDeviceProfile profile)
        {
            if (profile == ControlDeviceProfile.KeyboardMouse)
            {
                PendingInvertMouseY = !PendingInvertMouseY;
                return;
            }

            PendingInvertGamepadY = !PendingInvertGamepadY;
        }

        public void ToggleGamepadEnabled()
        {
            PendingGamepadEnabled = !PendingGamepadEnabled;
        }

        public void CycleKeyboardLayout(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            PendingKeyboardLayout =
                PendingKeyboardLayout == KeyboardLayoutProfile.Azerty
                    ? KeyboardLayoutProfile.Qwerty
                    : KeyboardLayoutProfile.Azerty;
            PendingKeyboardLayoutIsAutomatic = false;
            ApplyKeyboardLayout(PendingKeyboardLayout);
            bindingChangesPending = true;
        }

        public void ResetPendingToDefaults()
        {
            CancelInteractiveRebind();
            InputActions?.RemoveAllBindingOverrides();
            PendingKeyboardLayout = DetectedKeyboardLayout;
            PendingKeyboardLayoutIsAutomatic = true;
            PendingMouseSensitivity = Mathf.Clamp(
                defaultMouseSensitivity,
                MinimumMouseSensitivity,
                MaximumMouseSensitivity);
            PendingGamepadLookSpeed = Mathf.Clamp(
                defaultGamepadLookSpeed,
                MinimumGamepadLookSpeed,
                MaximumGamepadLookSpeed);
            PendingInvertMouseY = false;
            PendingInvertGamepadY = false;
            PendingGamepadEnabled = defaultGamepadEnabled;
            ApplyKeyboardLayout(PendingKeyboardLayout);
            bindingChangesPending = true;
        }

        public void ApplyPending()
        {
            CancelInteractiveRebind();
            AppliedMouseSensitivity = PendingMouseSensitivity;
            AppliedGamepadLookSpeed = PendingGamepadLookSpeed;
            AppliedInvertMouseY = PendingInvertMouseY;
            AppliedInvertGamepadY = PendingInvertGamepadY;
            AppliedGamepadEnabled = PendingGamepadEnabled;
            AppliedKeyboardLayout = PendingKeyboardLayout;
            AppliedKeyboardLayoutIsAutomatic =
                PendingKeyboardLayoutIsAutomatic;
            ApplyGamepadInputState(AppliedGamepadEnabled);
            editingBindingSnapshot = SaveBindingOverrides();
            IsEditing = false;
            bindingChangesPending = false;

            PlayerPrefs.SetString(
                BindingOverridesPreferenceKey,
                editingBindingSnapshot);
            PlayerPrefs.SetInt(
                KeyboardLayoutPreferenceKey,
                (int)AppliedKeyboardLayout);
            PlayerPrefs.SetInt(
                KeyboardLayoutExplicitPreferenceKey,
                AppliedKeyboardLayoutIsAutomatic ? 0 : 1);
            PlayerPrefs.SetInt(
                LastDetectedKeyboardLayoutPreferenceKey,
                (int)DetectedKeyboardLayout);
            PlayerPrefs.SetFloat(
                MouseSensitivityPreferenceKey,
                AppliedMouseSensitivity);
            PlayerPrefs.SetFloat(
                GamepadLookSpeedPreferenceKey,
                AppliedGamepadLookSpeed);
            PlayerPrefs.SetInt(
                InvertMouseYPreferenceKey,
                AppliedInvertMouseY ? 1 : 0);
            PlayerPrefs.SetInt(
                InvertGamepadYPreferenceKey,
                AppliedInvertGamepadY ? 1 : 0);
            PlayerPrefs.SetInt(
                GamepadEnabledPreferenceKey,
                AppliedGamepadEnabled ? 1 : 0);
            PlayerPrefs.SetInt(
                KeyboardLayoutMappingVersionPreferenceKey,
                CurrentKeyboardLayoutMappingVersion);
            PlayerPrefs.Save();
        }

        public void CancelPending()
        {
            CancelInteractiveRebind();
            RestoreBindingOverrides(editingBindingSnapshot);
            CopyAppliedSettingsToPending();
            IsEditing = false;
            bindingChangesPending = false;
        }

        public float GetLookSensitivity(bool pointerInput)
        {
            return pointerInput
                ? IsEditing
                    ? PendingMouseSensitivity
                    : AppliedMouseSensitivity
                : IsEditing
                    ? PendingGamepadLookSpeed
                    : AppliedGamepadLookSpeed;
        }

        public float GetVerticalLookMultiplier(bool pointerInput)
        {
            bool inverted = pointerInput
                ? IsEditing
                    ? PendingInvertMouseY
                    : AppliedInvertMouseY
                : IsEditing
                    ? PendingInvertGamepadY
                    : AppliedInvertGamepadY;
            return inverted ? -1f : 1f;
        }

        public string GetBindingLabel(
            string actionName,
            string compositePart,
            ControlDeviceProfile profile)
        {
            if (!TryFindBinding(
                    actionName,
                    compositePart,
                    profile,
                    out InputAction action,
                    out int bindingIndex))
            {
                return "NON ATTRIBUÉ";
            }

            string path = action.bindings[bindingIndex].effectivePath;
            InputControl displayDevice =
                profile == ControlDeviceProfile.KeyboardMouse
                    ? Keyboard.current
                    : null;
            return ToFrenchBindingLabel(
                path,
                displayDevice,
                DetectedKeyboardLayout);
        }

        public bool ApplyBindingOverride(
            string actionName,
            string compositePart,
            ControlDeviceProfile profile,
            string controlPath)
        {
            if (string.IsNullOrWhiteSpace(controlPath) ||
                !TryFindBinding(
                    actionName,
                    compositePart,
                    profile,
                    out InputAction action,
                    out int bindingIndex))
            {
                return false;
            }

            action.ApplyBindingOverride(bindingIndex, controlPath);
            bindingChangesPending = true;
            return true;
        }

        public bool StartInteractiveRebind(
            string actionName,
            string compositePart,
            ControlDeviceProfile profile,
            Action<bool> completed)
        {
            if (IsRebinding ||
                !TryFindBinding(
                    actionName,
                    compositePart,
                    profile,
                    out InputAction action,
                    out int bindingIndex))
            {
                return false;
            }

            rebindAction = action;
            rebindActionWasEnabled = action.enabled;
            bindingMaskBeforeRebind = InputActions?.bindingMask;
            if (profile == ControlDeviceProfile.Gamepad &&
                AppliedGamepadEnabled == false &&
                InputActions != null)
            {
                InputActions.bindingMask = null;
            }
            action.Disable();
            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(operation => FinishInteractiveRebind(false, completed))
                .OnComplete(operation => FinishInteractiveRebind(true, completed));

            if (profile == ControlDeviceProfile.Gamepad)
            {
                rebindOperation
                    .WithControlsHavingToMatchPath("<Gamepad>")
                    .WithCancelingThrough("<Gamepad>/buttonEast");
            }
            else
            {
                rebindOperation
                    .WithControlsExcluding("<Gamepad>")
                    .WithControlsExcluding("<Joystick>")
                    .WithControlsExcluding("<Touchscreen>")
                    .WithControlsExcluding("<XRController>");
            }

            rebindOperation.Start();
            return true;
        }

        public void CancelInteractiveRebind()
        {
            rebindOperation?.Cancel();
        }

        private void LoadSavedSettings()
        {
            KeyboardLayoutProfile savedKeyboardLayout =
                (KeyboardLayoutProfile)Mathf.Clamp(
                PlayerPrefs.GetInt(
                    KeyboardLayoutPreferenceKey,
                    (int)DetectedKeyboardLayout),
                0,
                1);
            AppliedKeyboardLayoutIsAutomatic =
                PlayerPrefs.GetInt(
                    KeyboardLayoutExplicitPreferenceKey,
                    0) == 0;
            AppliedKeyboardLayout = AppliedKeyboardLayoutIsAutomatic
                ? DetectedKeyboardLayout
                : savedKeyboardLayout;
            AppliedMouseSensitivity = Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    MouseSensitivityPreferenceKey,
                    defaultMouseSensitivity),
                MinimumMouseSensitivity,
                MaximumMouseSensitivity);
            AppliedGamepadLookSpeed = Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    GamepadLookSpeedPreferenceKey,
                    defaultGamepadLookSpeed),
                MinimumGamepadLookSpeed,
                MaximumGamepadLookSpeed);
            AppliedInvertMouseY =
                PlayerPrefs.GetInt(InvertMouseYPreferenceKey, 0) != 0;
            AppliedInvertGamepadY =
                PlayerPrefs.GetInt(InvertGamepadYPreferenceKey, 0) != 0;
            AppliedGamepadEnabled =
                PlayerPrefs.GetInt(
                    GamepadEnabledPreferenceKey,
                    defaultGamepadEnabled ? 1 : 0) != 0;

            string savedOverrides = PlayerPrefs.GetString(
                BindingOverridesPreferenceKey,
                string.Empty);
            bool requiresKeyboardLayoutMigration =
                PlayerPrefs.GetInt(
                    KeyboardLayoutMappingVersionPreferenceKey,
                    0) < CurrentKeyboardLayoutMappingVersion;
            bool automaticLayoutChanged =
                AppliedKeyboardLayoutIsAutomatic &&
                PlayerPrefs.GetInt(
                    LastDetectedKeyboardLayoutPreferenceKey,
                    -1) != (int)DetectedKeyboardLayout;
            bool standardMovementBindingsWereApplied = false;
            if (string.IsNullOrWhiteSpace(savedOverrides))
            {
                InputActions?.RemoveAllBindingOverrides();
                ApplyKeyboardLayout(AppliedKeyboardLayout);
                standardMovementBindingsWereApplied = true;
            }
            else
            {
                RestoreBindingOverrides(savedOverrides);
                if (requiresKeyboardLayoutMigration || automaticLayoutChanged)
                {
                    ApplyKeyboardLayout(AppliedKeyboardLayout);
                    standardMovementBindingsWereApplied = true;
                }
            }

            if (standardMovementBindingsWereApplied ||
                requiresKeyboardLayoutMigration ||
                automaticLayoutChanged)
            {
                PlayerPrefs.SetString(
                    BindingOverridesPreferenceKey,
                    SaveBindingOverrides());
                PlayerPrefs.SetInt(
                    KeyboardLayoutMappingVersionPreferenceKey,
                    CurrentKeyboardLayoutMappingVersion);
                PlayerPrefs.SetInt(
                    LastDetectedKeyboardLayoutPreferenceKey,
                    (int)DetectedKeyboardLayout);
                PlayerPrefs.Save();
            }
        }

        private void CopyAppliedSettingsToPending()
        {
            PendingMouseSensitivity = AppliedMouseSensitivity;
            PendingGamepadLookSpeed = AppliedGamepadLookSpeed;
            PendingInvertMouseY = AppliedInvertMouseY;
            PendingInvertGamepadY = AppliedInvertGamepadY;
            PendingGamepadEnabled = AppliedGamepadEnabled;
            PendingKeyboardLayout = AppliedKeyboardLayout;
            PendingKeyboardLayoutIsAutomatic =
                AppliedKeyboardLayoutIsAutomatic;
        }

        private void ApplyKeyboardLayout(KeyboardLayoutProfile profile)
        {
            string[] movementParts = { "up", "left", "down", "right" };
            foreach (string part in movementParts)
            {
                string path = ResolveMovementBindingPath(
                    DetectedKeyboardLayout,
                    profile,
                    part);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    SetMovementBinding(part, path);
                }
            }
        }

        private void SetMovementBinding(string part, string path)
        {
            ApplyBindingOverride(
                "Move",
                part,
                ControlDeviceProfile.KeyboardMouse,
                path);
        }

        private bool TryFindBinding(
            string actionName,
            string compositePart,
            ControlDeviceProfile profile,
            out InputAction action,
            out int bindingIndex)
        {
            action = InputActions?.FindAction(
                $"Player/{actionName}",
                false);
            bindingIndex = -1;
            if (action == null)
            {
                return false;
            }

            string bindingGroup = profile == ControlDeviceProfile.Gamepad
                ? GamepadBindingGroup
                : KeyboardMouseBindingGroup;
            bool withinPrimaryMovementComposite = false;
            for (int index = 0; index < action.bindings.Count; index++)
            {
                InputBinding binding = action.bindings[index];
                if (binding.isComposite)
                {
                    withinPrimaryMovementComposite =
                        actionName == "Move" && binding.name == "WASD";
                    continue;
                }

                if (binding.isPartOfComposite)
                {
                    if (withinPrimaryMovementComposite &&
                        string.Equals(
                            binding.name,
                            compositePart,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        bindingIndex = index;
                        return true;
                    }
                    continue;
                }

                withinPrimaryMovementComposite = false;
                if (!string.IsNullOrWhiteSpace(compositePart) ||
                    !BindingBelongsToGroup(binding, bindingGroup))
                {
                    continue;
                }

                bindingIndex = index;
                return true;
            }

            return false;
        }

        private void FinishInteractiveRebind(
            bool completedSuccessfully,
            Action<bool> completed)
        {
            InputActionRebindingExtensions.RebindingOperation operation =
                rebindOperation;
            rebindOperation = null;
            operation?.Dispose();
            if (InputActions != null)
            {
                InputActions.bindingMask = bindingMaskBeforeRebind;
            }
            bindingMaskBeforeRebind = null;
            if (rebindActionWasEnabled)
            {
                rebindAction?.Enable();
            }
            rebindAction = null;
            rebindActionWasEnabled = false;
            if (completedSuccessfully)
            {
                bindingChangesPending = true;
            }
            completed?.Invoke(completedSuccessfully);
        }

        private string SaveBindingOverrides()
        {
            return InputActions?.SaveBindingOverridesAsJson() ?? string.Empty;
        }

        private void ApplyGamepadInputState(bool enabled)
        {
            GamepadPromptsEnabled = enabled;
            if (InputActions == null)
            {
                return;
            }

            InputActions.bindingMask = enabled
                ? null
                : InputBinding.MaskByGroup(KeyboardMouseBindingGroup);
        }

        private void RestoreBindingOverrides(string json)
        {
            if (InputActions == null)
            {
                return;
            }

            InputActions.RemoveAllBindingOverrides();
            if (!string.IsNullOrWhiteSpace(json))
            {
                InputActions.LoadBindingOverridesFromJson(json);
            }
        }

        private static bool BindingBelongsToGroup(
            InputBinding binding,
            string bindingGroup)
        {
            return !string.IsNullOrWhiteSpace(binding.groups) &&
                binding.groups.IndexOf(
                    bindingGroup,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ToFrenchBindingLabel(
            string path,
            InputControl displayDevice,
            KeyboardLayoutProfile detectedKeyboardLayout)
        {
            if (detectedKeyboardLayout == KeyboardLayoutProfile.Azerty)
            {
                string azertyLabel = path switch
                {
                    "<Keyboard>/w" => "Z",
                    "<Keyboard>/z" => "W",
                    "<Keyboard>/a" => "Q",
                    "<Keyboard>/q" => "A",
                    _ => string.Empty
                };
                if (!string.IsNullOrEmpty(azertyLabel))
                {
                    return azertyLabel;
                }
            }

            return path switch
            {
                "<Mouse>/leftButton" => "CLIC GAUCHE",
                "<Mouse>/rightButton" => "CLIC DROIT",
                "<Mouse>/middleButton" => "CLIC MOLETTE",
                "<Gamepad>/leftTrigger" => "GÂCHETTE GAUCHE",
                "<Gamepad>/rightTrigger" => "GÂCHETTE DROITE",
                "<Gamepad>/leftShoulder" => "BOUTON SUPÉRIEUR GAUCHE",
                "<Gamepad>/rightShoulder" => "BOUTON SUPÉRIEUR DROIT",
                "<Gamepad>/leftStickPress" => "STICK GAUCHE",
                "<Gamepad>/rightStickPress" => "STICK DROIT",
                _ => InputControlPath.ToHumanReadableString(
                        path,
                        InputControlPath.HumanReadableStringOptions.OmitDevice,
                        displayDevice)
                    .ToUpperInvariant()
            };
        }

        private static float StepAndClamp(
            float current,
            int direction,
            float step,
            float minimum,
            float maximum)
        {
            if (direction == 0)
            {
                return current;
            }

            float value = current + Math.Sign(direction) * step;
            return Mathf.Clamp(
                Mathf.Round(value / step) * step,
                minimum,
                maximum);
        }
    }
}
