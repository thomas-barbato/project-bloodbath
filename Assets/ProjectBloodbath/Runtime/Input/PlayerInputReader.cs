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
        private InputAction jumpAction;
        private InputAction sprintAction;
        private bool jumpPressed;

        public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 Look => lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool AttackHeld => attackAction?.IsPressed() ?? false;
        public bool AttackPressedThisFrame => attackAction?.WasPressedThisFrame() ?? false;
        public bool SprintHeld => sprintAction?.IsPressed() ?? false;
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

        private void OnEnable()
        {
            CacheActions();
            if (playerMap == null)
            {
                enabled = false;
                return;
            }

            jumpAction.performed += OnJumpPerformed;
            playerMap.Enable();
        }

        private void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.performed -= OnJumpPerformed;
            }

            playerMap?.Disable();
            jumpPressed = false;
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
            jumpAction = playerMap.FindAction("Jump", true);
            sprintAction = playerMap.FindAction("Sprint", true);
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpPressed = true;
        }
    }
}
