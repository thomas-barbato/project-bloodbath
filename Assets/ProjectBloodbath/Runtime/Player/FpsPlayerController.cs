using ProjectBloodbath.Input;
using ProjectBloodbath.Settings;
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
        [SerializeField] private FirstPersonBodyPresentation bodyPresentation;

        private CharacterController characterController;
        private PlayerInputReader inputReader;
        private ControlSettingsManager controlSettings;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float pitch;
        private float standingHeight;
        private Vector3 standingCenter;
        private Vector3 cameraRestLocalPosition;
        private Vector3 slideDirection;
        private float slideEndsAt;
        private float nextSlideTime;
        private float slideRequestedUntil;
        private float slidePresentationAmount;
        private int jumpsPerformed;
        private bool wasGrounded;

        public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;
        public bool IsSliding { get; private set; }
        public float SlidePresentationAmount => slidePresentationAmount;
        public int JumpsPerformed => jumpsPerformed;
        public int RemainingJumps => settings == null
            ? 0
            : Mathf.Max(0, settings.MaximumJumpCount - jumpsPerformed);
        public float CurrentFieldOfView => playerCamera != null
            ? playerCamera.fieldOfView
            : settings != null
                ? settings.FieldOfView
                : 95f;

        public void Configure(
            Transform pivot,
            Camera cameraComponent,
            FpsControllerSettings controllerSettings)
        {
            cameraPivot = pivot;
            playerCamera = cameraComponent;
            settings = controllerSettings;
            bodyPresentation ??=
                GetComponentInChildren<FirstPersonBodyPresentation>(true);
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

        public void SetFieldOfView(float fieldOfView)
        {
            if (playerCamera == null)
            {
                return;
            }

            playerCamera.fieldOfView = Mathf.Clamp(fieldOfView, 1f, 179f);
        }

        public void ResetForRespawn()
        {
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            jumpsPerformed = 0;
            wasGrounded = false;
            pitch = 0f;
            inputReader?.ConsumeJumpPressed();
            inputReader?.ConsumeSlidePressed();
            slideRequestedUntil = 0f;
            StopSlide(true);

            if (cameraPivot != null)
            {
                cameraPivot.localPosition = cameraRestLocalPosition;
                cameraPivot.localRotation = Quaternion.identity;
            }

        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputReader = GetComponent<PlayerInputReader>();
            controlSettings = GetComponent<ControlSettingsManager>();
            bodyPresentation ??=
                GetComponentInChildren<FirstPersonBodyPresentation>(true);
            standingHeight = characterController.height;
            standingCenter = characterController.center;
            if (cameraPivot != null)
            {
                cameraRestLocalPosition = cameraPivot.localPosition;
            }
            ApplyCameraSettings();
        }

        private void OnEnable()
        {
            SetCursorCaptured(true);
        }

        private void OnDisable()
        {
            StopSlide(true);
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
            UpdateSlidePresentation();
        }

        private void UpdateCursorCapture()
        {
            if (inputReader.GameplaySuppressed)
            {
                SetCursorCaptured(false);
                return;
            }

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
            bool pointerInput = inputReader.LookUsesPointerDelta;
            float sensitivity = controlSettings != null
                ? controlSettings.GetLookSensitivity(pointerInput)
                : pointerInput
                    ? settings.MouseSensitivity
                    : settings.GamepadLookSpeed;
            float scale = pointerInput
                ? sensitivity
                : sensitivity * Time.deltaTime;
            float verticalMultiplier = controlSettings != null
                ? controlSettings.GetVerticalLookMultiplier(pointerInput)
                : 1f;

            transform.Rotate(Vector3.up, look.x * scale, Space.Self);
            pitch = Mathf.Clamp(
                pitch - look.y * scale * verticalMultiplier,
                settings.MinimumPitch,
                settings.MaximumPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateMovement()
        {
            bool grounded = characterController.isGrounded;
            bool hasStableGroundContact = grounded && verticalVelocity <= 0f;
            if (hasStableGroundContact)
            {
                jumpsPerformed = 0;
                verticalVelocity = settings.GroundedVerticalSpeed;
            }
            else if (!grounded && wasGrounded && jumpsPerformed == 0)
            {
                jumpsPerformed = 1;
            }
            wasGrounded = grounded;

            Vector2 moveInput = Vector2.ClampMagnitude(inputReader.Move, 1f);
            Vector3 desiredDirection =
                transform.right * moveInput.x + transform.forward * moveInput.y;

            if (!IsSliding && inputReader.ConsumeSlidePressed())
            {
                slideRequestedUntil =
                    Time.time + settings.SlideInputBufferTime;
            }

            if (
                !IsSliding &&
                slideRequestedUntil > 0f &&
                Time.time <= slideRequestedUntil &&
                grounded &&
                Time.time >= nextSlideTime &&
                horizontalVelocity.magnitude >= settings.SlideMinimumSpeed)
            {
                slideRequestedUntil = 0f;
                StartSlide(desiredDirection);
            }
            else if (slideRequestedUntil > 0f && Time.time > slideRequestedUntil)
            {
                slideRequestedUntil = 0f;
            }

            if (IsSliding)
            {
                UpdateSlideMovement(desiredDirection, grounded);
                return;
            }

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

            if (inputReader.ConsumeJumpPressed())
            {
                TryJump(hasStableGroundContact);
            }

            verticalVelocity += settings.Gravity * Time.deltaTime;
            characterController.Move(Velocity * Time.deltaTime);
        }

        private void StartSlide(Vector3 desiredDirection)
        {
            IsSliding = true;
            slideEndsAt = Time.time + settings.SlideDuration;
            slideDirection = horizontalVelocity.sqrMagnitude > 0.01f
                ? horizontalVelocity.normalized
                : desiredDirection.normalized;
            float startingSpeed = Mathf.Max(
                horizontalVelocity.magnitude,
                settings.SlideInitialSpeed);
            horizontalVelocity = slideDirection * startingSpeed;
        }

        private void UpdateSlideMovement(
            Vector3 desiredDirection,
            bool grounded)
        {
            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                slideDirection = Vector3.RotateTowards(
                    slideDirection,
                    desiredDirection.normalized,
                    settings.SlideSteeringSpeed * Mathf.Deg2Rad *
                        Time.deltaTime,
                    0f).normalized;
            }

            float slideSpeed = Mathf.MoveTowards(
                horizontalVelocity.magnitude,
                0f,
                settings.SlideDeceleration * Time.deltaTime);
            horizontalVelocity = slideDirection * slideSpeed;

            bool jumpRequested = inputReader.ConsumeJumpPressed();
            bool hasStableGroundContact = grounded && verticalVelocity <= 0f;
            if (jumpRequested && TryJump(hasStableGroundContact))
            {
                StopSlide(false);
            }
            else if (
                !grounded ||
                Time.time >= slideEndsAt ||
                slideSpeed < settings.SlideMinimumSpeed * 0.65f)
            {
                StopSlide(false);
            }

            verticalVelocity += settings.Gravity * Time.deltaTime;
            characterController.Move(Velocity * Time.deltaTime);
        }

        private bool TryJump(bool hasStableGroundContact)
        {
            if (
                !hasStableGroundContact &&
                jumpsPerformed >= settings.MaximumJumpCount)
            {
                return false;
            }

            float jumpHeight = hasStableGroundContact
                ? settings.JumpHeight
                : settings.JumpHeight * settings.AirJumpHeightMultiplier;
            if (jumpHeight <= 0f)
            {
                return false;
            }

            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * settings.Gravity);
            jumpsPerformed++;
            return true;
        }

        private void StopSlide(bool immediate)
        {
            if (IsSliding)
            {
                nextSlideTime = Time.time + settings.SlideCooldown;
            }

            IsSliding = false;
            if (!immediate)
            {
                return;
            }

            slidePresentationAmount = 0f;
            if (characterController != null)
            {
                characterController.height = standingHeight;
                characterController.center = standingCenter;
            }

            if (cameraPivot != null)
            {
                cameraPivot.localPosition = cameraRestLocalPosition;
            }

            bodyPresentation?.SetSlideAmount(0f);
        }

        private void UpdateSlidePresentation()
        {
            float targetAmount = IsSliding ? 1f : 0f;
            slidePresentationAmount = Mathf.MoveTowards(
                slidePresentationAmount,
                targetAmount,
                settings.SlideTransitionSpeed * Time.deltaTime);
            float easedPresentation = Mathf.SmoothStep(
                0f,
                1f,
                slidePresentationAmount);

            float height = Mathf.Lerp(
                standingHeight,
                Mathf.Min(standingHeight, settings.SlideHeight),
                easedPresentation);
            float standingBottom = standingCenter.y - standingHeight * 0.5f;
            characterController.height = height;
            characterController.center = new Vector3(
                standingCenter.x,
                standingBottom + height * 0.5f,
                standingCenter.z);

            if (cameraPivot != null)
            {
                cameraPivot.localPosition = cameraRestLocalPosition +
                    Vector3.down *
                    (settings.SlideCameraDrop * easedPresentation);
            }

            bodyPresentation?.SetSlideAmount(easedPresentation);
        }

        private void ApplyCameraSettings()
        {
            if (playerCamera != null && settings != null)
            {
                SetFieldOfView(settings.FieldOfView);
            }
        }

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
