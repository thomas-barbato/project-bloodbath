using System.Text;
using ProjectBloodbath.Input;
using ProjectBloodbath.Quests;
using ProjectBloodbath.Settings;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1050)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public abstract class PrototypeQuestInteraction :
        MonoBehaviour,
        IPrototypeModalView
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private QuestDefinition quest;
        [SerializeField] private CharacterQuestJournal questJournal;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PrototypeCharacterPanel characterPanel;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private Renderer terminalRenderer;
        [SerializeField, Min(0.5f)] private float interactionRange = 4f;
        [SerializeField, Range(-1f, 1f)] private float minimumAimDot = 0.84f;
        [SerializeField] private Color idleColor =
            new(0.14f, 0.22f, 0.19f, 1f);
        [SerializeField] private Color availableColor =
            new(0.72f, 0.16f, 0.055f, 1f);
        [SerializeField] private string speakerDisplayName = string.Empty;
        [SerializeField] private string interactionPrompt =
            "CONSULTER LE TERMINAL";
        [SerializeField] private bool tintWhenAvailable = true;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private GUIStyle promptStyle;
        private GUIStyle titleStyle;
        private GUIStyle statusStyle;
        private GUIStyle sectionStyle;
        private GUIStyle bodyStyle;
        private GUIStyle objectiveStyle;
        private GUIStyle actionStyle;
        private MaterialPropertyBlock propertyBlock;

        public QuestDefinition Quest => quest;
        public string SpeakerDisplayName => speakerDisplayName;
        public string InteractionPrompt => interactionPrompt;
        public bool DialogueOpen { get; private set; }
        bool IPrototypeModalView.IsOpen => DialogueOpen;
        public bool PromptVisible { get; private set; }
        public QuestStatus CurrentStatus => questJournal == null || quest == null
            ? QuestStatus.NotStarted
            : questJournal.GetStatus(quest);
        public bool QuestAvailable =>
            quest != null &&
            questJournal != null &&
            (CurrentStatus != QuestStatus.NotStarted ||
             questJournal.CanStartQuest(quest));
        public string CurrentDialogue => CurrentStatus switch
        {
            QuestStatus.NotStarted => quest?.OpeningDialogue ?? string.Empty,
            QuestStatus.Active => quest?.ActiveDialogue ?? string.Empty,
            QuestStatus.ReadyToTurnIn => quest?.ReadyDialogue ?? string.Empty,
            QuestStatus.Completed => quest?.CompletedDialogue ?? string.Empty,
            _ => string.Empty
        };
        public string ObjectiveSummary => BuildObjectiveSummary();

        public void Configure(
            QuestDefinition questDefinition,
            CharacterQuestJournal journal,
            PlayerInputReader reader,
            Camera cameraComponent,
            Collider terminalCollider,
            Renderer screenRenderer = null)
        {
            quest = questDefinition;
            questJournal = journal;
            inputReader = reader;
            playerCamera = cameraComponent;
            interactionCollider = terminalCollider;
            terminalRenderer = screenRenderer;
            RefreshPrompt();
        }

        public void ConfigurePresentation(
            string speakerName,
            string prompt,
            bool useAvailabilityTint)
        {
            speakerDisplayName = speakerName?.Trim() ?? string.Empty;
            interactionPrompt = string.IsNullOrWhiteSpace(prompt)
                ? "INTERAGIR"
                : prompt.Trim();
            tintWhenAvailable = useAvailabilityTint;
            UpdateTerminalColor();
        }

        public void OpenDialogue()
        {
            CacheReferences();
            if (
                quest == null ||
                questJournal == null ||
                inputReader == null ||
                !QuestAvailable)
            {
                return;
            }

            interfaceCoordinator?.Open(this);
            if (interfaceCoordinator == null)
            {
                characterPanel?.SetOpen(false);
            }
            DialogueOpen = true;
            PromptVisible = false;
            if (interfaceCoordinator == null)
            {
                inputReader.SetGameplaySuppressed(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            UpdateTerminalColor();
        }

        public bool SubmitDialogue()
        {
            if (!DialogueOpen || questJournal == null || quest == null)
            {
                return false;
            }

            bool changed = CurrentStatus switch
            {
                QuestStatus.NotStarted => questJournal.TryStartQuest(quest),
                QuestStatus.ReadyToTurnIn => questJournal.TryTurnInQuest(quest),
                _ => false
            };
            CloseDialogue();
            return changed;
        }

        public void CloseDialogue()
        {
            if (!DialogueOpen)
            {
                return;
            }

            DialogueOpen = false;
            PromptVisible = false;
            if (interfaceCoordinator != null)
            {
                interfaceCoordinator.Close(this);
            }
            else if (characterPanel == null || !characterPanel.IsOpen)
            {
                inputReader?.SetGameplaySuppressed(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            UpdateTerminalColor();
        }

        public void CloseFromCoordinator()
        {
            DialogueOpen = false;
            PromptVisible = false;
            UpdateTerminalColor();
        }

        public void RefreshPrompt()
        {
            CacheReferences();
            PromptVisible =
                !DialogueOpen &&
                inputReader != null &&
                !inputReader.GameplaySuppressed &&
                QuestAvailable &&
                IsPlayerAimingAtTerminal();
            UpdateTerminalColor();
        }

        protected virtual void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            CacheReferences();
            UpdateTerminalColor();
        }

        protected virtual void OnDisable()
        {
            CloseDialogue();
        }

        protected virtual void Update()
        {
            if (DialogueOpen)
            {
                if (inputReader == null || !inputReader.enabled)
                {
                    CloseDialogue();
                    return;
                }

                if (inputReader.ConsumeMenuCancelPressed())
                {
                    CloseDialogue();
                    return;
                }

                if (
                    inputReader.ConsumeInterfaceInteractPressed() ||
                    inputReader.ConsumeMenuSubmitPressed())
                {
                    if (!SubmitDialogue())
                    {
                        CloseDialogue();
                    }
                }

                return;
            }

            RefreshPrompt();
            if (PromptVisible && inputReader.ConsumeInteractPressed())
            {
                OpenDialogue();
            }
        }

        protected virtual void OnGUI()
        {
            if (!PromptVisible && !DialogueOpen)
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

            if (PromptVisible && !DialogueOpen)
            {
                GUI.Label(
                    new Rect(width * 0.5f - 270f, height - 190f, 540f, 42f),
                    $"{ControlSettingsManager.FormatShortcut("E", "X")}  •  " +
                    interactionPrompt.ToUpperInvariant(),
                    promptStyle);
            }

            if (DialogueOpen)
            {
                DrawRect(
                    new Rect(0f, 0f, width, height),
                    new Color(0.005f, 0.003f, 0.002f, 0.58f));
                Rect panel = new(
                    width * 0.5f - 430f,
                    height * 0.5f - 270f,
                    860f,
                    540f);
                DrawRect(panel, new Color(0.42f, 0.09f, 0.04f, 1f));
                DrawRect(
                    new Rect(panel.x + 3f, panel.y + 3f,
                        panel.width - 6f, panel.height - 6f),
                    new Color(0.025f, 0.016f, 0.012f, 0.98f));
                GUI.Label(
                    new Rect(panel.x + 32f, panel.y + 24f,
                        panel.width - 64f, 44f),
                    quest.DisplayName.ToUpperInvariant(),
                    titleStyle);
                GUI.Label(
                    new Rect(panel.x + 32f, panel.y + 68f,
                        panel.width - 64f, 28f),
                    BuildContextLabel(),
                    statusStyle);
                GUI.Label(
                    new Rect(panel.x + 32f, panel.y + 112f,
                        panel.width - 64f, 112f),
                    CurrentDialogue,
                    bodyStyle);
                DrawRect(
                    new Rect(panel.x + 32f, panel.y + 234f,
                        panel.width - 64f, 1f),
                    new Color(0.42f, 0.09f, 0.04f, 0.9f));
                GUI.Label(
                    new Rect(panel.x + 32f, panel.y + 252f,
                        panel.width - 64f, 28f),
                    "OBJECTIFS",
                    sectionStyle);
                GUI.Label(
                    new Rect(panel.x + 32f, panel.y + 286f,
                        panel.width - 64f, 108f),
                    ObjectiveSummary,
                    objectiveStyle);
                GUI.Label(
                    new Rect(panel.x + 32f, panel.y + 404f,
                        panel.width - 64f, 30f),
                    quest.ExperienceReward > 0
                        ? $"RÉCOMPENSE  •  {quest.ExperienceReward} EXPÉRIENCE"
                        : "RÉCOMPENSE  •  AUCUNE RÉCOMPENSE DÉFINIE",
                    sectionStyle);
                GUI.Label(
                    new Rect(panel.x + 32f, panel.yMax - 72f,
                        panel.width - 64f, 34f),
                    GetActionLabel(),
                    actionStyle);
            }

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private bool IsPlayerAimingAtTerminal()
        {
            if (playerCamera == null || interactionCollider == null)
            {
                return false;
            }

            Vector3 targetPoint = terminalRenderer != null
                ? terminalRenderer.bounds.center
                : interactionCollider.bounds.center;
            Vector3 offset = targetPoint - playerCamera.transform.position;
            float distance = offset.magnitude;
            return distance <= interactionRange &&
                distance > 0.01f &&
                Vector3.Dot(
                    playerCamera.transform.forward,
                    offset / distance) >= minimumAimDot;
        }

        private string GetActionLabel()
        {
            return CurrentStatus switch
            {
                QuestStatus.NotStarted =>
                    BuildActionLabel("ACCEPTER"),
                QuestStatus.ReadyToTurnIn =>
                    BuildActionLabel("VALIDER"),
                _ => BuildActionLabel("FERMER")
            };
        }

        private static string BuildActionLabel(string action)
        {
            return $"{ControlSettingsManager.FormatShortcut("E", "X")}  •  {action}     " +
                $"{ControlSettingsManager.FormatShortcut("ÉCHAP", "B")}  •  FERMER";
        }

        private string BuildObjectiveSummary()
        {
            if (quest == null || quest.Objectives.Count == 0)
            {
                return "OBJECTIF ACCOMPLI";
            }

            QuestRuntimeState state = questJournal?.GetState(quest);
            StringBuilder summary = new();
            for (int index = 0; index < quest.Objectives.Count; index++)
            {
                QuestObjectiveDefinition objective = quest.Objectives[index];
                if (objective == null)
                {
                    continue;
                }

                int progress = state?.GetObjectiveProgress(index) ?? 0;
                bool complete = progress >= objective.RequiredAmount;
                if (summary.Length > 0)
                {
                    summary.AppendLine();
                }

                summary.Append(complete ? "[ACCOMPLI]  " : "•  ");
                summary.Append(objective.Description);
                summary.Append("   ");
                summary.Append(progress);
                summary.Append(" / ");
                summary.Append(objective.RequiredAmount);
            }

            return summary.ToString();
        }

        private string GetCategoryLabel()
        {
            return quest.Category == QuestCategory.Main
                ? "QUÊTE PRINCIPALE"
                : "QUÊTE SECONDAIRE";
        }

        private string BuildContextLabel()
        {
            string questContext =
                $"{GetCategoryLabel()}  •  {GetStatusLabel()}";
            return string.IsNullOrWhiteSpace(speakerDisplayName)
                ? questContext
                : $"{speakerDisplayName.ToUpperInvariant()}  •  " +
                  questContext;
        }

        private string GetStatusLabel()
        {
            return CurrentStatus switch
            {
                QuestStatus.NotStarted => "NOUVELLE MISSION",
                QuestStatus.Active => "EN COURS",
                QuestStatus.ReadyToTurnIn => "À VALIDER",
                QuestStatus.Completed => "TERMINÉE",
                _ => string.Empty
            };
        }

        private void CacheReferences()
        {
            interactionCollider ??= GetComponent<Collider>();
            terminalRenderer ??= GetComponent<Renderer>();
            playerCamera ??= Camera.main;
            if (playerCamera == null)
            {
                return;
            }

            Transform playerRoot = playerCamera.transform.root;
            inputReader ??= playerRoot.GetComponent<PlayerInputReader>();
            questJournal ??= playerRoot.GetComponent<CharacterQuestJournal>();
            characterPanel ??=
                playerRoot.GetComponent<PrototypeCharacterPanel>();
            if (interfaceCoordinator == null)
            {
                interfaceCoordinator = playerRoot.GetComponent<
                    PrototypeInterfaceCoordinator>();
            }
        }

        private void UpdateTerminalColor()
        {
            if (
                !tintWhenAvailable ||
                terminalRenderer == null ||
                propertyBlock == null)
            {
                return;
            }

            terminalRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(
                BaseColorId,
                PromptVisible || DialogueOpen ? availableColor : idleColor);
            terminalRenderer.SetPropertyBlock(propertyBlock);
        }

        private void EnsureStyles()
        {
            promptStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.94f, 0.76f, 0.55f, 1f) }
            };
            titleStyle ??= new GUIStyle(promptStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 28,
                normal = { textColor = new Color(0.94f, 0.28f, 0.08f, 1f) }
            };
            statusStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 0.6f, 0.48f, 1f) }
            };
            sectionStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.94f, 0.4f, 0.12f, 1f) }
            };
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 20,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.82f, 0.68f, 1f) }
            };
            objectiveStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 18,
                normal = { textColor = new Color(0.94f, 0.84f, 0.65f, 1f) }
            };
            actionStyle ??= new GUIStyle(promptStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 17
            };

            RemoveHoverFeedback(titleStyle);
            RemoveHoverFeedback(statusStyle);
            RemoveHoverFeedback(sectionStyle);
            RemoveHoverFeedback(bodyStyle);
            RemoveHoverFeedback(objectiveStyle);
            RemoveHoverFeedback(actionStyle);
        }

        private static void RemoveHoverFeedback(GUIStyle style)
        {
            if (style == null)
            {
                return;
            }

            Color textColor = style.normal.textColor;
            Texture2D background = style.normal.background;
            style.hover.textColor = textColor;
            style.hover.background = background;
            style.active.textColor = textColor;
            style.active.background = background;
            style.focused.textColor = textColor;
            style.focused.background = background;
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
