using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Input;
using ProjectBloodbath.Prototype;
using ProjectBloodbath.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeVideoSettingsPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Keyboard keyboard;
        private PlayerInputReader inputReader;
        private PrototypeSystemMenu systemMenu;
        private PrototypeVideoSettingsPanel panel;
        private VideoSettingsManager videoSettings;
        private PlayerViewSettings playerViewSettings;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                ScenePath,
                LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null);
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            GameObject player = GameObject.Find("Player");
            Assert.That(player, Is.Not.Null);
            inputReader = player.GetComponent<PlayerInputReader>();
            systemMenu = player.GetComponent<PrototypeSystemMenu>();
            panel = player.GetComponent<PrototypeVideoSettingsPanel>();
            videoSettings = player.GetComponent<VideoSettingsManager>();
            playerViewSettings = player.GetComponent<PlayerViewSettings>();
            Assert.That(inputReader, Is.Not.Null);
            Assert.That(systemMenu, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            Assert.That(videoSettings, Is.Not.Null);
            Assert.That(playerViewSettings, Is.Not.Null);

            player.GetComponent<ProjectBloodbath.Player.FpsPlayerController>()
                .enabled = false;
            foreach (PrototypeEnemyController enemy in
                     Object.FindObjectsByType<PrototypeEnemyController>(
                         FindObjectsSortMode.None))
            {
                enemy.enabled = false;
            }

            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            panel?.CancelAndClose();
            systemMenu?.SetOpen(false);
            if (keyboard != null && keyboard.added)
            {
                SetKeys();
                InputSystem.RemoveDevice(keyboard);
            }

            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator EscapeOpensSystemMenuAndClosesWithoutPausingWorld()
        {
            float timeScaleBeforeOpening = Time.timeScale;

            SetKeys(Key.Escape);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(systemMenu.IsOpen, Is.True);
            Assert.That(panel.IsOpen, Is.False);
            Assert.That(inputReader.GameplaySuppressed, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBeforeOpening));
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));

            SetKeys(Key.Escape);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(systemMenu.IsOpen, Is.False);
            Assert.That(inputReader.GameplaySuppressed, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBeforeOpening));
        }

        [UnityTest]
        public IEnumerator VideoSubmenuReturnsToSystemMenu()
        {
            systemMenu.SetOpen(true);
            systemMenu.MoveSelection(2);
            Assert.That(systemMenu.SelectedIndex, Is.EqualTo(2));
            Assert.That(
                PrototypeSystemMenu.GetEntryLabel(2),
                Is.EqualTo("VIDÉO"));
            Assert.That(
                PrototypeSystemMenu.GetEntryLabel(3),
                Is.EqualTo("SON"));
            Assert.That(
                PrototypeSystemMenu.GetEntryLabel(4),
                Is.EqualTo("CONTRÔLE"));

            systemMenu.ActivateSelected();
            Assert.That(systemMenu.IsOpen, Is.False);
            Assert.That(panel.IsOpen, Is.True);

            panel.CancelAndClose();
            Assert.That(panel.IsOpen, Is.False);
            Assert.That(systemMenu.IsOpen, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            yield break;
        }

        [UnityTest]
        public IEnumerator PendingChoicesCanBeCancelledWithoutApplyingThem()
        {
            Assert.That(
                videoSettings.AvailableResolutions.Count,
                Is.GreaterThan(0));
            VideoDisplayMode originalMode =
                videoSettings.AppliedDisplayMode;
            int originalResolution = videoSettings.AppliedResolutionIndex;
            bool originalVSync = videoSettings.AppliedVSync;
            float originalFieldOfView =
                playerViewSettings.AppliedFieldOfView;
            float originalReticleSize =
                playerViewSettings.AppliedReticleSize;
            ReticleColorPreset originalReticleColor =
                playerViewSettings.AppliedReticleColor;
            ProjectBloodbath.Player.ReticleShape originalReticleShape =
                playerViewSettings.AppliedReticleShape;

            panel.SetOpen(true);
            panel.AdjustSelected(1);
            Assert.That(
                videoSettings.PendingDisplayMode,
                Is.Not.EqualTo(originalMode));

            panel.MoveSelection(2);
            Assert.That(panel.SelectedIndex, Is.EqualTo(2));
            panel.AdjustSelected(1);
            Assert.That(videoSettings.PendingVSync, Is.Not.EqualTo(originalVSync));

            panel.MoveSelection(1);
            panel.AdjustSelected(
                originalFieldOfView >= PlayerViewSettings.MaximumFieldOfView
                    ? -1
                    : 1);
            Assert.That(
                playerViewSettings.PendingFieldOfView,
                Is.Not.EqualTo(originalFieldOfView));
            panel.MoveSelection(1);
            panel.AdjustSelected(
                originalReticleSize >= PlayerViewSettings.MaximumReticleSize
                    ? -1
                    : 1);
            Assert.That(
                playerViewSettings.PendingReticleSize,
                Is.Not.EqualTo(originalReticleSize));
            panel.MoveSelection(1);
            panel.AdjustSelected(1);
            Assert.That(
                playerViewSettings.PendingReticleColor,
                Is.Not.EqualTo(originalReticleColor));
            panel.MoveSelection(1);
            panel.AdjustSelected(1);
            Assert.That(
                playerViewSettings.PendingReticleShape,
                Is.Not.EqualTo(originalReticleShape));
            Assert.That(videoSettings.HasPendingChanges, Is.True);
            Assert.That(playerViewSettings.HasPendingChanges, Is.True);

            panel.CancelAndClose();
            Assert.That(videoSettings.PendingDisplayMode, Is.EqualTo(originalMode));
            Assert.That(
                videoSettings.PendingResolutionIndex,
                Is.EqualTo(originalResolution));
            Assert.That(videoSettings.PendingVSync, Is.EqualTo(originalVSync));
            Assert.That(videoSettings.HasPendingChanges, Is.False);
            Assert.That(
                playerViewSettings.PendingFieldOfView,
                Is.EqualTo(originalFieldOfView));
            Assert.That(
                playerViewSettings.PendingReticleSize,
                Is.EqualTo(originalReticleSize));
            Assert.That(
                playerViewSettings.PendingReticleColor,
                Is.EqualTo(originalReticleColor));
            Assert.That(
                playerViewSettings.PendingReticleShape,
                Is.EqualTo(originalReticleShape));
            Assert.That(playerViewSettings.HasPendingChanges, Is.False);
            Assert.That(panel.IsOpen, Is.False);
            yield break;
        }

        [UnityTest]
        public IEnumerator ReticleShapeCycleSkipsRemovedCombinedCross()
        {
            ProjectBloodbath.Player.ReticleShape original =
                playerViewSettings.AppliedReticleShape;
            playerViewSettings.BeginEditing();

            for (int index = 0; index < 5; index++)
            {
                playerViewSettings.CycleReticleShape(1);
                Assert.That(
                    (int)playerViewSettings.PendingReticleShape,
                    Is.Not.EqualTo(2));
            }

            Assert.That(
                playerViewSettings.PendingReticleShape,
                Is.EqualTo(original));
            playerViewSettings.CancelPending();
            yield break;
        }

        [Test]
        public void DisplayModesMapToUnityModes()
        {
            Assert.That(
                VideoSettingsManager.ToUnityFullScreenMode(
                    VideoDisplayMode.ExclusiveFullscreen),
                Is.EqualTo(FullScreenMode.ExclusiveFullScreen));
            Assert.That(
                VideoSettingsManager.ToUnityFullScreenMode(
                    VideoDisplayMode.Borderless),
                Is.EqualTo(FullScreenMode.FullScreenWindow));
            Assert.That(
                VideoSettingsManager.ToUnityFullScreenMode(
                    VideoDisplayMode.Windowed),
                Is.EqualTo(FullScreenMode.Windowed));
        }

        [Test]
        public void InterfaceScaleSupportsFullHd4KAndSuperUltrawide()
        {
            Assert.That(
                PrototypeVideoSettingsPanel.CalculateInterfaceScale(
                    1920,
                    1080),
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(
                PrototypeVideoSettingsPanel.CalculateInterfaceScale(
                    3840,
                    2160),
                Is.EqualTo(2f).Within(0.001f));
            Assert.That(
                PrototypeVideoSettingsPanel.CalculateInterfaceScale(
                    5120,
                    1440),
                Is.EqualTo(4f / 3f).Within(0.001f),
                "Un écran 32:9 doit conserver les proportions de l'interface " +
                "en se fondant sur sa hauteur, sans étirement horizontal.");

            VideoResolutionOption ultraHd = new(3840, 2160, 120);
            Assert.That(ultraHd.Width, Is.EqualTo(3840));
            Assert.That(ultraHd.Height, Is.EqualTo(2160));
            Assert.That(ultraHd.Label, Does.Contain("3840 × 2160"));

            VideoResolutionOption superUltrawide =
                new(5120, 1440, 120);
            Assert.That(superUltrawide.Label, Does.Contain("5120 × 1440"));
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
