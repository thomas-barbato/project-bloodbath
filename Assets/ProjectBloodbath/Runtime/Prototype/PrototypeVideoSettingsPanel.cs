using ProjectBloodbath.Input;
using ProjectBloodbath.Player;
using ProjectBloodbath.Settings;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(VideoSettingsManager))]
    [RequireComponent(typeof(PlayerViewSettings))]
    public sealed class PrototypeVideoSettingsPanel :
        MonoBehaviour,
        IPrototypeModalView
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const int SettingRowCount = 7;
        private const int ApplyRowIndex = 7;
        private const int CancelRowIndex = 8;
        private const int TotalRowCount = 9;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private VideoSettingsManager videoSettings;
        [SerializeField] private PlayerViewSettings playerViewSettings;
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
        private GUIStyle subtitleStyle;
        private GUIStyle rowLabelStyle;
        private GUIStyle valueStyle;
        private GUIStyle arrowStyle;
        private GUIStyle actionStyle;
        private GUIStyle selectedActionStyle;
        private GUIStyle footerStyle;
        private int selectedIndex;
        private float nextNavigationTime;
        private int ignoreMenuInputThroughFrame = -1;
        private PrototypeSystemMenu returnMenu;

        public bool IsOpen { get; private set; }
        public int SelectedIndex => selectedIndex;
        public VideoSettingsManager VideoSettings => videoSettings;
        public PlayerViewSettings PlayerViewSettings => playerViewSettings;

        public static float CalculateInterfaceScale(int width, int height)
        {
            return Mathf.Max(
                0.5f,
                Mathf.Min(
                    width / ReferenceWidth,
                    height / ReferenceHeight));
        }

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

            bool optionsPressed = inputReader.ConsumeOptionsPressed();
            if (!IsOpen &&
                optionsPressed &&
                GetComponent<PrototypeSystemMenu>() == null)
            {
                SetOpen(true);
                return;
            }

            if (!IsOpen)
            {
                return;
            }

            if (!inputReader.enabled)
            {
                CancelAndClose();
                return;
            }

            if (Time.frameCount <= ignoreMenuInputThroughFrame)
            {
                inputReader.ConsumeMenuCancelPressed();
                inputReader.ConsumeMenuNavigatePressed();
                inputReader.ConsumeMenuSubmitPressed();
                return;
            }

            if (inputReader.ConsumeMenuCancelPressed())
            {
                CancelAndClose();
                return;
            }

            Vector2 navigation = inputReader.ConsumeMenuNavigatePressed();
            if (Time.unscaledTime >= nextNavigationTime)
            {
                if (Mathf.Abs(navigation.y) > 0.4f)
                {
                    MoveSelection(navigation.y > 0f ? -1 : 1);
                    nextNavigationTime = Time.unscaledTime + 0.16f;
                }
                else if (Mathf.Abs(navigation.x) > 0.4f)
                {
                    AdjustSelected(navigation.x > 0f ? 1 : -1);
                    nextNavigationTime = Time.unscaledTime + 0.16f;
                }
            }

            if (inputReader.ConsumeMenuSubmitPressed())
            {
                ActivateSelected();
            }
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
                videoSettings?.BeginEditing();
                playerViewSettings?.BeginEditing();
                interfaceCoordinator?.Open(this);
                IsOpen = true;
                selectedIndex = 0;
                nextNavigationTime = 0f;
                ignoreMenuInputThroughFrame = Time.frameCount + 1;
                if (interfaceCoordinator == null)
                {
                    ApplyFallbackInputState(true);
                }
                return;
            }

            videoSettings?.CancelPending();
            playerViewSettings?.CancelPending();
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

        public void OpenFromSystemMenu(PrototypeSystemMenu systemMenu)
        {
            returnMenu = systemMenu;
            SetOpen(true);
        }

        public void CloseFromCoordinator()
        {
            videoSettings?.CancelPending();
            playerViewSettings?.CancelPending();
            IsOpen = false;
            returnMenu = null;
        }

        public void MoveSelection(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            selectedIndex =
                (selectedIndex + direction + TotalRowCount) % TotalRowCount;
        }

        public void AdjustSelected(int direction)
        {
            if (videoSettings == null || direction == 0)
            {
                return;
            }

            switch (selectedIndex)
            {
                case 0:
                    videoSettings.CycleDisplayMode(direction);
                    break;
                case 1:
                    videoSettings.CycleResolution(direction);
                    break;
                case 2:
                    videoSettings.ToggleVSync();
                    break;
                case 3:
                    playerViewSettings?.ChangeFieldOfView(direction);
                    break;
                case 4:
                    playerViewSettings?.ChangeReticleSize(direction);
                    break;
                case 5:
                    playerViewSettings?.CycleReticleColor(direction);
                    break;
                case 6:
                    playerViewSettings?.CycleReticleShape(direction);
                    break;
            }
        }

        public void ActivateSelected()
        {
            if (selectedIndex < SettingRowCount)
            {
                AdjustSelected(1);
                return;
            }

            if (selectedIndex == ApplyRowIndex)
            {
                ApplyAndClose();
                return;
            }

            if (selectedIndex == CancelRowIndex)
            {
                CancelAndClose();
            }
        }

        public void ApplyAndClose()
        {
            videoSettings?.ApplyPending();
            playerViewSettings?.ApplyPending();
            CloseAfterDecision();
        }

        public void CancelAndClose()
        {
            videoSettings?.CancelPending();
            playerViewSettings?.CancelPending();
            CloseAfterDecision();
        }

        private void CloseAfterDecision()
        {
            if (!IsOpen)
            {
                return;
            }

            PrototypeSystemMenu menuToRestore = returnMenu;
            returnMenu = null;
            IsOpen = false;
            if (interfaceCoordinator != null)
            {
                interfaceCoordinator.Close(this);
            }
            else
            {
                ApplyFallbackInputState(false);
            }

            menuToRestore?.SetOpen(true);
        }

        private void OnGUI()
        {
            if (!IsOpen || videoSettings == null || playerViewSettings == null)
            {
                return;
            }

            EnsureStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            float scale = CalculateInterfaceScale(
                Screen.width,
                Screen.height);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            DrawRect(new Rect(0f, 0f, width, height), backdropColor);
            Rect panel = new(
                width * 0.5f - 460f,
                height * 0.5f - 430f,
                920f,
                860f);
            DrawPanel(panel);
            GUI.Label(
                new Rect(panel.x + 34f, panel.y + 24f, 520f, 44f),
                "OPTIONS D'AFFICHAGE",
                titleStyle);
            GUI.Label(
                new Rect(panel.x + 34f, panel.y + 72f,
                    panel.width - 68f, 30f),
                "AFFICHAGE, CAMÉRA ET RÉTICULE DU PROTOTYPE",
                subtitleStyle);

            DrawSettingRow(
                panel,
                0,
                "MODE D'AFFICHAGE",
                videoSettings.GetPendingDisplayModeLabel());
            DrawSettingRow(
                panel,
                1,
                "RÉSOLUTION",
                videoSettings.PendingResolution.Label);
            DrawSettingRow(
                panel,
                2,
                "SYNCHRONISATION VERTICALE",
                videoSettings.PendingVSync ? "ACTIVÉE" : "DÉSACTIVÉE");
            DrawSettingRow(
                panel,
                3,
                "CHAMP DE VISION (FOV)",
                $"{playerViewSettings.PendingFieldOfView:0}°");
            DrawSettingRow(
                panel,
                4,
                "TAILLE DU RÉTICULE",
                $"{playerViewSettings.PendingReticleSize:0.00} ×");
            Rect colorPreviewArea = DrawSettingRow(
                panel,
                5,
                "COULEUR DU RÉTICULE",
                string.Empty);
            DrawColorPreview(
                colorPreviewArea,
                PlayerViewSettings.GetReticleColor(
                    playerViewSettings.PendingReticleColor));
            Rect shapePreviewArea = DrawSettingRow(
                panel,
                6,
                "FORME DU RÉTICULE",
                string.Empty);
            PrototypeReticle.DrawPreview(
                shapePreviewArea,
                playerViewSettings.PendingReticleShape,
                PlayerViewSettings.GetReticleColor(
                    playerViewSettings.PendingReticleColor),
                1.15f);

            Rect applyRect = new(
                panel.x + 90f,
                panel.y + 650f,
                350f,
                64f);
            Rect cancelRect = new(
                panel.x + 480f,
                panel.y + 650f,
                350f,
                64f);
            if (GUI.Button(
                applyRect,
                "APPLIQUER",
                selectedIndex == ApplyRowIndex
                    ? selectedActionStyle
                    : actionStyle))
            {
                selectedIndex = ApplyRowIndex;
                ApplyAndClose();
            }
            if (GUI.Button(
                cancelRect,
                "ANNULER",
                selectedIndex == CancelRowIndex
                    ? selectedActionStyle
                    : actionStyle))
            {
                selectedIndex = CancelRowIndex;
                CancelAndClose();
            }

            string stateLabel =
                videoSettings.HasPendingChanges ||
                playerViewSettings.HasPendingChanges
                ? "MODIFICATIONS NON APPLIQUÉES"
                : "PARAMÈTRES ACTUELS";
            GUI.Label(
                new Rect(panel.x + 34f, panel.y + 734f,
                    panel.width - 68f, 26f),
                stateLabel,
                subtitleStyle);
            GUI.Label(
                new Rect(panel.x + 34f, panel.yMax - 58f,
                    panel.width - 68f, 28f),
                $"{ControlSettingsManager.FormatShortcut("ENTRÉE", "A")}  •  VALIDER     " +
                $"{ControlSettingsManager.FormatShortcut("ÉCHAP", "B")}  •  ANNULER",
                footerStyle);

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private Rect DrawSettingRow(
            Rect panel,
            int index,
            string label,
            string value)
        {
            Rect row = new(
                panel.x + 54f,
                panel.y + 112f + index * 74f,
                panel.width - 108f,
                62f);
            bool selected = selectedIndex == index;
            DrawRect(
                row,
                selected
                    ? new Color(0.27f, 0.055f, 0.02f, 0.98f)
                    : new Color(0.055f, 0.028f, 0.018f, 0.94f));
            if (selected)
            {
                DrawRect(new Rect(row.x, row.y, 4f, row.height), accentColor);
            }

            Rect labelRect = new(
                row.x + 16f,
                row.y + 5f,
                310f,
                52f);
            if (GUI.Button(labelRect, GUIContent.none, GUIStyle.none))
            {
                selectedIndex = index;
            }
            GUI.Label(labelRect, label, rowLabelStyle);
            if (GUI.Button(
                new Rect(row.x + 340f, row.y + 8f, 46f, 46f),
                "‹",
                arrowStyle))
            {
                selectedIndex = index;
                AdjustSelected(-1);
            }
            Rect valueRect = new(
                row.x + 394f,
                row.y + 5f,
                326f,
                52f);
            GUI.Label(
                valueRect,
                value,
                valueStyle);
            if (GUI.Button(
                new Rect(row.xMax - 62f, row.y + 8f, 46f, 46f),
                "›",
                arrowStyle))
            {
                selectedIndex = index;
                AdjustSelected(1);
            }

            return valueRect;
        }

        private static void DrawColorPreview(Rect area, Color color)
        {
            Rect swatch = new(
                area.center.x - 70f,
                area.center.y - 9f,
                140f,
                18f);
            DrawRect(
                new Rect(
                    swatch.x - 2f,
                    swatch.y - 2f,
                    swatch.width + 4f,
                    swatch.height + 4f),
                new Color(0.8f, 0.67f, 0.52f, 0.8f));
            DrawRect(swatch, color);
        }

        private void CacheReferences()
        {
            inputReader ??= GetComponent<PlayerInputReader>();
            interfaceCoordinator ??=
                GetComponent<PrototypeInterfaceCoordinator>();
            videoSettings ??= GetComponent<VideoSettingsManager>();
            playerViewSettings ??= GetComponent<PlayerViewSettings>();
        }

        private void ApplyFallbackInputState(bool open)
        {
            inputReader?.SetGameplaySuppressed(open);
            Cursor.lockState = open
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        private void DrawPanel(Rect rect)
        {
            DrawRect(rect, borderColor);
            DrawRect(
                new Rect(
                    rect.x + 3f,
                    rect.y + 3f,
                    rect.width - 6f,
                    rect.height - 6f),
                panelColor);
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = accentColor }
            };
            subtitleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.62f, 0.45f, 0.34f, 1f) }
            };
            rowLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor }
            };
            valueStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.67f, 0.32f, 1f) }
            };
            arrowStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            actionStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            selectedActionStyle ??= new GUIStyle(actionStyle)
            {
                normal = { textColor = accentColor },
                fontSize = 20
            };
            footerStyle ??= new GUIStyle(subtitleStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                normal = { textColor = new Color(0.94f, 0.64f, 0.33f, 1f) }
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
