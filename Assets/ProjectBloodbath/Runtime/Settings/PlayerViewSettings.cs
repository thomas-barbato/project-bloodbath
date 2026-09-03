using System;
using ProjectBloodbath.Player;
using UnityEngine;

namespace ProjectBloodbath.Settings
{
    public enum ReticleColorPreset
    {
        SpectralGreen,
        BloodRed,
        IndustrialAmber,
        White
    }

    [DefaultExecutionOrder(-850)]
    [DisallowMultipleComponent]
    public sealed class PlayerViewSettings : MonoBehaviour
    {
        private static readonly ReticleShape[] SelectableReticleShapes =
        {
            ReticleShape.Cross,
            ReticleShape.Dot,
            ReticleShape.XCross,
            ReticleShape.Circle,
            ReticleShape.Chevron
        };

        public const float MinimumFieldOfView = 60f;
        public const float MaximumFieldOfView = 120f;
        public const float FieldOfViewStep = 5f;
        public const float MinimumReticleSize = 0.5f;
        public const float MaximumReticleSize = 2f;
        public const float ReticleSizeStep = 0.25f;

        public const string FieldOfViewPreferenceKey =
            "project_bloodbath.view.field_of_view";
        public const string ReticleSizePreferenceKey =
            "project_bloodbath.view.reticle_size";
        public const string ReticleColorPreferenceKey =
            "project_bloodbath.view.reticle_color";
        public const string ReticleShapePreferenceKey =
            "project_bloodbath.view.reticle_shape";

        [SerializeField] private FpsPlayerController playerController;
        [SerializeField] private PrototypeReticle reticle;
        [SerializeField] private bool loadSavedSettings = true;
        [SerializeField] private bool persistAppliedSettings = true;

        private float pendingFieldOfView;
        private float appliedFieldOfView;
        private float pendingReticleSize;
        private float appliedReticleSize;
        private ReticleColorPreset pendingReticleColor;
        private ReticleColorPreset appliedReticleColor;
        private ReticleShape pendingReticleShape;
        private ReticleShape appliedReticleShape;

        public float PendingFieldOfView => pendingFieldOfView;
        public float AppliedFieldOfView => appliedFieldOfView;
        public float PendingReticleSize => pendingReticleSize;
        public float AppliedReticleSize => appliedReticleSize;
        public ReticleColorPreset PendingReticleColor => pendingReticleColor;
        public ReticleColorPreset AppliedReticleColor => appliedReticleColor;
        public ReticleShape PendingReticleShape => pendingReticleShape;
        public ReticleShape AppliedReticleShape => appliedReticleShape;
        public bool HasPendingChanges =>
            !Mathf.Approximately(
                pendingFieldOfView,
                appliedFieldOfView) ||
            !Mathf.Approximately(
                pendingReticleSize,
                appliedReticleSize) ||
            pendingReticleColor != appliedReticleColor ||
            pendingReticleShape != appliedReticleShape;

        private void Awake()
        {
            CacheReferences();
        }

        private void Start()
        {
            CaptureCurrentSettings();
            if (loadSavedSettings)
            {
                LoadSavedSettings();
            }
            ApplyToPlayer();
        }

        public void BeginEditing()
        {
            pendingFieldOfView = appliedFieldOfView;
            pendingReticleSize = appliedReticleSize;
            pendingReticleColor = appliedReticleColor;
            pendingReticleShape = appliedReticleShape;
        }

        public void ChangeFieldOfView(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            pendingFieldOfView = Mathf.Clamp(
                pendingFieldOfView +
                Math.Sign(direction) * FieldOfViewStep,
                MinimumFieldOfView,
                MaximumFieldOfView);
        }

        public void ChangeReticleSize(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            pendingReticleSize = Mathf.Clamp(
                pendingReticleSize +
                Math.Sign(direction) * ReticleSizeStep,
                MinimumReticleSize,
                MaximumReticleSize);
        }

        public void CycleReticleColor(int direction)
        {
            pendingReticleColor = (ReticleColorPreset)CycleEnum(
                (int)pendingReticleColor,
                direction,
                Enum.GetValues(typeof(ReticleColorPreset)).Length);
        }

        public void CycleReticleShape(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            int currentIndex = Array.IndexOf(
                SelectableReticleShapes,
                pendingReticleShape);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }
            int nextIndex = CycleEnum(
                currentIndex,
                direction,
                SelectableReticleShapes.Length);
            pendingReticleShape = SelectableReticleShapes[nextIndex];
        }

        public void ApplyPending()
        {
            appliedFieldOfView = pendingFieldOfView;
            appliedReticleSize = pendingReticleSize;
            appliedReticleColor = pendingReticleColor;
            appliedReticleShape = pendingReticleShape;
            ApplyToPlayer();
            if (persistAppliedSettings)
            {
                SaveAppliedSettings();
            }
        }

