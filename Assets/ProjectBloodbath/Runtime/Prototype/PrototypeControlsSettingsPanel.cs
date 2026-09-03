using ProjectBloodbath.Input;
using ProjectBloodbath.Settings;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(ControlSettingsManager))]
    public sealed class PrototypeControlsSettingsPanel :
        MonoBehaviour,
        IPrototypeModalView
    {
        private enum ControlCategory
        {
            Combat,
            Movement,
            Interface
        }

        private readonly struct BindingDescriptor
        {
            public BindingDescriptor(
                string label,
                string actionName,
                string compositePart = "")
            {
                Label = label;
                ActionName = actionName;
                CompositePart = compositePart;
            }

            public string Label { get; }
            public string ActionName { get; }
            public string CompositePart { get; }
        }

        private const int DeviceRowIndex = 0;
        private const int GamepadEnabledRowIndex = 1;
        private const int LayoutRowIndex = 2;
        private const int CategoryRowIndex = 3;
        private const int SensitivityRowIndex = 4;
        private const int InvertYRowIndex = 5;
        private const int FirstBindingRowIndex = 6;

        private static readonly BindingDescriptor[] CombatBindings =
        {
            new("MAIN DROITE", "Attack"),
            new("MAIN GAUCHE", "UseLeftHand"),
            new("RECHARGER", "Reload"),
            new("COMPÉTENCE 1", "Ability1"),
            new("ARME À DISTANCE", "SelectRanged"),
            new("ARME DE MÊLÉE", "SelectMelee")
        };

        private static readonly BindingDescriptor[] KeyboardMovementBindings =
        {
            new("AVANCER", "Move", "up"),
            new("RECULER", "Move", "down"),
            new("ALLER À GAUCHE", "Move", "left"),
            new("ALLER À DROITE", "Move", "right"),
            new("SAUTER", "Jump"),
            new("SPRINTER", "Sprint"),
            new("GLISSER", "Slide")
        };

        private static readonly BindingDescriptor[] GamepadMovementBindings =
        {
            new("SAUTER", "Jump"),
            new("SPRINTER", "Sprint"),
            new("GLISSER", "Slide")
        };

        private static readonly BindingDescriptor[] InterfaceBindings =
        {
            new("INTERAGIR", "Interact"),
            new("PERSONNAGE", "Inventory"),
            new("JOURNAL", "QuestJournal"),
            new("CARTE", "WorldMap"),
            new("MENU", "Options")
        };

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private ControlSettingsManager controlSettings;
        [SerializeField] private Color backdropColor =
            new(0.005f, 0.003f, 0.002f, 0.86f);
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
        private GUIStyle sectionStyle;
        private GUIStyle rowLabelStyle;
        private GUIStyle valueStyle;
        private GUIStyle arrowStyle;
        private GUIStyle actionStyle;
        private GUIStyle selectedActionStyle;
        private GUIStyle footerStyle;
        private PrototypeSystemMenu returnMenu;
        private ControlDeviceProfile deviceProfile;
        private ControlCategory category;
        private int selectedIndex;
        private float nextNavigationTime;
        private int ignoreMenuInputThroughFrame = -1;
        private string statusMessage = string.Empty;

        public bool IsOpen { get; private set; }
        public int SelectedIndex => selectedIndex;
        public ControlSettingsManager ControlSettings => controlSettings;

        private BindingDescriptor[] CurrentBindings => category switch
        {
            ControlCategory.Combat => CombatBindings,
            ControlCategory.Movement =>
                deviceProfile == ControlDeviceProfile.Gamepad
                    ? GamepadMovementBindings
                    : KeyboardMovementBindings,
            _ => InterfaceBindings
        };

        private int ResetRowIndex =>
            FirstBindingRowIndex + CurrentBindings.Length;
        private int ApplyRowIndex => ResetRowIndex + 1;
        private int CancelRowIndex => ResetRowIndex + 2;
        private int TotalRowCount => ResetRowIndex + 3;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnDisable()
        {
            if (IsOpen)
            {
                controlSettings?.CancelPending();
                IsOpen = false;
                returnMenu = null;
            }
        }

        private void Update()
        {
            if (!IsOpen || inputReader == null)
            {
                return;
            }

            if (controlSettings?.IsRebinding == true)
            {
                inputReader.ConsumeMenuCancelPressed();
                inputReader.ConsumeMenuNavigatePressed();
                inputReader.ConsumeMenuSubmitPressed();
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

        public void OpenFromSystemMenu(PrototypeSystemMenu systemMenu)
        {
            returnMenu = systemMenu;
            SetOpen(true);
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
                controlSettings?.BeginEditing();
                selectedIndex = DeviceRowIndex;
                deviceProfile = ControlDeviceProfile.KeyboardMouse;
                category = ControlCategory.Combat;
                statusMessage = string.Empty;
                nextNavigationTime = 0f;
                ignoreMenuInputThroughFrame = Time.frameCount + 1;
                interfaceCoordinator?.Open(this);
                IsOpen = true;
                if (interfaceCoordinator == null)
                {
                    ApplyFallbackInputState(true);
                }
                return;
            }

            controlSettings?.CancelPending();
            CloseAfterDecision();
        }

        public void CloseFromCoordinator()
        {
            controlSettings?.CancelPending();
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
            statusMessage = string.Empty;
        }

        public void AdjustSelected(int direction)
        {
            if (direction == 0 || controlSettings == null)
            {
                return;
            }

            switch (selectedIndex)
            {
                case DeviceRowIndex:
                    deviceProfile =
                        deviceProfile == ControlDeviceProfile.KeyboardMouse
                            ? ControlDeviceProfile.Gamepad
                            : ControlDeviceProfile.KeyboardMouse;
                    ClampSelection();
                    break;
                case GamepadEnabledRowIndex:
                    controlSettings.ToggleGamepadEnabled();
                    break;
                case LayoutRowIndex:
                    if (deviceProfile == ControlDeviceProfile.KeyboardMouse)
                    {
                        controlSettings.CycleKeyboardLayout(direction);
                    }
                    break;
                case CategoryRowIndex:
                    int categoryCount = 3;
                    category = (ControlCategory)(
                        ((int)category + direction + categoryCount) %
                        categoryCount);
                    ClampSelection();
                    break;
                case SensitivityRowIndex:
                    if (deviceProfile == ControlDeviceProfile.KeyboardMouse)
                    {
                        controlSettings.ChangeMouseSensitivity(direction);
                    }
                    else
                    {
                        controlSettings.ChangeGamepadLookSpeed(direction);
                    }
                    break;
                case InvertYRowIndex:
                    controlSettings.ToggleInvertY(deviceProfile);
                    break;
            }
        }

        public void ActivateSelected()
        {
            if (selectedIndex < FirstBindingRowIndex)
            {
                AdjustSelected(1);
                return;
            }

            int bindingIndex = selectedIndex - FirstBindingRowIndex;
            if (bindingIndex >= 0 &&
                bindingIndex < CurrentBindings.Length)
            {
                BeginSelectedRebind(CurrentBindings[bindingIndex]);
                return;
            }

            if (selectedIndex == ResetRowIndex)
            {
                controlSettings?.ResetPendingToDefaults();
                statusMessage = "COMMANDES PAR DÉFAUT RESTAURÉES";
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
            controlSettings?.ApplyPending();
            CloseAfterDecision();
        }

        public void CancelAndClose()
        {
            controlSettings?.CancelPending();
            CloseAfterDecision();
        }

        private void BeginSelectedRebind(BindingDescriptor binding)
        {
            statusMessage = deviceProfile == ControlDeviceProfile.Gamepad
                ? "APPUYEZ SUR UN BOUTON — B POUR ANNULER"
                : "APPUYEZ SUR UNE TOUCHE — ÉCHAP POUR ANNULER";
            bool started = controlSettings.StartInteractiveRebind(
                binding.ActionName,
                binding.CompositePart,
                deviceProfile,
                succeeded =>
                {
                    statusMessage = succeeded
                        ? "NOUVELLE COMMANDE ATTRIBUÉE"
                        : "RÉAFFECTATION ANNULÉE";
                });
            if (!started)
            {
                statusMessage = "CETTE COMMANDE NE PEUT PAS ÊTRE MODIFIÉE";
            }
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
            if (!IsOpen || controlSettings == null)
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
                width * 0.5f - 470f,
                height * 0.5f - 490f,
                940f,
                980f);
            DrawPanel(panel);
            GUI.Label(
                new Rect(panel.x + 34f, panel.y + 20f, 520f, 44f),
                "CONTRÔLE",
                titleStyle);
            GUI.Label(
                new Rect(panel.x + 34f, panel.y + 64f,
                    panel.width - 68f, 28f),
                "CLAVIER, SOURIS ET MANETTE — PROFILS SÉPARÉS",
                subtitleStyle);

            DrawSectionHeader(panel, panel.y + 94f, "OPTIONS");

            DrawSettingRow(
                panel,
                DeviceRowIndex,
                "PÉRIPHÉRIQUE",
                deviceProfile == ControlDeviceProfile.KeyboardMouse
                    ? "CLAVIER / SOURIS"
                    : "MANETTE",
                true);
            DrawSettingRow(
                panel,
                GamepadEnabledRowIndex,
                "MANETTE ACTIVÉE",
                controlSettings.PendingGamepadEnabled ? "OUI" : "NON",
                true);
            DrawSettingRow(
                panel,
                LayoutRowIndex,
                deviceProfile == ControlDeviceProfile.KeyboardMouse
                    ? "DISPOSITION"
                    : "AFFECTATION DES MAINS",
                deviceProfile == ControlDeviceProfile.KeyboardMouse
                    ? controlSettings.PendingKeyboardLayout.ToString()
                        .ToUpperInvariant()
                    : "LT = GAUCHE  •  RT = DROITE",
                deviceProfile == ControlDeviceProfile.KeyboardMouse);
            DrawSettingRow(
                panel,
                CategoryRowIndex,
                "CATÉGORIE",
                GetCategoryLabel(),
                true);
            DrawSettingRow(
                panel,
                SensitivityRowIndex,
                deviceProfile == ControlDeviceProfile.KeyboardMouse
                    ? "SENSIBILITÉ SOURIS"
                    : "VITESSE CAMÉRA MANETTE",
                deviceProfile == ControlDeviceProfile.KeyboardMouse
                    ? $"{controlSettings.PendingMouseSensitivity:0.00} ×"
                    : $"{controlSettings.PendingGamepadLookSpeed:0}° / S",
                true);
            bool invertY = deviceProfile == ControlDeviceProfile.KeyboardMouse
                ? controlSettings.PendingInvertMouseY
                : controlSettings.PendingInvertGamepadY;
            DrawSettingRow(
                panel,
                InvertYRowIndex,
                "INVERSER L'AXE VERTICAL",
                invertY ? "OUI" : "NON",
                true);

            DrawSectionHeader(
                panel,
                panel.y + 416f,
                $"RACCOURCIS — {GetCategoryLabel()}");

            BindingDescriptor[] bindings = CurrentBindings;
            for (int index = 0; index < bindings.Length; index++)
            {
                BindingDescriptor binding = bindings[index];
                string value =
                    controlSettings.IsRebinding &&
                    selectedIndex == FirstBindingRowIndex + index
                        ? "EN ATTENTE..."
                        : controlSettings.GetBindingLabel(
                            binding.ActionName,
                            binding.CompositePart,
                            deviceProfile);
                DrawBindingRow(
                    panel,
                    FirstBindingRowIndex + index,
                    binding,
                    value);
            }

            Rect resetRect = new(
                panel.x + 48f,
                panel.y + 790f,
                260f,
                60f);
            Rect applyRect = new(
                panel.x + 340f,
                panel.y + 790f,
                260f,
                60f);
            Rect cancelRect = new(
                panel.x + 632f,
                panel.y + 790f,
                260f,
                60f);
            DrawActionButton(resetRect, "PAR DÉFAUT", ResetRowIndex);
            DrawActionButton(applyRect, "APPLIQUER", ApplyRowIndex);
            DrawActionButton(cancelRect, "ANNULER", CancelRowIndex);

            string stateLabel = !string.IsNullOrWhiteSpace(statusMessage)
                ? statusMessage
                : controlSettings.HasPendingChanges
                    ? "MODIFICATIONS NON APPLIQUÉES"
                    : "PARAMÈTRES ACTUELS";
            GUI.Label(
                new Rect(panel.x + 34f, panel.y + 866f,
                    panel.width - 68f, 32f),
                stateLabel,
                subtitleStyle);
            GUI.Label(
                new Rect(panel.x + 34f, panel.yMax - 54f,
                    panel.width - 68f, 28f),
                controlSettings.PendingGamepadEnabled
                    ? "ENTRÉE / A  •  MODIFIER     ÉCHAP / B  •  RETOUR"
                    : "ENTRÉE  •  MODIFIER     ÉCHAP  •  RETOUR",
                footerStyle);

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawSettingRow(
            Rect panel,
            int index,
            string label,
            string value,
            bool adjustable)
        {
            Rect row = GetRowRect(panel, index);
            DrawRowBackground(row, index);
            GUI.Label(
                new Rect(row.x + 14f, row.y + 2f, 320f, 40f),
                label,
                rowLabelStyle);
            if (adjustable && GUI.Button(
                    new Rect(row.x + 350f, row.y + 3f, 42f, 38f),
                    "‹",
                    arrowStyle))
            {
                selectedIndex = index;
                AdjustSelected(-1);
            }
            GUI.Label(
                new Rect(row.x + 400f, row.y + 2f, 350f, 40f),
                value,
                valueStyle);
            if (adjustable && GUI.Button(
                    new Rect(row.xMax - 56f, row.y + 3f, 42f, 38f),
                    "›",
                    arrowStyle))
            {
                selectedIndex = index;
                AdjustSelected(1);
            }
        }

        private void DrawBindingRow(
            Rect panel,
            int index,
            BindingDescriptor binding,
            string value)
        {
            Rect row = GetRowRect(panel, index);
            DrawRowBackground(row, index);
            GUI.Label(
                new Rect(row.x + 14f, row.y + 2f, 350f, 40f),
                binding.Label,
                rowLabelStyle);
            if (GUI.Button(
                new Rect(row.x + 400f, row.y + 3f, 400f, 38f),
                value,
                valueStyle))
            {
                selectedIndex = index;
                ActivateSelected();
            }
        }

        private void DrawActionButton(Rect rect, string label, int index)
        {
            if (GUI.Button(
                rect,
                label,
                selectedIndex == index
                    ? selectedActionStyle
                    : actionStyle))
            {
                selectedIndex = index;
                ActivateSelected();
            }
        }

        private Rect GetRowRect(Rect panel, int index)
        {
            float rowY = index < FirstBindingRowIndex
                ? panel.y + 124f + index * 48f
                : panel.y + 448f +
                    (index - FirstBindingRowIndex) * 48f;
            return new Rect(
                panel.x + 52f,
                rowY,
                panel.width - 104f,
                44f);
        }

        private void DrawSectionHeader(Rect panel, float y, string label)
        {
            GUI.Label(
                new Rect(panel.x + 52f, y, 250f, 24f),
                label,
                sectionStyle);
            DrawRect(
                new Rect(panel.x + 300f, y + 12f, panel.width - 352f, 2f),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.45f));
        }

        private void DrawRowBackground(Rect row, int index)
        {
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
        }

        private string GetCategoryLabel()
        {
            return category switch
            {
                ControlCategory.Combat => "COMBAT",
                ControlCategory.Movement => "DÉPLACEMENT",
                _ => "INTERFACE"
            };
        }

        private void ClampSelection()
        {
            selectedIndex = Mathf.Clamp(
                selectedIndex,
                0,
                TotalRowCount - 1);
            statusMessage = string.Empty;
        }

        private void CacheReferences()
        {
            inputReader ??= GetComponent<PlayerInputReader>();
            interfaceCoordinator ??=
                GetComponent<PrototypeInterfaceCoordinator>();
            controlSettings ??= GetComponent<ControlSettingsManager>();
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
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.68f, 0.47f, 0.34f, 1f) }
            };
            sectionStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = accentColor }
            };
            rowLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor }
            };
            valueStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.67f, 0.32f, 1f) },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            arrowStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            actionStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            selectedActionStyle ??= new GUIStyle(actionStyle)
            {
                fontSize = 19,
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
