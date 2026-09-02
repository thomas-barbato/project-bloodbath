using UnityEngine;

namespace ProjectBloodbath.Enemies
{
    public enum EnemyMovementStyle
    {
        Pursuer,
        MaintainDistance,
        Stationary
    }

    public enum EnemyBehaviorState
    {
        Idle,
        Pursuing,
        Repositioning,
        Combat,
        Returning
    }

    [CreateAssetMenu(
        fileName = "EnemyBehaviorProfile",
        menuName = "Project Bloodbath/Enemies/Behavior Profile")]
    public sealed class EnemyBehaviorProfile : ScriptableObject
    {
        [Header("Perception et territoire")]
        [SerializeField, Min(0.1f)] private float detectionRange = 16f;
        [SerializeField, Min(0.2f)] private float leashRange = 28f;

        [Header("Vision")]
        [SerializeField, Range(1f, 360f)] private float viewAngle = 140f;
        [SerializeField, Min(0f)] private float eyeHeight = 1.25f;
        [SerializeField, Min(0f)] private float targetHeight = 0.9f;
        [SerializeField] private bool requiresLineOfSight = true;
        [SerializeField] private LayerMask lineOfSightMask = ~0;

        [Header("Déplacement")]
        [SerializeField] private EnemyMovementStyle movementStyle =
            EnemyMovementStyle.Pursuer;
        [SerializeField, Min(0f)] private float movementSpeed = 4.5f;
        [SerializeField, Min(0f)] private float acceleration = 28f;
        [SerializeField, Min(0f)] private float angularSpeed = 720f;
        [SerializeField, Min(0f)] private float stoppingDistance = 1.35f;
        [SerializeField, Min(0.01f)] private float returnTolerance = 0.15f;

        [Header("Maintien de distance")]
        [SerializeField, Min(0f)] private float preferredMinimumDistance = 4.5f;
        [SerializeField, Min(0f)] private float preferredMaximumDistance = 8f;
        [SerializeField, Min(0.1f)] private float retreatStepDistance = 3f;

        public float DetectionRange => detectionRange;
        public float LeashRange => leashRange;
        public float ViewAngle => viewAngle;
        public float EyeHeight => eyeHeight;
        public float TargetHeight => targetHeight;
        public bool RequiresLineOfSight => requiresLineOfSight;
        public LayerMask LineOfSightMask => lineOfSightMask;
        public EnemyMovementStyle MovementStyle => movementStyle;
        public float MovementSpeed => movementSpeed;
        public float Acceleration => acceleration;
        public float AngularSpeed => angularSpeed;
        public float StoppingDistance => stoppingDistance;
        public float ReturnTolerance => returnTolerance;
        public float PreferredMinimumDistance => preferredMinimumDistance;
        public float PreferredMaximumDistance => preferredMaximumDistance;
        public float RetreatStepDistance => retreatStepDistance;

        private void OnValidate()
        {
            detectionRange = Mathf.Max(0.1f, detectionRange);
            leashRange = Mathf.Max(detectionRange + 0.1f, leashRange);
            viewAngle = Mathf.Clamp(viewAngle, 1f, 360f);
            eyeHeight = Mathf.Max(0f, eyeHeight);
            targetHeight = Mathf.Max(0f, targetHeight);
            movementSpeed = Mathf.Max(0f, movementSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            angularSpeed = Mathf.Max(0f, angularSpeed);
            stoppingDistance = Mathf.Max(0f, stoppingDistance);
            returnTolerance = Mathf.Max(0.01f, returnTolerance);
            preferredMinimumDistance = Mathf.Max(
                0f,
                preferredMinimumDistance);
            preferredMaximumDistance = Mathf.Max(
                preferredMinimumDistance,
                preferredMaximumDistance);
            retreatStepDistance = Mathf.Max(0.1f, retreatStepDistance);
        }
    }
}
