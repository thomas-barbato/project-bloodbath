using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectBloodbath.Input
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";

        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction reloadAction;
        private InputAction ability1Action;
        private InputAction interactAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction slideAction;
        private InputAction selectRangedAction;
        private InputAction selectMeleeAction;
        private bool jumpPressed;
        private bool slidePressed;
        private bool reloadPressed;
        private bool ability1Pressed;
        private bool interactPressed;

        public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 Look => lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool AttackHeld => attackAction?.IsPressed() ?? false;
        public bool AttackPressedThisFrame => attackAction?.WasPressedThisFrame() ?? false;
        public bool SprintHeld => sprintAction?.IsPressed() ?? false;
        public bool SelectRangedPressedThisFrame =>
            selectRangedAction?.WasPressedThisFrame() ?? false;
        public bool SelectMeleePressedThisFrame =>
            selectMeleeAction?.WasPressedThisFrame() ?? false;
        public bool LookUsesPointerDelta => lookAction?.activeControl?.device is Pointer;

        public void Configure(InputActionAsset actions)
        {
            inputActions = actions;
            CacheActions();
        }

        public bool ConsumeJumpPressed()
        {
            bool value = jumpPressed;
            jumpPressed = false;
            return value;
        }

        public bool ConsumeSlidePressed()
        {
            bool value = slidePressed;
            slidePressed = false;
            return value;
        }

        public bool ConsumeReloadPressed()
        {
            bool value = reloadPressed;
            reloadPressed = false;
            return value;
        }

        public bool ConsumeAbility1Pressed()
        {
            bool value = ability1Pressed;
            ability1Pressed = false;
            return value;
        }

        public bool ConsumeInteractPressed()
        {
            bool value = interactPressed;
            interactPressed = false;
            return value;
        }

        private void OnEnable()
        {
            CacheActions();
            if (playerMap == null)
            {
                enabled = false;
                return;
            }

            jumpAction.performed += OnJumpPerformed;
            slideAction.performed += OnSlidePerformed;
            reloadAction.performed += OnReloadPerformed;
            ability1Action.performed += OnAbility1Performed;
            interactAction.performed += OnInteractPerformed;
            playerMap.Enable();
        }

        private void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.performed -= OnJumpPerformed;
            }

            if (slideAction != null)
            {
                slideAction.performed -= OnSlidePerformed;
            }
            if (reloadAction != null)
            {
                reloadAction.performed -= OnReloadPerformed;
            }
            if (ability1Action != null)
            {
                ability1Action.performed -= OnAbility1Performed;
            }
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
            }

            playerMap?.Disable();
            jumpPressed = false;
            slidePressed = false;
            reloadPressed = false;
            ability1Pressed = false;
            interactPressed = false;
        }

        private void CacheActions()
        {
            if (inputActions == null)
            {
                Debug.LogError("Aucun InputActionAsset n'est configuré.", this);
                playerMap = null;
                return;
            }

            playerMap = inputActions.FindActionMap(actionMapName, true);
            moveAction = playerMap.FindAction("Move", true);
            lookAction = playerMap.FindAction("Look", true);
            attackAction = playerMap.FindAction("Attack", true);
            reloadAction = playerMap.FindAction("Reload", true);
            ability1Action = playerMap.FindAction("Ability1", true);
            interactAction = playerMap.FindAction("Interact", true);
            jumpAction = playerMap.FindAction("Jump", true);
            sprintAction = playerMap.FindAction("Sprint", true);
            slideAction = playerMap.FindAction("Slide", true);
            selectRangedAction = playerMap.FindAction("SelectRanged", true);
            selectMeleeAction = playerMap.FindAction("SelectMelee", true);
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpPressed = true;
        }

        private void OnSlidePerformed(InputAction.CallbackContext context)
        {
            slidePressed = true;
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            reloadPressed = true;
        }

        private void OnAbility1Performed(InputAction.CallbackContext context)
        {
            ability1Pressed = true;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            interactPressed = true;
        }
    }
}
