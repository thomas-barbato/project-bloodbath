using System;
using System.Collections.Generic;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Settings;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader), typeof(CharacterInventory))]
    [RequireComponent(typeof(CharacterEquipment), typeof(CharacterStatistics))]
    public sealed class PrototypeCharacterPanel :
        MonoBehaviour,
        IPrototypeModalView
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private CharacterInventory inventory;
        [SerializeField] private CharacterEquipment equipment;
        [SerializeField] private CharacterStatistics statistics;
        [SerializeField] private CharacterSecondaryStatistics secondaryStatistics;
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
        private GUIStyle rowStyle;
        private GUIStyle valueStyle;
        private GUIStyle buttonStyle;
        private GUIStyle feedbackStyle;
        private int selectedIndex;
        private string feedbackMessage = string.Empty;
        private float feedbackUntil;
        private float nextNavigationTime;
        private int firstVisibleInventoryIndex;

        private const int VisibleInventoryRows = 4;

        public bool IsOpen { get; private set; }
        public int SelectedIndex => selectedIndex;

        private int TotalSelectableRows =>
            (statistics?.Statistics.Count ?? 0) +
            (inventory?.Items.Count ?? 0) +
            Enum.GetValues(typeof(EquipmentSlot)).Length;

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

            if (inputReader.ConsumeInventoryPressed())
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
                ActivateSelection();
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
                interfaceCoordinator?.Open(this);
                ApplyOpenState(true);
                if (interfaceCoordinator == null)
                {
                    ApplyFallbackInputState(true);
                }
                return;
            }

            ApplyOpenState(false);
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
            ApplyOpenState(false);
        }

        private void ApplyOpenState(bool open)
        {
            IsOpen = open;
            nextNavigationTime = 0f;
            selectedIndex = Mathf.Clamp(
                selectedIndex,
                0,
                Mathf.Max(0, TotalSelectableRows - 1));
        }

        private void ApplyFallbackInputState(bool open)
        {
            inputReader?.SetGameplaySuppressed(open);
            Cursor.lockState = open
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        public bool TryIncreaseStat(CharacterStatDefinition definition)
        {
            bool increased =
                statistics != null &&
                statistics.TrySpendAttributePoints(definition, 1);
            SetFeedback(increased
                ? $"{definition.DisplayName.ToUpperInvariant()} AUGMENTÉE"
                : "AUCUN POINT DISPONIBLE");
            return increased;
        }

        public bool TryEquip(WorldPickupDefinition item)
        {
            bool equipped = equipment != null && equipment.TryEquip(item);
            if (equipped)
            {
                SetFeedback($"ÉQUIPÉ : {item.DisplayName.ToUpperInvariant()}");
                return true;
            }

            EquipmentStatRequirement missing =
                equipment?.LastFailedRequirement;
            SetFeedback(missing?.Statistic == null
                ? "CET OBJET NE PEUT PAS ÊTRE ÉQUIPÉ"
                : $"{missing.Statistic.DisplayName.ToUpperInvariant()} " +
                  $"{missing.MinimumValue} REQUIS");
            return false;
        }

        public bool TryUnequip(EquipmentSlot slot)
        {
            WorldPickupDefinition item = equipment?.GetEquippedItem(slot);
            bool unequipped = equipment != null && equipment.TryUnequip(slot);
            SetFeedback(unequipped
                ? $"RETIRÉ : {item.DisplayName.ToUpperInvariant()}"
                : "EMPLACEMENT VIDE");
            return unequipped;
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
            bool previousEnabled = GUI.enabled;

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
                width * 0.5f - 500f,
                height * 0.5f - 370f,
                1000f,
                740f);
            DrawPanel(panel);
            GUI.Label(
                new Rect(panel.x + 28f, panel.y + 20f, 650f, 40f),
                "DOSSIER DU PERSONNAGE",
                titleStyle);
            GUI.Label(
                new Rect(panel.x + 700f, panel.y + 24f, 270f, 30f),
                $"{ControlSettingsManager.FormatShortcut("TAB", "START")}  •  FERMER",
                valueStyle);

            DrawStatistics(panel);
            DrawInventoryAndEquipment(panel);

            if (Time.unscaledTime < feedbackUntil)
            {
                GUI.Label(
                    new Rect(panel.x + 30f, panel.yMax - 50f,
                        panel.width - 60f, 28f),
                    feedbackMessage,
                    feedbackStyle);
            }

            GUI.enabled = previousEnabled;
            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawStatistics(Rect panel)
        {
            Rect area = new(panel.x + 28f, panel.y + 78f, 444f, 598f);
            DrawInnerPanel(area);
            GUI.Label(new Rect(area.x + 18f, area.y + 14f, 300f, 28f),
                "STATISTIQUES PRINCIPALES", sectionStyle);
            GUI.Label(new Rect(area.x + 295f, area.y + 15f, 130f, 24f),
                $"{statistics.UnspentAttributePoints} POINTS",
                valueStyle);

            int rowIndex = 0;
            float y = area.y + 56f;
            foreach (CharacterStatValue value in statistics.Statistics)
            {
                DrawSelectableRow(
                    new Rect(area.x + 16f, y, area.width - 32f, 42f),
                    rowIndex,
                    value.Definition.DisplayName.ToUpperInvariant(),
                    value.BaseValue.ToString(),
                    statistics.UnspentAttributePoints > 0,
                    () => TryIncreaseStat(value.Definition),
                    "+");
                rowIndex++;
                y += 48f;
            }

            GUI.Label(new Rect(area.x + 18f, y + 20f, 300f, 28f),
                "STATISTIQUES SECONDAIRES", sectionStyle);
            float damageMultiplier = secondaryStatistics?.GetValue(
                "outgoing_damage_multiplier",
                1f) ?? 1f;
            GUI.Label(new Rect(area.x + 18f, y + 58f, 260f, 26f),
                "DÉGÂTS INFLIGÉS", rowStyle);
            GUI.Label(new Rect(area.x + 280f, y + 58f, 128f, 26f),
                $"{damageMultiplier * 100f:0}%", valueStyle);
        }

        private void DrawInventoryAndEquipment(Rect panel)
        {
            int statCount = statistics.Statistics.Count;
            Rect inventoryArea = new(
                panel.x + 492f,
                panel.y + 78f,
                480f,
                272f);
            DrawInnerPanel(inventoryArea);
            GUI.Label(new Rect(inventoryArea.x + 18f,
                inventoryArea.y + 14f, 300f, 28f),
                "INVENTAIRE", sectionStyle);

            List<WorldPickupDefinition> items = new(inventory.Items);
            float y = inventoryArea.y + 54f;
            if (items.Count == 0)
            {
                GUI.Label(new Rect(inventoryArea.x + 18f, y, 400f, 28f),
                    "AUCUN OBJET ÉQUIPABLE", rowStyle);
            }

            UpdateVisibleInventoryPage(statCount, items.Count);
            if (items.Count > VisibleInventoryRows)
            {
                int visibleEnd = Mathf.Min(
                    items.Count,
                    firstVisibleInventoryIndex + VisibleInventoryRows);
                GUI.Label(new Rect(inventoryArea.x + 310f,
                    inventoryArea.y + 15f, 150f, 24f),
                    $"{firstVisibleInventoryIndex + 1}-{visibleEnd} / {items.Count}",
                    valueStyle);
            }

            int visibleItemCount = Mathf.Min(
                items.Count,
                firstVisibleInventoryIndex + VisibleInventoryRows);
            for (int index = firstVisibleInventoryIndex;
                 index < visibleItemCount;
                 index++)
            {
                WorldPickupDefinition item = items[index];
                bool canEquip = item.Equipment != null;
                DrawSelectableRow(
                    new Rect(inventoryArea.x + 16f, y,
                        inventoryArea.width - 32f, 42f),
                    statCount + index,
                    item.DisplayName.ToUpperInvariant(),
                    canEquip ? GetSlotLabel(item.Equipment.Slot) : "OBJET",
                    canEquip,
                    () => TryEquip(item),
                    "ÉQUIPER");
                y += 48f;
            }

            Rect equipmentArea = new(
                panel.x + 492f,
                panel.y + 366f,
                480f,
                310f);
            DrawInnerPanel(equipmentArea);
            GUI.Label(new Rect(equipmentArea.x + 18f,
                equipmentArea.y + 14f, 300f, 28f),
                "ÉQUIPEMENT", sectionStyle);

            Array slots = Enum.GetValues(typeof(EquipmentSlot));
            y = equipmentArea.y + 52f;
            for (int index = 0; index < slots.Length; index++)
            {
                EquipmentSlot slot = (EquipmentSlot)slots.GetValue(index);
                WorldPickupDefinition equippedItem =
                    equipment.GetEquippedItem(slot);
                DrawSelectableRow(
                    new Rect(equipmentArea.x + 16f, y,
                        equipmentArea.width - 32f, 28f),
                    statCount + inventory.Items.Count + index,
                    GetSlotLabel(slot),
                    equippedItem == null
                        ? "VIDE"
                        : equippedItem.DisplayName.ToUpperInvariant(),
                    equippedItem != null,
                    () => TryUnequip(slot),
                    "RETIRER");
                y += 30f;
            }
        }

        private void DrawSelectableRow(
            Rect rect,
            int rowIndex,
            string label,
            string value,
            bool actionEnabled,
            Action action,
            string actionLabel)
        {
            Color rowColor = selectedIndex == rowIndex
                ? new Color(0.26f, 0.055f, 0.02f, 0.95f)
                : new Color(0.055f, 0.028f, 0.018f, 0.92f);
            DrawRect(rect, rowColor);
            float actionLeft = rect.xMax - 112f;
            float valueLeft = rect.x + rect.width * 0.39f;
            GUI.Label(new Rect(rect.x + 10f, rect.y + 5f,
                valueLeft - rect.x - 18f, rect.height - 8f),
                selectedIndex == rowIndex ? $"> {label}" : label,
                rowStyle);
            GUI.Label(new Rect(valueLeft, rect.y + 5f,
                actionLeft - valueLeft - 8f, rect.height - 8f),
                value,
                valueStyle);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = actionEnabled;
            if (GUI.Button(new Rect(
                actionLeft,
                rect.y + 4f,
                104f,
                rect.height - 8f),
                actionLabel,
                buttonStyle))
            {
                selectedIndex = rowIndex;
                action?.Invoke();
            }
            GUI.enabled = previousEnabled;
        }

        private void ActivateSelection()
        {
            int statCount = statistics.Statistics.Count;
            if (selectedIndex < statCount)
            {
                TryIncreaseStat(
                    statistics.Statistics[selectedIndex].Definition);
                return;
            }

            int inventoryIndex = selectedIndex - statCount;
            if (inventoryIndex < inventory.Items.Count)
            {
                TryEquip(inventory.Items[inventoryIndex]);
                ClampSelection();
                return;
            }

            int slotIndex = inventoryIndex - inventory.Items.Count;
            Array slots = Enum.GetValues(typeof(EquipmentSlot));
            if (slotIndex >= 0 && slotIndex < slots.Length)
            {
                TryUnequip((EquipmentSlot)slots.GetValue(slotIndex));
                ClampSelection();
            }
        }

        private void MoveSelection(int direction)
        {
            int total = TotalSelectableRows;
            if (total <= 0)
            {
                selectedIndex = 0;
                return;
            }

            selectedIndex = (selectedIndex + direction + total) % total;
        }

        private void ClampSelection()
        {
            selectedIndex = Mathf.Clamp(
                selectedIndex,
                0,
                Mathf.Max(0, TotalSelectableRows - 1));
        }

        private void UpdateVisibleInventoryPage(
            int statCount,
            int itemCount)
        {
            if (itemCount <= VisibleInventoryRows)
            {
                firstVisibleInventoryIndex = 0;
                return;
            }

            int inventoryIndex = selectedIndex - statCount;
            if (inventoryIndex >= 0 && inventoryIndex < itemCount)
            {
                if (inventoryIndex < firstVisibleInventoryIndex)
                {
                    firstVisibleInventoryIndex = inventoryIndex;
                }
                else if (
                    inventoryIndex >=
                    firstVisibleInventoryIndex + VisibleInventoryRows)
                {
                    firstVisibleInventoryIndex =
                        inventoryIndex - VisibleInventoryRows + 1;
                }
            }

            firstVisibleInventoryIndex = Mathf.Clamp(
                firstVisibleInventoryIndex,
                0,
                itemCount - VisibleInventoryRows);
        }

        private void SetFeedback(string message)
        {
            feedbackMessage = message;
            feedbackUntil = Time.unscaledTime + 2.5f;
        }

        private void CacheReferences()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (inventory == null)
            {
                inventory = GetComponent<CharacterInventory>();
            }

            if (equipment == null)
            {
                equipment = GetComponent<CharacterEquipment>();
            }

            if (statistics == null)
            {
                statistics = GetComponent<CharacterStatistics>();
            }

            if (secondaryStatistics == null)
            {
                secondaryStatistics =
                    GetComponent<CharacterSecondaryStatistics>();
            }

            if (interfaceCoordinator == null)
            {
                interfaceCoordinator =
                    GetComponent<PrototypeInterfaceCoordinator>();
            }
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
            rowStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = textColor }
            };
            valueStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = textColor }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            feedbackStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = accentColor }
            };
        }

        private void DrawPanel(Rect rect)
        {
            DrawRect(rect, borderColor);
            DrawRect(new Rect(rect.x + 3f, rect.y + 3f,
                rect.width - 6f, rect.height - 6f), panelColor);
        }

        private void DrawInnerPanel(Rect rect)
        {
            DrawRect(rect, new Color(
                borderColor.r,
                borderColor.g,
                borderColor.b,
                0.72f));
            DrawRect(new Rect(rect.x + 2f, rect.y + 2f,
                rect.width - 4f, rect.height - 4f),
                new Color(0.015f, 0.01f, 0.008f, 0.98f));
        }

        private static string GetSlotLabel(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Head => "TÊTE",
                EquipmentSlot.Torso => "TORSE",
                EquipmentSlot.Hands => "MAINS",
                EquipmentSlot.Legs => "JAMBES",
                EquipmentSlot.Feet => "PIEDS",
                EquipmentSlot.Implant => "IMPLANT",
                EquipmentSlot.Amulet => "AMULETTE",
                EquipmentSlot.Ring => "ANNEAU",
                _ => slot.ToString().ToUpperInvariant()
            };
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
