using ProjectBloodbath.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectBloodbath.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public sealed class FpsPlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private FpsControllerSettings settings;

        private CharacterController characterController;
        private PlayerInputReader inputReader;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float pitch;

        public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;

        public void Configure(
            Transform pivot,
            Camera cameraComponent,
            FpsControllerSettings controllerSettings)
        {
            cameraPivot = pivot;
            playerCamera = cameraComponent;
            settings = controllerSettings;
            ApplyCameraSettings();
        }

        public void AddLookImpulse(float pitchDegrees, float yawDegrees)
        {
            if (settings == null || cameraPivot == null)
            {
                return;
            }

            transform.Rotate(Vector3.up, yawDegrees, Space.Self);
            pitch = Mathf.Clamp(
                pitch - pitchDegrees,
                settings.MinimumPitch,
                settings.MaximumPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputReader = GetComponent<PlayerInputReader>();
            ApplyCameraSettings();
        }

        private void OnEnable()
        {
            SetCursorCaptured(true);
        }

        private void OnDisable()
        {
            SetCursorCaptured(false);
        }

        private void Update()
        {
            if (settings == null || cameraPivot == null)
            {
                return;
            }

            UpdateCursorCapture();
            UpdateLook();
            UpdateMovement();
        }

        private void UpdateCursorCapture()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                SetCursorCaptured(false);
            }
            else if (
                Cursor.lockState != CursorLockMode.Locked &&
                Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                SetCursorCaptured(true);
            }
        }

        private void UpdateLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2 look = inputReader.Look;
            float scale = inputReader.LookUsesPointerDelta
                ? settings.MouseSensitivity
                : settings.GamepadLookSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, look.x * scale, Space.Self);
            pitch = Mathf.Clamp(
                pitch - look.y * scale,
                settings.MinimumPitch,
                settings.MaximumPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateMovement()
        {
            bool grounded = characterController.isGrounded;
            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = settings.GroundedVerticalSpeed;
            }

            Vector2 moveInput = Vector2.ClampMagnitude(inputReader.Move, 1f);
            Vector3 desiredDirection =
                transform.right * moveInput.x + transform.forward * moveInput.y;
            float speed = inputReader.SprintHeld
                ? settings.SprintSpeed
                : settings.WalkSpeed;
            Vector3 desiredVelocity = desiredDirection * speed;
            float acceleration = grounded
                ? settings.GroundAcceleration
                : settings.AirAcceleration;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredVelocity,
                acceleration * Time.deltaTime);

            if (grounded && inputReader.ConsumeJumpPressed())
            {
                verticalVelocity = Mathf.Sqrt(
                    settings.JumpHeight * -2f * settings.Gravity);
            }

            verticalVelocity += settings.Gravity * Time.deltaTime;
            characterController.Move(Velocity * Time.deltaTime);
        }

        private void ApplyCameraSettings()
        {
            if (playerCamera != null && settings != null)
            {
                playerCamera.fieldOfView = settings.FieldOfView;
            }
        }

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
