using System;
using System.Collections.Generic;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(CharacterSkillProgression))]
    [RequireComponent(typeof(ActiveSkillBar))]
    public sealed class PrototypeSkillTreePanel :
        MonoBehaviour,
        IPrototypeModalView,
        IPrototypeActiveSkillBarOverlay
    {
        private enum SkillNodeRelationship
        {
            None,
            Prerequisite,
            Unlock,
            Synergy
        }

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float SkillBarPanelWidth = 410f;
        private const float SkillBarSlotSize = 68f;
        private const float SkillBarSlotGap = 8f;
        private const float SkillBarPanelBottom = 34f;
        private const float SkillBarPanelHeight = 128f;
        private const int MaximumDisplayedValues = 8;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private CharacterSkillProgression skillProgression;
        [SerializeField] private ActiveSkillBar activeSkillBar;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private Color backdropColor =
            new(0.006f, 0.012f, 0.014f, 0.88f);
        [SerializeField] private Color panelColor =
            new(0.018f, 0.033f, 0.038f, 0.98f);
        [SerializeField] private Color borderColor =
            new(0.3f, 0.43f, 0.42f, 1f);
        [SerializeField] private Color accentColor =
            new(0.77f, 0.57f, 0.24f, 1f);
        [SerializeField] private Color activeColor =
            new(0.32f, 0.7f, 0.72f, 1f);
        [SerializeField] private Color textColor =
            new(0.9f, 0.86f, 0.72f, 1f);

        private readonly Rect[] nodeRects = new Rect[16];
        private GUIStyle titleStyle;
        private GUIStyle smallStyle;
        private GUIStyle nodeNameStyle;
        private GUIStyle nodeMarkStyle;
        private GUIStyle detailTitleStyle;
        private GUIStyle detailBodyStyle;
        private GUIStyle detailLabelStyle;
        private GUIStyle detailValueStyle;
        private GUIStyle buttonStyle;
        private GUIStyle feedbackStyle;
        private GUIStyle invisibleButtonStyle;
        private int selectedTreeIndex;
        private int selectedSkillIndex;
        private int selectedControlIndex;
        private float nextNavigationTime;
        private string feedbackMessage = string.Empty;
        private float feedbackUntil;
        private SkillDefinition blockedSkill;
        private float blockedSkillFlashUntil;

        public bool IsOpen { get; private set; }
        public int SelectedTreeIndex => selectedTreeIndex;
        public int SelectedSkillIndex => selectedSkillIndex;
        public CharacterSkillProgression SkillProgression => skillProgression;
        public ActiveSkillBar SkillBar => activeSkillBar;
        public SkillTreeDefinition SelectedTree =>
            skillProgression != null &&
            selectedTreeIndex >= 0 &&
            selectedTreeIndex < skillProgression.AvailableTrees.Count
                ? skillProgression.AvailableTrees[selectedTreeIndex]
                : null;
        public SkillDefinition SelectedSkill =>
            SelectedTree != null &&
            selectedSkillIndex >= 0 &&
            selectedSkillIndex < SelectedTree.Skills.Count
                ? SelectedTree.Skills[selectedSkillIndex]
                : null;

        private int CurrentSkillCount => SelectedTree?.Skills.Count ?? 0;
        private int InvestControlIndex => CurrentSkillCount;
        private int FirstSlotControlIndex => InvestControlIndex + 1;
        private int TotalControlCount =>
            FirstSlotControlIndex + ActiveSkillBar.SlotCount;

        private void Awake()
        {
            CacheReferences();
            ClampSelection();
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

            if (inputReader.ConsumeSkillTreePressed())
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
                Mathf.Max(Mathf.Abs(navigation.x), Mathf.Abs(navigation.y)) >
                0.4f &&
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
                ActivateSelectedControl();
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
                IsOpen = true;
                nextNavigationTime = 0f;
                feedbackMessage = string.Empty;
                ClampSelection();
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

        public bool SelectSkill(int skillIndex)
        {
            if (
                SelectedTree == null ||
                skillIndex < 0 ||
                skillIndex >= SelectedTree.Skills.Count)
            {
                return false;
            }

            selectedSkillIndex = skillIndex;
            selectedControlIndex = skillIndex;
            feedbackMessage = string.Empty;
            return true;
        }

        public bool SelectTree(int treeIndex)
        {
            if (
                skillProgression == null ||
                treeIndex < 0 ||
                treeIndex >= skillProgression.AvailableTrees.Count)
            {
                return false;
            }

            selectedTreeIndex = treeIndex;
            selectedSkillIndex = 0;
            selectedControlIndex = 0;
            feedbackMessage = string.Empty;
            return true;
        }

        public bool TryInvestSelectedPoint()
        {
            SkillDefinition skill = SelectedSkill;
            bool invested =
                skillProgression != null &&
                skillProgression.TryInvestPoint(skill);
            SkillInvestmentBlocker blocker = invested
                ? SkillInvestmentBlocker.None
                : skillProgression?.GetInvestmentBlocker(skill) ??
                  SkillInvestmentBlocker.MissingDefinition;
            if (
                blocker == SkillInvestmentBlocker.LevelLocked ||
                blocker == SkillInvestmentBlocker.MissingPrerequisite)
            {
                blockedSkill = skill;
                blockedSkillFlashUntil = Time.unscaledTime + 0.45f;
            }

            SetFeedback(invested
                ? $"{skill.DisplayName} — rang " +
                  skillProgression.GetInvestedRank(skill)
                : GetInvestmentBlockerLabel(blocker));
            return invested;
        }

        public bool TryAssignSelectedToSlot(int slotIndex)
        {
            SkillDefinition skill = SelectedSkill;
            bool assigned =
                activeSkillBar != null &&
                activeSkillBar.TryAssign(slotIndex, skill);
            if (assigned)
            {
                SetFeedback($"{skill.DisplayName} — emplacement {slotIndex + 1}");
                return true;
            }

            SkillAssignmentBlocker blocker = activeSkillBar == null
                ? SkillAssignmentBlocker.MissingDefinition
                : activeSkillBar.GetAssignmentBlocker(slotIndex, skill);
            SetFeedback(GetAssignmentBlockerLabel(blocker));
            return false;
        }

        public bool ClearSlot(int slotIndex)
        {
            bool cleared = activeSkillBar != null &&
                activeSkillBar.Clear(slotIndex);
            SetFeedback(cleared
                ? $"Emplacement {slotIndex + 1} libéré"
                : "Emplacement déjà vide");
            return cleared;
        }

        private void MoveSelection(int direction)
        {
            if (direction == 0 || TotalControlCount <= 0)
            {
                return;
            }

            selectedControlIndex =
                (selectedControlIndex + direction + TotalControlCount) %
                TotalControlCount;
            if (selectedControlIndex < CurrentSkillCount)
            {
                selectedSkillIndex = selectedControlIndex;
            }
        }

        private void ActivateSelectedControl()
        {
            if (selectedControlIndex <= InvestControlIndex)
            {
                TryInvestSelectedPoint();
                return;
            }

            int slotIndex = selectedControlIndex - FirstSlotControlIndex;
            if (slotIndex >= 0 && slotIndex < ActiveSkillBar.SlotCount)
            {
                TryAssignSelectedToSlot(slotIndex);
            }
        }

        private void OnGUI()
        {
            if (!IsOpen)
            {
                return;
            }

            PrototypeInterfaceCursor.BeginFrame();
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

            DrawBackdrop(width, height);
            Rect panel = new(
                width * 0.5f - 790f,
                26f,
                1580f,
                Mathf.Min(824f, height - 224f));
            DrawMainFrame(panel);
            DrawHeader(panel);

            Rect tabs = new(
                panel.x + 22f,
                panel.y + 90f,
                78f,
                panel.height - 122f);
            Rect graph = new(
                tabs.xMax + 12f,
                tabs.y,
                1012f,
                tabs.height);
            Rect details = new(
                graph.xMax + 14f,
                graph.y,
                panel.xMax - graph.xMax - 36f,
                graph.height);

            DrawTreeTabs(tabs);
            DrawGraph(graph);
            DrawDetails(details);
            DrawSkillBarInteractions(width, height);

            if (Time.unscaledTime < feedbackUntil)
            {
                GUI.Label(
                    new Rect(
                        width * 0.5f - 330f,
                        panel.yMax + 10f,
                        660f,
                        28f),
                    feedbackMessage,
                    feedbackStyle);
            }

            PrototypeInterfaceCursor.EndFrame();
            GUI.enabled = previousEnabled;
            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawBackdrop(float width, float height)
        {
            float reservedTop = height - SkillBarPanelHeight -
                SkillBarPanelBottom - 10f;
            DrawRect(new Rect(0f, 0f, width, reservedTop), backdropColor);
            PrototypeHudSkin.DrawTiledTexture(
                new Rect(0f, 0f, width, reservedTop),
                new Color(0.12f, 0.16f, 0.15f, 0.17f),
                480f);

            float reservedWidth = SkillBarPanelWidth + 34f;
            float sideWidth = Mathf.Max(0f, (width - reservedWidth) * 0.5f);
            DrawRect(
                new Rect(0f, reservedTop, sideWidth, height - reservedTop),
                backdropColor);
            DrawRect(
                new Rect(
                    width - sideWidth,
                    reservedTop,
                    sideWidth,
                    height - reservedTop),
                backdropColor);
        }

        private void DrawMainFrame(Rect rect)
        {
            DrawNotchedFill(
                ExpandRect(rect, 5f),
                new Color(0.004f, 0.008f, 0.009f, 0.98f),
                16f);
            DrawNotchedFill(rect, new Color(0.28f, 0.31f, 0.27f, 1f), 14f);
            PrototypeHudSkin.DrawTiledNotchedTexture(
                rect,
                new Color(0.62f, 0.59f, 0.47f, 0.82f),
                14f,
                230f);

            Rect inner = new(
                rect.x + 9f,
                rect.y + 9f,
                rect.width - 18f,
                rect.height - 18f);
            DrawNotchedFill(inner, panelColor, 10f);
            PrototypeHudSkin.DrawTiledNotchedTexture(
                inner,
                new Color(0.28f, 0.34f, 0.31f, 0.22f),
                10f,
                320f);
            DrawOutline(rect, new Color(0.06f, 0.1f, 0.1f, 1f), 2f);
            DrawBolt(new Vector2(rect.x + 22f, rect.y + 22f));
            DrawBolt(new Vector2(rect.xMax - 22f, rect.y + 22f));
            DrawBolt(new Vector2(rect.x + 22f, rect.yMax - 22f));
            DrawBolt(new Vector2(rect.xMax - 22f, rect.yMax - 22f));
        }

        private void DrawHeader(Rect panel)
        {
            SkillTreeDefinition tree = SelectedTree;
            string title = tree == null
                ? "COMPÉTENCES"
                : tree.DisplayName.ToUpperInvariant();
            GUI.Label(
                new Rect(panel.x + 34f, panel.y + 20f, 1000f, 42f),
                title,
                titleStyle);

            string points = skillProgression == null
                ? "POINTS  00"
                : $"POINTS  {skillProgression.UnspentSkillPoints:00}";
            GUI.Label(
                new Rect(panel.xMax - 330f, panel.y + 24f, 210f, 30f),
                points,
                detailLabelStyle);
            GUI.Label(
                new Rect(panel.xMax - 112f, panel.y + 24f, 70f, 30f),
                "K",
                smallStyle);
            DrawRect(
                new Rect(panel.x + 32f, panel.y + 68f, panel.width - 64f, 2f),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.48f));
        }

        private void DrawTreeTabs(Rect area)
        {
            DrawInset(area);
            int treeCount = skillProgression?.AvailableTrees.Count ?? 0;
            for (int index = 0; index < treeCount; index++)
            {
                Rect tab = new(area.x + 9f, area.y + 12f + index * 74f, 60f, 60f);
                bool selected = index == selectedTreeIndex;
                bool hovered = tab.Contains(Event.current.mousePosition);
                Color outline = selected
                    ? accentColor
                    : hovered
                        ? activeColor
                        : new Color(borderColor.r, borderColor.g, borderColor.b, 0.55f);
                DrawNotchedFill(tab, outline, 7f);
                Rect inside = new(tab.x + 3f, tab.y + 3f, tab.width - 6f, tab.height - 6f);
                DrawNotchedFill(inside, new Color(0.018f, 0.037f, 0.04f, 1f), 5f);
                DrawTreeGlyph(inside, index, selected ? accentColor : textColor);
                PrototypeInterfaceCursor.RegisterInteractive(tab, true);
                if (GUI.Button(tab, GUIContent.none, invisibleButtonStyle))
                {
                    SelectTree(index);
                }
            }
        }

        private void DrawGraph(Rect area)
        {
            DrawInset(area);
            SkillTreeDefinition tree = SelectedTree;
            if (tree == null)
            {
                GUI.Label(area, "Aucun arbre disponible", detailBodyStyle);
                return;
            }

            BuildNodeRects(area, tree);
            int hoveredIndex = FindHoveredNode(tree);
            SkillDefinition focused = hoveredIndex >= 0
                ? tree.Skills[hoveredIndex]
                : SelectedSkill;

            for (int index = 0; index < tree.Skills.Count; index++)
            {
                SkillDefinition skill = tree.Skills[index];
                if (skill != null)
                {
                    DrawSkillNode(
                        nodeRects[index],
                        skill,
                        index,
                        index == selectedSkillIndex,
                        index == hoveredIndex,
                        GetNodeRelationship(focused, skill));
                }
            }

            DrawRelationshipLegend(area, focused);
        }

        private void BuildNodeRects(Rect area, SkillTreeDefinition tree)
        {
            Rect field = new(
                area.x + 30f,
                area.y + 24f,
                area.width - 60f,
                area.height - 60f);
            int count = Mathf.Min(tree.Skills.Count, nodeRects.Length);
            for (int index = 0; index < count; index++)
            {
                Vector2 position = GetNodePosition(tree.Skills[index], index);
                nodeRects[index] = new Rect(
                    field.x + position.x * field.width - 61f,
                    field.y + position.y * field.height - 50f,
                    122f,
                    112f);
            }
        }

        private int FindHoveredNode(SkillTreeDefinition tree)
        {
            int count = Mathf.Min(tree.Skills.Count, nodeRects.Length);
            for (int index = count - 1; index >= 0; index--)
            {
                if (nodeRects[index].Contains(Event.current.mousePosition))
                {
                    return index;
                }
            }

            return -1;
        }

        private static SkillNodeRelationship GetNodeRelationship(
            SkillDefinition focused,
            SkillDefinition candidate)
        {
            if (focused == null || candidate == null || focused == candidate)
            {
                return SkillNodeRelationship.None;
            }

            if (HasDirectPrerequisite(focused, candidate))
            {
                return SkillNodeRelationship.Prerequisite;
            }

            if (HasDirectPrerequisite(candidate, focused))
            {
                return SkillNodeRelationship.Unlock;
            }

            if (
                HasInvestedSynergy(focused, candidate) ||
                HasInvestedSynergy(candidate, focused))
            {
                return SkillNodeRelationship.Synergy;
            }

            return SkillNodeRelationship.None;
        }

        private void DrawSkillNode(
            Rect rect,
            SkillDefinition skill,
            int index,
            bool selected,
            bool hovered,
            SkillNodeRelationship relationship)
        {
            Rect body = GetNodeBodyRect(rect, skill);
            int investedRank = skillProgression?.GetInvestedRank(skill) ?? 0;
            int effectiveRank = skillProgression?.GetEffectiveRank(skill) ?? 0;
            bool learned = investedRank > 0;
            SkillInvestmentBlocker blocker = skillProgression?
                .GetInvestmentBlocker(skill) ??
                SkillInvestmentBlocker.MissingDefinition;
            bool progressionLocked =
                blocker == SkillInvestmentBlocker.LevelLocked ||
                blocker == SkillInvestmentBlocker.MissingPrerequisite;
            bool blockedFlash =
                skill == blockedSkill &&
                Time.unscaledTime < blockedSkillFlashUntil;

            Color stateColor = learned
                ? new Color(0.42f, 0.72f, 0.58f, 1f)
                : progressionLocked
                    ? new Color(0.23f, 0.26f, 0.25f, 1f)
                    : new Color(0.42f, 0.55f, 0.52f, 1f);
            Color relationshipColor = GetRelationshipColor(relationship);
            Color outline = blockedFlash
                ? new Color(0.76f, 0.16f, 0.1f, 1f)
                : selected
                    ? accentColor
                    : hovered
                        ? activeColor
                        : relationship == SkillNodeRelationship.None
                            ? stateColor
                            : relationshipColor;

            if (skill.SkillType == SkillType.Active)
            {
                DrawNotchedFill(ExpandRect(body, 4f), outline, 9f);
                DrawNotchedFill(body, new Color(0.022f, 0.047f, 0.052f, 1f), 7f);
                PrototypeHudSkin.DrawTiledNotchedTexture(
                    body,
                    new Color(stateColor.r, stateColor.g, stateColor.b, 0.24f),
                    7f,
                    126f);
            }
            else
            {
                PrototypeHudSkin.DrawDisc(ExpandRect(body, 5f), outline);
                PrototypeHudSkin.DrawDisc(
                    body,
                    new Color(0.025f, 0.048f, 0.047f, 1f));
                PrototypeHudSkin.DrawDisc(
                    new Rect(body.x + 5f, body.y + 5f,
                        body.width - 10f, body.height - 10f),
                    new Color(stateColor.r, stateColor.g, stateColor.b, 0.18f));
            }

            Rect iconRect = new(
                body.center.x - 25f,
                body.center.y - 25f,
                50f,
                50f);
            if (skill.Icon != null)
            {
                PrototypeHudSkin.DrawSprite(iconRect, skill.Icon);
            }
            else
            {
                DrawSkillGlyph(iconRect, skill, stateColor);
            }

            if (progressionLocked)
            {
                DrawLockedMarker(body);
            }

            GUI.Label(
                new Rect(body.x + 5f, body.y + 3f, 20f, 18f),
                skill.SkillType == SkillType.Active ? "A" : "P",
                nodeMarkStyle);
            GUI.Label(
                new Rect(body.xMax - 45f, body.yMax - 21f, 40f, 18f),
                effectiveRank > investedRank
                    ? $"{investedRank}+{effectiveRank - investedRank}"
                    : investedRank.ToString(),
                nodeMarkStyle);
            GUI.Label(
                new Rect(rect.x - 5f, body.yMax + 8f, rect.width + 10f, 38f),
                skill.DisplayName,
                nodeNameStyle);

            if (relationship != SkillNodeRelationship.None)
            {
                DrawRect(
                    new Rect(body.center.x - 15f, body.yMax + 3f, 30f, 3f),
                    relationshipColor);
            }

            PrototypeInterfaceCursor.RegisterInteractive(rect, true);
            if (GUI.Button(rect, GUIContent.none, invisibleButtonStyle))
            {
                SelectSkill(index);
            }
        }

        private Color GetRelationshipColor(SkillNodeRelationship relationship)
        {
            return relationship switch
            {
                SkillNodeRelationship.Prerequisite => activeColor,
                SkillNodeRelationship.Unlock =>
                    new Color(0.48f, 0.68f, 0.52f, 1f),
                SkillNodeRelationship.Synergy => new Color(
                    0.66f,
                    0.49f,
                    0.78f,
                    1f),
                _ => Color.clear
            };
        }

        private static void DrawLockedMarker(Rect body)
        {
            Color rust = new(0.48f, 0.24f, 0.13f, 1f);
            Rect marker = new(body.xMax - 22f, body.y + 6f, 14f, 16f);
            DrawRect(new Rect(marker.x, marker.y + 7f, marker.width, 9f), rust);
            DrawRect(new Rect(marker.x + 3f, marker.y + 2f, 2f, 7f), rust);
            DrawRect(new Rect(marker.xMax - 5f, marker.y + 2f, 2f, 7f), rust);
            DrawRect(new Rect(marker.x + 5f, marker.y, 4f, 2f), rust);
            DrawRect(
                new Rect(marker.center.x - 1f, marker.y + 10f, 2f, 4f),
                new Color(0.12f, 0.08f, 0.05f, 1f));
        }

        private void DrawRelationshipLegend(Rect area, SkillDefinition focused)
        {
            if (focused == null)
            {
                return;
            }

            float itemWidth = 128f;
            float x = area.center.x - itemWidth * 1.5f;
            float y = area.yMax - 27f;
            DrawRelationshipLegendItem(
                new Rect(x, y, itemWidth, 18f),
                GetRelationshipColor(SkillNodeRelationship.Prerequisite),
                "PRÉREQUIS");
            DrawRelationshipLegendItem(
                new Rect(x + itemWidth, y, itemWidth, 18f),
                GetRelationshipColor(SkillNodeRelationship.Unlock),
                "DÉBLOQUE");
            DrawRelationshipLegendItem(
                new Rect(x + itemWidth * 2f, y, itemWidth, 18f),
                GetRelationshipColor(SkillNodeRelationship.Synergy),
                "SYNERGIE");
        }

        private void DrawRelationshipLegendItem(
            Rect rect,
            Color color,
            string label)
        {
            DrawRect(
                new Rect(rect.x, rect.center.y - 2f, 15f, 4f),
                color);
            GUI.Label(
                new Rect(rect.x + 21f, rect.y, rect.width - 21f, rect.height),
                label,
                detailValueStyle);
        }

        private static bool HasDirectPrerequisite(
            SkillDefinition target,
            SkillDefinition source)
        {
            if (target == null || source == null)
            {
                return false;
            }

            foreach (SkillPrerequisite prerequisite in target.Prerequisites)
            {
                if (prerequisite?.Skill == source)
                {
                    return true;
                }
            }

            foreach (SkillPrerequisiteGroup group in target.PrerequisiteGroups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (SkillPrerequisite prerequisite in group.Prerequisites)
                {
                    if (prerequisite?.Skill == source)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasInvestedSynergy(
            SkillDefinition receiver,
            SkillDefinition source)
        {
            if (receiver == null || source == null)
            {
                return false;
            }

            foreach (SkillInvestedRankSynergy synergy in
                receiver.InvestedRankSynergies)
            {
                if (synergy?.SourceSkill == source)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawDetails(Rect area)
        {
            DrawInset(area);
            SkillDefinition skill = SelectedSkill;
            if (skill == null)
            {
                return;
            }

            float x = area.x + 22f;
            float width = area.width - 44f;
            float y = area.y + 20f;
            GUI.Label(
                new Rect(x, y, width, 22f),
                $"{(skill.SkillType == SkillType.Active ? "ACTIVE" : "PASSIVE")}  ·  NIVEAU {skill.UnlockLevel}",
                detailLabelStyle);
            y += 28f;
            GUI.Label(
                new Rect(x, y, width, 66f),
                skill.DisplayName,
                detailTitleStyle);
            y += 70f;
            GUI.Label(
                new Rect(x, y, width, 74f),
                skill.Description,
                detailBodyStyle);
            y += 82f;

            int investedRank = skillProgression?.GetInvestedRank(skill) ?? 0;
            int effectiveRank = skillProgression?.GetEffectiveRank(skill) ?? 0;
            GUI.Label(
                new Rect(x, y, width, 22f),
                effectiveRank > investedRank
                    ? $"RANG  {investedRank}/{skill.MaximumInvestedRank}  (+{effectiveRank - investedRank})"
                    : $"RANG  {investedRank}/{skill.MaximumInvestedRank}",
                detailLabelStyle);
            y += 27f;

            string prerequisiteLabel = GetPrerequisiteLabel(skill);
            if (!string.IsNullOrEmpty(prerequisiteLabel))
            {
                GUI.Label(
                    new Rect(x, y, width, 43f),
                    prerequisiteLabel,
                    detailValueStyle);
                y += 49f;
            }

            int previewRank = Mathf.Max(1, effectiveRank);
            GUI.Label(
                new Rect(x, y, width, 22f),
                effectiveRank > 0
                    ? $"EFFET — RANG {effectiveRank}"
                    : "EFFET — APERÇU RANG 1",
                detailLabelStyle);
            y += 25f;
            if (skill.SkillType == SkillType.Active && skill.ResourceCost > 0f)
            {
                GUI.Label(
                    new Rect(x, y, width, 20f),
                    $"Énergie  {skill.ResourceCost:0.##}",
                    detailValueStyle);
                y += 20f;
            }

            int shown = 0;
            foreach (SkillRankValue value in skill.RankValues)
            {
                if (value == null || shown >= MaximumDisplayedValues)
                {
                    continue;
                }

                GUI.Label(
                    new Rect(x, y, width, 20f),
                    $"{GetValueLabel(value.Identifier)}  {FormatValue(value, previewRank, skill.MaximumInvestedRank)}",
                    detailValueStyle);
                y += 20f;
                shown++;
            }

            y += 8f;
            y = DrawOptionalDetailSection(
                x,
                y,
                width,
                "SYNERGIES REÇUES",
                GetReceivedSynergyLabel(skill));
            DrawOptionalDetailSection(
                x,
                y,
                width,
                "BONUS ACCORDÉS",
                GetGrantedSynergyLabel(skill));

            Rect investButton = new(
                x,
                area.yMax - 58f,
                width,
                36f);
            bool canInvest =
                skillProgression?.GetInvestmentBlocker(skill) ==
                SkillInvestmentBlocker.None;
            GUI.enabled = canInvest;
            bool investPressed = DrawButton(
                investButton,
                investedRank >= skill.MaximumInvestedRank
                    ? "RANG MAXIMUM"
                    : "+ 1 POINT",
                selectedControlIndex == InvestControlIndex);
            GUI.enabled = true;
            if (!canInvest)
            {
                PrototypeInterfaceCursor.RegisterInteractive(
                    investButton,
                    true);
                investPressed = GUI.Button(
                    investButton,
                    GUIContent.none,
                    invisibleButtonStyle);
            }

            if (investPressed)
            {
                TryInvestSelectedPoint();
            }
        }

        private float DrawOptionalDetailSection(
            float x,
            float y,
            float width,
            string title,
            string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return y;
            }

            GUI.Label(
                new Rect(x, y, width, 22f),
                title,
                detailLabelStyle);
            y += 23f;
            float contentHeight = Mathf.Clamp(
                detailValueStyle.CalcHeight(new GUIContent(content), width),
                18f,
                70f);
            GUI.Label(
                new Rect(x, y, width, contentHeight),
                content,
                detailValueStyle);
            return y + contentHeight + 9f;
        }

        private void DrawSkillBarInteractions(float width, float height)
        {
            if (activeSkillBar == null)
            {
                return;
            }

            float panelX = width * 0.5f - SkillBarPanelWidth * 0.5f;
            float panelY = height - SkillBarPanelBottom - SkillBarPanelHeight;
            float slotsX = panelX + 16f;
            float slotsY = panelY + 42f;
            for (int index = 0; index < ActiveSkillBar.SlotCount; index++)
            {
                Rect slot = new(
                    slotsX + index * (SkillBarSlotSize + SkillBarSlotGap),
                    slotsY,
                    SkillBarSlotSize,
                    SkillBarSlotSize);
                bool selected =
                    selectedControlIndex == FirstSlotControlIndex + index;
                bool hovered = slot.Contains(Event.current.mousePosition);
                if (selected || hovered)
                {
                    DrawOutline(
                        ExpandRect(slot, 3f),
                        selected ? accentColor : activeColor,
                        2f);
                }

                PrototypeInterfaceCursor.RegisterInteractive(slot, true);
                if (Event.current.type == EventType.MouseDown &&
                    Event.current.button == 1 &&
                    slot.Contains(Event.current.mousePosition))
                {
                    ClearSlot(index);
                    Event.current.Use();
                }
                else if (GUI.Button(slot, GUIContent.none, invisibleButtonStyle))
                {
                    selectedControlIndex = FirstSlotControlIndex + index;
                    TryAssignSelectedToSlot(index);
                }
            }
        }

        private string GetPrerequisiteLabel(SkillDefinition skill)
        {
            string label = string.Empty;
            foreach (SkillPrerequisite prerequisite in skill.Prerequisites)
            {
                label = AppendName(label, prerequisite?.Skill?.DisplayName, " ET ");
            }

            foreach (SkillPrerequisiteGroup group in skill.PrerequisiteGroups)
            {
                if (group == null || group.Prerequisites.Count == 0)
                {
                    continue;
                }

                string separator = group.Mode == SkillPrerequisiteMode.Any
                    ? " OU "
                    : " ET ";
                string groupLabel = string.Empty;
                foreach (SkillPrerequisite prerequisite in group.Prerequisites)
                {
                    groupLabel = AppendName(
                        groupLabel,
                        prerequisite?.Skill?.DisplayName,
                        separator);
                }

                if (group.Mode == SkillPrerequisiteMode.AtLeast)
                {
                    groupLabel = $"{group.RequiredCount} parmi : {groupLabel}";
                }

                label = AppendName(label, groupLabel, " ET ");
            }

            return string.IsNullOrEmpty(label)
                ? string.Empty
                : "PRÉREQUIS  " + label;
        }

        private string GetReceivedSynergyLabel(SkillDefinition skill)
        {
            string label = string.Empty;
            foreach (SkillInvestedRankSynergy synergy in
                skill.InvestedRankSynergies)
            {
                label = AppendName(
                    label,
                    GetSynergyDescription(
                        synergy?.SourceSkill?.DisplayName,
                        synergy),
                    "\n");
            }

            return label;
        }

        private string GetGrantedSynergyLabel(SkillDefinition source)
        {
            SkillTreeDefinition tree = SelectedTree;
            if (tree == null)
            {
                return string.Empty;
            }

            string label = string.Empty;
            foreach (SkillDefinition target in tree.Skills)
            {
                if (target == null)
                {
                    continue;
                }

                foreach (SkillInvestedRankSynergy synergy in
                    target.InvestedRankSynergies)
                {
                    if (synergy?.SourceSkill == source)
                    {
                        label = AppendName(
                            label,
                            GetSynergyDescription(
                                target.DisplayName,
                                synergy),
                            "\n");
                    }
                }
            }

            return label;
        }

        private static string GetSynergyDescription(
            string relatedSkillName,
            SkillInvestedRankSynergy synergy)
        {
            if (string.IsNullOrWhiteSpace(relatedSkillName) || synergy == null)
            {
                return string.Empty;
            }

            float bonus = synergy.BonusPerInvestedRank;
            string sign = bonus >= 0f ? "+" : string.Empty;
            string unit = synergy.Operation ==
                SkillSynergyOperation.AdditivePercent
                    ? " %"
                    : string.Empty;
            return $"{relatedSkillName} · {sign}{bonus:0.##}{unit} " +
                $"{GetSynergyValueLabel(synergy.AffectedValueIdentifier)} par point";
        }

        private static string GetSynergyValueLabel(string identifier)
        {
            return identifier switch
            {
                "weapon_damage_percent" => "de dégâts",
                "movement_distance_percent" => "de distance",
                "magazine_refill_percent" => "de chargeur rechargé",
                "range_percent" => "de portée",
                "riddled_detonation_weapon_damage_percent" =>
                    "de dégâts de détonation de Criblé",
                "cone_width_percent" => "de largeur de gerbe",
                "duration_percent" => "de durée",
                "adrenaline_duration_percent" =>
                    "de durée d'Adrénaline",
                "armour_broken_duration_percent" =>
                    "de durée d'Armure rompue",
                "stagger_bonus_percent" => "de stagger",
                "empty_magazine_damage_percent" =>
                    "de dégâts en vidant le chargeur",
                "anchored_target_damage_percent" =>
                    "de dégâts contre une cible ancrée",
                "siege_protocol_duration_percent" =>
                    "de durée du Protocole de siège",
                "persistence_percent" => "de persistance",
                "lingering_damage_percent" => "de dégâts persistants",
                "shredded_target_damage_percent" =>
                    "de dégâts contre une cible Déchiquetée",
                "scorched_earth_duration_percent" =>
                    "de durée de Terre brûlée",
                "secondary_reaction_damage_percent" =>
                    "de dégâts de réaction secondaire",
                "activation_speed_percent" => "de vitesse d'activation",
                "armour_efficiency_bonus_percent" =>
                    "d'efficacité d'armure",
                "arrival_power_percent" => "de puissance à l'arrivée",
                "attack_speed_bonus_percent" =>
                    "de vitesse d'attaque",
                "base_damage_percent" => "de dégâts de base",
                "base_execution_damage_percent" =>
                    "de dégâts d'exécution",
                "chain_range_bonus_percent" => "de portée de chaîne",
                "damage_per_bandwidth_unit_percent" =>
                    "de dégâts par unité de bande passante",
                "damage_per_strike_percent" => "de dégâts par frappe",
                "damage_percent" => "de dégâts",
                "damage_reduction_percent" =>
                    "de réduction des dégâts",
                "destroyed_main_chassis_scrap_units" =>
                    "de ferraille du châssis détruit",
                "displaced_target_damage_percent" =>
                    "de dégâts contre les cibles déplacées",
                "displaced_target_stagger_percent" =>
                    "de stagger contre les cibles déplacées",
                "duration_seconds" => "de durée",
                "emergence_damage_percent" =>
                    "de dégâts d'émergence",
                "energy_regeneration_per_target_per_second" =>
                    "d'Énergie par cible et par seconde",
                "first_rotation_damage_percent" =>
                    "de dégâts de la première rotation",
                "flashover_burn_retention_percent" =>
                    "de Combustion conservée par Flashover",
                "ghost_hit_damage_percent" =>
                    "de dégâts de la frappe secondaire",
                "implosion_damage_percent" => "de dégâts d'implosion",
                "inertia_duration_percent" => "de durée d'Inertie",
                "initial_damage_percent" => "de dégâts initiaux",
                "kill_cooldown_recovery_percent" =>
                    "de recharge récupérée sur élimination",
                "kill_pulse_duration_percent" =>
                    "de durée d'impulsion d'élimination",
                "link_duration_seconds" => "de durée du Lien",
                "maximum_added_duration_per_effect_seconds" =>
                    "au plafond de prolongation",
                "maximum_neighbour_power_bonus_percent" =>
                    "au plafond de puissance de la nuée",
                "next_burn_duration_bonus_percent" =>
                    "de durée à la prochaine Combustion",
                "next_electric_skill_damage_bonus_percent" =>
                    "de dégâts à la prochaine compétence électrique",
                "next_ice_cryostasis_bonus_percent" =>
                    "de Cryostase à la prochaine compétence de glace",
                "order_power_percent" => "de puissance de l'Ordre",
                "projectile_explosion_reduction_percent" =>
                    "de réduction contre projectiles et explosions",
                "propagation_radius_metres" =>
                    "m de rayon de propagation",
                "radius_metres" => "m de rayon",
                "radius_percent" => "de rayon",
                "remaining_burn_damage_conversion_percent" =>
                    "de conversion des Combustions restantes",
                "restoration_percent" => "de restauration",
                "return_health_percent" => "de santé au retour",
                "screen_damage_reduction_percent" =>
                    "de réduction de l'écran",
                "secondary_arc_damage_percent" =>
                    "de dégâts des arcs secondaires",
                "shatter_explosion_damage_percent" =>
                    "de dégâts d'explosion de rupture",
                "shatter_secondary_damage_percent" =>
                    "de dégâts secondaires de rupture",
                "shield_restoration_percent" =>
                    "de bouclier restauré",
                "shield_restored_percent_per_trigger" =>
                    "de bouclier rendu par déclenchement",
                "signature_unshielded_health_restoration_percent" =>
                    "de santé rendue sans bouclier",
                "thermal_shock_damage_percent" =>
                    "de dégâts de choc thermique",
                "trauma_duration_percent" => "de durée de Trauma",
                _ => "de " + HumanizeIdentifier(identifier)
            };
        }

        private static string AppendName(
            string current,
            string value,
            string separator)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return current;
            }

            return string.IsNullOrEmpty(current)
                ? value
                : current + separator + value;
        }

        private static Vector2 GetNodePosition(
            SkillDefinition skill,
            int fallbackIndex)
        {
            return skill?.Identifier switch
            {
                "marine_terminal_burst" => new Vector2(0.42f, 0.57f),
                "marine_predatory_cadence" => new Vector2(0.10f, 0.86f),
                "marine_combat_reload" => new Vector2(0.23f, 0.14f),
                "marine_ballistic_mobility" => new Vector2(0.13f, 0.48f),
                "marine_breech_sweep" => new Vector2(0.38f, 0.82f),
                "marine_brutal_feed" => new Vector2(0.68f, 0.11f),
                "marine_double_trigger" => new Vector2(0.68f, 0.72f),
                "marine_rain_of_casings" => new Vector2(0.85f, 0.46f),
                "marine_breech_storm" => new Vector2(0.91f, 0.80f),
                "marine_adrenaline_hunter" => new Vector2(0.48f, 0.31f),
                "marine_overcharged_striker" => new Vector2(0.18f, 0.17f),
                "marine_dense_core_ammunition" => new Vector2(0.08f, 0.82f),
                "marine_demolition_line" => new Vector2(0.29f, 0.69f),
                "marine_hydraulic_mount" => new Vector2(0.40f, 0.12f),
                "marine_anchor_shot" => new Vector2(0.60f, 0.23f),
                "marine_stopping_mass" => new Vector2(0.50f, 0.78f),
                "marine_seismic_impact" => new Vector2(0.69f, 0.57f),
                "marine_sacrificial_chamber" => new Vector2(0.80f, 0.82f),
                "marine_cannon_overload" => new Vector2(0.91f, 0.48f),
                "marine_siege_architecture" => new Vector2(0.86f, 0.14f),
                "marine_m13_skinner_grenade" => new Vector2(0.16f, 0.18f),
                "marine_demolition_belt" => new Vector2(0.08f, 0.81f),
                "marine_scavenger_mine" => new Vector2(0.31f, 0.61f),
                "marine_pit_compounds" => new Vector2(0.39f, 0.12f),
                "marine_breach_charge" => new Vector2(0.52f, 0.76f),
                "marine_industrial_shrapnel" => new Vector2(0.63f, 0.25f),
                "marine_thermobaric_rocket" => new Vector2(0.71f, 0.57f),
                "marine_chain_reaction" => new Vector2(0.84f, 0.15f),
                "marine_charge_crown" => new Vector2(0.91f, 0.78f),
                "marine_scorched_earth_protocol" => new Vector2(0.92f, 0.45f),
                _ => GetFallbackNodePosition(fallbackIndex)
            };
        }

        private static Vector2 GetFallbackNodePosition(int index)
        {
            Vector2[] positions =
            {
                new(0.16f, 0.18f),
                new(0.08f, 0.81f),
                new(0.31f, 0.61f),
                new(0.39f, 0.12f),
                new(0.52f, 0.76f),
                new(0.63f, 0.25f),
                new(0.71f, 0.57f),
                new(0.84f, 0.15f),
                new(0.91f, 0.78f),
                new(0.92f, 0.45f)
            };
            return positions[Mathf.Abs(index) % positions.Length];
        }

        private static Rect GetNodeBodyRect(Rect rect, SkillDefinition skill)
        {
            if (skill != null && skill.SkillType == SkillType.Passive)
            {
                return new Rect(
                    rect.center.x - 39f,
                    rect.y,
                    78f,
                    78f);
            }

            return new Rect(rect.x + 12f, rect.y, rect.width - 24f, 78f);
        }

        private void DrawSkillGlyph(
            Rect rect,
            SkillDefinition skill,
            Color color)
        {
            switch (skill.Identifier)
            {
                case "marine_terminal_burst":
                    for (int index = 0; index < 3; index++)
                    {
                        DrawLine(
                            new Vector2(rect.x + 7f, rect.y + 14f + index * 11f),
                            new Vector2(rect.xMax - 8f, rect.y + 8f + index * 11f),
                            color,
                            4f);
                    }
                    break;
                case "marine_predatory_cadence":
                    for (int index = 0; index < 4; index++)
                    {
                        DrawRect(
                            new Rect(
                                rect.x + 6f,
                                rect.yMax - 8f - index * 9f,
                                12f + index * 8f,
                                5f),
                            color);
                    }
                    break;
                case "marine_combat_reload":
                    DrawRect(new Rect(rect.x + 12f, rect.y + 12f, 16f, 30f), color);
                    DrawLine(
                        new Vector2(rect.x + 7f, rect.y + 9f),
                        new Vector2(rect.x + 40f, rect.y + 9f),
                        color,
                        4f);
                    DrawLine(
                        new Vector2(rect.x + 40f, rect.y + 9f),
                        new Vector2(rect.x + 33f, rect.y + 3f),
                        color,
                        4f);
                    break;
                case "marine_ballistic_mobility":
                    DrawChevron(rect, color, 0f);
                    DrawChevron(rect, color, 12f);
                    break;
                case "marine_breech_sweep":
                    for (int index = -2; index <= 2; index++)
                    {
                        DrawLine(
                            new Vector2(rect.x + 8f, rect.center.y),
                            new Vector2(rect.xMax - 5f, rect.center.y + index * 9f),
                            color,
                            3f);
                    }
                    break;
                case "marine_brutal_feed":
                    DrawRect(new Rect(rect.x + 13f, rect.y + 6f, 22f, 38f), color);
                    DrawRect(new Rect(rect.x + 8f, rect.y + 2f, 32f, 7f), color);
                    for (int index = 0; index < 3; index++)
                    {
                        DrawRect(new Rect(rect.x + 17f, rect.y + 12f + index * 9f, 14f, 3f), panelColor);
                    }
                    break;
                case "marine_double_trigger":
                    DrawPistol(rect, color, -7f);
                    DrawPistol(rect, color, 10f);
                    break;
                case "marine_rain_of_casings":
                    for (int index = 0; index < 5; index++)
                    {
                        float x = rect.x + 7f + index * 9f;
                        float y = rect.y + 8f + (index % 2) * 8f;
                        DrawLine(
                            new Vector2(x, y),
                            new Vector2(x - 5f, y + 26f),
                            color,
                            4f);
                    }
                    break;
                case "marine_breech_storm":
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.x + 10f, rect.y + 10f, 30f, 30f),
                        color);
                    DrawRect(new Rect(rect.x + 2f, rect.center.y - 3f, 46f, 6f), color);
                    DrawRect(new Rect(rect.center.x - 3f, rect.y + 2f, 6f, 46f), color);
                    break;
                case "marine_adrenaline_hunter":
                    DrawCrosshair(rect, color);
                    break;
                case "marine_overcharged_striker":
                    DrawRect(
                        new Rect(rect.x + 6f, rect.center.y - 5f, 29f, 10f),
                        color);
                    DrawRect(
                        new Rect(rect.x + 31f, rect.center.y - 10f, 8f, 20f),
                        color);
                    DrawLine(
                        new Vector2(rect.x + 40f, rect.y + 8f),
                        new Vector2(rect.x + 47f, rect.y + 1f),
                        color,
                        3f);
                    DrawLine(
                        new Vector2(rect.x + 40f, rect.yMax - 8f),
                        new Vector2(rect.x + 47f, rect.yMax - 1f),
                        color,
                        3f);
                    break;
                case "marine_dense_core_ammunition":
                    DrawRect(new Rect(rect.x + 20f, rect.y + 6f, 10f, 31f), color);
                    DrawRect(new Rect(rect.x + 17f, rect.y + 37f, 16f, 7f), color);
                    DrawLine(
                        new Vector2(rect.x + 20f, rect.y + 6f),
                        new Vector2(rect.center.x, rect.y + 1f),
                        color,
                        4f);
                    DrawLine(
                        new Vector2(rect.center.x, rect.y + 1f),
                        new Vector2(rect.x + 30f, rect.y + 6f),
                        color,
                        4f);
                    break;
                case "marine_demolition_line":
                    DrawRect(new Rect(rect.x + 3f, rect.center.y - 3f, 44f, 6f), color);
                    for (int index = 0; index < 3; index++)
                    {
                        DrawRect(
                            new Rect(rect.x + 13f + index * 12f, rect.y + 15f, 3f, 20f),
                            color);
                    }
                    break;
                case "marine_hydraulic_mount":
                    DrawRect(new Rect(rect.x + 12f, rect.y + 10f, 26f, 11f), color);
                    DrawRect(new Rect(rect.center.x - 3f, rect.y + 20f, 6f, 14f), color);
                    DrawLine(
                        new Vector2(rect.center.x, rect.y + 32f),
                        new Vector2(rect.x + 8f, rect.yMax - 5f),
                        color,
                        5f);
                    DrawLine(
                        new Vector2(rect.center.x, rect.y + 32f),
                        new Vector2(rect.xMax - 8f, rect.yMax - 5f),
                        color,
                        5f);
                    break;
                case "marine_anchor_shot":
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.center.x - 6f, rect.y + 4f, 12f, 12f),
                        color);
                    DrawRect(new Rect(rect.center.x - 3f, rect.y + 12f, 6f, 29f), color);
                    DrawLine(
                        new Vector2(rect.center.x, rect.y + 38f),
                        new Vector2(rect.x + 8f, rect.y + 29f),
                        color,
                        5f);
                    DrawLine(
                        new Vector2(rect.center.x, rect.y + 38f),
                        new Vector2(rect.xMax - 8f, rect.y + 29f),
                        color,
                        5f);
                    break;
                case "marine_stopping_mass":
                    DrawRect(new Rect(rect.x + 8f, rect.y + 9f, 34f, 8f), color);
                    DrawRect(new Rect(rect.x + 5f, rect.y + 21f, 40f, 8f), color);
                    DrawRect(new Rect(rect.x + 11f, rect.y + 33f, 28f, 8f), color);
                    break;
                case "marine_seismic_impact":
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.center.x - 8f, rect.center.y - 8f, 16f, 16f),
                        color);
                    DrawLine(new Vector2(rect.x + 3f, rect.center.y), new Vector2(rect.x + 15f, rect.center.y), color, 3f);
                    DrawLine(new Vector2(rect.xMax - 15f, rect.center.y), new Vector2(rect.xMax - 3f, rect.center.y), color, 3f);
                    DrawLine(new Vector2(rect.center.x, rect.y + 3f), new Vector2(rect.center.x, rect.y + 15f), color, 3f);
                    DrawLine(new Vector2(rect.center.x, rect.yMax - 15f), new Vector2(rect.center.x, rect.yMax - 3f), color, 3f);
                    break;
                case "marine_sacrificial_chamber":
                    DrawRect(new Rect(rect.x + 16f, rect.y + 8f, 18f, 34f), color);
                    DrawRect(new Rect(rect.x + 20f, rect.y + 13f, 10f, 6f), panelColor);
                    DrawRect(new Rect(rect.x + 20f, rect.y + 23f, 10f, 6f), panelColor);
                    DrawRect(new Rect(rect.x + 20f, rect.y + 33f, 10f, 6f), accentColor);
                    break;
                case "marine_cannon_overload":
                    DrawRect(new Rect(rect.x + 3f, rect.center.y - 5f, 31f, 10f), color);
                    DrawRect(new Rect(rect.x + 8f, rect.center.y + 5f, 9f, 15f), color);
                    DrawLine(new Vector2(rect.x + 35f, rect.center.y), new Vector2(rect.xMax - 1f, rect.y + 5f), color, 4f);
                    DrawLine(new Vector2(rect.x + 35f, rect.center.y), new Vector2(rect.xMax - 1f, rect.yMax - 5f), color, 4f);
                    break;
                case "marine_siege_architecture":
                    DrawRect(new Rect(rect.x + 7f, rect.y + 17f, 36f, 25f), color);
                    DrawRect(new Rect(rect.x + 10f, rect.y + 9f, 8f, 12f), color);
                    DrawRect(new Rect(rect.center.x - 4f, rect.y + 5f, 8f, 16f), color);
                    DrawRect(new Rect(rect.xMax - 18f, rect.y + 9f, 8f, 12f), color);
                    DrawRect(new Rect(rect.center.x - 5f, rect.y + 27f, 10f, 15f), panelColor);
                    break;
                case "marine_m13_skinner_grenade":
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.x + 10f, rect.y + 14f, 30f, 30f),
                        color);
                    DrawRect(new Rect(rect.x + 20f, rect.y + 7f, 10f, 10f), color);
                    DrawLine(
                        new Vector2(rect.x + 27f, rect.y + 8f),
                        new Vector2(rect.x + 39f, rect.y + 3f),
                        color,
                        3f);
                    break;
                case "marine_demolition_belt":
                    DrawRect(new Rect(rect.x + 4f, rect.y + 20f, 42f, 7f), color);
                    for (int index = 0; index < 3; index++)
                    {
                        DrawRect(
                            new Rect(rect.x + 7f + index * 14f, rect.y + 12f, 10f, 23f),
                            color);
                        DrawRect(
                            new Rect(rect.x + 10f + index * 14f, rect.y + 16f, 4f, 15f),
                            panelColor);
                    }
                    break;
                case "marine_scavenger_mine":
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.x + 9f, rect.y + 15f, 32f, 22f),
                        color);
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.x + 20f, rect.y + 20f, 10f, 10f),
                        panelColor);
                    DrawRect(new Rect(rect.x + 2f, rect.center.y - 2f, 9f, 4f), color);
                    DrawRect(new Rect(rect.xMax - 11f, rect.center.y - 2f, 9f, 4f), color);
                    DrawRect(new Rect(rect.center.x - 2f, rect.y + 8f, 4f, 9f), color);
                    DrawRect(new Rect(rect.center.x - 2f, rect.yMax - 15f, 4f, 9f), color);
                    break;
                case "marine_pit_compounds":
                    DrawRect(new Rect(rect.x + 21f, rect.y + 5f, 8f, 12f), color);
                    DrawLine(
                        new Vector2(rect.x + 21f, rect.y + 16f),
                        new Vector2(rect.x + 10f, rect.y + 39f),
                        color,
                        4f);
                    DrawLine(
                        new Vector2(rect.x + 29f, rect.y + 16f),
                        new Vector2(rect.x + 40f, rect.y + 39f),
                        color,
                        4f);
                    DrawRect(new Rect(rect.x + 10f, rect.y + 37f, 30f, 5f), color);
                    DrawRect(new Rect(rect.x + 15f, rect.y + 30f, 20f, 7f), accentColor);
                    break;
                case "marine_breach_charge":
                    DrawRect(new Rect(rect.x + 9f, rect.y + 8f, 32f, 34f), color);
                    DrawRect(new Rect(rect.x + 14f, rect.y + 13f, 22f, 24f), panelColor);
                    DrawRect(new Rect(rect.x + 21f, rect.y + 17f, 8f, 16f), color);
                    DrawRect(new Rect(rect.x + 4f, rect.y + 18f, 7f, 14f), accentColor);
                    break;
                case "marine_industrial_shrapnel":
                    for (int index = 0; index < 8; index++)
                    {
                        float angle = index * Mathf.PI * 0.25f;
                        Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                        DrawLine(
                            rect.center + direction * 7f,
                            rect.center + direction * (18f + index % 2 * 4f),
                            color,
                            4f);
                    }
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.center.x - 6f, rect.center.y - 6f, 12f, 12f),
                        accentColor);
                    break;
                case "marine_thermobaric_rocket":
                    DrawRect(new Rect(rect.x + 10f, rect.y + 20f, 27f, 10f), color);
                    DrawLine(
                        new Vector2(rect.x + 37f, rect.y + 20f),
                        new Vector2(rect.x + 46f, rect.center.y),
                        color,
                        5f);
                    DrawLine(
                        new Vector2(rect.x + 46f, rect.center.y),
                        new Vector2(rect.x + 37f, rect.y + 30f),
                        color,
                        5f);
                    DrawLine(
                        new Vector2(rect.x + 12f, rect.y + 20f),
                        new Vector2(rect.x + 4f, rect.y + 12f),
                        color,
                        4f);
                    DrawLine(
                        new Vector2(rect.x + 12f, rect.y + 30f),
                        new Vector2(rect.x + 4f, rect.y + 38f),
                        color,
                        4f);
                    break;
                case "marine_chain_reaction":
                    DrawLine(
                        new Vector2(rect.x + 17f, rect.y + 16f),
                        new Vector2(rect.x + 33f, rect.y + 25f),
                        color,
                        5f);
                    DrawLine(
                        new Vector2(rect.x + 33f, rect.y + 25f),
                        new Vector2(rect.x + 17f, rect.y + 35f),
                        color,
                        5f);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.x + 8f, rect.y + 7f, 18f, 18f), color);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.x + 25f, rect.y + 16f, 18f, 18f), color);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.x + 8f, rect.y + 26f, 18f, 18f), color);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.x + 13f, rect.y + 12f, 8f, 8f), panelColor);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.x + 30f, rect.y + 21f, 8f, 8f), panelColor);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.x + 13f, rect.y + 31f, 8f, 8f), panelColor);
                    break;
                case "marine_charge_crown":
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.center.x - 7f, rect.center.y - 7f, 14f, 14f),
                        panelColor);
                    for (int index = 0; index < 8; index++)
                    {
                        float angle = index * Mathf.PI * 0.25f;
                        Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                        Vector2 centre = rect.center + direction * 18f;
                        DrawRect(new Rect(centre.x - 4f, centre.y - 4f, 8f, 8f), color);
                    }
                    break;
                case "marine_scorched_earth_protocol":
                    PrototypeHudSkin.DrawDisc(
                        new Rect(rect.center.x - 7f, rect.center.y - 7f, 14f, 14f),
                        accentColor);
                    for (int index = 0; index < 3; index++)
                    {
                        float angle = -Mathf.PI * 0.5f + index * Mathf.PI * 2f / 3f;
                        Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                        Vector2 tangent = new(-direction.y, direction.x);
                        Vector2 root = rect.center + direction * 10f;
                        Vector2 tip = rect.center + direction * 22f;
                        DrawLine(root + tangent * 5f, tip, color, 5f);
                        DrawLine(tip, root - tangent * 5f, color, 5f);
                    }
                    break;
                default:
                    DrawGeneratedSkillGlyph(rect, skill.Identifier, color);
                    break;
            }
        }

        private void DrawGeneratedSkillGlyph(
            Rect rect,
            string identifier,
            Color color)
        {
            int hash = 17;
            if (!string.IsNullOrEmpty(identifier))
            {
                for (int index = 0; index < identifier.Length; index++)
                {
                    hash = unchecked(hash * 31 + identifier[index]);
                }
            }

            int motif = (hash & int.MaxValue) % 8;
            Rect core = new(rect.center.x - 7f, rect.center.y - 7f, 14f, 14f);
            switch (motif)
            {
                case 0:
                    DrawChevron(rect, color, 0f);
                    DrawChevron(rect, color, 12f);
                    break;
                case 1:
                    DrawPistol(rect, color, 4f);
                    DrawLine(
                        new Vector2(rect.x + 9f, rect.y + 38f),
                        new Vector2(rect.xMax - 6f, rect.y + 38f),
                        color,
                        3f);
                    break;
                case 2:
                    PrototypeHudSkin.DrawDisc(core, color);
                    DrawLine(rect.center, new Vector2(rect.x + 5f, rect.y + 5f), color, 4f);
                    DrawLine(rect.center, new Vector2(rect.xMax - 5f, rect.y + 5f), color, 4f);
                    DrawLine(rect.center, new Vector2(rect.center.x, rect.yMax - 4f), color, 4f);
                    break;
                case 3:
                    DrawRect(new Rect(rect.x + 7f, rect.y + 9f, 36f, 8f), color);
                    DrawRect(new Rect(rect.x + 12f, rect.y + 21f, 26f, 8f), color);
                    DrawRect(new Rect(rect.x + 18f, rect.y + 33f, 14f, 8f), color);
                    break;
                case 4:
                    DrawLine(new Vector2(rect.x + 7f, rect.center.y), new Vector2(rect.center.x, rect.y + 6f), color, 5f);
                    DrawLine(new Vector2(rect.center.x, rect.y + 6f), new Vector2(rect.xMax - 7f, rect.center.y), color, 5f);
                    DrawLine(new Vector2(rect.xMax - 7f, rect.center.y), new Vector2(rect.center.x, rect.yMax - 6f), color, 5f);
                    DrawLine(new Vector2(rect.center.x, rect.yMax - 6f), new Vector2(rect.x + 7f, rect.center.y), color, 5f);
                    break;
                case 5:
                    PrototypeHudSkin.DrawDisc(new Rect(rect.x + 5f, rect.y + 17f, 16f, 16f), color);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.center.x - 8f, rect.y + 7f, 16f, 16f), color);
                    PrototypeHudSkin.DrawDisc(new Rect(rect.xMax - 21f, rect.y + 17f, 16f, 16f), color);
                    DrawLine(new Vector2(rect.x + 13f, rect.y + 25f), new Vector2(rect.xMax - 13f, rect.y + 25f), color, 3f);
                    break;
                case 6:
                    DrawRect(new Rect(rect.center.x - 4f, rect.y + 5f, 8f, 38f), color);
                    DrawRect(new Rect(rect.x + 6f, rect.center.y - 4f, 38f, 8f), color);
                    PrototypeHudSkin.DrawDisc(core, new Color(0.02f, 0.04f, 0.04f, 1f));
                    break;
                default:
                    DrawCrosshair(rect, color);
                    DrawRect(new Rect(rect.center.x - 3f, rect.center.y - 3f, 6f, 6f), accentColor);
                    break;
            }
        }

        private static void DrawChevron(Rect rect, Color color, float yOffset)
        {
            Vector2 left = new(rect.x + 7f, rect.y + 11f + yOffset);
            Vector2 middle = new(rect.center.x, rect.y + 22f + yOffset);
            Vector2 right = new(rect.xMax - 7f, rect.y + 11f + yOffset);
            DrawLine(left, middle, color, 4f);
            DrawLine(middle, right, color, 4f);
        }

        private static void DrawPistol(Rect rect, Color color, float yOffset)
        {
            DrawRect(new Rect(rect.x + 5f, rect.y + 10f + yOffset, 34f, 7f), color);
            DrawRect(new Rect(rect.x + 25f, rect.y + 16f + yOffset, 8f, 18f), color);
        }

        private static void DrawCrosshair(Rect rect, Color color)
        {
            PrototypeHudSkin.DrawDisc(
                new Rect(rect.center.x - 15f, rect.center.y - 15f, 30f, 30f),
                color);
            PrototypeHudSkin.DrawDisc(
                new Rect(rect.center.x - 10f, rect.center.y - 10f, 20f, 20f),
                new Color(0.02f, 0.04f, 0.04f, 1f));
            DrawRect(new Rect(rect.x + 2f, rect.center.y - 2f, 46f, 4f), color);
            DrawRect(new Rect(rect.center.x - 2f, rect.y + 2f, 4f, 46f), color);
        }

        private static void DrawTreeGlyph(Rect rect, int index, Color color)
        {
            if (index == 0)
            {
                DrawRect(new Rect(rect.x + 12f, rect.y + 19f, 30f, 11f), color);
                DrawRect(new Rect(rect.x + 42f, rect.y + 21f, 9f, 5f), color);
                DrawRect(new Rect(rect.x + 5f, rect.y + 16f, 9f, 16f), color);
                DrawRect(new Rect(rect.x + 18f, rect.y + 29f, 6f, 14f), color);
                DrawRect(new Rect(rect.x + 31f, rect.y + 29f, 8f, 17f), color);
                return;
            }

            if (index == 1)
            {
                DrawRect(new Rect(rect.x + 7f, rect.y + 18f, 42f, 13f), color);
                DrawRect(new Rect(rect.x + 31f, rect.y + 31f, 11f, 14f), color);
                return;
            }

            PrototypeHudSkin.DrawDisc(
                new Rect(rect.center.x - 15f, rect.center.y - 15f, 30f, 30f),
                color);
            DrawRect(new Rect(rect.center.x - 3f, rect.y + 5f, 6f, 44f), color);
            DrawRect(new Rect(rect.x + 5f, rect.center.y - 3f, 44f, 6f), color);
        }

        private static int IndexOfSkill(
            SkillTreeDefinition tree,
            SkillDefinition skill)
        {
            if (tree == null || skill == null)
            {
                return -1;
            }

            for (int index = 0; index < tree.Skills.Count; index++)
            {
                if (tree.Skills[index] == skill)
                {
                    return index;
                }
            }

            return -1;
        }

        private static string FormatValue(
            SkillRankValue value,
            int rank,
            int maximumInvestedRank)
        {
            float evaluated = value.Evaluate(rank, maximumInvestedRank);
            string formatted = value.IsWholeNumber
                ? Mathf.RoundToInt(evaluated).ToString()
                : evaluated.ToString("0.##");
            return formatted + GetValueUnit(value.Identifier);
        }

        private static string GetValueUnit(string identifier)
        {
            if (identifier == null)
            {
                return string.Empty;
            }

            if (identifier.Contains("percent"))
            {
                return " %";
            }

            if (identifier.EndsWith("seconds", StringComparison.Ordinal))
            {
                return " s";
            }

            if (identifier.EndsWith("metres", StringComparison.Ordinal))
            {
                return " m";
            }

            return identifier.EndsWith("degrees", StringComparison.Ordinal)
                ? "°"
                : string.Empty;
        }

        private static string GetValueLabel(string identifier)
        {
            return identifier switch
            {
                "cooldown_seconds" => "Recharge",
                "duration_seconds" => "Durée",
                "direct_damage_percent" => "Dégâts directs",
                "base_damage_percent" => "Dégâts de base",
                "primary_damage_percent" => "Dégâts principaux",
                "damage_percent" => "Dégâts",
                "damage_reduction_percent" => "Réduction des dégâts",
                "damage_per_second_percent" => "Dégâts par seconde",
                "knockback_resistance_percent" => "Résistance au recul",
                "movement_penalty_percent" => "Pénalité de déplacement",
                "guard_generation_internal_cooldown_seconds" =>
                    "Délai interne de Garde",
                "shield_block_angle_bonus_degrees" =>
                    "Angle de blocage du bouclier",
                "burn_charges_applied" => "Combustions appliquées",
                "burn_damage_per_second_percent" =>
                    "Combustion par seconde",
                "burn_duration_seconds" => "Durée de Combustion",
                "maximum_active_main_chassis" =>
                    "Châssis principaux actifs",
                "shared_deployment_cooldown_seconds" =>
                    "Recharge de déploiement partagée",
                "companion_damage_percent" => "Dégâts du compagnon",
                "movement_speed_percent" => "Vitesse de déplacement",
                "signature_leap_damage_percent" =>
                    "Dégâts du bond signature",
                "signature_leap_radius_metres" =>
                    "Rayon du bond signature",
                "signature_cooldown_seconds" =>
                    "Recharge de la commande signature",
                "all_chassis_action_speed_per_invested_rank_percent" =>
                    "Vitesse de tous les châssis par rang dur",
                "projectile_count" => "Tirs",
                "weapon_damage_percent" => "Dégâts",
                "burst_duration_seconds" => "Durée de la rafale",
                "arming_window_seconds" => "Fenêtre d'armement",
                "additional_ammunition_cost" =>
                    "Munitions supplémentaires",
                "ammunition_cost" => "Munitions consommées",
                "impact_force_percent" => "Force d'impact",
                "fracture_charges" => "Fractures appliquées",
                "movement_distance_metres" => "Distance",
                "magazine_refill_percent" => "Chargeur",
                "saturation_retention_seconds" =>
                    "Saturation conservée",
                "slide_distance_bonus_percent" =>
                    "Bonus de glissade",
                "successive_hit_window_seconds" =>
                    "Fenêtre entre impacts",
                "maximum_saturation_charges" => "Saturation maximale",
                "fire_rate_per_charge_percent" => "Cadence par charge",
                "reload_speed_per_charge_percent" => "Recharge par charge",
                "decay_delay_seconds" => "Délai avant perte",
                "decay_interval_seconds" => "Perte d'une charge",
                "ballistic_momentum_duration_seconds" => "Durée de l'Élan",
                "saturation_loss_reduction_percent" => "Perte de Saturation réduite",
                "energy_cost_reduction_percent" => "Coût d'Énergie réduit",
                "sprint_reload_unlock_rank" =>
                    "Recharge en sprint au rang",
                "slide_reload_unlock_rank" =>
                    "Recharge en glissade au rang",
                "cone_angle_degrees" => "Angle",
                "range_metres" => "Portée",
                "shots_per_weapon" => "Tirs par arme",
                "maximum_hits_per_target" => "Impacts max. par cible",
                "maximum_consumed_saturation" =>
                    "Saturation consommée",
                "damage_per_consumed_charge_percent" =>
                    "Dégâts par charge",
                "first_riddled_threshold" => "Premier Criblé à",
                "second_riddled_threshold" => "Second Criblé à",
                "retained_saturation_percent" =>
                    "Saturation conservée",
                "empowered_shot_count" => "Tirs renforcés",
                "empowered_shot_damage_percent" => "Dégâts renforcés",
                "penetration_unlock_rank" =>
                    "Pénétration débloquée au rang",
                "required_saturation_charges" =>
                    "Saturation requise",
                "impact_trigger_interval" => "Impacts requis",
                "internal_cooldown_seconds" => "Délai interne",
                "ammunition_restored_per_kill" =>
                    "Munitions rendues par élimination",
                "close_range_metres" => "Portée rapprochée",
                "close_range_damage_bonus_percent" =>
                    "Bonus à courte portée",
                "riddled_detonation_weapon_damage_percent" =>
                    "Détonation de Criblé",
                "fire_rate_bonus_percent" => "Bonus de cadence",
                "reload_speed_bonus_percent" => "Bonus de rechargement",
                "armour_ignored_percent" => "Armure ignorée",
                "stagger_bonus_percent" => "Bonus de stagger",
                "first_penetration_unlock_rank" =>
                    "Première traversée au rang",
                "second_penetration_unlock_rank" =>
                    "Seconde traversée au rang",
                "damage_loss_per_penetrated_target_percent" =>
                    "Perte par cible traversée",
                "minimum_transmitted_damage_percent" =>
                    "Dégâts transmis minimaux",
                "grounded_activation_delay_seconds" =>
                    "Délai d'activation au sol",
                "brace_grace_duration_seconds" => "Grâce de l'Affût",
                "preparation_time_reduction_percent" =>
                    "Préparation réduite",
                "movement_penalty_reduction_percent" =>
                    "Pénalité de déplacement réduite",
                "preparation_seconds" => "Préparation",
                "anchor_duration_seconds" => "Durée d'ancrage",
                "boss_stagger_bonus_percent" => "Stagger contre les boss",
                "boss_armour_broken_extension_seconds" =>
                    "Armure rompue prolongée",
                "maximum_fracture_charges" => "Fractures maximales",
                "heavy_damage_per_fracture_percent" =>
                    "Dégâts lourds par Fracture",
                "stagger_per_fracture_percent" =>
                    "Stagger par Fracture",
                "armour_broken_duration_seconds" =>
                    "Durée d'Armure rompue",
                "armour_reduction_percent" => "Armure réduite",
                "shockwave_radius_metres" => "Rayon de l'onde",
                "immovable_stagger_bonus_percent" =>
                    "Stagger contre cible inamovible",
                "last_round_damage_bonus_percent" =>
                    "Dégâts de la dernière munition",
                "magazine_refill_on_trigger_percent" =>
                    "Chargeur chambré",
                "maximum_triggers_per_reload" =>
                    "Déclenchements par rechargement",
                "shockwave_damage_percent" => "Dégâts de l'onde",
                "maximum_consumed_fracture_charges" =>
                    "Fractures consommées",
                "damage_per_consumed_fracture_percent" =>
                    "Dégâts par Fracture",
                "armour_broken_damage_bonus_percent" =>
                    "Bonus contre Armure rompue",
                "nearby_fracture_charges" =>
                    "Fractures aux cibles secondaires",
                "siege_protocol_duration_seconds" =>
                    "Durée du Protocole",
                "heavy_skill_cooldown_recovery_percent" =>
                    "Récupération des compétences lourdes",
                "additional_fracture_from_explosion" =>
                    "Fracture explosive supplémentaire",
                "explosion_fracture_internal_cooldown_seconds" =>
                    "Délai interne par cible",
                "movement_speed_bonus_percent" =>
                    "Bonus de déplacement",
                "saturation_retained_on_reload_percent" =>
                    "Saturation conservée au rechargement",
                "duration_per_kill_seconds" =>
                    "Durée gagnée par élimination",
                "maximum_added_duration_seconds" =>
                    "Durée supplémentaire maximale",
                "required_saturation_percent" => "Saturation requise",
                "adrenaline_duration_seconds" => "Durée d'Adrénaline",
                "minimum_retained_saturation_percent" =>
                    "Plancher de Saturation",
                "riddled_kill_combat_reload_recovery_seconds" =>
                    "Recharge récupérée sur élimination Criblée",
                "maximum_charges" => "Charges maximales",
                "charge_cooldown_seconds" => "Recharge d'une charge",
                "explosion_radius_metres" => "Rayon d'explosion",
                "maximum_fuse_seconds" => "Fusée maximale",
                "explosion_radius_bonus_percent" => "Bonus de rayon",
                "self_explosion_damage_reduction_percent" =>
                    "Dégâts personnels réduits",
                "grenade_charge_unlock_rank" =>
                    "Charge de grenade au rang",
                "mine_charge_unlock_rank" => "Charge de mine au rang",
                "maximum_active_mines" => "Mines actives",
                "persistence_seconds" => "Persistance",
                "alternation_window_seconds" => "Fenêtre d'alternance",
                "unstable_mix_duration_seconds" => "Durée du Mélange",
                "mixed_explosive_damage_bonus_percent" =>
                    "Dégâts du Mélange",
                "secondary_effect_damage_bonus_percent" =>
                    "Effets secondaires du Mélange",
                "maximum_unstable_mix_charges" => "Mélanges maximaux",
                "maximum_active_charges" => "Charges actives",
                "direct_hit_bonus_percent" => "Bonus sur cible fixée",
                "additional_attached_charge_damage_percent" =>
                    "Dégâts des charges suivantes",
                "shredded_duration_seconds" => "Durée de Déchiqueté",
                "physical_damage_taken_percent" =>
                    "Dégâts physiques reçus",
                "fragments_per_explosive_kill" =>
                    "Fragments par élimination",
                "fragment_damage_percent" => "Dégâts des fragments",
                "fragment_spawn_internal_cooldown_seconds" =>
                    "Délai interne des fragments",
                "maximum_secondary_chain_depth" =>
                    "Cascades secondaires",
                "burning_zone_duration_seconds" => "Durée de combustion",
                "lingering_damage_percent" => "Dégâts persistants",
                "maximum_primer_charges" => "Amorces maximales",
                "reaction_damage_per_primer_percent" =>
                    "Dégâts par Amorce",
                "secondary_radius_metres" => "Rayon secondaire",
                "reaction_internal_cooldown_seconds" =>
                    "Délai interne de réaction",
                "arming_delay_seconds" => "Délai d'armement",
                "safe_inner_radius_metres" => "Rayon intérieur sûr",
                "outer_radius_metres" => "Rayon extérieur",
                "additional_hit_damage_percent" =>
                    "Dégâts des impacts suivants",
                "sequence_window_seconds" => "Fenêtre de séquence",
                "required_distinct_explosive_skills" =>
                    "Compétences distinctes requises",
                "scorched_earth_duration_seconds" =>
                    "Durée de Terre brûlée",
                "explosive_damage_bonus_percent" =>
                    "Bonus de dégâts explosifs",
                "secondary_reaction_damage_bonus_percent" =>
                    "Bonus des réactions secondaires",
                "first_followup_charge_cooldown_refund_percent" =>
                    "Recharge remboursée",
                _ => HumanizeIdentifier(identifier)
            };
        }

        private static string HumanizeIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return "Valeur";
            }

            string[] source = identifier.Split('_');
            List<string> translated = new(source.Length);
            foreach (string token in source)
            {
                string word = token switch
                {
                    "damage" => "dégâts",
                    "direct" => "directs",
                    "base" => "base",
                    "primary" => "principaux",
                    "duration" => "durée",
                    "maximum" => "maximum",
                    "minimum" => "minimum",
                    "bonus" => "bonus",
                    "reduction" => "réduction",
                    "restored" => "rendue",
                    "restoration" => "restauration",
                    "generation" => "génération",
                    "regeneration" => "régénération",
                    "health" => "santé",
                    "shield" => "bouclier",
                    "energy" => "énergie",
                    "armour" => "armure",
                    "resistance" => "résistance",
                    "knockback" => "recul",
                    "penalty" => "pénalité",
                    "guard" => "Garde",
                    "carnage" => "Carnage",
                    "burn" => "Combustion",
                    "cryostasis" => "Cryostase",
                    "conductivity" => "Conductivité",
                    "corrosion" => "Corrosion",
                    "radius" => "rayon",
                    "range" => "portée",
                    "charges" => "charges",
                    "charge" => "charge",
                    "targets" => "cibles",
                    "target" => "cible",
                    "consumed" => "consommées",
                    "applied" => "appliquées",
                    "transferred" => "transmises",
                    "remaining" => "restants",
                    "seconds" => "secondes",
                    "second" => "seconde",
                    "metres" => "mètres",
                    "count" => "nombre",
                    "speed" => "vitesse",
                    "movement" => "déplacement",
                    "action" => "action",
                    "attack" => "attaque",
                    "cooldown" => "recharge",
                    "internal" => "interne",
                    "shared" => "partagée",
                    "deployment" => "déploiement",
                    "signature" => "signature",
                    "leap" => "bond",
                    "companion" => "compagnon",
                    "chassis" => "châssis",
                    "shielded" => "avec bouclier",
                    "unshielded" => "sans bouclier",
                    "block" => "blocage",
                    "angle" => "angle",
                    "degrees" => "degrés",
                    "per" => "par",
                    "of" => "de",
                    "on" => "sur",
                    "next" => "prochaine",
                    "skill" => "compétence",
                    "hit" => "impact",
                    "hits" => "impacts",
                    "kill" => "élimination",
                    "kills" => "éliminations",
                    "percent" => string.Empty,
                    _ => token
                };
                if (!string.IsNullOrEmpty(word))
                {
                    translated.Add(word);
                }
            }

            string label = string.Join(" ", translated);
            return char.ToUpperInvariant(label[0]) + label.Substring(1);
        }

        private static string GetInvestmentBlockerLabel(
            SkillInvestmentBlocker blocker)
        {
            return blocker switch
            {
                SkillInvestmentBlocker.None => "Point disponible",
                SkillInvestmentBlocker.LevelLocked => "Niveau requis non atteint",
                SkillInvestmentBlocker.MissingPrerequisite => "Prérequis manquant",
                SkillInvestmentBlocker.MaximumRankReached => "Rang maximum atteint",
                SkillInvestmentBlocker.NoSkillPoints => "Aucun point disponible",
                SkillInvestmentBlocker.UnavailableToCharacter =>
                    "Compétence indisponible",
                _ => "Compétence manquante"
            };
        }

        private static string GetAssignmentBlockerLabel(
            SkillAssignmentBlocker blocker)
        {
            return blocker switch
            {
                SkillAssignmentBlocker.None => "Affectation inchangée",
                SkillAssignmentBlocker.InvalidSlot => "Emplacement invalide",
                SkillAssignmentBlocker.PassiveSkill =>
                    "Une compétence passive ne peut pas être équipée",
                SkillAssignmentBlocker.SkillNotLearned =>
                    "Cette compétence active n'est pas apprise",
                _ => "Sélectionnez une compétence active"
            };
        }

        private void CacheReferences()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (skillProgression == null)
            {
                skillProgression = GetComponent<CharacterSkillProgression>();
            }

            if (activeSkillBar == null)
            {
                activeSkillBar = GetComponent<ActiveSkillBar>();
            }

            if (interfaceCoordinator == null)
            {
                interfaceCoordinator =
                    GetComponent<PrototypeInterfaceCoordinator>();
            }
        }

        private void ClampSelection()
        {
            int treeCount = skillProgression?.AvailableTrees.Count ?? 0;
            selectedTreeIndex = Mathf.Clamp(
                selectedTreeIndex,
                0,
                Mathf.Max(0, treeCount - 1));
            selectedSkillIndex = Mathf.Clamp(
                selectedSkillIndex,
                0,
                Mathf.Max(0, CurrentSkillCount - 1));
            selectedControlIndex = Mathf.Clamp(
                selectedControlIndex,
                0,
                Mathf.Max(0, TotalControlCount - 1));
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

        private void SetFeedback(string message)
        {
            feedbackMessage = message ?? string.Empty;
            feedbackUntil = Time.unscaledTime + 2.5f;
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 29,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor }
            };
            smallStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.56f, 0.72f, 0.69f, 1f) }
            };
            nodeNameStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                normal = { textColor = textColor }
            };
            nodeMarkStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textColor }
            };
            detailTitleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = textColor }
            };
            detailBodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = new Color(0.82f, 0.8f, 0.7f, 1f) }
            };
            detailLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = accentColor }
            };
            detailValueStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.68f, 0.76f, 0.69f, 1f) }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textColor },
                hover = { textColor = Color.white },
                active = { textColor = accentColor }
            };
            feedbackStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = accentColor }
            };
            invisibleButtonStyle ??= new GUIStyle(GUIStyle.none);
        }

        private bool DrawButton(Rect rect, string label, bool selected)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);
            PrototypeInterfaceCursor.RegisterInteractive(rect, GUI.enabled);
            Color outer = selected
                ? accentColor
                : hovered
                    ? activeColor
                    : borderColor;
            DrawNotchedFill(rect, outer, 5f);
            Rect inner = new(
                rect.x + 2f,
                rect.y + 2f,
                rect.width - 4f,
                rect.height - 4f);
            DrawNotchedFill(
                inner,
                GUI.enabled
                    ? new Color(0.024f, 0.047f, 0.052f, 0.98f)
                    : new Color(0.024f, 0.03f, 0.032f, 0.9f),
                4f);
            return GUI.Button(rect, label, buttonStyle);
        }

        private void DrawInset(Rect rect)
        {
            DrawNotchedFill(rect, new Color(0.17f, 0.22f, 0.21f, 1f), 9f);
            PrototypeHudSkin.DrawTiledNotchedTexture(
                rect,
                new Color(0.45f, 0.48f, 0.4f, 0.38f),
                9f,
                215f);
            Rect inner = new(
                rect.x + 4f,
                rect.y + 4f,
                rect.width - 8f,
                rect.height - 8f);
            DrawNotchedFill(inner, new Color(0.01f, 0.022f, 0.024f, 0.98f), 7f);
            PrototypeHudSkin.DrawTiledNotchedTexture(
                inner,
                new Color(0.19f, 0.27f, 0.25f, 0.2f),
                7f,
                340f);
        }

        private static void DrawLine(
            Vector2 start,
            Vector2 end,
            Color color,
            float thickness)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            Vector2 delta = end - start;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.color = color;
            GUI.DrawTexture(
                new Rect(start.x, start.y - thickness * 0.5f,
                    delta.magnitude, thickness),
                Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private static void DrawBolt(Vector2 centre)
        {
            PrototypeHudSkin.DrawDisc(
                new Rect(centre.x - 5f, centre.y - 5f, 10f, 10f),
                new Color(0.08f, 0.1f, 0.09f, 1f));
            DrawLine(
                new Vector2(centre.x - 3f, centre.y + 2f),
                new Vector2(centre.x + 3f, centre.y - 2f),
                new Color(0.45f, 0.43f, 0.34f, 1f),
                1f);
        }

        private static Rect ExpandRect(Rect rect, float amount)
        {
            return new Rect(
                rect.x - amount,
                rect.y - amount,
                rect.width + amount * 2f,
                rect.height + amount * 2f);
        }

        private static void DrawOutline(
            Rect rect,
            Color color,
            float thickness)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(
                rect.x,
                rect.yMax - thickness,
                rect.width,
                thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(
                rect.xMax - thickness,
                rect.y,
                thickness,
                rect.height), color);
        }

        private static void DrawNotchedFill(
            Rect rect,
            Color color,
            float notch)
        {
            DrawRect(
                new Rect(
                    rect.x + notch,
                    rect.y,
                    rect.width - notch * 2f,
                    rect.height),
                color);
            DrawRect(
                new Rect(
                    rect.x,
                    rect.y + notch,
                    rect.width,
                    rect.height - notch * 2f),
                color);
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
