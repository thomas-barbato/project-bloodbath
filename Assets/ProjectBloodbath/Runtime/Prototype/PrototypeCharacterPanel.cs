using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    public enum CharacterInventoryFilter
    {
        All,
        Weapons,
        Armor,
        Implants,
        QuestItems
    }

    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader), typeof(CharacterInventory))]
    [RequireComponent(typeof(CharacterEquipment), typeof(CharacterStatistics))]
    [RequireComponent(typeof(CharacterIdentity), typeof(CharacterProgression))]
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
        [SerializeField] private CharacterProgression progression;
        [SerializeField] private CharacterIdentity identity;
        [SerializeField] private PrototypeWeaponLoadout weaponLoadout;
        [SerializeField] private Texture2D anatomySilhouetteTexture;
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
        private GUIStyle slotLabelStyle;
        private GUIStyle slotValueStyle;
        private GUIStyle tooltipStyle;
        private GUIStyle levelStyle;
        private GUIStyle identityStyle;
        private GUIStyle searchStyle;
        private GUIStyle pendingValueStyle;
        private int selectedIndex;
        private string feedbackMessage = string.Empty;
        private float feedbackUntil;
        private float nextNavigationTime;
        private WorldPickupDefinition pendingHandItem;
        private int inventoryPage;
        private CharacterInventoryFilter inventoryFilter;
        private InventorySortMode inventorySortMode;
        private string inventorySearch = string.Empty;
        private string tooltipText = string.Empty;
        private Rect tooltipAnchor;
        private bool tooltipUsesPointer;
        private bool focusTooltipsEnabled;
        private readonly List<WorldPickupDefinition> filteredItems = new();
        private readonly Dictionary<string, float> observedStatValues = new();
        private readonly Dictionary<string, float> changedStatUntil = new();

        private const int InventoryColumns = 6;
        private const int InventoryRows = 5;
        private const int InventoryPageSize = CharacterInventory.SlotsPerPage;
        private const int InventoryPageCount = CharacterInventory.PageCount;
        private const int VisibleEquipmentSlotCount = 10;
        private const int InventoryFilterCount = 5;
        private const int InventorySortButtonCount = 2;
        private const int SwapButtonCount = 1;

        public bool IsOpen { get; private set; }
        public int SelectedIndex => selectedIndex;
        public WorldPickupDefinition PendingHandItem => pendingHandItem;
        public CharacterInventoryFilter InventoryFilter => inventoryFilter;
        public InventorySortMode InventorySortMode => inventorySortMode;
        public string InventorySearch => inventorySearch;
        public int InventoryPage => inventoryPage;
        public bool AttributeActionsVisible =>
            statistics != null && statistics.HasPendingAttributeChanges;
        public int FilteredInventoryCount
        {
            get
            {
                RefreshFilteredItems();
                return filteredItems.Count;
            }
        }
        public string DisplayedCharacterName => identity == null
            ? "Mara Voss"
            : identity.CharacterName;
        public string DisplayedClassName => identity == null
            ? "Classe prototype"
            : identity.ClassDisplayName;

        private int TotalSelectableRows =>
            StatisticSelectableCount +
            AttributeActionCount +
            SwapButtonCount +
            VisibleEquipmentSlotCount +
            InventoryFilterCount +
            InventorySortButtonCount +
            VisibleInventoryItemCount +
            InventoryPageCount;

        private int StatisticSelectableCount =>
            (statistics?.Statistics.Count ?? 0) +
            (secondaryStatistics?.Definitions.Count ?? 0);

        private int AttributeActionCount => AttributeActionsVisible ? 2 : 0;

        private int SaveAttributesSelectableIndex => StatisticSelectableCount;
        private int CancelAttributesSelectableIndex =>
            SaveAttributesSelectableIndex + 1;
        private int SwapSelectableIndex =>
            StatisticSelectableCount + AttributeActionCount;
        private int EquipmentSelectableStart => SwapSelectableIndex + 1;
        private int FilterSelectableStart =>
            EquipmentSelectableStart + VisibleEquipmentSlotCount;
        private int SortSelectableStart =>
            FilterSelectableStart + InventoryFilterCount;
        private int InventorySelectableStart =>
            SortSelectableStart + InventorySortButtonCount;
        private int PageSelectableStart =>
            InventorySelectableStart + VisibleInventoryItemCount;

        private int VisibleInventoryItemCount
        {
            get
            {
                int first = inventoryPage * InventoryPageSize;
                return Mathf.Clamp(
                    filteredItems.Count - first,
                    0,
                    InventoryPageSize);
            }
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

            if (inputReader.ConsumeInterfaceSwapHandSetPressed())
            {
                SwapActiveHandSet();
            }

            Vector2 navigation = inputReader.ConsumeMenuNavigatePressed();
            if (navigation.sqrMagnitude > 0.16f)
            {
                focusTooltipsEnabled = true;
            }
            if (
                Mathf.Max(
                    Mathf.Abs(navigation.x),
                    Mathf.Abs(navigation.y)) > 0.4f &&
                Time.unscaledTime >= nextNavigationTime)
            {
                bool backwards = Mathf.Abs(navigation.y) >=
                    Mathf.Abs(navigation.x)
                        ? navigation.y > 0f
                        : navigation.x < 0f;
                MoveSelection(backwards ? -1 : 1);
                nextNavigationTime = Time.unscaledTime + 0.16f;
            }

            if (inputReader.ConsumeMenuSubmitPressed())
            {
                focusTooltipsEnabled = true;
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
            if (!open)
            {
                pendingHandItem = null;
            }
            RefreshFilteredItems();
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
            if (!open)
            {
                PrototypeInterfaceCursor.Reset();
            }
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

        public bool CommitAttributeDistribution()
        {
            bool committed =
                statistics != null &&
                statistics.CommitPendingAttributePoints();
            SetFeedback(committed
                ? "ATTRIBUTS SAUVEGARDÉS"
                : "AUCUNE MODIFICATION À SAUVEGARDER");
            return committed;
        }

        public bool CancelAttributeDistribution()
        {
            bool cancelled =
                statistics != null &&
                statistics.CancelPendingAttributePoints();
            SetFeedback(cancelled
                ? "DISTRIBUTION ANNULÉE"
                : "AUCUNE MODIFICATION À ANNULER");
            return cancelled;
        }

        public bool TryEquip(WorldPickupDefinition item)
        {
            bool equipped = equipment != null && equipment.TryEquip(item);
            if (equipped)
            {
                SetFeedback($"ÉQUIPÉ : {item.DisplayName.ToUpperInvariant()}");
                RefreshFilteredItems();
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

        public bool TryEquipToHand(
            WorldPickupDefinition item,
            EquipmentSlot slot)
        {
            if (item?.Equipment == null || !item.Equipment.IsHandEquipment)
            {
                SetFeedback("CET OBJET NE PEUT PAS ÊTRE PLACÉ ICI");
                return false;
            }

            return TryEquipToSlot(item, slot);
        }

        private bool TryEquipToSlot(
            WorldPickupDefinition item,
            EquipmentSlot slot)
        {
            bool equipped =
                item?.Equipment != null &&
                item.Equipment.CanEquipIn(slot) &&
                equipment != null &&
                equipment.TryEquip(item, slot);
            if (equipped)
            {
                pendingHandItem = null;
                SetFeedback(
                    $"PLACÉ : {item.DisplayName.ToUpperInvariant()} — " +
                    GetSlotLabel(slot));
                RefreshFilteredItems();
                return true;
            }

            SetFeedback("CET OBJET NE PEUT PAS ÊTRE PLACÉ ICI");
            return false;
        }

        public bool TryUnequip(EquipmentSlot slot)
        {
            WorldPickupDefinition item = equipment?.GetEquippedItem(slot);
            bool unequipped = equipment != null && equipment.TryUnequip(slot);
            if (unequipped)
            {
                RefreshFilteredItems();
            }
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

            PrototypeInterfaceCursor.BeginFrame();
            EnsureStyles();
            if (
                Event.current.isMouse &&
                (Event.current.type == EventType.MouseMove ||
                 Event.current.type == EventType.MouseDown))
            {
                focusTooltipsEnabled = false;
            }
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            bool previousEnabled = GUI.enabled;

            float scale = Mathf.Max(
                0.4f,
                Mathf.Min(
                    Screen.width / ReferenceWidth,
                    Screen.height / ReferenceHeight));
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            tooltipText = string.Empty;
            tooltipUsesPointer = false;

            DrawRect(new Rect(0f, 0f, width, height), backdropColor);
            Rect panel = new(
                width * 0.5f - 890f,
                height * 0.5f - 450f,
                1780f,
                900f);
            DrawPanel(panel);
            GUI.Label(
                new Rect(panel.x + 28f, panel.y + 17f, 1000f, 44f),
                DisplayedCharacterName.ToUpperInvariant(),
                titleStyle);
            Rect closeRect = new(
                panel.xMax - 66f,
                panel.y + 16f,
                38f,
                38f);
            if (DrawClosedButton(closeRect, "×"))
            {
                SetOpen(false);
            }
            RegisterTooltip(closeRect, "Fermer la fiche du personnage.");

            DrawLeftColumn(panel);
            DrawEquipmentColumn(panel);
            DrawInventoryColumn(panel);

            if (Time.unscaledTime < feedbackUntil)
            {
                GUI.Label(
                    new Rect(panel.x + 30f, panel.yMax - 50f,
                        panel.width - 60f, 28f),
                    feedbackMessage,
                    feedbackStyle);
            }

            DrawTooltip(panel);
            PrototypeInterfaceCursor.EndFrame();

            GUI.enabled = previousEnabled;
            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawLeftColumn(Rect panel)
        {
            Rect area = new(panel.x + 24f, panel.y + 72f, 350f, 786f);
            DrawInnerPanel(area);
            Rect content = new(
                area.x + 12f,
                area.y + 12f,
                area.width - 24f,
                area.height - 24f);
            DrawDetailedStatistics(content);
        }

        private void DrawDetailedStatistics(Rect area)
        {
            DrawRect(area, new Color(0.008f, 0.006f, 0.005f, 0.82f));
            Rect identityCard = new(
                area.x + 8f,
                area.y + 8f,
                area.width - 16f,
                112f);
            DrawClosedCell(
                identityCard,
                new Color(0.035f, 0.02f, 0.014f, 0.98f),
                new Color(accentColor.r, accentColor.g,
                    accentColor.b, 0.82f));
            GUI.Label(new Rect(identityCard.x + 14f,
                identityCard.y + 10f, 92f, 20f),
                "NIVEAU",
                slotLabelStyle);
            GUI.Label(new Rect(identityCard.x + 12f,
                identityCard.y + 25f, 96f, 66f),
                (progression == null ? 1 : progression.CurrentLevel)
                    .ToString(),
                levelStyle);
            GUI.Label(new Rect(identityCard.x + 112f,
                identityCard.y + 13f, identityCard.width - 126f, 30f),
                DisplayedClassName.ToUpperInvariant(),
                identityStyle);
            GUI.Label(new Rect(identityCard.x + 112f,
                identityCard.y + 48f, identityCard.width - 126f, 20f),
                progression == null || progression.IsAtMaximumLevel
                    ? "EXPÉRIENCE MAXIMALE"
                    : $"EXPÉRIENCE  {progression.CurrentExperience} / " +
                      progression.ExperienceRequiredForNextLevel,
                slotLabelStyle);
            Rect experienceTrack = new(
                identityCard.x + 112f,
                identityCard.y + 76f,
                identityCard.width - 130f,
                12f);
            DrawClosedCell(
                experienceTrack,
                new Color(0.012f, 0.009f, 0.007f, 1f),
                new Color(0.25f, 0.08f, 0.035f, 1f));
            float ratio = progression == null
                ? 0f
                : progression.ExperienceRatio;
            DrawRect(new Rect(experienceTrack.x + 2f,
                experienceTrack.y + 2f,
                (experienceTrack.width - 4f) * ratio,
                experienceTrack.height - 4f), accentColor);

            GUI.Label(new Rect(area.x + 14f, area.y + 136f,
                area.width - 28f, 28f),
                "ATTRIBUTS PRINCIPAUX",
                sectionStyle);
            GUI.Label(new Rect(area.x + 184f, area.y + 138f,
                area.width - 204f, 24f),
                $"{statistics.UnspentAttributePoints} POINTS",
                valueStyle);

            int rowIndex = 0;
            float y = area.y + 174f;
            foreach (CharacterStatValue value in statistics.Statistics)
            {
                Rect row = new(area.x + 10f, y, area.width - 20f, 42f);
                DrawStatisticRow(
                    row,
                    rowIndex,
                    value.Definition.DisplayName.ToUpperInvariant(),
                    statistics.GetValue(value.Definition),
                    statistics.GetPendingIncrease(value.Definition) > 0,
                    statistics.UnspentAttributePoints > 0,
                    () => TryIncreaseStat(value.Definition),
                    GetPrimaryStatTooltip(value.Definition));
                rowIndex++;
                y += 48f;
            }

            if (AttributeActionsVisible)
            {
                Rect saveRect = new(
                    area.x + 10f,
                    y + 4f,
                    148f,
                    36f);
                Rect cancelRect = new(
                    area.x + 168f,
                    y + 4f,
                    area.width - 178f,
                    36f);
                if (DrawClosedButton(
                    saveRect,
                    "SAUVEGARDER",
                    false,
                    selectedIndex == SaveAttributesSelectableIndex))
                {
                    selectedIndex = SaveAttributesSelectableIndex;
                    CommitAttributeDistribution();
                }
                if (DrawClosedButton(
                    cancelRect,
                    "ANNULER",
                    false,
                    selectedIndex == CancelAttributesSelectableIndex))
                {
                    selectedIndex = CancelAttributesSelectableIndex;
                    CancelAttributeDistribution();
                }
                RegisterTooltip(
                    saveRect,
                    "Valider définitivement les points distribués.",
                    SaveAttributesSelectableIndex);
                RegisterTooltip(
                    cancelRect,
                    "Rendre tous les points distribués depuis la dernière sauvegarde.",
                    CancelAttributesSelectableIndex);
            }

            y += 58f;
            GUI.Label(new Rect(area.x + 14f, y,
                area.width - 28f, 28f),
                "STATISTIQUES SECONDAIRES",
                sectionStyle);
            y += 38f;

            if (secondaryStatistics == null ||
                secondaryStatistics.Definitions.Count == 0)
            {
                GUI.Label(new Rect(area.x + 14f, y,
                    area.width - 28f, 28f),
                    "AUCUNE STATISTIQUE SECONDAIRE",
                    rowStyle);
                return;
            }

            foreach (SecondaryStatDefinition definition in
                secondaryStatistics.Definitions)
            {
                float currentValue = secondaryStatistics.GetValue(definition);
                Rect row = new(area.x + 10f, y, area.width - 20f, 38f);
                DrawSecondaryStatisticRow(
                    row,
                    rowIndex,
                    definition,
                    currentValue);
                rowIndex++;
                y += 43f;
            }
        }

        private void DrawEquipmentColumn(Rect panel)
        {
            Rect equipmentArea = new(
                panel.x + 388f,
                panel.y + 72f,
                500f,
                786f);
            DrawInnerPanel(equipmentArea);
            GUI.Label(new Rect(equipmentArea.x + 18f,
                equipmentArea.y + 14f, 300f, 28f),
                "ÉQUIPEMENT",
                sectionStyle);

            DrawEquipmentSilhouette(equipmentArea);

            DrawEquipmentSlotCard(
                new Rect(equipmentArea.center.x - 54f,
                    equipmentArea.y + 62f, 108f, 52f),
                EquipmentSlot.Head);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.center.x - 65f,
                    equipmentArea.y + 192f, 130f, 58f),
                EquipmentSlot.Torso);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.x + 22f,
                    equipmentArea.y + 306f, 126f, 58f),
                EquipmentSlot.Hands);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.center.x - 67f,
                    equipmentArea.y + 390f, 134f, 58f),
                EquipmentSlot.Legs);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.center.x - 67f,
                    equipmentArea.y + 500f, 134f, 58f),
                EquipmentSlot.Feet);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.x + 22f,
                    equipmentArea.y + 180f, 126f, 54f),
                EquipmentSlot.Implant);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.xMax - 148f,
                    equipmentArea.y + 180f, 126f, 54f),
                EquipmentSlot.ImplantSecondary);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.center.x - 63f,
                    equipmentArea.y + 306f, 126f, 58f),
                EquipmentSlot.ImplantTertiary);

            PrototypeHandSetSlot activeSet = weaponLoadout == null
                ? PrototypeHandSetSlot.Primary
                : weaponLoadout.ActiveHandSet;
            EquipmentSlot leftHand = PrototypeWeaponLoadout.GetHandSlot(
                activeSet,
                CombatHand.Left);
            EquipmentSlot rightHand = PrototypeWeaponLoadout.GetHandSlot(
                activeSet,
                CombatHand.Right);

            Rect swapRect = new(
                equipmentArea.x + 22f,
                equipmentArea.y + 602f,
                124f,
                38f);
            if (DrawClosedButton(
                swapRect,
                "I  ⇄  II",
                false,
                selectedIndex == SwapSelectableIndex))
            {
                selectedIndex = SwapSelectableIndex;
                SwapActiveHandSet();
            }
            RegisterTooltip(
                swapRect,
                "Permute immédiatement les deux ensembles de mains. " +
                "Le même résultat est produit par l'action de permutation " +
                "configurée dans les contrôles.",
                SwapSelectableIndex);

            DrawEquipmentSlotCard(
                new Rect(equipmentArea.x + 22f,
                    equipmentArea.y + 650f, 220f, 104f),
                leftHand);
            DrawEquipmentSlotCard(
                new Rect(equipmentArea.xMax - 242f,
                    equipmentArea.y + 650f, 220f, 104f),
                rightHand);
        }

        private void DrawEquipmentSilhouette(Rect area)
        {
            if (anatomySilhouetteTexture == null)
            {
                anatomySilhouetteTexture =
                    Resources.Load<Texture2D>("CharacterAnatomySilhouette");
            }
            if (anatomySilhouetteTexture == null)
            {
                return;
            }

            Rect silhouetteArea = new(
                area.center.x - 165f,
                area.y + 56f,
                330f,
                580f);
            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(
                silhouetteArea,
                anatomySilhouetteTexture,
                ScaleMode.ScaleToFit,
                true);
            GUI.color = previousColor;
        }

        private void DrawEquipmentSlotCard(
            Rect rect,
            EquipmentSlot slot)
        {
            WorldPickupDefinition equippedItem =
                equipment.GetEquippedItem(slot);
            bool placingItem =
                pendingHandItem?.Equipment != null &&
                (EquipmentDefinition.IsHandSlot(slot) ||
                 EquipmentDefinition.IsImplantSlot(slot));
            bool canPlace = placingItem &&
                pendingHandItem.Equipment.CanEquipIn(slot);
            int rowIndex = EquipmentSelectableStart +
                GetVisibleEquipmentIndex(slot);
            bool selected = selectedIndex == rowIndex;
            bool actionable = canPlace || equippedItem != null;
            bool hovered =
                actionable && rect.Contains(Event.current.mousePosition);
            Color cardBorder = canPlace
                ? accentColor
                : selected
                    ? new Color(0.82f, 0.31f, 0.07f, 1f)
                    : hovered
                        ? new Color(0.68f, 0.18f, 0.055f, 1f)
                    : new Color(borderColor.r, borderColor.g,
                        borderColor.b, 0.78f);
            DrawClosedCell(
                rect,
                new Color(0.035f, 0.021f, 0.015f, 0.97f),
                cardBorder,
                2f);
            Rect inner = new(rect.x + 3f, rect.y + 3f,
                rect.width - 6f, rect.height - 6f);

            if (actionable)
            {
                PrototypeInterfaceCursor.RegisterInteractive(rect);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    selectedIndex = rowIndex;
                    if (canPlace)
                    {
                        TryEquipToSlot(pendingHandItem, slot);
                    }
                    else
                    {
                        TryUnequip(slot);
                    }
                }
            }

            GUI.Label(new Rect(inner.x + 8f, inner.y + 5f,
                inner.width - 16f, 20f),
                GetSlotLabel(slot),
                slotLabelStyle);
            GUI.Label(new Rect(inner.x + 8f, inner.y + 25f,
                inner.width - 16f, Mathf.Max(20f, inner.height - 31f)),
                equippedItem == null
                    ? "VIDE"
                    : equippedItem.DisplayName.ToUpperInvariant(),
                slotValueStyle);

            RegisterTooltip(
                rect,
                GetEquipmentSlotTooltip(slot, equippedItem),
                rowIndex);
        }

        private void DrawInventoryColumn(Rect panel)
        {
            if (
                pendingHandItem != null &&
                !inventory.ContainsItem(pendingHandItem))
            {
                pendingHandItem = null;
            }
            RefreshFilteredItems();

            Rect area = new(
                panel.x + 902f,
                panel.y + 72f,
                854f,
                786f);
            DrawInnerPanel(area);
            GUI.Label(new Rect(area.x + 18f, area.y + 14f,
                280f, 28f),
                "INVENTAIRE",
                sectionStyle);
            GUI.Label(new Rect(area.x + 420f, area.y + 15f,
                414f, 24f),
                $"{filteredItems.Count} AFFICHÉ(S)  •  " +
                $"{inventory.Items.Count} / " +
                CharacterInventory.MaximumItemCapacity,
                valueStyle);

            float filterWidth = (area.width - 68f) / InventoryFilterCount;
            for (int index = 0; index < InventoryFilterCount; index++)
            {
                CharacterInventoryFilter filter =
                    (CharacterInventoryFilter)index;
                Rect filterRect = new(
                    area.x + 18f + index * (filterWidth + 8f),
                    area.y + 52f,
                    filterWidth,
                    36f);
                int focusIndex = FilterSelectableStart + index;
                if (DrawClosedButton(
                    filterRect,
                    GetFilterLabel(filter),
                    inventoryFilter == filter,
                    selectedIndex == focusIndex))
                {
                    selectedIndex = focusIndex;
                    SetInventoryFilter(filter);
                }
                RegisterTooltip(
                    filterRect,
                    $"Afficher : {GetFilterLabel(filter).ToLowerInvariant()}.",
                    focusIndex);
            }

            DrawInventorySearch(new Rect(
                area.x + 18f,
                area.y + 98f,
                450f,
                38f));

            Rect sortByNameRect = new(
                area.x + 486f,
                area.y + 98f,
                160f,
                38f);
            int nameSortIndex = SortSelectableStart;
            if (DrawClosedButton(
                sortByNameRect,
                "TRIER : NOM",
                false,
                selectedIndex == nameSortIndex))
            {
                selectedIndex = nameSortIndex;
                SortInventory(InventorySortMode.Name);
            }
            RegisterTooltip(sortByNameRect,
                "Ranger tous les objets par ordre alphabétique.",
                nameSortIndex);

            Rect sortByTypeRect = new(
                area.x + 656f,
                area.y + 98f,
                180f,
                38f);
            int typeSortIndex = SortSelectableStart + 1;
            if (DrawClosedButton(
                sortByTypeRect,
                "TRIER : TYPE",
                false,
                selectedIndex == typeSortIndex))
            {
                selectedIndex = typeSortIndex;
                SortInventory(InventorySortMode.Type);
            }
            RegisterTooltip(sortByTypeRect,
                "Regrouper les objets par catégorie, puis par nom.",
                typeSortIndex);

            inventoryPage = Mathf.Clamp(
                inventoryPage,
                0,
                InventoryPageCount - 1);
            int firstItem = inventoryPage * InventoryPageSize;
            float cellWidth = (area.width - 76f) / InventoryColumns;
            const float cellHeight = 98f;
            const float gap = 8f;
            float gridX = area.x + 18f;
            float gridY = area.y + 148f;

            for (int cell = 0; cell < InventoryPageSize; cell++)
            {
                int column = cell % InventoryColumns;
                int row = cell / InventoryColumns;
                Rect cellRect = new(
                    gridX + column * (cellWidth + gap),
                    gridY + row * (cellHeight + gap),
                    cellWidth,
                    cellHeight);
                int itemIndex = firstItem + cell;
                if (itemIndex < filteredItems.Count)
                {
                    DrawInventoryCell(
                        cellRect,
                        cell,
                        filteredItems[itemIndex]);
                }
                else
                {
                    DrawClosedCell(
                        cellRect,
                        new Color(0.012f, 0.01f, 0.009f, 0.96f),
                        new Color(0.17f, 0.055f, 0.028f, 0.72f));
                }
            }

            float pageWidth = 54f;
            float pageGap = 10f;
            float pagesWidth =
                InventoryPageCount * pageWidth +
                (InventoryPageCount - 1) * pageGap;
            float pageX = area.center.x - pagesWidth * 0.5f;
            for (int page = 0; page < InventoryPageCount; page++)
            {
                Rect pageRect = new(
                    pageX + page * (pageWidth + pageGap),
                    area.y + 724f,
                    pageWidth,
                    38f);
                int focusIndex = PageSelectableStart + page;
                if (DrawClosedButton(
                    pageRect,
                    (page + 1).ToString(),
                    inventoryPage == page,
                    selectedIndex == focusIndex))
                {
                    selectedIndex = focusIndex;
                    SetInventoryPage(page);
                }
                RegisterTooltip(pageRect,
                    $"Afficher la page {page + 1} sur {InventoryPageCount}.",
                    focusIndex);
            }
        }

        private void DrawInventoryCell(
            Rect rect,
            int visibleCellIndex,
            WorldPickupDefinition item)
        {
            int rowIndex = InventorySelectableStart + visibleCellIndex;
            bool selected = selectedIndex == rowIndex ||
                pendingHandItem == item;
            bool hovered = rect.Contains(Event.current.mousePosition);
            Color border = selected
                ? accentColor
                : hovered
                    ? new Color(0.66f, 0.17f, 0.05f, 1f)
                    : new Color(0.24f, 0.07f, 0.032f, 0.9f);
            DrawClosedCell(rect, new Color(
                item.PrototypeColor.r * 0.16f,
                item.PrototypeColor.g * 0.16f,
                item.PrototypeColor.b * 0.16f,
                0.98f), border, 2f);
            Rect inner = new(rect.x + 3f, rect.y + 3f,
                rect.width - 6f, rect.height - 6f);

            PrototypeInterfaceCursor.RegisterInteractive(rect);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                selectedIndex = rowIndex;
                ActivateInventoryItem(item);
            }
            GUI.Label(new Rect(inner.x + 7f, inner.y + 8f,
                inner.width - 14f, 55f),
                item.DisplayName.ToUpperInvariant(),
                slotValueStyle);
            GUI.Label(new Rect(inner.x + 7f, inner.yMax - 25f,
                inner.width - 14f, 18f),
                GetInventoryCategoryLabel(item.InventoryCategory),
                slotLabelStyle);
            RegisterTooltip(rect, GetItemTooltip(item), rowIndex);
        }

        public void SwapActiveHandSet()
        {
            weaponLoadout?.SwapHandSet();
        }

        public void SetInventoryFilter(CharacterInventoryFilter filter)
        {
            inventoryFilter = filter;
            inventoryPage = 0;
            RefreshFilteredItems();
            ClampSelection();
        }

        public void SetInventorySearch(string search)
        {
            inventorySearch = search?.TrimStart() ?? string.Empty;
            inventoryPage = 0;
            RefreshFilteredItems();
            ClampSelection();
        }

        public void SortInventory(InventorySortMode sortMode)
        {
            inventorySortMode = sortMode;
            inventory?.SortItems(sortMode);
            RefreshFilteredItems();
            ClampSelection();
        }

        public void SetInventoryPage(int page)
        {
            inventoryPage = Mathf.Clamp(page, 0, InventoryPageCount - 1);
            ClampSelection();
        }

        private void DrawInventorySearch(Rect rect)
        {
            DrawClosedCell(
                rect,
                new Color(0.012f, 0.009f, 0.007f, 0.98f),
                new Color(0.28f, 0.075f, 0.032f, 1f));
            Rect fieldRect = new(
                rect.x + 12f,
                rect.y + 3f,
                rect.width - 54f,
                rect.height - 6f);
            GUI.SetNextControlName("InventorySearch");
            PrototypeInterfaceCursor.RegisterInteractive(fieldRect);
            string updated = GUI.TextField(
                fieldRect,
                inventorySearch,
                48,
                searchStyle);
            if (!string.Equals(updated, inventorySearch,
                StringComparison.Ordinal))
            {
                SetInventorySearch(updated);
            }

            if (string.IsNullOrEmpty(inventorySearch) &&
                GUI.GetNameOfFocusedControl() != "InventorySearch")
            {
                GUI.Label(fieldRect,
                    "RECHERCHER UN OBJET...",
                    searchStyle);
            }

            Rect clearRect = new(
                rect.xMax - 38f,
                rect.y + 4f,
                32f,
                rect.height - 8f);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = inventorySearch.Length > 0;
            if (DrawClosedButton(clearRect, "×"))
            {
                SetInventorySearch(string.Empty);
                GUI.FocusControl(null);
            }
            GUI.enabled = previousEnabled;
            if (inventorySearch.Length > 0)
            {
                RegisterTooltip(clearRect, "Effacer la recherche.");
            }
            RegisterTooltip(
                rect,
                "La recherche filtre les noms à partir de trois caractères " +
                "consécutifs. Elle se combine avec la catégorie active.");
        }

        private void RefreshFilteredItems()
        {
            filteredItems.Clear();
            if (inventory == null)
            {
                return;
            }

            string normalizedSearch = NormalizeForSearch(inventorySearch);
            bool searchEnabled = normalizedSearch.Length >= 3;
            foreach (WorldPickupDefinition item in inventory.Items)
            {
                if (item == null || !MatchesFilter(item))
                {
                    continue;
                }

                if (searchEnabled && !NormalizeForSearch(item.DisplayName)
                    .Contains(normalizedSearch, StringComparison.Ordinal))
                {
                    continue;
                }

                filteredItems.Add(item);
            }

            inventoryPage = Mathf.Clamp(
                inventoryPage,
                0,
                InventoryPageCount - 1);
        }

        private bool MatchesFilter(WorldPickupDefinition item)
        {
            return inventoryFilter switch
            {
                CharacterInventoryFilter.Weapons =>
                    item.InventoryCategory == InventoryItemCategory.Weapon,
                CharacterInventoryFilter.Armor =>
                    item.InventoryCategory == InventoryItemCategory.Armor,
                CharacterInventoryFilter.Implants =>
                    item.InventoryCategory == InventoryItemCategory.Implant,
                CharacterInventoryFilter.QuestItems =>
                    item.InventoryCategory == InventoryItemCategory.QuestItem,
                _ => true
            };
        }

        private static string NormalizeForSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string decomposed = value.Trim().ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);
            StringBuilder result = new(decomposed.Length);
            foreach (char character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                    UnicodeCategory.NonSpacingMark)
                {
                    result.Append(character);
                }
            }

            return result.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string GetFilterLabel(
            CharacterInventoryFilter filter)
        {
            return filter switch
            {
                CharacterInventoryFilter.Weapons => "ARMES",
                CharacterInventoryFilter.Armor => "ARMURES",
                CharacterInventoryFilter.Implants => "IMPLANTS",
                CharacterInventoryFilter.QuestItems => "QUÊTES",
                _ => "TOUT"
            };
        }

        private static string GetInventoryCategoryLabel(
            InventoryItemCategory category)
        {
            return category switch
            {
                InventoryItemCategory.Weapon => "ARME",
                InventoryItemCategory.Armor => "ARMURE",
                InventoryItemCategory.Implant => "IMPLANT",
                InventoryItemCategory.QuestItem => "OBJET DE QUÊTE",
                _ => "OBJET"
            };
        }

        private void DrawStatisticRow(
            Rect rect,
            int rowIndex,
            string label,
            float value,
            bool pending,
            bool canIncrease,
            Action increase,
            string tooltip)
        {
            DrawRect(
                rect,
                new Color(0.05f, 0.03f, 0.021f, 0.94f));
            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f,
                rect.width - 110f, rect.height - 12f),
                label,
                rowStyle);
            GUI.Label(new Rect(rect.xMax - 94f, rect.y + 6f,
                46f, rect.height - 12f),
                value.ToString("0"),
                pending ? pendingValueStyle : valueStyle);

            if (canIncrease)
            {
                Rect increaseRect = new(rect.xMax - 40f, rect.y + 5f,
                    32f, rect.height - 10f);
                if (DrawClosedButton(
                    increaseRect,
                    "+",
                    false,
                    selectedIndex == rowIndex))
                {
                    selectedIndex = rowIndex;
                    increase?.Invoke();
                }
            }
            RegisterTooltip(rect, tooltip, rowIndex);
        }

        private void DrawSecondaryStatisticRow(
            Rect rect,
            int rowIndex,
            SecondaryStatDefinition definition,
            float value)
        {
            bool changed = TrackStatValue(
                $"secondary:{definition.Identifier}",
                value);
            DrawRect(
                rect,
                changed
                    ? new Color(0.34f, 0.12f, 0.025f, 0.98f)
                    : new Color(0.05f, 0.03f, 0.021f, 0.94f));
            GUI.Label(new Rect(rect.x + 10f, rect.y + 5f,
                rect.width - 100f, rect.height - 10f),
                definition.DisplayName.ToUpperInvariant(),
                rowStyle);
            GUI.Label(new Rect(rect.xMax - 92f, rect.y + 5f,
                82f, rect.height - 10f),
                FormatSecondaryStatValue(definition, value),
                valueStyle);
            RegisterTooltip(
                rect,
                GetSecondaryStatTooltip(definition),
                rowIndex);
        }

        private bool TrackStatValue(string identifier, float value)
        {
            if (
                observedStatValues.TryGetValue(
                    identifier,
                    out float previousValue) &&
                !Mathf.Approximately(previousValue, value))
            {
                changedStatUntil[identifier] = Time.unscaledTime + 1.25f;
            }

            observedStatValues[identifier] = value;
            return changedStatUntil.TryGetValue(identifier, out float until) &&
                Time.unscaledTime < until;
        }

        private void RegisterTooltip(
            Rect rect,
            string text,
            int focusIndex = -1)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);
            bool focused = focusTooltipsEnabled &&
                focusIndex >= 0 &&
                selectedIndex == focusIndex;
            if (!hovered && (!focused || !string.IsNullOrEmpty(tooltipText)))
            {
                return;
            }

            tooltipText = text;
            tooltipAnchor = rect;
            tooltipUsesPointer = hovered;
        }

        private void DrawTooltip(Rect panel)
        {
            if (string.IsNullOrWhiteSpace(tooltipText))
            {
                return;
            }

            const float tooltipWidth = 390f;
            float tooltipHeight = Mathf.Clamp(
                tooltipStyle.CalcHeight(
                    new GUIContent(tooltipText),
                    tooltipWidth - 28f) + 24f,
                72f,
                240f);
            Vector2 pointer = Event.current.mousePosition;
            float x = tooltipUsesPointer
                ? pointer.x + 20f
                : tooltipAnchor.xMax + 12f;
            float y = tooltipUsesPointer
                ? pointer.y + 18f
                : tooltipAnchor.y;
            x = Mathf.Clamp(
                x,
                panel.x + 10f,
                panel.xMax - tooltipWidth - 10f);
            y = Mathf.Clamp(
                y,
                panel.y + 10f,
                panel.yMax - tooltipHeight - 10f);

            Rect rect = new(x, y, tooltipWidth, tooltipHeight);
            DrawRect(rect, new Color(0.7f, 0.16f, 0.045f, 1f));
            DrawRect(new Rect(rect.x + 2f, rect.y + 2f,
                rect.width - 4f, rect.height - 4f),
                new Color(0.018f, 0.012f, 0.009f, 0.995f));
            GUI.Label(new Rect(rect.x + 14f, rect.y + 12f,
                rect.width - 28f, rect.height - 24f),
                tooltipText,
                tooltipStyle);
        }

        private int GetVisibleEquipmentIndex(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Head => 0,
                EquipmentSlot.Torso => 1,
                EquipmentSlot.Hands => 2,
                EquipmentSlot.Legs => 3,
                EquipmentSlot.Feet => 4,
                EquipmentSlot.Implant => 5,
                EquipmentSlot.ImplantSecondary => 6,
                EquipmentSlot.ImplantTertiary => 7,
                _ when slot == GetVisibleEquipmentSlot(8) => 8,
                _ when slot == GetVisibleEquipmentSlot(9) => 9,
                _ => 0
            };
        }

        private EquipmentSlot GetVisibleEquipmentSlot(int index)
        {
            if (index < 8)
            {
                return index switch
                {
                    0 => EquipmentSlot.Head,
                    1 => EquipmentSlot.Torso,
                    2 => EquipmentSlot.Hands,
                    3 => EquipmentSlot.Legs,
                    4 => EquipmentSlot.Feet,
                    5 => EquipmentSlot.Implant,
                    6 => EquipmentSlot.ImplantSecondary,
                    _ => EquipmentSlot.ImplantTertiary
                };
            }

            PrototypeHandSetSlot set = weaponLoadout == null
                ? PrototypeHandSetSlot.Primary
                : weaponLoadout.ActiveHandSet;
            return PrototypeWeaponLoadout.GetHandSlot(
                set,
                index == 8 ? CombatHand.Left : CombatHand.Right);
        }

        private static string GetPrimaryStatTooltip(
            CharacterStatDefinition definition)
        {
            string description = definition.Identifier switch
            {
                "strength" =>
                    "Représente la puissance physique. Elle pourra renforcer " +
                    "les attaques physiques et satisfaire les prérequis " +
                    "d'équipement lourd.",
                "agility" =>
                    "Représente la précision et la maîtrise du mouvement. " +
                    "Elle pourra influencer le maniement des armes et les " +
                    "actions reposant sur la rapidité.",
                "intelligence" =>
                    "Représente la puissance mentale et occulte. Elle pourra " +
                    "améliorer la magie et certaines compétences actives.",
                "spirit" =>
                    "Représente la maîtrise de la ressource intérieure. Elle " +
                    "pourra influencer l'énergie, sa récupération et les soins.",
                "constitution" =>
                    "Représente la robustesse du personnage. Elle pourra " +
                    "augmenter la vie et certaines résistances.",
                _ => "Attribut principal du personnage."
            };
            return $"{definition.DisplayName.ToUpperInvariant()}\n" +
                description +
                "\nLes conversions numériques exactes seront définies " +
                "pendant l'équilibrage.";
        }

        private static string GetSecondaryStatTooltip(
            SecondaryStatDefinition definition)
        {
            if (definition.Identifier == "outgoing_damage_multiplier")
            {
                return "DÉGÂTS INFLIGÉS\nMultiplicateur appliqué aux dégâts " +
                    "produits par le personnage. Il réagit immédiatement aux " +
                    "objets équipés, passifs et effets temporaires.";
            }

            return $"{definition.DisplayName.ToUpperInvariant()}\n" +
                "Statistique dérivée recalculée à partir des attributs, de " +
                "l'équipement, des passifs et des effets actifs.";
        }

        private static string GetEquipmentSlotTooltip(
            EquipmentSlot slot,
            WorldPickupDefinition item)
        {
            string description = slot switch
            {
                EquipmentSlot.Head =>
                    "Emplacement réservé aux protections et objets de tête.",
                EquipmentSlot.Torso =>
                    "Emplacement réservé aux protections du torse.",
                EquipmentSlot.Hands =>
                    "Emplacement réservé aux gants et protections des mains.",
                EquipmentSlot.Legs =>
                    "Emplacement réservé aux protections des jambes.",
                EquipmentSlot.Feet =>
                    "Emplacement réservé aux bottes et protections des pieds.",
                EquipmentSlot.Implant or
                EquipmentSlot.ImplantSecondary or
                EquipmentSlot.ImplantTertiary =>
                    "Un des trois emplacements réservés aux implants du personnage.",
                EquipmentSlot.PrimaryLeftHand or
                EquipmentSlot.SecondaryLeftHand =>
                    "Main gauche de cet ensemble. Elle accepte les armes " +
                    "compatibles et pourra recevoir un bouclier.",
                _ =>
                    "Main droite de cet ensemble. Elle accepte les armes " +
                    "compatibles avec cette main."
            };
            string content = item == null
                ? "Emplacement actuellement vide."
                : $"Équipé : {item.DisplayName}. Utilisez RETIRER pour " +
                  "rendre l'objet à l'inventaire.";
            return $"{GetSlotLabel(slot)}\n{description}\n{content}";
        }

        private static string GetItemTooltip(WorldPickupDefinition item)
        {
            if (item?.Equipment == null)
            {
                return item == null
                    ? string.Empty
                    : $"{item.DisplayName.ToUpperInvariant()}\n" +
                      $"{GetInventoryCategoryLabel(item.InventoryCategory)}\n" +
                      "Objet non équipable transporté dans l'inventaire.";
            }

            EquipmentDefinition definition = item.Equipment;
            string tooltip = $"{item.DisplayName.ToUpperInvariant()}\n" +
                $"{GetInventoryItemTypeLabel(definition)}";
            if (definition.DamageMultiplierBonus > 0f)
            {
                tooltip +=
                    $"\nDégâts infligés : +{definition.DamageMultiplierBonus * 100f:0}%";
            }

            foreach (SecondaryStatModifier modifier in
                definition.SecondaryStatModifiers)
            {
                if (modifier?.Statistic == null)
                {
                    continue;
                }

                string value = modifier.Operation ==
                    SecondaryStatModifierOperation.Flat
                        ? $"{modifier.Value:+0.##;-0.##;0}"
                        : $"{modifier.Value * 100f:+0.##;-0.##;0}%";
                tooltip +=
                    $"\n{modifier.Statistic.DisplayName} : {value}";
            }

            foreach (EquipmentStatRequirement requirement in
                definition.Requirements)
            {
                if (requirement?.Statistic != null)
                {
                    tooltip +=
                        $"\nRequis : {requirement.Statistic.DisplayName} " +
                        requirement.MinimumValue;
                }
            }

            return tooltip;
        }

        private static string FormatSecondaryStatValue(
            SecondaryStatDefinition definition,
            float value)
        {
            return definition.Identifier.Contains("multiplier",
                StringComparison.Ordinal)
                    ? $"{value * 100f:0.#}%"
                    : value.ToString("0.##");
        }

        private void ActivateSelection()
        {
            RefreshFilteredItems();
            int primaryStatCount = statistics.Statistics.Count;
            if (selectedIndex < primaryStatCount)
            {
                TryIncreaseStat(
                    statistics.Statistics[selectedIndex].Definition);
                return;
            }

            if (selectedIndex < StatisticSelectableCount)
            {
                return;
            }

            if (
                AttributeActionsVisible &&
                selectedIndex == SaveAttributesSelectableIndex)
            {
                CommitAttributeDistribution();
                return;
            }

            if (
                AttributeActionsVisible &&
                selectedIndex == CancelAttributesSelectableIndex)
            {
                CancelAttributeDistribution();
                return;
            }

            if (selectedIndex == SwapSelectableIndex)
            {
                SwapActiveHandSet();
                return;
            }

            int slotIndex = selectedIndex - EquipmentSelectableStart;
            if (slotIndex >= 0 && slotIndex < VisibleEquipmentSlotCount)
            {
                EquipmentSlot slot = GetVisibleEquipmentSlot(slotIndex);
                if (pendingHandItem?.Equipment != null)
                {
                    TryEquipToSlot(pendingHandItem, slot);
                }
                else
                {
                    TryUnequip(slot);
                }
                ClampSelection();
                return;
            }

            int filterIndex = selectedIndex - FilterSelectableStart;
            if (filterIndex >= 0 && filterIndex < InventoryFilterCount)
            {
                SetInventoryFilter((CharacterInventoryFilter)filterIndex);
                return;
            }

            int sortIndex = selectedIndex - SortSelectableStart;
            if (sortIndex >= 0 && sortIndex < InventorySortButtonCount)
            {
                SortInventory(sortIndex == 0
                    ? InventorySortMode.Name
                    : InventorySortMode.Type);
                return;
            }

            int visibleItemIndex = selectedIndex - InventorySelectableStart;
            if (visibleItemIndex >= 0 &&
                visibleItemIndex < VisibleInventoryItemCount)
            {
                int itemIndex = inventoryPage * InventoryPageSize +
                    visibleItemIndex;
                if (itemIndex < filteredItems.Count)
                {
                    ActivateInventoryItem(filteredItems[itemIndex]);
                    ClampSelection();
                }
                return;
            }

            int pageIndex = selectedIndex - PageSelectableStart;
            if (pageIndex >= 0 && pageIndex < InventoryPageCount)
            {
                SetInventoryPage(pageIndex);
                selectedIndex = PageSelectableStart + pageIndex;
            }
        }

        private void ActivateInventoryItem(WorldPickupDefinition item)
        {
            if (item?.Equipment == null)
            {
                SetFeedback("OBJET NON ÉQUIPABLE");
                return;
            }

            if (
                !item.Equipment.IsHandEquipment &&
                !item.Equipment.IsImplantEquipment)
            {
                pendingHandItem = null;
                TryEquip(item);
                return;
            }

            pendingHandItem = item;
            SetFeedback(item.Equipment.IsImplantEquipment
                ? $"CHOISISSEZ UN EMPLACEMENT D'IMPLANT POUR " +
                  item.DisplayName.ToUpperInvariant()
                : $"CHOISISSEZ UNE MAIN POUR " +
                  item.DisplayName.ToUpperInvariant());
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

            if (progression == null)
            {
                progression = GetComponent<CharacterProgression>();
            }

            if (identity == null)
            {
                identity = GetComponent<CharacterIdentity>();
            }

            if (weaponLoadout == null)
            {
                weaponLoadout = GetComponent<PrototypeWeaponLoadout>();
            }

            if (interfaceCoordinator == null)
            {
                interfaceCoordinator =
                    GetComponent<PrototypeInterfaceCoordinator>();
            }

            if (anatomySilhouetteTexture == null)
            {
                anatomySilhouetteTexture =
                    Resources.Load<Texture2D>("CharacterAnatomySilhouette");
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
            buttonStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
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
            slotLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.72f, 0.58f, 0.43f) }
            };
            slotValueStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = textColor }
            };
            tooltipStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                richText = false,
                normal = { textColor = new Color(0.94f, 0.86f, 0.72f) }
            };
            levelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = accentColor }
            };
            identityStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = textColor }
            };
            searchStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0),
                normal = { textColor = new Color(0.86f, 0.76f, 0.62f) },
                focused = { textColor = Color.white }
            };
            pendingValueStyle ??= new GUIStyle(valueStyle)
            {
                normal = { textColor = new Color(0.32f, 0.7f, 1f, 1f) }
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

        private bool DrawClosedButton(
            Rect rect,
            string label,
            bool active = false,
            bool focused = false)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);
            PrototypeInterfaceCursor.RegisterInteractive(rect, GUI.enabled);
            Color fill = !GUI.enabled
                ? new Color(0.025f, 0.019f, 0.016f, 0.9f)
                : active
                    ? new Color(0.36f, 0.07f, 0.025f, 0.98f)
                    : hovered || focused
                        ? new Color(0.2f, 0.045f, 0.018f, 0.98f)
                        : new Color(0.055f, 0.03f, 0.021f, 0.98f);
            Color border = active || focused
                ? accentColor
                : hovered
                    ? new Color(0.72f, 0.19f, 0.055f, 1f)
                    : new Color(0.33f, 0.085f, 0.038f, 1f);
            DrawClosedCell(rect, fill, border, 2f);
            return GUI.Button(rect, label, buttonStyle);
        }

        private static void DrawClosedCell(
            Rect rect,
            Color fill,
            Color border,
            float thickness = 2f)
        {
            DrawRect(rect, fill);
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), border);
            DrawRect(new Rect(
                rect.x,
                rect.yMax - thickness,
                rect.width,
                thickness), border);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), border);
            DrawRect(new Rect(
                rect.xMax - thickness,
                rect.y,
                thickness,
                rect.height), border);
        }

        private static string GetSlotLabel(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Head => "TÊTE",
                EquipmentSlot.Torso => "TORSE",
                EquipmentSlot.Hands => "GANTS",
                EquipmentSlot.Legs => "JAMBES",
                EquipmentSlot.Feet => "PIEDS",
                EquipmentSlot.Implant => "IMPLANT I",
                EquipmentSlot.ImplantSecondary => "IMPLANT II",
                EquipmentSlot.ImplantTertiary => "IMPLANT III",
                EquipmentSlot.PrimaryRightHand => "I · MAIN DROITE",
                EquipmentSlot.PrimaryLeftHand => "I · MAIN GAUCHE",
                EquipmentSlot.SecondaryRightHand => "II · MAIN DROITE",
                EquipmentSlot.SecondaryLeftHand => "II · MAIN GAUCHE",
                _ => slot.ToString().ToUpperInvariant()
            };
        }

        private static string GetInventoryItemTypeLabel(
            EquipmentDefinition definition)
        {
            return definition.HandEquipmentType switch
            {
                HandEquipmentType.RangedWeapon => "ARME À DISTANCE",
                HandEquipmentType.MeleeWeapon => "ARME DE MÊLÉE",
                HandEquipmentType.Shield => "BOUCLIER",
                _ => GetSlotLabel(definition.Slot)
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
