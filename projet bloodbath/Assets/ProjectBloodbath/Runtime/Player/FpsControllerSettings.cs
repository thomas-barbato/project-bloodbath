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
        [SerializeField] private float gravity = -30f;
        [SerializeField] private float groundedVerticalSpeed = -3f;

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
        public float Gravity => gravity;
        public float GroundedVerticalSpeed => groundedVerticalSpeed;
        public float MouseSensitivity => mouseSensitivity;
        public float GamepadLookSpeed => gamepadLookSpeed;
        public float MinimumPitch => minimumPitch;
        public float MaximumPitch => maximumPitch;
        public float FieldOfView => fieldOfView;

        private void OnValidate()
        {
            sprintSpeed = Mathf.Max(sprintSpeed, walkSpeed);
            gravity = Mathf.Min(gravity, -0.01f);
            groundedVerticalSpeed = Mathf.Min(groundedVerticalSpeed, -0.01f);
            maximumPitch = Mathf.Max(maximumPitch, minimumPitch);
        }
    }
}
