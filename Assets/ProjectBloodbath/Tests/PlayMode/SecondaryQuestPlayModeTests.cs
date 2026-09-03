using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using ProjectBloodbath.Quests;
using ProjectBloodbath.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class SecondaryQuestPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private CharacterQuestJournal questJournal;
        private CharacterInventory inventory;
        private PrototypeQuestTracker questTracker;
        private PrototypeQuestJournalPanel journalPanel;
        private PrototypeMapPanel mapPanel;
        private PrototypeQuestTerminal mainTerminal;
        private PrototypeQuestNpc secondaryNpc;
        private WorldPickup samplePickup;
        private WorldMapMarker sampleMarker;
        private WorldMapMarker enemyMarker;
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

            GameObject player = GameObject.Find("Player");
            mainTerminal = GameObject.Find("QuestTerminal")
                .GetComponent<PrototypeQuestTerminal>();
            secondaryNpc = GameObject.Find("QuarantineTechnicianNpc")
                .GetComponent<PrototypeQuestNpc>();
            samplePickup = GameObject.Find("ContaminatedSamplePickup_Test")
                .GetComponent<WorldPickup>();
            sampleMarker = samplePickup.GetComponent<WorldMapMarker>();
            GameObject enemy = GameObject.Find("PrototypeEnemy");
            enemyMarker = enemy.GetComponent<WorldMapMarker>();
            enemy.GetComponent<PrototypeEnemyController>().enabled = false;
            GameObject skirmisher = GameObject.Find("PrototypeSkirmisher");
            skirmisher.GetComponent<PrototypeEnemyController>().enabled = false;

            questJournal = player.GetComponent<CharacterQuestJournal>();
            inventory = player.GetComponent<CharacterInventory>();
            questTracker = player.GetComponent<PrototypeQuestTracker>();
            journalPanel = player.GetComponent<PrototypeQuestJournalPanel>();
            mapPanel = player.GetComponent<PrototypeMapPanel>();

            Assert.That(
                player.GetComponent<InventoryQuestEventBridge>(),
                Is.Not.Null);
            Assert.That(mainTerminal.Quest.Category, Is.EqualTo(QuestCategory.Main));
            Assert.That(
                secondaryNpc.Quest.Category,
                Is.EqualTo(QuestCategory.Secondary));
            Assert.That(
                secondaryNpc.SpeakerDisplayName,
                Is.EqualTo("Technicienne de quarantaine"));
            Assert.That(secondaryNpc.InteractionPrompt, Is.EqualTo("PARLER"));
            WorldMapMarker npcMarker = secondaryNpc.GetComponent<WorldMapMarker>();
            Assert.That(npcMarker, Is.Not.Null);
            Assert.That(
                npcMarker.MarkerType,
                Is.EqualTo(WorldMapMarkerType.NonPlayerCharacter));
            Assert.That(
                samplePickup.Definition.Identifier,
                Is.EqualTo("contaminated_sample"));

            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            journalPanel?.SetOpen(false);
            mainTerminal?.CloseDialogue();
            secondaryNpc?.CloseDialogue();
            if (keyboard != null && keyboard.added)
            {
                SetKeys();
                InputSystem.RemoveDevice(keyboard);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PickupCompletesSecondaryQuestWithoutChangingMainQuest()
        {
            QuestDefinition mainQuest = mainTerminal.Quest;
            QuestDefinition secondaryQuest = secondaryNpc.Quest;
            Assert.That(questJournal.TryStartQuest(mainQuest), Is.True);
            Assert.That(questJournal.TryStartQuest(secondaryQuest), Is.True);
            Assert.That(questJournal.QuestStates.Count, Is.EqualTo(2));

            QuestRuntimeState mainState = questJournal.GetState(mainQuest);
            QuestRuntimeState secondaryState =
                questJournal.GetState(secondaryQuest);
            Assert.That(mainState.Status, Is.EqualTo(QuestStatus.Active));
            Assert.That(secondaryState.Status, Is.EqualTo(QuestStatus.Active));

            Assert.That(samplePickup.TryCollect(inventory), Is.True);
            Assert.That(
                secondaryState.GetObjectiveProgress(0),
                Is.EqualTo(1));
            Assert.That(
                secondaryState.Status,
                Is.EqualTo(QuestStatus.ReadyToTurnIn));
            Assert.That(mainState.Status, Is.EqualTo(QuestStatus.Active));
            Assert.That(
                inventory.ContainsItem(samplePickup.Definition),
                Is.True);
            Assert.That(samplePickup.gameObject.activeSelf, Is.False);

            Assert.That(
                questJournal.TryTurnInQuest(secondaryQuest),
                Is.True);
            Assert.That(
                secondaryState.Status,
                Is.EqualTo(QuestStatus.Completed));
            Assert.That(
                questJournal.LastGrantedExperience,
                Is.EqualTo(secondaryQuest.ExperienceReward));
            yield break;
        }

        [UnityTest]
        public IEnumerator NpcUsesInteractForOpenAndAccept()
        {
            GameObject player = GameObject.Find("Player");
            player.GetComponent<ProjectBloodbath.Player.FpsPlayerController>()
                .enabled = false;
            player.transform.position = new Vector3(0.5f, 0.05f, -8f);
            player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Physics.SyncTransforms();
            secondaryNpc.RefreshPrompt();

            Assert.That(
                secondaryNpc.PromptVisible,
                Is.True,
                "Le PNJ visé doit proposer l'action Parler.");

            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(secondaryNpc.DialogueOpen, Is.True);
            Assert.That(
                secondaryNpc.CurrentDialogue,
                Is.EqualTo(secondaryNpc.Quest.OpeningDialogue));

            SetKeys(Key.E);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(secondaryNpc.DialogueOpen, Is.False);
            Assert.That(
                questJournal.GetStatus(secondaryNpc.Quest),
                Is.EqualTo(QuestStatus.Active),
                "La même action doit accepter la quête racontée par le PNJ.");
        }

        [UnityTest]
        public IEnumerator JournalSelectionChangesTrackerAndMapObjective()
        {
            Assert.That(
                questJournal.TryStartQuest(mainTerminal.Quest),
                Is.True);
            Assert.That(
                questJournal.TryStartQuest(secondaryNpc.Quest),
                Is.True);
            Assert.That(
                questTracker.TrackedQuest.Definition,
                Is.SameAs(mainTerminal.Quest));
            Assert.That(
                mapPanel.IsTrackedQuestObjective(enemyMarker),
                Is.True);
            Assert.That(
                mapPanel.IsTrackedQuestObjective(sampleMarker),
                Is.False);

            journalPanel.SetOpen(true);
            Assert.That(journalPanel.SelectedIndex, Is.Zero);
            SetKeys(Key.DownArrow);
            yield return null;
            SetKeys();
            yield return null;
            Assert.That(journalPanel.SelectedIndex, Is.EqualTo(1));

            SetKeys(Key.Enter);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(
                questTracker.TrackedQuest.Definition,
                Is.SameAs(secondaryNpc.Quest));
            Assert.That(
                mapPanel.IsTrackedQuestObjective(sampleMarker),
                Is.True);
            Assert.That(
                mapPanel.IsTrackedQuestObjective(enemyMarker),
                Is.False);
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
