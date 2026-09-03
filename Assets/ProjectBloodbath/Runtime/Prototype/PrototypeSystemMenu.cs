using System;
using ProjectBloodbath.Input;
using ProjectBloodbath.Settings;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(950)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PrototypeInterfaceCoordinator))]
    public sealed class PrototypeSystemMenu :
        MonoBehaviour,
        IPrototypeModalView
    {
        private const int ResumeRowIndex = 0;
        private const int SaveAndQuitRowIndex = 1;
        private const int VideoRowIndex = 2;
        private const int SoundRowIndex = 3;
        private const int ControlsRowIndex = 4;
        private const int TotalRowCount = 5;

        private static readonly string[] EntryLabels =
        {
            "REPRENDRE",
            "SAUVEGARDER & QUITTER",
            "VIDÉO",
            "SON",
            "CONTRÔLE"
        };

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private PrototypeVideoSettingsPanel videoPanel;
        [SerializeField] private PrototypeControlsSettingsPanel controlsPanel;
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
        private GUIStyle buttonStyle;
        private GUIStyle selectedButtonStyle;
        private GUIStyle footerStyle;
        private int selectedIndex;
        private float nextNavigationTime;
        private int ignoreMenuInputThroughFrame = -1;
        private string statusMessage = string.Empty;

        public event Action SaveAndQuitRequested;

        public bool IsOpen { get; private set; }
        public int SelectedIndex => selectedIndex;
        public bool QuitRequested { get; private set; }
        public string StatusMessage => statusMessage;

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

            if (!IsOpen)
            {
                if (inputReader.ConsumeOptionsPressed())
                {
                    SetOpen(true);
                }
                return;
            }

            if (!inputReader.enabled)
            {
                SetOpen(false);
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
                SetOpen(false);
                return;
            }

            Vector2 navigation = inputReader.ConsumeMenuNavigatePressed();
            if (Time.unscaledTime >= nextNavigationTime &&
                Mathf.Abs(navigation.y) > 0.4f)
            {
                MoveSelection(navigation.y > 0f ? -1 : 1);
                nextNavigationTime = Time.unscaledTime + 0.16f;
            }

            if (inputReader.ConsumeMenuSubmitPressed())
            {
                ActivateSelected();
            }
        }

        public static string GetEntryLabel(int index)
        {
            return index >= 0 && index < EntryLabels.Length
                ? EntryLabels[index]
                : string.Empty;
        }

        public void SetOpen(bool open)
        {
            if (IsOpen == open)
            {
                return;
            }

            CacheReferences();
            IsOpen = open;
            if (open)
            {
                selectedIndex = ResumeRowIndex;
                statusMessage = string.Empty;
                nextNavigationTime = 0f;
                ignoreMenuInputThroughFrame = Time.frameCount + 1;
                interfaceCoordinator?.Open(this);
                if (interfaceCoordinator == null)
                {
                    ApplyFallbackInputState(true);
                }
                return;
            }

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

        public void MoveSelection(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            selectedIndex =
                (selectedIndex + direction + TotalRowCount) % TotalRowCount;
            statusMessage = string.Empty;
        }

        public void ActivateSelected()
        {
            switch (selectedIndex)
            {
                case ResumeRowIndex:
                    SetOpen(false);
                    break;
                case SaveAndQuitRowIndex:
                    RequestSaveAndQuit();
                    break;
                case VideoRowIndex:
                    statusMessage = string.Empty;
                    videoPanel?.OpenFromSystemMenu(this);
                    break;
                case SoundRowIndex:
                    statusMessage =
                        "LES RÉGLAGES DE SON SERONT AJOUTÉS PROCHAINEMENT";
                    break;
                case ControlsRowIndex:
                    statusMessage = string.Empty;
                    controlsPanel?.OpenFromSystemMenu(this);
                    break;
            }
        }

        private void RequestSaveAndQuit()
        {
            PlayerPrefs.Save();
            QuitRequested = true;
            SaveAndQuitRequested?.Invoke();

#if UNITY_EDITOR
            statusMessage =
                "QUITTERA LA PARTIE UNE FOIS LA SAUVEGARDE DE JEU CONNECTÉE";
#else
            Application.Quit();
#endif
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
            float scale = PrototypeVideoSettingsPanel.CalculateInterfaceScale(
                Screen.width,
                Screen.height);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            DrawRect(new Rect(0f, 0f, width, height), backdropColor);
            Rect panel = new(
                width * 0.5f - 330f,
                height * 0.5f - 380f,
                660f,
                760f);
            DrawPanel(panel);
            GUI.Label(
                new Rect(panel.x + 42f, panel.y + 34f,
                    panel.width - 84f, 52f),
                "MENU",
                titleStyle);
            GUI.Label(
                new Rect(panel.x + 42f, panel.y + 88f,
                    panel.width - 84f, 30f),
                "LA SIMULATION CONTINUE PENDANT CE MENU",
                subtitleStyle);

            for (int index = 0; index < TotalRowCount; index++)
            {
                Rect buttonRect = new(
                    panel.x + 70f,
                    panel.y + 145f + index * 88f,
                    panel.width - 140f,
                    66f);
                if (GUI.Button(
                    buttonRect,
                    EntryLabels[index],
                    selectedIndex == index
                        ? selectedButtonStyle
                        : buttonStyle))
                {
                    selectedIndex = index;
                    ActivateSelected();
                }
            }

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                GUI.Label(
                    new Rect(panel.x + 42f, panel.y + 600f,
                        panel.width - 84f, 54f),
                    statusMessage,
                    subtitleStyle);
            }
            GUI.Label(
                new Rect(panel.x + 32f, panel.yMax - 62f,
                    panel.width - 64f, 30f),
                $"{ControlSettingsManager.FormatShortcut("ENTRÉE", "A")}  •  VALIDER     " +
                $"{ControlSettingsManager.FormatShortcut("ÉCHAP", "B")}  •  REPRENDRE",
                footerStyle);

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void CacheReferences()
        {
            inputReader ??= GetComponent<PlayerInputReader>();
            interfaceCoordinator ??=
                GetComponent<PrototypeInterfaceCoordinator>();
            videoPanel ??= GetComponent<PrototypeVideoSettingsPanel>();
            controlsPanel ??=
                GetComponent<PrototypeControlsSettingsPanel>();
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
                alignment = TextAnchor.MiddleCenter,
                fontSize = 38,
                fontStyle = FontStyle.Bold,
                normal = { textColor = accentColor }
            };
            subtitleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.72f, 0.5f, 0.35f, 1f) }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            selectedButtonStyle ??= new GUIStyle(buttonStyle)
            {
                fontSize = 22,
                normal = { textColor = accentColor }
            };
            footerStyle ??= new GUIStyle(subtitleStyle)
            {
                fontSize = 15,
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
