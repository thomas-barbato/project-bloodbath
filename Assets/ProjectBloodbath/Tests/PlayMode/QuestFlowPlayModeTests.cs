using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using ProjectBloodbath.Quests;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class QuestFlowPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private GameObject player;
        private CharacterQuestJournal questJournal;
        private CharacterProgression progression;
        private PlayerInputReader inputReader;
        private PrototypeQuestTerminal terminal;
        private QuestDefinition quest;
        private Health pursuerHealth;
        private Health skirmisherHealth;
        private Keyboard keyboard;
        private bool ownsKeyboard;

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
            GameObject pursuer = GameObject.Find("PrototypeEnemy");
            GameObject skirmisher = GameObject.Find("PrototypeSkirmisher");
            terminal = GameObject.Find("QuestTerminal")
                .GetComponent<PrototypeQuestTerminal>();

            Assert.That(player, Is.Not.Null);
            Assert.That(pursuer, Is.Not.Null);
            Assert.That(skirmisher, Is.Not.Null);
            Assert.That(terminal, Is.Not.Null);

            pursuer.GetComponent<PrototypeEnemyController>().enabled = false;
            skirmisher.GetComponent<PrototypeEnemyController>().enabled = false;
            questJournal = player.GetComponent<CharacterQuestJournal>();
            progression = player.GetComponent<CharacterProgression>();
            inputReader = player.GetComponent<PlayerInputReader>();
            quest = terminal.Quest;
            pursuerHealth = pursuer.GetComponent<Health>();
            skirmisherHealth = skirmisher.GetComponent<Health>();

            Assert.That(questJournal, Is.Not.Null);
            Assert.That(progression, Is.Not.Null);
            Assert.That(inputReader, Is.Not.Null);
            Assert.That(quest, Is.Not.Null);
            Assert.That(pursuerHealth, Is.Not.Null);
            Assert.That(skirmisherHealth, Is.Not.Null);
            Assert.That(
                pursuer.GetComponent<QuestTargetIdentity>().Identifier,
                Is.EqualTo("movement_lab_hostile"));
            Assert.That(
                skirmisher.GetComponent<QuestTargetIdentity>().Identifier,
                Is.EqualTo("movement_lab_hostile"));

            keyboard = InputSystem.AddDevice<Keyboard>();
            ownsKeyboard = true;

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            terminal?.CloseDialogue();
            if (keyboard != null && keyboard.added)
            {
                SetKeys();
            }

            if (ownsKeyboard && keyboard != null && keyboard.added)
            {
                InputSystem.RemoveDevice(keyboard);
            }

            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator QuestTracksOnlyLocalKillsAndRewardsOneTurnIn()
        {
            Assert.That(
                questJournal.GetStatus(quest),
                Is.EqualTo(QuestStatus.NotStarted));
            Assert.That(questJournal.TryStartQuest(quest), Is.True);
            QuestRuntimeState state = questJournal.GetState(quest);
            Assert.That(state, Is.Not.Null);
            Assert.That(state.Status, Is.EqualTo(QuestStatus.Active));

            GameObject foreignSource = new("ForeignPlayer");
            QuestGameplayEvents.Publish(new QuestGameplayEvent(
                QuestEventIdentifiers.EnemyKilled,
                "movement_lab_hostile",
                foreignSource,
                null));
            Assert.That(state.GetObjectiveProgress(0), Is.Zero);
            Object.Destroy(foreignSource);

            Kill(pursuerHealth);
            Assert.That(state.GetObjectiveProgress(0), Is.EqualTo(1));
            Assert.That(state.Status, Is.EqualTo(QuestStatus.Active));

            Kill(skirmisherHealth);
            Assert.That(state.GetObjectiveProgress(0), Is.EqualTo(2));
            Assert.That(state.Status, Is.EqualTo(QuestStatus.ReadyToTurnIn));

            Assert.That(questJournal.TryTurnInQuest(quest), Is.True);
            Assert.That(state.Status, Is.EqualTo(QuestStatus.Completed));
            Assert.That(
                questJournal.LastGrantedExperience,
                Is.EqualTo(quest.ExperienceReward));
            int levelAfterReward = progression.CurrentLevel;
            int experienceAfterReward = progression.CurrentExperience;

            Assert.That(questJournal.TryTurnInQuest(quest), Is.False);
            Assert.That(progression.CurrentLevel, Is.EqualTo(levelAfterReward));
            Assert.That(
                progression.CurrentExperience,
                Is.EqualTo(experienceAfterReward));
            yield break;
        }

        [UnityTest]
        public IEnumerator DialogueUsesLocalInputSuppressionWithoutPausingWorld()
        {
            float timeScaleBeforeDialogue = Time.timeScale;
            Assert.That(terminal.CurrentDialogue, Is.EqualTo(quest.OpeningDialogue));

            terminal.OpenDialogue();
            Assert.That(terminal.DialogueOpen, Is.True);
            Assert.That(inputReader.GameplaySuppressed, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBeforeDialogue));

            Assert.That(terminal.SubmitDialogue(), Is.True);
            Assert.That(terminal.DialogueOpen, Is.False);
            Assert.That(inputReader.GameplaySuppressed, Is.False);
            Assert.That(
                questJournal.GetStatus(quest),
                Is.EqualTo(QuestStatus.Active));
            Assert.That(terminal.CurrentDialogue, Is.EqualTo(quest.ActiveDialogue));
            yield break;
        }

        [UnityTest]
        public IEnumerator BriefInteractPressOpensAimedTerminal()
        {
            player.GetComponent<ProjectBloodbath.Player.FpsPlayerController>()
                .enabled = false;
            player.transform.position = new Vector3(-0.5f, 0.05f, -8f);
            player.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            Physics.SyncTransforms();
            terminal.RefreshPrompt();
            Assert.That(terminal.PromptVisible, Is.True);

            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(
                terminal.DialogueOpen,
                Is.True,
                "Une pression brève sur Interagir doit ouvrir le terminal visé.");
        }

        [UnityTest]
        public IEnumerator SecondInteractPressAcceptsDisplayedQuest()
        {
            player.GetComponent<ProjectBloodbath.Player.FpsPlayerController>()
                .enabled = false;
            player.transform.position = new Vector3(-0.5f, 0.05f, -8f);
            player.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            Physics.SyncTransforms();
            terminal.RefreshPrompt();

            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;
            Assert.That(terminal.DialogueOpen, Is.True);
            Assert.That(
                terminal.ObjectiveSummary,
                Does.Contain(quest.Objectives[0].Description));

            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(terminal.DialogueOpen, Is.False);
            Assert.That(
                questJournal.GetStatus(quest),
                Is.EqualTo(QuestStatus.Active),
                "La même action Interagir doit accepter la quête affichée.");
        }

        [UnityTest]
        public IEnumerator CompletedPrerequisiteUnlocksButDoesNotStartFollowUp()
        {
            QuestDefinition prerequisite = CreateImmediateQuest(
                "test_prerequisite",
                "Première étape");
            QuestDefinition followUp = CreateImmediateQuest(
                "test_follow_up",
                "Étape suivante",
                new[] { prerequisite });

            Assert.That(questJournal.CanStartQuest(followUp), Is.False);
            Assert.That(questJournal.TryStartQuest(followUp), Is.False);

            Assert.That(questJournal.TryStartQuest(prerequisite), Is.True);
            Assert.That(
                questJournal.GetStatus(prerequisite),
                Is.EqualTo(QuestStatus.ReadyToTurnIn));
            Assert.That(questJournal.CanStartQuest(followUp), Is.False);

            Assert.That(
                questJournal.TryTurnInQuest(prerequisite),
                Is.True);
            Assert.That(questJournal.CanStartQuest(followUp), Is.True);
            Assert.That(
                questJournal.GetStatus(followUp),
                Is.EqualTo(QuestStatus.NotStarted),
                "La suite doit devenir disponible sans être acceptée automatiquement.");

            Assert.That(questJournal.TryStartQuest(followUp), Is.True);
            Object.Destroy(prerequisite);
            Object.Destroy(followUp);
            yield break;
        }

        private void Kill(Health targetHealth)
        {
            targetHealth.ApplyDamage(new DamageInfo(
                targetHealth.Current + 1f,
                DamageType.Ballistic,
                targetHealth.transform.position,
                Vector3.up,
                Vector3.forward,
                0f,
                player));
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private static QuestDefinition CreateImmediateQuest(
            string identifier,
            string displayName,
            QuestDefinition[] prerequisites = null)
        {
            QuestDefinition definition =
                ScriptableObject.CreateInstance<QuestDefinition>();
            definition.Configure(
                identifier,
                displayName,
                QuestCategory.Main,
                "Début",
                "En cours",
                "À rendre",
                "Terminée",
                System.Array.Empty<QuestObjectiveDefinition>(),
                0,
                prerequisites);
            return definition;
        }
    }
}
