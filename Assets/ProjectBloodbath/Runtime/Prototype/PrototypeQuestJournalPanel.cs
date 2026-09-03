using ProjectBloodbath.Input;
using ProjectBloodbath.Quests;
using ProjectBloodbath.Settings;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(CharacterQuestJournal))]
    public sealed class PrototypeQuestJournalPanel :
        MonoBehaviour,
        IPrototypeModalView
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private CharacterQuestJournal questJournal;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private Color backdropColor =
            new(0.005f, 0.003f, 0.002f, 0.82f);
        [SerializeField] private Color panelColor =
            new(0.025f, 0.016f, 0.012f, 0.98f);
        [SerializeField] private Color borderColor =
            new(0.52f, 0.105f, 0.045f, 1f);
        [SerializeField] private Color accentColor =
            new(0.9f, 0.25f, 0.07f, 1f);
        [SerializeField] private Color textColor =
            new(0.91f, 0.83f, 0.69f, 1f);

        private GUIStyle titleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle questStyle;
        private GUIStyle selectedQuestStyle;
        private GUIStyle bodyStyle;
        private GUIStyle statusStyle;
        private int selectedIndex;
        private float nextNavigationTime;

        public bool IsOpen { get; private set; }
        public int SelectedIndex => selectedIndex;
        public int QuestCount => questJournal?.QuestStates.Count ?? 0;
        public QuestRuntimeState SelectedQuest => GetSelectedQuest();
        public string SelectedQuestPresentation =>
            SelectedQuest?.Definition?.OpeningDialogue ?? string.Empty;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnDisable()
        {
            SetOpen(false);
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            if (IsOpen && !inputReader.enabled)
            {
                SetOpen(false);
                return;
            }

            if (inputReader.ConsumeQuestJournalPressed())
            {
                SetOpen(!IsOpen);
            }

            if (!IsOpen)
            {
                return;
            }

            if (inputReader.ConsumeMenuCancelPressed())
            {
                SetOpen(false);
                return;
            }

            Vector2 navigation = inputReader.ConsumeMenuNavigatePressed();
            if (
                Mathf.Abs(navigation.y) > 0.4f &&
                Time.unscaledTime >= nextNavigationTime)
            {
                MoveSelection(navigation.y > 0f ? -1 : 1);
                nextNavigationTime = Time.unscaledTime + 0.16f;
            }

            if (inputReader.ConsumeMenuSubmitPressed())
            {
                TryTrackSelectedQuest();
            }
        }

        public bool TryTrackSelectedQuest()
        {
            QuestRuntimeState selected = SelectedQuest;
            return selected?.Definition != null &&
                questJournal != null &&
                questJournal.TryTrackQuest(selected.Definition);
        }

        public void SetOpen(bool open)
        {
            if (IsOpen == open)
            {
                return;
            }

            CacheReferences();
            if (open)
            {
                interfaceCoordinator?.Open(this);
                IsOpen = true;
                ClampSelection();
                nextNavigationTime = 0f;
                if (interfaceCoordinator == null)
                {
                    ApplyFallbackInputState(true);
                }
                return;
            }

            IsOpen = false;
            if (interfaceCoordinator != null)
            {
                interfaceCoordinator.Close(this);
            }
            else
            {
                ApplyFallbackInputState(false);
            }
        }

        public void CloseFromCoordinator()
        {
            IsOpen = false;
        }

        private void OnGUI()
        {
            if (!IsOpen)
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
            float height = Screen.height / scale;

            DrawRect(new Rect(0f, 0f, width, height), backdropColor);
            Rect panel = new(
                width * 0.5f - 600f,
                height * 0.5f - 380f,
                1200f,
                760f);
            DrawPanel(panel);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 20f, 650f, 44f),
                "JOURNAL DE QUÊTES",
                titleStyle);
            GUI.Label(
                new Rect(panel.x + 790f, panel.y + 24f, 380f, 30f),
                "J / SELECT  •  FERMER",
                statusStyle);

            Rect listArea = new(
                panel.x + 28f,
                panel.y + 78f,
                390f,
                650f);
            Rect detailsArea = new(
                panel.x + 438f,
                panel.y + 78f,
                734f,
                650f);
            DrawInnerPanel(listArea);
            DrawInnerPanel(detailsArea);
            DrawQuestList(listArea);
            DrawQuestDetails(detailsArea);

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawQuestList(Rect area)
        {
            GUI.Label(
                new Rect(area.x + 18f, area.y + 14f, 330f, 30f),
                "MISSIONS CONNUES",
                sectionStyle);

            if (QuestCount == 0)
            {
                GUI.Label(
                    new Rect(area.x + 18f, area.y + 66f,
                        area.width - 36f, 80f),
                    "AUCUNE QUÊTE ACCEPTÉE",
                    bodyStyle);
                return;
            }

            float y = area.y + 58f;
            for (int index = 0; index < QuestCount; index++)
            {
                QuestRuntimeState state = questJournal.QuestStates[index];
                if (state?.Definition == null)
                {
                    continue;
                }

                Rect row = new(
                    area.x + 14f,
                    y,
                    area.width - 28f,
                    64f);
                bool selected = index == selectedIndex;
                DrawRect(
                    row,
                    selected
                        ? new Color(0.26f, 0.055f, 0.02f, 0.95f)
                        : new Color(0.055f, 0.028f, 0.018f, 0.92f));
                if (GUI.Button(
                    row,
                    $"{GetCategoryLabel(state.Definition.Category)}\n" +
                    state.Definition.DisplayName.ToUpperInvariant(),
                    selected ? selectedQuestStyle : questStyle))
                {
                    selectedIndex = index;
                }

                GUI.Label(
                    new Rect(row.x + 224f, row.y + 7f, 122f, 24f),
                    questJournal.IsQuestTracked(state.Definition)
                        ? "SUIVIE"
                        : GetStatusLabel(state.Status),
                    statusStyle);
                y += 72f;
            }
        }

        private void DrawQuestDetails(Rect area)
        {
            QuestRuntimeState state = SelectedQuest;
            if (state?.Definition == null)
            {
                GUI.Label(
                    new Rect(area.x + 24f, area.y + 24f,
                        area.width - 48f, 100f),
                    "Les quêtes acceptées et terminées apparaîtront ici.",
                    bodyStyle);
                return;
            }

            QuestDefinition definition = state.Definition;
            GUI.Label(
                new Rect(area.x + 24f, area.y + 20f,
                    area.width - 48f, 40f),
                definition.DisplayName.ToUpperInvariant(),
                titleStyle);
            GUI.Label(
                new Rect(area.x + 24f, area.y + 66f,
                    area.width - 48f, 28f),
                $"{GetCategoryLabel(definition.Category)}  •  " +
                GetStatusLabel(state.Status) +
                (questJournal.IsQuestTracked(definition)
                    ? "  •  SUIVIE"
                    : string.Empty),
                statusStyle);

            float y = area.y + 116f;
            GUI.Label(
                new Rect(area.x + 24f, y, 300f, 30f),
                "PRÉSENTATION",
                sectionStyle);
            y += 38f;
            float presentationHeight = Mathf.Clamp(
                bodyStyle.CalcHeight(
                    new GUIContent(SelectedQuestPresentation),
                    area.width - 48f),
                54f,
                132f);
            GUI.Label(
                new Rect(area.x + 24f, y,
                    area.width - 48f, presentationHeight),
                SelectedQuestPresentation,
                bodyStyle);
            y += presentationHeight + 18f;

            GUI.Label(
                new Rect(area.x + 24f, y, 300f, 30f),
                "OBJECTIFS",
                sectionStyle);
            y += 42f;
            if (definition.Objectives.Count == 0)
            {
                GUI.Label(
                    new Rect(area.x + 24f, y,
                        area.width - 48f, 34f),
                    "OBJECTIF ACCOMPLI",
                    bodyStyle);
                y += 48f;
            }
            else
            {
                for (int index = 0;
                    index < definition.Objectives.Count;
                    index++)
                {
                    QuestObjectiveDefinition objective =
                        definition.Objectives[index];
                    if (objective == null)
                    {
                        continue;
                    }

                    int progress = state.GetObjectiveProgress(index);
                    string marker = progress >= objective.RequiredAmount
                        ? "[ACCOMPLI]"
                        : "[EN COURS]";
                    GUI.Label(
                        new Rect(area.x + 24f, y,
                            area.width - 48f, 58f),
                        $"{marker}  {objective.Description}\n" +
                        $"{progress} / {objective.RequiredAmount}",
                        bodyStyle);
                    y += 70f;
                }
            }

            GUI.Label(
                new Rect(area.x + 24f, y + 18f, 300f, 30f),
                "RÉCOMPENSE",
                sectionStyle);
            GUI.Label(
                new Rect(area.x + 24f, y + 58f,
                    area.width - 48f, 34f),
                definition.ExperienceReward > 0
                    ? $"{definition.ExperienceReward} EXPÉRIENCE"
                    : "AUCUNE RÉCOMPENSE DÉFINIE",
                bodyStyle);

            bool trackable =
                state.Status == QuestStatus.Active ||
                state.Status == QuestStatus.ReadyToTurnIn;
            GUI.Label(
                new Rect(area.x + 24f, area.yMax - 48f,
                    area.width - 48f, 28f),
                questJournal.IsQuestTracked(definition)
                    ? "QUÊTE ACTUELLEMENT SUIVIE"
                    : trackable
                        ? $"{ControlSettingsManager.FormatShortcut("ENTRÉE", "A")}  •  SUIVRE CETTE QUÊTE"
                        : string.Empty,
                statusStyle);
        }

        private QuestRuntimeState GetSelectedQuest()
        {
            if (questJournal == null || questJournal.QuestStates.Count == 0)
            {
                return null;
            }

            ClampSelection();
            return questJournal.QuestStates[selectedIndex];
        }

        private void MoveSelection(int direction)
        {
            if (QuestCount <= 0)
            {
                selectedIndex = 0;
                return;
            }

            selectedIndex =
                (selectedIndex + direction + QuestCount) % QuestCount;
        }

        private void ClampSelection()
        {
            selectedIndex = Mathf.Clamp(
                selectedIndex,
                0,
                Mathf.Max(0, QuestCount - 1));
        }

        private void CacheReferences()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (questJournal == null)
            {
                questJournal = GetComponent<CharacterQuestJournal>();
            }

            if (interfaceCoordinator == null)
            {
                interfaceCoordinator =
                    GetComponent<PrototypeInterfaceCoordinator>();
            }
        }

        private void ApplyFallbackInputState(bool open)
        {
            inputReader?.SetGameplaySuppressed(open);
            Cursor.lockState = open
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = accentColor }
            };
            sectionStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor }
            };
            questStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                padding = new RectOffset(12, 132, 5, 5),
                normal = { textColor = textColor },
                hover = { textColor = Color.white }
            };
            selectedQuestStyle ??= new GUIStyle(questStyle)
            {
                normal = { textColor = accentColor }
            };
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                wordWrap = true,
                normal = { textColor = textColor }
            };
            statusStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.72f, 0.62f, 0.5f, 1f) }
            };
        }

        private void DrawPanel(Rect rect)
        {
            DrawRect(rect, borderColor);
            DrawRect(
                new Rect(rect.x + 3f, rect.y + 3f,
                    rect.width - 6f, rect.height - 6f),
                panelColor);
        }

        private void DrawInnerPanel(Rect rect)
        {
            DrawRect(
                rect,
                new Color(
                    borderColor.r,
                    borderColor.g,
                    borderColor.b,
                    0.72f));
            DrawRect(
                new Rect(rect.x + 2f, rect.y + 2f,
                    rect.width - 4f, rect.height - 4f),
                new Color(0.015f, 0.01f, 0.008f, 0.98f));
        }

        private static string GetCategoryLabel(QuestCategory category)
        {
            return category == QuestCategory.Main
                ? "QUÊTE PRINCIPALE"
                : "QUÊTE SECONDAIRE";
        }

        private static string GetStatusLabel(QuestStatus status)
        {
            return status switch
            {
                QuestStatus.Active => "EN COURS",
                QuestStatus.ReadyToTurnIn => "À RENDRE",
                QuestStatus.Completed => "TERMINÉE",
                _ => "INDISPONIBLE"
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
