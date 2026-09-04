using ProjectBloodbath.Input;
using ProjectBloodbath.Narrative;
using ProjectBloodbath.Settings;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1050)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class PrototypeNarrativeInteraction :
        MonoBehaviour,
        IPrototypeModalView
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private NarrativeEntryDefinition entry;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private Renderer screenRenderer;
        [SerializeField, Min(0.5f)] private float interactionRange = 4f;
        [SerializeField, Range(-1f, 1f)] private float minimumAimDot = 0.84f;
        [SerializeField] private string interactionPrompt = "CONSULTER";
        [SerializeField] private Color idleColor =
            new(0.14f, 0.22f, 0.19f, 1f);
        [SerializeField] private Color activeColor =
            new(0.72f, 0.16f, 0.055f, 1f);

        private static readonly int BaseColorId = Shader.PropertyToID(
            "_BaseColor");

        private GUIStyle promptStyle;
        private GUIStyle titleStyle;
        private GUIStyle metadataStyle;
        private GUIStyle bodyStyle;
        private GUIStyle actionStyle;
        private MaterialPropertyBlock propertyBlock;
        private Vector2 scrollPosition;

        public NarrativeEntryDefinition Entry => entry;
        public bool EntryOpen { get; private set; }
        bool IPrototypeModalView.IsOpen => EntryOpen;
        public bool PromptVisible { get; private set; }
        public string InteractionPrompt => interactionPrompt;

        public void Configure(
            NarrativeEntryDefinition narrativeEntry,
            PlayerInputReader reader,
            Camera cameraComponent,
            Collider targetCollider,
            Renderer targetRenderer = null,
            string prompt = "CONSULTER")
        {
            entry = narrativeEntry;
            inputReader = reader;
            playerCamera = cameraComponent;
            interactionCollider = targetCollider;
            screenRenderer = targetRenderer;
            interactionPrompt = string.IsNullOrWhiteSpace(prompt)
                ? "CONSULTER"
                : prompt.Trim();
            RefreshPrompt();
        }

        public void OpenEntry()
        {
            CacheReferences();
            if (entry == null || inputReader == null)
            {
                return;
            }

            interfaceCoordinator?.Open(this);
            EntryOpen = true;
            PromptVisible = false;
            scrollPosition = Vector2.zero;
            if (interfaceCoordinator == null)
            {
                inputReader.SetGameplaySuppressed(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            UpdateScreenColor();
        }

        public void CloseEntry()
        {
            if (!EntryOpen)
            {
                return;
            }

            EntryOpen = false;
            PromptVisible = false;
            if (interfaceCoordinator != null)
            {
                interfaceCoordinator.Close(this);
            }
            else
            {
                inputReader?.SetGameplaySuppressed(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                PrototypeInterfaceCursor.Reset();
            }

            UpdateScreenColor();
        }

        public void CloseFromCoordinator()
        {
            EntryOpen = false;
            PromptVisible = false;
            UpdateScreenColor();
        }

        public void RefreshPrompt()
        {
            CacheReferences();
            PromptVisible =
                !EntryOpen &&
                entry != null &&
                inputReader != null &&
                !inputReader.GameplaySuppressed &&
                IsPlayerAimingAtEntry();
            UpdateScreenColor();
        }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            CacheReferences();
            UpdateScreenColor();
        }

        private void OnDisable()
        {
            CloseEntry();
        }

        private void Update()
        {
            if (EntryOpen)
            {
                if (inputReader == null || !inputReader.enabled)
                {
                    CloseEntry();
                    return;
                }

                if (
                    inputReader.ConsumeMenuCancelPressed() ||
                    inputReader.ConsumeInterfaceInteractPressed() ||
                    inputReader.ConsumeMenuSubmitPressed())
                {
                    CloseEntry();
                }

                return;
            }

            RefreshPrompt();
            if (PromptVisible && inputReader.ConsumeInteractPressed())
            {
                OpenEntry();
            }
        }

        private void OnGUI()
        {
            if (!PromptVisible && !EntryOpen)
            {
                return;
            }

            if (EntryOpen)
            {
                PrototypeInterfaceCursor.BeginFrame();
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

            if (PromptVisible && !EntryOpen)
            {
                Rect promptRect = new(
                    width * 0.5f - 250f,
                    height - 258f,
                    500f,
                    36f);
                PrototypeHudSkin.DrawPromptFrame(promptRect);
                GUI.Label(
                    promptRect,
                    $"{ControlSettingsManager.FormatShortcut("E", "X")}  •  " +
                    interactionPrompt.ToUpperInvariant(),
                    promptStyle);
            }

            if (EntryOpen)
            {
                DrawEntry(width, height);
                PrototypeInterfaceCursor.EndFrame();
            }

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawEntry(float width, float height)
        {
            DrawRect(
                new Rect(0f, 0f, width, height),
                new Color(0.005f, 0.003f, 0.002f, 0.58f));
            Rect panel = new(
                width * 0.5f - 430f,
                height * 0.5f - 250f,
                860f,
                500f);
            DrawRect(panel, new Color(0.42f, 0.09f, 0.04f, 1f));
            DrawRect(
                new Rect(
                    panel.x + 3f,
                    panel.y + 3f,
                    panel.width - 6f,
                    panel.height - 6f),
                new Color(0.025f, 0.016f, 0.012f, 0.98f));
            GUI.Label(
                new Rect(panel.x + 32f, panel.y + 24f,
                    panel.width - 64f, 44f),
                entry.DisplayName.ToUpperInvariant(),
                titleStyle);
            GUI.Label(
                new Rect(panel.x + 32f, panel.y + 70f,
                    panel.width - 64f, 28f),
                BuildMetadataLabel(),
                metadataStyle);
            DrawRect(
                new Rect(panel.x + 32f, panel.y + 110f,
                    panel.width - 64f, 1f),
                new Color(0.42f, 0.09f, 0.04f, 0.9f));

            Rect viewport = new(
                panel.x + 32f,
                panel.y + 130f,
                panel.width - 64f,
                270f);
            float bodyHeight = Mathf.Max(
                viewport.height,
                bodyStyle.CalcHeight(
                    new GUIContent(entry.Body),
                    viewport.width - 20f));
            scrollPosition = GUI.BeginScrollView(
                viewport,
                scrollPosition,
                new Rect(0f, 0f, viewport.width - 20f, bodyHeight));
            GUI.Label(
                new Rect(0f, 0f, viewport.width - 20f, bodyHeight),
                entry.Body,
                bodyStyle);
            GUI.EndScrollView();

            Rect closeRect = new(
                panel.center.x - 130f,
                panel.yMax - 68f,
                260f,
                38f);
            PrototypeInterfaceCursor.RegisterInteractive(closeRect);
            if (GUI.Button(
                closeRect,
                "FERMER",
                actionStyle))
            {
                CloseEntry();
            }
        }

        private string BuildMetadataLabel()
        {
            string kindLabel = entry.Kind switch
            {
                NarrativeEntryKind.TerminalReport => "RAPPORT DE TERMINAL",
                NarrativeEntryKind.WrittenNote => "NOTE ÉCRITE",
                NarrativeEntryKind.AudioLog => "ENREGISTREMENT",
                NarrativeEntryKind.Examination => "OBSERVATION",
                _ => "ARCHIVE"
            };
            return string.IsNullOrWhiteSpace(entry.SourceDisplayName)
                ? kindLabel
                : $"{kindLabel}  •  {entry.SourceDisplayName.ToUpperInvariant()}";
        }

        private bool IsPlayerAimingAtEntry()
        {
            if (playerCamera == null || interactionCollider == null)
            {
                return false;
            }

            Vector3 targetPoint = screenRenderer != null
                ? screenRenderer.bounds.center
                : interactionCollider.bounds.center;
            Vector3 offset = targetPoint - playerCamera.transform.position;
            float distance = offset.magnitude;
            return distance <= interactionRange &&
                distance > 0.01f &&
                Vector3.Dot(
                    playerCamera.transform.forward,
                    offset / distance) >= minimumAimDot;
        }

        private void CacheReferences()
        {
            interactionCollider ??= GetComponent<Collider>();
            screenRenderer ??= GetComponent<Renderer>();
            playerCamera ??= Camera.main;
            if (playerCamera == null)
            {
                return;
            }

            Transform playerRoot = playerCamera.transform.root;
            inputReader ??= playerRoot.GetComponent<PlayerInputReader>();
            interfaceCoordinator ??=
                playerRoot.GetComponent<PrototypeInterfaceCoordinator>();
        }

        private void UpdateScreenColor()
        {
            if (screenRenderer == null || propertyBlock == null)
            {
                return;
            }

            screenRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(
                BaseColorId,
                PromptVisible || EntryOpen ? activeColor : idleColor);
            screenRenderer.SetPropertyBlock(propertyBlock);
        }

        private void EnsureStyles()
        {
            promptStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.8f, 0.86f, 0.73f, 1f) }
            };
            titleStyle ??= new GUIStyle(promptStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 28,
                normal = { textColor = new Color(0.94f, 0.28f, 0.08f, 1f) }
            };
            metadataStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.68f, 0.52f, 0.39f, 1f) }
            };
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 18,
                wordWrap = true,
                richText = false,
                normal = { textColor = new Color(0.91f, 0.84f, 0.73f, 1f) }
            };
            actionStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.94f, 0.64f, 0.33f, 1f) }
            };

            RemoveHoverFeedback(promptStyle);
            RemoveHoverFeedback(titleStyle);
            RemoveHoverFeedback(metadataStyle);
            RemoveHoverFeedback(bodyStyle);
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
