using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectBloodbath.Input
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string uiActionMapName = "UI";

        private InputActionMap playerMap;
        private InputActionMap uiMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction useLeftHandAction;
        private InputAction reloadAction;
        private InputAction ability1Action;
        private InputAction interactAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction slideAction;
        private InputAction swapHandSetAction;
        private InputAction inventoryAction;
        private InputAction questJournalAction;
        private InputAction worldMapAction;
        private InputAction optionsAction;
        private InputAction menuNavigateAction;
        private InputAction menuSubmitAction;
        private InputAction menuCancelAction;
        private bool jumpPressed;
        private bool slidePressed;
        private bool reloadPressed;
        private bool ability1Pressed;
        private bool interactPressed;
        private bool inventoryPressed;
        private bool questJournalPressed;
        private bool worldMapPressed;
        private bool optionsPressed;
        private bool menuSubmitPressed;
        private bool menuCancelPressed;
        private Vector2 menuNavigatePressed;

        public bool GameplaySuppressed { get; private set; }
        public Vector2 Move => GameplaySuppressed
            ? Vector2.zero
            : moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 Look => GameplaySuppressed
            ? Vector2.zero
            : lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool AttackHeld => !GameplaySuppressed &&
            (attackAction?.IsPressed() ?? false);
        public bool AttackPressedThisFrame => !GameplaySuppressed &&
            (attackAction?.WasPressedThisFrame() ?? false);
        public bool RightHandHeld => AttackHeld;
        public bool RightHandPressedThisFrame => AttackPressedThisFrame;
        public bool LeftHandHeld => !GameplaySuppressed &&
            (useLeftHandAction?.IsPressed() ?? false);
        public bool LeftHandPressedThisFrame => !GameplaySuppressed &&
            (useLeftHandAction?.WasPressedThisFrame() ?? false);
        public bool SprintHeld => !GameplaySuppressed &&
            (sprintAction?.IsPressed() ?? false);
        public bool SwapHandSetPressedThisFrame =>
            !GameplaySuppressed &&
            (swapHandSetAction?.WasPressedThisFrame() ?? false);
        public bool LookUsesPointerDelta => lookAction?.activeControl?.device is Pointer;
        public InputActionAsset InputActions => inputActions;

        public void Configure(InputActionAsset actions)
        {
            inputActions = actions;
            CacheActions();
        }

        public bool ConsumeJumpPressed()
        {
            bool value = !GameplaySuppressed && jumpPressed;
            jumpPressed = false;
            return value;
        }

        public bool ConsumeSlidePressed()
        {
            bool value = !GameplaySuppressed && slidePressed;
            slidePressed = false;
            return value;
        }

        public bool ConsumeReloadPressed()
        {
            bool value = !GameplaySuppressed && reloadPressed;
            reloadPressed = false;
            return value;
        }

        public bool ConsumeAbility1Pressed()
        {
            bool value = !GameplaySuppressed && ability1Pressed;
            ability1Pressed = false;
            return value;
        }

        public bool ConsumeInteractPressed()
        {
            bool value = !GameplaySuppressed && interactPressed;
            interactPressed = false;
            return value;
        }

        public bool ConsumeInterfaceInteractPressed()
        {
            bool value = interactPressed;
            interactPressed = false;
            return value;
        }

        public bool ConsumeInterfaceSwapHandSetPressed()
        {
            return swapHandSetAction?.WasPressedThisFrame() ?? false;
        }

        public bool ConsumeInventoryPressed()
        {
            bool value = inventoryPressed ||
                (inventoryAction?.WasPressedThisFrame() ?? false);
            inventoryPressed = false;
            return value;
        }

        public bool ConsumeQuestJournalPressed()
        {
            bool value = questJournalPressed ||
                (questJournalAction?.WasPressedThisFrame() ?? false);
            questJournalPressed = false;
            return value;
        }

        public bool ConsumeWorldMapPressed()
        {
            bool value = worldMapPressed ||
                (worldMapAction?.WasPressedThisFrame() ?? false);
            worldMapPressed = false;
            return value;
        }

        public bool ConsumeOptionsPressed()
        {
            bool value = !GameplaySuppressed &&
                (optionsPressed ||
                 (optionsAction?.WasPressedThisFrame() ?? false));
            optionsPressed = false;
            return value;
        }

        public Vector2 ConsumeMenuNavigatePressed()
        {
            Vector2 value = menuNavigatePressed;
            menuNavigatePressed = Vector2.zero;
            return value;
        }

        public bool ConsumeMenuSubmitPressed()
        {
            bool value = menuSubmitPressed;
            menuSubmitPressed = false;
            return value;
        }

        public bool ConsumeMenuCancelPressed()
        {
            bool value = menuCancelPressed;
            menuCancelPressed = false;
            return value;
        }

        public void SetGameplaySuppressed(bool suppressed)
        {
            GameplaySuppressed = suppressed;
            if (!suppressed)
            {
                uiMap?.Disable();
                return;
            }

            uiMap?.Enable();
            jumpPressed = false;
            slidePressed = false;
            reloadPressed = false;
            ability1Pressed = false;
            interactPressed = false;
            optionsPressed = false;
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
            inventoryAction.performed += OnInventoryPerformed;
            questJournalAction.performed += OnQuestJournalPerformed;
            worldMapAction.performed += OnWorldMapPerformed;
            optionsAction.performed += OnOptionsPerformed;
            menuNavigateAction.performed += OnMenuNavigatePerformed;
            menuSubmitAction.performed += OnMenuSubmitPerformed;
            menuCancelAction.performed += OnMenuCancelPerformed;
            playerMap.Enable();
            uiMap.Disable();
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
            if (inventoryAction != null)
            {
                inventoryAction.performed -= OnInventoryPerformed;
            }
            if (questJournalAction != null)
            {
                questJournalAction.performed -= OnQuestJournalPerformed;
            }
            if (worldMapAction != null)
            {
                worldMapAction.performed -= OnWorldMapPerformed;
            }
            if (optionsAction != null)
            {
                optionsAction.performed -= OnOptionsPerformed;
            }
            if (menuNavigateAction != null)
            {
                menuNavigateAction.performed -= OnMenuNavigatePerformed;
            }
            if (menuSubmitAction != null)
            {
                menuSubmitAction.performed -= OnMenuSubmitPerformed;
            }
            if (menuCancelAction != null)
            {
                menuCancelAction.performed -= OnMenuCancelPerformed;
            }

            playerMap?.Disable();
            uiMap?.Disable();
            jumpPressed = false;
            slidePressed = false;
            reloadPressed = false;
            ability1Pressed = false;
            interactPressed = false;
            inventoryPressed = false;
            questJournalPressed = false;
            worldMapPressed = false;
            optionsPressed = false;
            menuSubmitPressed = false;
            menuCancelPressed = false;
            menuNavigatePressed = Vector2.zero;
            GameplaySuppressed = false;
        }

        private void CacheActions()
        {
            if (inputActions == null)
            {
                Debug.LogError("Aucun InputActionAsset n'est configuré.", this);
                playerMap = null;
                uiMap = null;
                return;
            }

            playerMap = inputActions.FindActionMap(actionMapName, true);
            uiMap = inputActions.FindActionMap(uiActionMapName, true);
            moveAction = playerMap.FindAction("Move", true);
            lookAction = playerMap.FindAction("Look", true);
            attackAction = playerMap.FindAction("Attack", true);
            useLeftHandAction = playerMap.FindAction("UseLeftHand", true);
            reloadAction = playerMap.FindAction("Reload", true);
            ability1Action = playerMap.FindAction("Ability1", true);
            interactAction = playerMap.FindAction("Interact", true);
            jumpAction = playerMap.FindAction("Jump", true);
            sprintAction = playerMap.FindAction("Sprint", true);
            slideAction = playerMap.FindAction("Slide", true);
            swapHandSetAction = playerMap.FindAction("SwapHandSet", true);
            inventoryAction = playerMap.FindAction("Inventory", true);
            questJournalAction = playerMap.FindAction("QuestJournal", true);
            worldMapAction = playerMap.FindAction("WorldMap", true);
            optionsAction = playerMap.FindAction("Options", true);
            menuNavigateAction = uiMap.FindAction("Navigate", true);
            menuSubmitAction = uiMap.FindAction("Submit", true);
            menuCancelAction = uiMap.FindAction("Cancel", true);
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

        private void OnInventoryPerformed(InputAction.CallbackContext context)
        {
            inventoryPressed = true;
        }

        private void OnQuestJournalPerformed(
            InputAction.CallbackContext context)
        {
            questJournalPressed = true;
        }

        private void OnWorldMapPerformed(InputAction.CallbackContext context)
        {
            worldMapPressed = true;
        }

        private void OnOptionsPerformed(InputAction.CallbackContext context)
        {
            optionsPressed = true;
        }

        private void OnMenuNavigatePerformed(InputAction.CallbackContext context)
        {
            menuNavigatePressed = context.ReadValue<Vector2>();
        }

        private void OnMenuSubmitPerformed(InputAction.CallbackContext context)
        {
            menuSubmitPressed = true;
        }

        private void OnMenuCancelPerformed(InputAction.CallbackContext context)
        {
            menuCancelPressed = true;
        }
    }
}
