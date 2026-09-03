using UnityEngine;

namespace ProjectBloodbath.Player
{
    [CreateAssetMenu(
        fileName = "FpsControllerSettings",
        menuName = "Project Bloodbath/Player/FPS Controller Settings")]
    public sealed class FpsControllerSettings : ScriptableObject
    {
        [Header("Déplacement")]
        [SerializeField, Min(0f)] private float walkSpeed = 7.5f;
        [SerializeField, Min(0f)] private float sprintSpeed = 10.5f;
        [SerializeField, Min(0f)] private float groundAcceleration = 65f;
        [SerializeField, Min(0f)] private float airAcceleration = 20f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.35f;
        [SerializeField, Min(1)] private int maximumJumpCount = 2;
        [SerializeField, Min(0f)] private float airJumpHeightMultiplier = 1f;
        [SerializeField] private float gravity = -30f;
        [SerializeField] private float groundedVerticalSpeed = -3f;

        [Header("Glissade")]
        [SerializeField, Min(0f)] private float slideMinimumSpeed = 6.8f;
        [SerializeField, Min(0f)] private float slideInitialSpeed = 12.5f;
        [SerializeField, Min(0.05f)] private float slideDuration = 0.75f;
        [SerializeField, Min(0f)] private float slideDeceleration = 7.5f;
        [SerializeField, Min(0f)] private float slideSteeringSpeed = 105f;
        [SerializeField, Min(0f)] private float slideCooldown = 0.35f;
        [SerializeField, Min(0f)] private float slideInputBufferTime = 0.2f;
        [SerializeField, Min(0.5f)] private float slideHeight = 0.95f;
        [SerializeField, Min(0f)] private float slideCameraDrop = 0.72f;
        [SerializeField, Min(0.1f)] private float slideTransitionSpeed = 12f;

        [Header("Caméra")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.1f;
        [SerializeField, Min(0f)] private float gamepadLookSpeed = 180f;
        [SerializeField, Range(-89f, 0f)] private float minimumPitch = -85f;
        [SerializeField, Range(0f, 89f)] private float maximumPitch = 85f;
        [SerializeField, Range(60f, 130f)] private float fieldOfView = 95f;

        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float GroundAcceleration => groundAcceleration;
        public float AirAcceleration => airAcceleration;
        public float JumpHeight => jumpHeight;
        public int MaximumJumpCount => maximumJumpCount;
        public float AirJumpHeightMultiplier => airJumpHeightMultiplier;
        public float Gravity => gravity;
        public float GroundedVerticalSpeed => groundedVerticalSpeed;
        public float SlideMinimumSpeed => slideMinimumSpeed;
        public float SlideInitialSpeed => slideInitialSpeed;
        public float SlideDuration => slideDuration;
        public float SlideDeceleration => slideDeceleration;
        public float SlideSteeringSpeed => slideSteeringSpeed;
        public float SlideCooldown => slideCooldown;
        public float SlideInputBufferTime => slideInputBufferTime;
        public float SlideHeight => slideHeight;
        public float SlideCameraDrop => slideCameraDrop;
        public float SlideTransitionSpeed => slideTransitionSpeed;
        public float MouseSensitivity => mouseSensitivity;
        public float GamepadLookSpeed => gamepadLookSpeed;
        public float MinimumPitch => minimumPitch;
        public float MaximumPitch => maximumPitch;
        public float FieldOfView => fieldOfView;

        private void OnValidate()
        {
            sprintSpeed = Mathf.Max(sprintSpeed, walkSpeed);
            maximumJumpCount = Mathf.Max(1, maximumJumpCount);
            airJumpHeightMultiplier = Mathf.Max(0f, airJumpHeightMultiplier);
            gravity = Mathf.Min(gravity, -0.01f);
            groundedVerticalSpeed = Mathf.Min(groundedVerticalSpeed, -0.01f);
            slideMinimumSpeed = Mathf.Max(0f, slideMinimumSpeed);
            slideInitialSpeed = Mathf.Max(slideMinimumSpeed, slideInitialSpeed);
            slideDuration = Mathf.Max(0.05f, slideDuration);
            slideDeceleration = Mathf.Max(0f, slideDeceleration);
            slideSteeringSpeed = Mathf.Max(0f, slideSteeringSpeed);
            slideCooldown = Mathf.Max(0f, slideCooldown);
            slideInputBufferTime = Mathf.Max(0f, slideInputBufferTime);
            slideHeight = Mathf.Max(0.5f, slideHeight);
            slideCameraDrop = Mathf.Max(0f, slideCameraDrop);
            slideTransitionSpeed = Mathf.Max(0.1f, slideTransitionSpeed);
            maximumPitch = Mathf.Max(maximumPitch, minimumPitch);
        }
    }
}
