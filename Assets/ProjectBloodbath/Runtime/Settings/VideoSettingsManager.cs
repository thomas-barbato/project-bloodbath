using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Settings
{
    public enum VideoDisplayMode
    {
        ExclusiveFullscreen,
        Borderless,
        Windowed
    }

    public readonly struct VideoResolutionOption :
        IEquatable<VideoResolutionOption>
    {
        public VideoResolutionOption(int width, int height, int refreshRate)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            RefreshRate = Mathf.Max(1, refreshRate);
        }

        public int Width { get; }
        public int Height { get; }
        public int RefreshRate { get; }
        public string Label =>
            $"{Width} × {Height}  •  {RefreshRate} HZ";

        public bool Equals(VideoResolutionOption other)
        {
            return Width == other.Width &&
                Height == other.Height &&
                RefreshRate == other.RefreshRate;
        }

        public override bool Equals(object obj)
        {
            return obj is VideoResolutionOption other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height, RefreshRate);
        }
    }

    [DefaultExecutionOrder(-900)]
    [DisallowMultipleComponent]
    public sealed class VideoSettingsManager : MonoBehaviour
    {
        public const string WidthPreferenceKey =
            "project_bloodbath.video.width";
        public const string HeightPreferenceKey =
            "project_bloodbath.video.height";
        public const string RefreshRatePreferenceKey =
            "project_bloodbath.video.refresh_rate";
        public const string DisplayModePreferenceKey =
            "project_bloodbath.video.display_mode";
        public const string VSyncPreferenceKey =
            "project_bloodbath.video.vsync";

        [SerializeField] private bool loadSavedSettings = true;
        [SerializeField] private bool applySettingsOnAwake = true;
        [SerializeField] private bool persistAppliedSettings = true;

        private readonly List<VideoResolutionOption> availableResolutions =
            new();
        private int pendingResolutionIndex;
        private int appliedResolutionIndex;
        private VideoDisplayMode pendingDisplayMode;
        private VideoDisplayMode appliedDisplayMode;
        private bool pendingVSync;
        private bool appliedVSync;

        public IReadOnlyList<VideoResolutionOption> AvailableResolutions =>
            availableResolutions;
        public int PendingResolutionIndex => pendingResolutionIndex;
        public int AppliedResolutionIndex => appliedResolutionIndex;
        public VideoDisplayMode PendingDisplayMode => pendingDisplayMode;
        public VideoDisplayMode AppliedDisplayMode => appliedDisplayMode;
        public bool PendingVSync => pendingVSync;
        public bool AppliedVSync => appliedVSync;
        public bool HasPendingChanges =>
            pendingResolutionIndex != appliedResolutionIndex ||
            pendingDisplayMode != appliedDisplayMode ||
            pendingVSync != appliedVSync;
        public VideoResolutionOption PendingResolution =>
            availableResolutions.Count == 0
                ? new VideoResolutionOption(
                    Screen.width,
                    Screen.height,
                    GetCurrentRefreshRate())
                : availableResolutions[Mathf.Clamp(
                    pendingResolutionIndex,
                    0,
                    availableResolutions.Count - 1)];

        private void Awake()
        {
            RefreshAvailableResolutions();
            CaptureCurrentSettings();
            bool hasSavedSettings =
                loadSavedSettings && HasCompleteSavedSettings();
            if (hasSavedSettings)
            {
                LoadSavedSettings();
            }

            if (applySettingsOnAwake && hasSavedSettings)
            {
                ApplyToDisplay();
            }
        }

        public void RefreshAvailableResolutions()
        {
            availableResolutions.Clear();
            Resolution[] resolutions = Screen.resolutions;
            for (int index = 0; index < resolutions.Length; index++)
            {
                Resolution resolution = resolutions[index];
                AddResolutionIfMissing(new VideoResolutionOption(
                    resolution.width,
                    resolution.height,
                    GetRoundedRefreshRate(resolution.refreshRateRatio)));
            }

            if (availableResolutions.Count == 0)
            {
                AddResolutionIfMissing(new VideoResolutionOption(
                    Screen.width,
                    Screen.height,
                    GetCurrentRefreshRate()));
            }
            availableResolutions.Sort(CompareResolutions);
        }

        public void BeginEditing()
        {
            pendingResolutionIndex = appliedResolutionIndex;
            pendingDisplayMode = appliedDisplayMode;
            pendingVSync = appliedVSync;
        }

        public void CycleResolution(int direction)
        {
            if (availableResolutions.Count == 0 || direction == 0)
            {
                return;
            }

            pendingResolutionIndex = WrapIndex(
                pendingResolutionIndex + Math.Sign(direction),
                availableResolutions.Count);
        }

        public void CycleDisplayMode(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            int modeCount = Enum.GetValues(typeof(VideoDisplayMode)).Length;
            pendingDisplayMode = (VideoDisplayMode)WrapIndex(
                (int)pendingDisplayMode + Math.Sign(direction),
                modeCount);
        }

        public void ToggleVSync()
        {
            pendingVSync = !pendingVSync;
        }

        public void ApplyPending()
        {
            appliedResolutionIndex = pendingResolutionIndex;
            appliedDisplayMode = pendingDisplayMode;
            appliedVSync = pendingVSync;
            ApplyToDisplay();
            if (persistAppliedSettings)
            {
                SaveAppliedSettings();
            }
        }

        public void CancelPending()
        {
            BeginEditing();
        }

        public string GetPendingDisplayModeLabel()
        {
            return pendingDisplayMode switch
            {
                VideoDisplayMode.ExclusiveFullscreen =>
                    "PLEIN ÉCRAN EXCLUSIF",
                VideoDisplayMode.Borderless =>
                    "PLEIN ÉCRAN FENÊTRÉ",
                VideoDisplayMode.Windowed => "FENÊTRÉ",
                _ => "FENÊTRÉ"
            };
        }

        public static FullScreenMode ToUnityFullScreenMode(
            VideoDisplayMode displayMode)
        {
            return displayMode switch
            {
                VideoDisplayMode.ExclusiveFullscreen =>
                    FullScreenMode.ExclusiveFullScreen,
                VideoDisplayMode.Borderless =>
                    FullScreenMode.FullScreenWindow,
                _ => FullScreenMode.Windowed
            };
        }

        private void CaptureCurrentSettings()
        {
            appliedResolutionIndex = FindBestResolutionIndex(
                Screen.width,
                Screen.height,
                GetCurrentRefreshRate());
            appliedDisplayMode = FromUnityFullScreenMode(
                Screen.fullScreenMode);
            appliedVSync = QualitySettings.vSyncCount > 0;
            BeginEditing();
        }

        private void LoadSavedSettings()
        {
            pendingResolutionIndex = FindBestResolutionIndex(
                PlayerPrefs.GetInt(
                    WidthPreferenceKey,
                    PendingResolution.Width),
                PlayerPrefs.GetInt(
                    HeightPreferenceKey,
                    PendingResolution.Height),
                PlayerPrefs.GetInt(
                    RefreshRatePreferenceKey,
                    PendingResolution.RefreshRate));

            pendingDisplayMode = (VideoDisplayMode)Mathf.Clamp(
                PlayerPrefs.GetInt(
                    DisplayModePreferenceKey,
                    (int)appliedDisplayMode),
                0,
                Enum.GetValues(typeof(VideoDisplayMode)).Length - 1);
            pendingVSync = PlayerPrefs.GetInt(
                VSyncPreferenceKey,
                appliedVSync ? 1 : 0) != 0;

            appliedResolutionIndex = pendingResolutionIndex;
            appliedDisplayMode = pendingDisplayMode;
            appliedVSync = pendingVSync;
        }

        private static bool HasCompleteSavedSettings()
        {
            return PlayerPrefs.HasKey(WidthPreferenceKey) &&
                PlayerPrefs.HasKey(HeightPreferenceKey) &&
                PlayerPrefs.HasKey(RefreshRatePreferenceKey) &&
                PlayerPrefs.HasKey(DisplayModePreferenceKey) &&
                PlayerPrefs.HasKey(VSyncPreferenceKey);
        }

        private void ApplyToDisplay()
        {
            VideoResolutionOption resolution = PendingResolution;
            QualitySettings.vSyncCount = pendingVSync ? 1 : 0;
            Screen.SetResolution(
                resolution.Width,
                resolution.Height,
                ToUnityFullScreenMode(pendingDisplayMode),
                resolution.RefreshRate);
        }

        private void SaveAppliedSettings()
        {
            VideoResolutionOption resolution = PendingResolution;
            PlayerPrefs.SetInt(WidthPreferenceKey, resolution.Width);
            PlayerPrefs.SetInt(HeightPreferenceKey, resolution.Height);
            PlayerPrefs.SetInt(
                RefreshRatePreferenceKey,
                resolution.RefreshRate);
            PlayerPrefs.SetInt(
                DisplayModePreferenceKey,
                (int)pendingDisplayMode);
            PlayerPrefs.SetInt(VSyncPreferenceKey, pendingVSync ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void AddResolutionIfMissing(VideoResolutionOption option)
        {
            if (!availableResolutions.Contains(option))
            {
                availableResolutions.Add(option);
            }
        }

        private int FindBestResolutionIndex(
            int width,
            int height,
            int refreshRate)
        {
            int bestIndex = 0;
            long bestScore = long.MaxValue;
            for (int index = 0; index < availableResolutions.Count; index++)
            {
                VideoResolutionOption option = availableResolutions[index];
                long sizeDifference =
                    Math.Abs((long)option.Width - width) +
                    Math.Abs((long)option.Height - height);
                long refreshDifference = Math.Abs(
                    (long)option.RefreshRate - refreshRate);
                long score = sizeDifference * 1000L + refreshDifference;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private static int CompareResolutions(
            VideoResolutionOption left,
            VideoResolutionOption right)
        {
            int pixelComparison =
                (left.Width * left.Height).CompareTo(
                    right.Width * right.Height);
            if (pixelComparison != 0)
            {
                return pixelComparison;
            }

            int widthComparison = left.Width.CompareTo(right.Width);
            return widthComparison != 0
                ? widthComparison
                : left.RefreshRate.CompareTo(right.RefreshRate);
        }

        private static int WrapIndex(int value, int count)
        {
            return (value % count + count) % count;
        }

        private static VideoDisplayMode FromUnityFullScreenMode(
            FullScreenMode mode)
        {
            return mode switch
            {
                FullScreenMode.ExclusiveFullScreen =>
                    VideoDisplayMode.ExclusiveFullscreen,
                FullScreenMode.Windowed => VideoDisplayMode.Windowed,
                _ => VideoDisplayMode.Borderless
            };
        }

        private static int GetCurrentRefreshRate()
        {
            return GetRoundedRefreshRate(
                Screen.currentResolution.refreshRateRatio);
        }

        private static int GetRoundedRefreshRate(RefreshRate refreshRate)
        {
            double value = refreshRate.value;
            return double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value <= 0d
                ? 60
                : Mathf.Max(1, Mathf.RoundToInt((float)value));
        }
    }
}
