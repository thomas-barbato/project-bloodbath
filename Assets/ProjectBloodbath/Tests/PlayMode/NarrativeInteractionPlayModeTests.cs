using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Input;
using ProjectBloodbath.Narrative;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class NarrativeInteractionPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private GameObject player;
        private PlayerInputReader inputReader;
        private PrototypeNarrativeInteraction interaction;
        private Keyboard keyboard;

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

            player = GameObject.Find("Player");
            interaction = GameObject.Find("QuarantineLoreTerminal")
                ?.GetComponent<PrototypeNarrativeInteraction>();
            Assert.That(player, Is.Not.Null);
            Assert.That(interaction, Is.Not.Null);

            inputReader = player.GetComponent<PlayerInputReader>();
            Assert.That(inputReader, Is.Not.Null);
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
            interaction?.CloseEntry();
            if (keyboard != null && keyboard.added)
            {
                SetKeys();
                InputSystem.RemoveDevice(keyboard);
            }

            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReportOpensAndClosesWithInteractWithoutPausingWorld()
        {
            NarrativeEntryDefinition entry = interaction.Entry;
            Assert.That(entry, Is.Not.Null);
            Assert.That(
                entry.Identifier,
                Is.EqualTo("movement_lab_quarantine_report"));
            Assert.That(entry.Kind, Is.EqualTo(NarrativeEntryKind.TerminalReport));
            Assert.That(entry.Body, Does.Contain("échantillon"));
            Assert.That(
                interaction.GetComponent<PrototypeQuestInteraction>(),
                Is.Null,
                "Une archive environnementale ne doit pas devenir une quête.");

            Camera cameraComponent = Camera.main;
            interaction.transform.position =
                cameraComponent.transform.position +
                cameraComponent.transform.forward * 2f -
                Vector3.up * 0.8f;
            Physics.SyncTransforms();
            interaction.RefreshPrompt();
            Assert.That(interaction.PromptVisible, Is.True);
            Assert.That(interaction.InteractionPrompt, Is.EqualTo("CONSULTER"));

            float timeScaleBeforeOpening = Time.timeScale;
            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(interaction.EntryOpen, Is.True);
            Assert.That(inputReader.GameplaySuppressed, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBeforeOpening));

            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(interaction.EntryOpen, Is.False);
            Assert.That(inputReader.GameplaySuppressed, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBeforeOpening));
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
