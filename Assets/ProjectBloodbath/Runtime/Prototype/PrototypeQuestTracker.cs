using ProjectBloodbath.Quests;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterQuestJournal))]
    public sealed class PrototypeQuestTracker : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private CharacterQuestJournal questJournal;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;

        private GUIStyle titleStyle;
        private GUIStyle objectiveStyle;
        private GUIStyle feedbackStyle;
        private string feedback = string.Empty;
        private float feedbackUntil;

        public QuestRuntimeState TrackedQuest => questJournal?.TrackedQuest;
        public bool TrackerVisible => TrackedQuest != null;
        public string ObjectiveLabel => BuildObjectiveLabel(TrackedQuest);

        private void Awake()
        {
            questJournal ??= GetComponent<CharacterQuestJournal>();
            if (interfaceCoordinator == null)
            {
                interfaceCoordinator =
                    GetComponent<PrototypeInterfaceCoordinator>();
            }
        }

        private void OnEnable()
        {
            if (questJournal == null)
            {
                questJournal = GetComponent<CharacterQuestJournal>();
            }

            if (questJournal == null)
            {
                return;
            }

            questJournal.QuestStarted += OnQuestStarted;
            questJournal.ObjectiveProgressChanged += OnObjectiveProgressChanged;
            questJournal.QuestReadyToTurnIn += OnQuestReady;
            questJournal.QuestCompleted += OnQuestCompleted;
        }

        private void OnDisable()
        {
            if (questJournal == null)
            {
                return;
            }

            questJournal.QuestStarted -= OnQuestStarted;
            questJournal.ObjectiveProgressChanged -= OnObjectiveProgressChanged;
            questJournal.QuestReadyToTurnIn -= OnQuestReady;
            questJournal.QuestCompleted -= OnQuestCompleted;
        }

        private void OnGUI()
        {
            if (interfaceCoordinator != null &&
                interfaceCoordinator.HasOpenView)
            {
                return;
            }

            QuestRuntimeState tracked = TrackedQuest;
            if (tracked == null && Time.unscaledTime >= feedbackUntil)
            {
                return;
            }

            EnsureStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            float scale = Mathf.Max(
                0.5f,
                Mathf.Min(
                    Screen.width / ReferenceWidth,
                    Screen.height / ReferenceHeight));
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;

            if (tracked != null)
            {
                Rect panel = new(width - 470f, 340f, 420f, 112f);
                DrawRect(panel, new Color(0.025f, 0.016f, 0.012f, 0.88f));
                DrawRect(new Rect(panel.x, panel.y, 4f, panel.height),
                    new Color(0.78f, 0.17f, 0.055f, 1f));
                GUI.Label(
                    new Rect(panel.x + 18f, panel.y + 12f,
                        panel.width - 36f, 30f),
                    tracked.Definition.DisplayName.ToUpperInvariant(),
                    titleStyle);
                GUI.Label(
                    new Rect(panel.x + 18f, panel.y + 48f,
                        panel.width - 36f, 52f),
                    BuildObjectiveLabel(tracked),
                    objectiveStyle);
            }

            if (Time.unscaledTime < feedbackUntil)
            {
                GUI.Label(
                    new Rect(width * 0.5f - 360f, 76f, 720f, 42f),
                    feedback,
                    feedbackStyle);
            }

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private static string BuildObjectiveLabel(QuestRuntimeState state)
        {
            if (state?.Definition == null)
            {
                return string.Empty;
            }

            if (state.Status == QuestStatus.ReadyToTurnIn)
            {
                return "RETOURNER AU TERMINAL";
            }

            if (state.Definition.Objectives.Count == 0)
            {
                return "OBJECTIF ACCOMPLI";
            }

            QuestObjectiveDefinition objective = state.Definition.Objectives[0];
            return $"{objective.Description}  " +
                $"{state.GetObjectiveProgress(0)} / {objective.RequiredAmount}";
        }

        private void OnQuestStarted(QuestRuntimeState state)
        {
            SetFeedback("NOUVELLE QUÊTE");
        }

        private void OnObjectiveProgressChanged(
            QuestRuntimeState state,
            int objectiveIndex)
        {
            SetFeedback("OBJECTIF MIS À JOUR");
        }

        private void OnQuestReady(QuestRuntimeState state)
        {
            SetFeedback("OBJECTIF ACCOMPLI — RETOURNEZ AU TERMINAL");
        }

        private void OnQuestCompleted(QuestRuntimeState state)
        {
            int reward = questJournal?.LastGrantedExperience ?? 0;
            SetFeedback(reward > 0
                ? $"QUÊTE TERMINÉE  •  +{reward} EXP"
                : "QUÊTE TERMINÉE");
        }

        private void SetFeedback(string message)
        {
            feedback = message;
            feedbackUntil = Time.unscaledTime + 2.2f;
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.25f, 0.07f, 1f) }
            };
            objectiveStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.82f, 0.68f, 1f) }
            };
            feedbackStyle ??= new GUIStyle(titleStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            };
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