        public void CancelPending()
        {
            BeginEditing();
        }

        public static Color GetReticleColor(ReticleColorPreset preset)
        {
            return preset switch
            {
                ReticleColorPreset.BloodRed =>
                    new Color(0.95f, 0.14f, 0.08f, 0.95f),
                ReticleColorPreset.IndustrialAmber =>
                    new Color(1f, 0.62f, 0.12f, 0.95f),
                ReticleColorPreset.White =>
                    new Color(0.96f, 0.96f, 0.93f, 0.95f),
                _ => new Color(0.75f, 0.95f, 0.85f, 0.9f)
            };
        }

        private void CacheReferences()
        {
            playerController ??= GetComponent<FpsPlayerController>();
            reticle ??= GetComponentInChildren<PrototypeReticle>(true);
        }

        private void CaptureCurrentSettings()
        {
            appliedFieldOfView = Mathf.Clamp(
                playerController != null
                    ? playerController.CurrentFieldOfView
                    : 95f,
                MinimumFieldOfView,
                MaximumFieldOfView);
            appliedReticleSize = Mathf.Clamp(
                reticle != null ? reticle.SizeMultiplier : 1f,
                MinimumReticleSize,
                MaximumReticleSize);
            appliedReticleColor = FindNearestColorPreset(
                reticle != null
                    ? reticle.Color
                    : GetReticleColor(ReticleColorPreset.SpectralGreen));
            appliedReticleShape = reticle != null
                ? reticle.Shape
                : ReticleShape.Cross;
            BeginEditing();
        }

        private void LoadSavedSettings()
        {
            pendingFieldOfView = Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    FieldOfViewPreferenceKey,
                    appliedFieldOfView),
                MinimumFieldOfView,
                MaximumFieldOfView);
            pendingReticleSize = Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    ReticleSizePreferenceKey,
                    appliedReticleSize),
                MinimumReticleSize,
                MaximumReticleSize);
            pendingReticleColor = (ReticleColorPreset)Mathf.Clamp(
                PlayerPrefs.GetInt(
                    ReticleColorPreferenceKey,
                    (int)appliedReticleColor),
                0,
                Enum.GetValues(typeof(ReticleColorPreset)).Length - 1);
            pendingReticleShape = ReadSavedReticleShape(
                PlayerPrefs.GetInt(
                    ReticleShapePreferenceKey,
                    (int)appliedReticleShape));

            appliedFieldOfView = pendingFieldOfView;
            appliedReticleSize = pendingReticleSize;
            appliedReticleColor = pendingReticleColor;
            appliedReticleShape = pendingReticleShape;
        }

        private void ApplyToPlayer()
        {
            CacheReferences();
            playerController?.SetFieldOfView(pendingFieldOfView);
            reticle?.ConfigureAppearance(
                pendingReticleSize,
                GetReticleColor(pendingReticleColor),
                pendingReticleShape);
        }

        private void SaveAppliedSettings()
        {
            PlayerPrefs.SetFloat(
                FieldOfViewPreferenceKey,
                appliedFieldOfView);
            PlayerPrefs.SetFloat(
                ReticleSizePreferenceKey,
                appliedReticleSize);
            PlayerPrefs.SetInt(
                ReticleColorPreferenceKey,
                (int)appliedReticleColor);
            PlayerPrefs.SetInt(
                ReticleShapePreferenceKey,
                (int)appliedReticleShape);
            PlayerPrefs.Save();
        }

        private static int CycleEnum(int current, int direction, int count)
        {
            if (direction == 0)
            {
                return current;
            }

            int next = current + Math.Sign(direction);
            return (next % count + count) % count;
        }

        private static ReticleColorPreset FindNearestColorPreset(Color color)
        {
            ReticleColorPreset bestPreset = ReticleColorPreset.SpectralGreen;
            float bestDifference = float.MaxValue;
            foreach (ReticleColorPreset preset in
                     Enum.GetValues(typeof(ReticleColorPreset)))
            {
                Color candidate = GetReticleColor(preset);
                float difference =
                    Mathf.Abs(candidate.r - color.r) +
                    Mathf.Abs(candidate.g - color.g) +
                    Mathf.Abs(candidate.b - color.b);
                if (difference < bestDifference)
                {
                    bestDifference = difference;
                    bestPreset = preset;
                }
            }

            return bestPreset;
        }

        private static ReticleShape ReadSavedReticleShape(int savedValue)
        {
            for (int index = 0; index < SelectableReticleShapes.Length; index++)
            {
                if ((int)SelectableReticleShapes[index] == savedValue)
                {
                    return SelectableReticleShapes[index];
                }
            }

            return ReticleShape.Cross;
        }
    }
}
