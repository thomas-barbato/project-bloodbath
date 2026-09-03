using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Input;
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
    public sealed class PrototypeQuestJournalAndMapPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Keyboard keyboard;
        private PlayerInputReader inputReader;
        private CharacterQuestJournal questJournal;
        private PrototypeQuestJournalPanel journalPanel;
        private PrototypeMapPanel mapPanel;
        private PrototypeCharacterPanel characterPanel;
        private PrototypeQuestTerminal terminal;

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

            GameObject enemy = GameObject.Find("PrototypeEnemy");
            GameObject skirmisher = GameObject.Find("PrototypeSkirmisher");
            GameObject pickup = GameObject.Find("ManualItemPickup_Test");
            GameObject wall = GameObject.Find("Wall_North");
            Assert.That(
                enemy.GetComponent<WorldMapMarker>().MarkerType,
                Is.EqualTo(WorldMapMarkerType.Hostile));
            Assert.That(
                skirmisher.GetComponent<WorldMapMarker>().MarkerType,
                Is.EqualTo(WorldMapMarkerType.Hostile));
            Assert.That(
                pickup.GetComponent<WorldMapMarker>().MarkerType,
                Is.EqualTo(WorldMapMarkerType.Loot));
            Assert.That(wall.GetComponent<WorldMapGeometry>(), Is.Not.Null);
            enemy.SetActive(false);
            skirmisher.SetActive(false);

            GameObject player = GameObject.Find("Player");
            Assert.That(player, Is.Not.Null);
            inputReader = player.GetComponent<PlayerInputReader>();
            questJournal = player.GetComponent<CharacterQuestJournal>();
            journalPanel = player.GetComponent<PrototypeQuestJournalPanel>();
            mapPanel = player.GetComponent<PrototypeMapPanel>();
            characterPanel = player.GetComponent<PrototypeCharacterPanel>();
            terminal = GameObject.Find("QuestTerminal")
                .GetComponent<PrototypeQuestTerminal>();

            Assert.That(inputReader, Is.Not.Null);
            Assert.That(questJournal, Is.Not.Null);
            Assert.That(journalPanel, Is.Not.Null);
            Assert.That(mapPanel, Is.Not.Null);
            Assert.That(characterPanel, Is.Not.Null);
            Assert.That(terminal, Is.Not.Null);
            Assert.That(
                terminal.GetComponent<WorldMapMarker>(),
                Is.Not.Null);

            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            journalPanel?.SetOpen(false);
            mapPanel?.SetOpen(false);
            characterPanel?.SetOpen(false);
            terminal?.CloseDialogue();
            if (keyboard != null && keyboard.added)
            {
                SetKeys();
                InputSystem.RemoveDevice(keyboard);
            }

            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator JournalOpensWithJAndShowsAcceptedQuest()
        {
            Assert.That(
                questJournal.TryStartQuest(terminal.Quest),
                Is.True,
                "La quête de scène doit pouvoir être acceptée avant le test du journal.");
            float timeScaleBefore = Time.timeScale;

            SetKeys(Key.J);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(
                journalPanel.IsOpen,
                Is.True,
                "J doit ouvrir le journal.");
            Assert.That(journalPanel.QuestCount, Is.EqualTo(1));
            Assert.That(
                journalPanel.SelectedQuest.Definition,
                Is.SameAs(terminal.Quest));
            Assert.That(
                journalPanel.SelectedQuestPresentation,
                Is.EqualTo(terminal.Quest.OpeningDialogue),
                "Le journal doit réutiliser le texte de présentation de la quête.");
            Assert.That(
                inputReader.GameplaySuppressed,
                Is.True,
                "Le journal doit neutraliser les commandes de gameplay.");
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBefore));

            SetKeys(Key.J);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(journalPanel.IsOpen, Is.False);
            Assert.That(inputReader.GameplaySuppressed, Is.False);
        }

        [UnityTest]
        public IEnumerator MapOpensWithMAndReplacesAnotherMenu()
        {
            float timeScaleBefore = Time.timeScale;
            Assert.That(mapPanel.MiniMapVisible, Is.True);

            characterPanel.SetOpen(true);
            Assert.That(
                characterPanel.IsOpen,
                Is.True,
                "Le dossier de personnage doit être ouvert au départ.");
            Assert.That(
                mapPanel.MiniMapVisible,
                Is.False,
                "La mini-carte doit se masquer sous un écran plein.");

            SetKeys(Key.M);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(
                characterPanel.IsOpen,
                Is.False,
                "M doit fermer l'écran plein précédent.");
            Assert.That(
                mapPanel.IsOpen,
                Is.True,
                "M doit ouvrir la grande carte.");
            Assert.That(inputReader.GameplaySuppressed, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBefore));
            Assert.That(mapPanel.VisibleMarkerCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(mapPanel.VisibleGeometryCount, Is.GreaterThanOrEqualTo(5));

            SetKeys(Key.M);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(mapPanel.IsOpen, Is.False);
            Assert.That(mapPanel.MiniMapVisible, Is.True);
            Assert.That(inputReader.GameplaySuppressed, Is.False);
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
