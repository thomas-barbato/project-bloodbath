using ProjectBloodbath.Combat;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(PrototypeWeaponLoadout))]
    [RequireComponent(typeof(AbilityResource))]
    public sealed class PrototypeCombatHud : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const int AbilitySlotCount = 5;

        [SerializeField] private Health health;
        [SerializeField] private AbilityResource abilityResource;
        [SerializeField] private PrototypeWeaponLoadout weaponLoadout;
        [SerializeField] private HitscanWeapon rangedWeapon;
        [SerializeField] private HitscanWeapon leftRangedWeapon;
        [SerializeField] private PrototypePlayerLife playerLife;
        [SerializeField] private PrototypeShockwaveAbility activeAbility;
        [SerializeField] private PrototypeBloodHarvestPassive passiveAbility;
        [SerializeField] private CharacterProgression characterProgression;
        [SerializeField] private CharacterStatistics characterStatistics;
        [SerializeField] private ActiveSkillBar activeSkillBar;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private Color panelColor =
            new(0.018f, 0.033f, 0.038f, 0.94f);
        [SerializeField] private Color borderColor =
            new(0.28f, 0.4f, 0.4f, 0.95f);
        [SerializeField] private Color accentColor =
            new(0.72f, 0.06f, 0.045f, 0.98f);
        [SerializeField] private Color textColor =
            new(0.88f, 0.86f, 0.72f, 1f);

        private GUIStyle smallLabelStyle;
        private GUIStyle valueStyle;
        private GUIStyle centeredStatusStyle;
        private GUIStyle microLabelStyle;
        private GUIStyle abilityHeaderStyle;
        private GUIStyle slotLabelStyle;
        private GUIStyle slotShortcutStyle;

        private HitscanWeapon DisplayedRightRangedWeapon =>
            weaponLoadout != null
                ? weaponLoadout.ActiveRightRangedWeapon
                : rangedWeapon;
        private HitscanWeapon DisplayedLeftRangedWeapon =>
            weaponLoadout != null
                ? weaponLoadout.ActiveLeftRangedWeapon
                : leftRangedWeapon;

        public float HealthRatio => health == null || health.Maximum <= 0f
            ? 0f
            : Mathf.Clamp01(health.Current / health.Maximum);
        public string HealthLabel => health == null
            ? "-- / --"
            : $"{Mathf.CeilToInt(health.Current)} / " +
              $"{Mathf.CeilToInt(health.Maximum)}";
        public float AbilityResourceRatio => abilityResource == null
            ? 0f
            : abilityResource.Ratio;
        public string AbilityResourceLabel => abilityResource == null
            ? "-- / --"
            : $"{Mathf.CeilToInt(abilityResource.Current)} / " +
              $"{Mathf.CeilToInt(abilityResource.Maximum)}";
        public string WeaponLabel => weaponLoadout == null
            ? "AUCUNE ARME"
            : weaponLoadout.ActiveRightWeapon == null &&
              weaponLoadout.ActiveLeftWeapon == null
                ? "MAINS VIDES"
                : weaponLoadout.HasTwoActiveRangedWeapons
                    ? "FUSILS PROTOTYPES"
                    : weaponLoadout.ActiveRightMeleeWeapon != null &&
                      weaponLoadout.ActiveLeftWeapon == null
                        ? "ARME DE MÊLÉE"
                        : "ENSEMBLE DE MAINS";
        public string AmmunitionLabel
        {
            get
            {
                HitscanWeapon rightWeapon = DisplayedRightRangedWeapon;
                HitscanWeapon leftWeapon = DisplayedLeftRangedWeapon;
                if (!ShowsAmmunition)
                {
                    return "—";
                }

                if (rightWeapon == null)
                {
                    return $"G {FormatAmmunition(leftWeapon)}";
                }

                if (leftWeapon == null)
                {
                    return $"D {FormatAmmunition(rightWeapon)}";
                }

                return $"D {FormatAmmunition(rightWeapon)}   " +
                    $"G {FormatAmmunition(leftWeapon)}";
            }
        }
        public bool ShowsAmmunition =>
            DisplayedRightRangedWeapon != null ||
            DisplayedLeftRangedWeapon != null;
        public bool ShowsReload => ShowsAmmunition &&
            ((DisplayedRightRangedWeapon != null &&
              DisplayedRightRangedWeapon.IsReloading) ||
             (DisplayedLeftRangedWeapon != null &&
              DisplayedLeftRangedWeapon.IsReloading));
        public bool ShowsSoulRecovery =>
            playerLife != null && playerLife.IsSoul;
        public bool ShowsResurrectionPenalty =>
            playerLife != null &&
            !playerLife.IsSoul &&
            playerLife.ResurrectionPenaltyRemaining > 0f;
        public bool KeepsAbilityBarVisibleForOpenView =>
            interfaceCoordinator?.ActiveView is
                IPrototypeActiveSkillBarOverlay;
        public string AbilityLabel
        {
            get
            {
                if (KeepsAbilityBarVisibleForOpenView)
                {
                    return "COMPÉTENCES";
                }

                SkillDefinition skill = GetFirstAssignedSkill();
                return skill == null
                    ? "AUCUNE COMPÉTENCE"
                    : skill.DisplayName.ToUpperInvariant();
            }
        }
        public string AbilityStatusLabel
        {
            get
            {
                if (KeepsAbilityBarVisibleForOpenView)
                {
                    return string.Empty;
                }

                SkillDefinition skill = GetFirstAssignedSkill();
                if (skill == null)
                {
                    return "AUCUNE COMPÉTENCE ASSIGNÉE";
                }

                int rank = activeSkillBar?.SkillProgression?.GetEffectiveRank(
                    skill) ?? 0;
                return $"RANG {rank:00}  •  {skill.ResourceCost:0.#} ÉNERGIE";
            }
        }
        public int AbilitySlotCapacity => AbilitySlotCount;
        public bool IsAbilitySlotOccupied(int slotIndex)
        {
            return GetAbilitySlotSkill(slotIndex) != null;
        }

        public SkillDefinition GetAbilitySlotSkill(int slotIndex)
        {
            return activeSkillBar?.GetSkill(slotIndex);
        }

        public int GetAbilitySlotEffectiveRank(int slotIndex)
        {
            return activeSkillBar?.GetEffectiveRank(slotIndex) ?? 0;
        }

        public bool ShowsPassiveFeedback =>
            passiveAbility != null && passiveAbility.FeedbackRemaining > 0f;
        public string PassiveFeedbackLabel => passiveAbility?.Settings == null
            ? string.Empty
            : $"{passiveAbility.Settings.DisplayName.ToUpperInvariant()}  •  " +
              $"+{passiveAbility.LastRestoredAmount:0} ÉNERGIE";
        public string LevelLabel => characterProgression == null
            ? "NIVEAU —"
            : $"NIVEAU {characterProgression.CurrentLevel}";
        public string ExperienceLabel => characterProgression == null
            ? "EXP — / —"
            : characterProgression.IsAtMaximumLevel
                ? "EXP MAXIMUM"
                : $"EXP {characterProgression.CurrentExperience} / " +
                  $"{characterProgression.ExperienceRequiredForNextLevel}";
        public float ExperienceRatio => characterProgression == null
            ? 0f
            : characterProgression.ExperienceRatio;
        public string AttributePointsLabel => characterStatistics == null
            ? "— PT"
            : $"{characterStatistics.UnspentAttributePoints} PT" +
              (characterStatistics.UnspentAttributePoints > 1 ? "S" : string.Empty);

        public void Configure(
            Health playerHealth,
            AbilityResource resource,
            PrototypeWeaponLoadout loadout,
            HitscanWeapon rifle,
            PrototypePlayerLife life,
            PrototypeShockwaveAbility ability,
            PrototypeBloodHarvestPassive passive)
        {
            health = playerHealth;
            abilityResource = resource;
            weaponLoadout = loadout;
            rangedWeapon = rifle;
            playerLife = life;
            activeAbility = ability;
            passiveAbility = passive;
            leftRangedWeapon = loadout?.ActiveLeftRangedWeapon;
        }

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (abilityResource == null)
            {
                abilityResource = GetComponent<AbilityResource>();
            }

            if (weaponLoadout == null)
            {
                weaponLoadout = GetComponent<PrototypeWeaponLoadout>();
            }

            if (playerLife == null)
            {
                playerLife = GetComponent<PrototypePlayerLife>();
            }

            if (activeAbility == null)
            {
                activeAbility = GetComponent<PrototypeShockwaveAbility>();
            }

            if (passiveAbility == null)
            {
                passiveAbility = GetComponent<PrototypeBloodHarvestPassive>();
            }

            if (characterProgression == null)
            {
                characterProgression = GetComponent<CharacterProgression>();
            }

            if (characterStatistics == null)
            {
                characterStatistics = GetComponent<CharacterStatistics>();
            }

            if (activeSkillBar == null)
            {
                activeSkillBar = GetComponent<ActiveSkillBar>();
            }

            if (rangedWeapon == null)
            {
                rangedWeapon = weaponLoadout?.ActiveRightRangedWeapon ??
                    GetComponentInChildren<HitscanWeapon>(true);
            }

            if (leftRangedWeapon == null)
            {
                leftRangedWeapon = weaponLoadout?.ActiveLeftRangedWeapon;
            }

            if (interfaceCoordinator == null)
            {
                interfaceCoordinator =
                    GetComponent<PrototypeInterfaceCoordinator>();
            }
        }

        private void OnGUI()
        {
            bool abilityBarOnly = KeepsAbilityBarVisibleForOpenView;
            if (
                health == null ||
                weaponLoadout == null ||
                (interfaceCoordinator != null &&
                 interfaceCoordinator.HasOpenView &&
                 !abilityBarOnly))
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

            if (abilityBarOnly)
            {
                DrawAbilityPanel(width, height);
                GUI.color = previousColor;
                GUI.matrix = previousMatrix;
                return;
            }

            DrawHealthPanel(height);
            DrawProgressionPanel();
            DrawWeaponPanel(width, height);
            DrawAbilityPanel(width, height);
            DrawCentralStatus(width, height);
            DrawDamageVignette(width, height);
            DrawAbilityFeedback(width, height);

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawHealthPanel(float height)
        {
            Rect panel = new(30f, height - 166f, 370f, 132f);
            DrawPanel(panel);
            DrawStatusLamp(new Rect(panel.x + 16f, panel.y + 14f, 8f, 8f),
                GetHealthDisplayColor());
            GUI.Label(new Rect(panel.x + 31f, panel.y + 9f, 250f, 20f),
                "BIO-MONITEUR  //  SUJET ACTIF", microLabelStyle);
            GUI.Label(new Rect(panel.x + 16f, panel.y + 35f, 120f, 22f),
                "INTÉGRITÉ", smallLabelStyle);
            GUI.Label(new Rect(panel.x + 184f, panel.y + 30f, 166f, 28f),
                HealthLabel, valueStyle);

            Rect bar = new(panel.x + 16f, panel.y + 61f, panel.width - 32f, 14f);
            DrawSegmentedBar(
                bar,
                HealthRatio,
                18,
                GetHealthDisplayColor());

            GUI.Label(new Rect(panel.x + 16f, panel.y + 82f, 120f, 22f),
                "ÉNERGIE", smallLabelStyle);
            GUI.Label(new Rect(panel.x + 184f, panel.y + 77f, 166f, 28f),
                AbilityResourceLabel, valueStyle);
            Rect resourceBar = new(panel.x + 16f, panel.y + 108f,
                panel.width - 32f, 11f);
            DrawSegmentedBar(
                resourceBar,
                AbilityResourceRatio,
                18,
                new Color(0.34f, 0.68f, 0.72f, 1f));
        }

        private void DrawWeaponPanel(float width, float height)
        {
            Rect panel = new(width - 464f, height - 112f, 430f, 72f);
            DrawPanel(panel);
            GUI.Label(new Rect(panel.x + 16f, panel.y + 10f, 190f, 22f),
                WeaponLabel, smallLabelStyle);
            GUI.Label(new Rect(panel.x + 116f, panel.y + 29f, 296f, 32f),
                AmmunitionLabel, valueStyle);

            if (!ShowsReload)
            {
                return;
            }

            float progress = Mathf.Max(
                DisplayedRightRangedWeapon?.ReloadProgress ?? 0f,
                DisplayedLeftRangedWeapon?.ReloadProgress ?? 0f);
            Rect reloadBar = new(panel.x + 16f, panel.y + 51f,
                panel.width - 32f, 6f);
            DrawRect(reloadBar, new Color(0.025f, 0.055f, 0.06f, 1f));
            DrawRect(new Rect(reloadBar.x, reloadBar.y,
                reloadBar.width * progress, reloadBar.height), textColor);
        }

        private void DrawProgressionPanel()
        {
            if (characterProgression == null)
            {
                return;
            }

            Rect panel = new(34f, 34f, 350f, 62f);
            DrawPanel(panel);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 9f, 94f, 22f),
                LevelLabel, smallLabelStyle);
            GUI.Label(new Rect(panel.x + 104f, panel.y + 9f, 162f, 22f),
                ExperienceLabel, smallLabelStyle);
            GUI.Label(new Rect(panel.x + 268f, panel.y + 9f, 66f, 22f),
                AttributePointsLabel, smallLabelStyle);

            Rect bar = new(panel.x + 14f, panel.y + 41f,
                panel.width - 28f, 7f);
            DrawRect(bar, new Color(0.025f, 0.055f, 0.06f, 1f));
            DrawRect(new Rect(
                bar.x,
                bar.y,
                bar.width * ExperienceRatio,
                bar.height),
                new Color(0.48f, 0.66f, 0.58f, 1f));
        }

        private void DrawCentralStatus(float width, float height)
        {
            if (ShowsSoulRecovery)
            {
                DrawStatus(width, height,
                    "ÂME — RETROUVEZ VOTRE CORPS", new Color(0.5f, 0.82f, 0.9f));
                return;
            }

            if (ShowsResurrectionPenalty)
            {
                DrawStatus(width, height,
                    $"RÉINCARNATION — DÉGÂTS RÉDUITS  " +
                    $"{playerLife.ResurrectionPenaltyRemaining:0.0}s",
                    textColor);
                return;
            }

            if (activeAbility != null && activeAbility.ShowsSynergyFeedback)
            {
                DrawStatus(
                    width,
                    height,
                    activeAbility.SynergyFeedbackLabel,
                    new Color(0.58f, 0.78f, 0.76f));
                return;
            }

            if (ShowsPassiveFeedback)
            {
                DrawStatus(
                    width,
                    height,
                    PassiveFeedbackLabel,
                    new Color(0.42f, 0.8f, 0.72f));
                return;
            }

            if (ShowsReload)
            {
                DrawStatus(width, height, "RECHARGEMENT", textColor);
            }
        }

        private void DrawAbilityPanel(float width, float height)
        {
            const float slotSize = 68f;
            const float slotGap = 8f;
            const float panelWidth = 410f;
            Rect panel = new(
                width * 0.5f - panelWidth * 0.5f,
                height - 162f,
                panelWidth,
                128f);
            DrawPanel(panel);
            DrawStatusLamp(
                new Rect(panel.x + 16f, panel.y + 14f, 8f, 8f),
                GetFirstAssignedSkill() != null
                    ? new Color(0.46f, 0.76f, 0.64f)
                    : new Color(0.62f, 0.46f, 0.2f));
            GUI.Label(
                new Rect(panel.x + 31f, panel.y + 7f, 240f, 22f),
                "COMPÉTENCES",
                microLabelStyle);

            float slotsWidth = AbilitySlotCount * slotSize +
                (AbilitySlotCount - 1) * slotGap;
            float firstSlotX = panel.x + (panel.width - slotsWidth) * 0.5f;
            for (int index = 0; index < AbilitySlotCount; index++)
            {
                DrawAbilitySlot(
                    new Rect(
                        firstSlotX + index * (slotSize + slotGap),
                        panel.y + 42f,
                        slotSize,
                        slotSize),
                    index);
            }
        }

        private void DrawAbilitySlot(Rect slot, int slotIndex)
        {
            SkillDefinition skill = GetAbilitySlotSkill(slotIndex);
            bool occupied = skill != null;
            Color outer = occupied
                ? new Color(0.4f, 0.58f, 0.56f, 1f)
                : new Color(0.19f, 0.27f, 0.28f, 1f);
            DrawNotchedFill(slot, outer, 6f);
            PrototypeHudSkin.DrawTiledNotchedTexture(
                slot,
                occupied
                    ? new Color(0.56f, 0.66f, 0.64f, 0.72f)
                    : new Color(0.42f, 0.48f, 0.48f, 0.52f),
                6f,
                256f);
            Rect interior = new(
                slot.x + 3f,
                slot.y + 3f,
                slot.width - 6f,
                slot.height - 6f);
            DrawNotchedFill(
                interior,
                occupied
                    ? new Color(0.035f, 0.045f, 0.052f, 0.98f)
                    : new Color(0.022f, 0.027f, 0.03f, 0.96f),
                4f);
            PrototypeHudSkin.DrawDisplayGlass(interior, occupied ? 0.98f : 0.88f);

            DrawRect(
                new Rect(interior.x + 4f, interior.y + 4f, interior.width - 8f, 2f),
                occupied
                    ? new Color(0.52f, 0.7f, 0.65f, 0.86f)
                    : new Color(0.25f, 0.34f, 0.34f, 0.66f));
            DrawCornerBolts(slot);

            if (!occupied)
            {
                DrawEmptySlotMark(interior);
                GUI.Label(
                    new Rect(slot.x + 7f, slot.y + 7f, 18f, 15f),
                    $"0{slotIndex + 1}",
                    slotShortcutStyle);
                return;
            }

            Rect iconRect = new(slot.x + 16f, slot.y + 14f, 36f, 36f);
            if (skill.Icon != null)
            {
                PrototypeHudSkin.DrawSprite(iconRect, skill.Icon);
            }
            else
            {
                DrawAssignedSkillGlyph(iconRect, slotIndex);
            }
            GUI.Label(
                new Rect(slot.x + 4f, slot.y + 48f, slot.width - 8f, 15f),
                GetCompactSkillLabel(skill),
                slotLabelStyle);
            GUI.Label(
                new Rect(slot.x + 5f, slot.y + 5f, slot.width - 10f, 16f),
                $"0{slotIndex + 1}",
                slotShortcutStyle);
            GUI.Label(
                new Rect(slot.x + 37f, slot.y + 5f, 25f, 16f),
                $"R{GetAbilitySlotEffectiveRank(slotIndex):00}",
                slotShortcutStyle);
        }

        private static void DrawAssignedSkillGlyph(Rect rect, int slotIndex)
        {
            Color glow = new(0.34f, 0.68f, 0.72f, 1f);
            Color core = new(0.88f, 0.86f, 0.72f, 1f);
            for (int index = 0; index < 3; index++)
            {
                float width = 12f + ((slotIndex + index * 2) % 4) * 3f;
                DrawRect(
                    new Rect(
                        rect.x + 7f,
                        rect.y + 8f + index * 9f,
                        width,
                        4f),
                    index == 1 ? core : glow);
            }
        }

        private SkillDefinition GetFirstAssignedSkill()
        {
            if (activeSkillBar == null)
            {
                return null;
            }

            for (int index = 0; index < AbilitySlotCount; index++)
            {
                SkillDefinition skill = activeSkillBar.GetSkill(index);
                if (skill != null)
                {
                    return skill;
                }
            }

            return null;
        }

        private static string GetCompactSkillLabel(SkillDefinition skill)
        {
            if (skill == null || string.IsNullOrWhiteSpace(skill.DisplayName))
            {
                return "VIDE";
            }

            string firstWord = skill.DisplayName.Trim().Split(' ')[0];
            return firstWord.Length <= 8
                ? firstWord.ToUpperInvariant()
                : firstWord[..8].ToUpperInvariant();
        }

        private static void DrawEmptySlotMark(Rect rect)
        {
            Color mark = new(0.29f, 0.42f, 0.41f, 0.58f);
            DrawRect(new Rect(rect.center.x - 9f, rect.center.y - 1f, 18f, 2f), mark);
            DrawRect(new Rect(rect.center.x - 1f, rect.center.y - 9f, 2f, 18f), mark);
            DrawRect(new Rect(rect.center.x - 3f, rect.center.y - 3f, 6f, 6f),
                new Color(0.02f, 0.038f, 0.042f, 1f));
        }

        private void DrawStatus(
            float width,
            float height,
            string message,
            Color color)
        {
            Rect panel = new(width * 0.5f - 230f, height - 216f, 460f, 38f);
            DrawPanel(panel);
            centeredStatusStyle.normal.textColor = color;
            GUI.Label(panel, message, centeredStatusStyle);
        }

        private void DrawDamageVignette(float width, float height)
        {
            if (playerLife == null || playerLife.DamageFlashRemaining <= 0f)
            {
                return;
            }

            Color flash = new(0.74f, 0.015f, 0.005f,
                Mathf.Clamp01(playerLife.DamageFlashRemaining / 0.14f) * 0.3f);
            DrawRect(new Rect(0f, 0f, width, 24f), flash);
            DrawRect(new Rect(0f, height - 24f, width, 24f), flash);
            DrawRect(new Rect(0f, 0f, 24f, height), flash);
            DrawRect(new Rect(width - 24f, 0f, 24f, height), flash);
        }

        private void DrawAbilityFeedback(float width, float height)
        {
            if (
                activeAbility == null ||
                activeAbility.ActivationFeedbackRemaining <= 0f)
            {
                return;
            }

            float alpha = Mathf.Clamp01(
                activeAbility.ActivationFeedbackRemaining / 0.16f) * 0.22f;
            Color pulse = new(0.22f, 0.62f, 0.66f, alpha);
            DrawRect(new Rect(0f, 0f, width, 14f), pulse);
            DrawRect(new Rect(0f, height - 14f, width, 14f), pulse);
            DrawRect(new Rect(0f, 0f, 14f, height), pulse);
            DrawRect(new Rect(width - 14f, 0f, 14f, height), pulse);
        }

        private void DrawPanel(Rect rect)
        {
            DrawNotchedFill(rect, new Color(0.03f, 0.045f, 0.048f, 0.99f), 10f);
            PrototypeHudSkin.DrawTiledNotchedTexture(
                rect,
                new Color(0.5f, 0.56f, 0.55f, 0.82f),
                10f,
                256f);
            DrawNotchedFill(
                new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f),
                new Color(0.26f, 0.38f, 0.38f, borderColor.a),
                8f);
            Rect interior = new(
                rect.x + 8f,
                rect.y + 8f,
                rect.width - 16f,
                rect.height - 16f);
            DrawNotchedFill(interior, panelColor, 6f);
            PrototypeHudSkin.DrawDisplayGlass(interior, panelColor.a);
            PrototypeHudSkin.DrawTiledNotchedTexture(
                interior,
                new Color(0.72f, 0.76f, 0.72f, 0.34f),
                6f,
                384f);
            DrawRect(
                new Rect(rect.x + 15f, rect.y + 7f, rect.width - 30f, 1f),
                new Color(0.52f, 0.7f, 0.65f, 0.48f));
            DrawRect(
                new Rect(rect.x + 15f, rect.yMax - 8f, rect.width - 30f, 2f),
                new Color(0.05f, 0.075f, 0.075f, 0.9f));
            DrawRect(
                new Rect(rect.x + 4f, rect.center.y - 11f, 3f, 22f),
                new Color(0.58f, 0.53f, 0.31f, 0.72f));
            DrawCornerBolts(rect);
        }

        private static void DrawNotchedFill(Rect rect, Color color, float notch)
        {
            DrawRect(
                new Rect(rect.x + notch, rect.y, rect.width - notch * 2f, rect.height),
                color);
            DrawRect(
                new Rect(rect.x, rect.y + notch, rect.width, rect.height - notch * 2f),
                color);
        }

        private static void DrawCornerBolts(Rect rect)
        {
            DrawBolt(new Vector2(rect.x + 9f, rect.y + 9f));
            DrawBolt(new Vector2(rect.xMax - 9f, rect.y + 9f));
            DrawBolt(new Vector2(rect.x + 9f, rect.yMax - 9f));
            DrawBolt(new Vector2(rect.xMax - 9f, rect.yMax - 9f));
        }

        private static void DrawBolt(Vector2 center)
        {
            DrawRect(new Rect(center.x - 3f, center.y - 3f, 6f, 6f),
                new Color(0.015f, 0.028f, 0.03f, 1f));
            DrawRect(new Rect(center.x - 2f, center.y - 2f, 4f, 4f),
                new Color(0.48f, 0.44f, 0.28f, 1f));
            DrawRect(new Rect(center.x - 1f, center.y - 2f, 2f, 4f),
                new Color(0.08f, 0.11f, 0.11f, 1f));
        }

        private static void DrawStatusLamp(Rect rect, Color color)
        {
            DrawRect(new Rect(rect.x - 2f, rect.y - 2f, rect.width + 4f, rect.height + 4f),
                new Color(0.01f, 0.025f, 0.028f, 1f));
            DrawRect(rect, color);
            DrawRect(new Rect(rect.x + 2f, rect.y + 1f, rect.width - 4f, 2f),
                new Color(1f, 0.86f, 0.55f, 0.42f));
        }

        private static void DrawSegmentedBar(
            Rect rect,
            float ratio,
            int segmentCount,
            Color fillColor)
        {
            DrawRect(rect, new Color(0.012f, 0.03f, 0.034f, 1f));
            Rect interior = new(rect.x + 2f, rect.y + 2f,
                rect.width - 4f, rect.height - 4f);
            float gap = 2f;
            float segmentWidth =
                (interior.width - gap * (segmentCount - 1)) / segmentCount;
            float filledSegments = Mathf.Clamp01(ratio) * segmentCount;
            for (int index = 0; index < segmentCount; index++)
            {
                Color color = index + 1 <= Mathf.CeilToInt(filledSegments)
                    ? fillColor
                    : new Color(0.055f, 0.09f, 0.09f, 0.82f);
                DrawRect(
                    new Rect(
                        interior.x + index * (segmentWidth + gap),
                        interior.y,
                        segmentWidth,
                        interior.height),
                    color);
            }
        }

        private void EnsureStyles()
        {
            smallLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = textColor }
            };
            valueStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
                normal = { textColor = textColor }
            };
            centeredStatusStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            microLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.55f, 0.72f, 0.68f, 1f) }
            };
            abilityHeaderStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
                normal = { textColor = textColor }
            };
            slotLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textColor }
            };
            slotShortcutStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.72f, 0.78f, 0.65f, 1f) }
            };
        }

        private Color GetHealthDisplayColor()
        {
            const float criticalThreshold = 0.2f;
            const float warningThreshold = 0.5f;
            Color critical = accentColor;
            Color warning = new(0.7f, 0.5f, 0.2f, 1f);
            Color stable = new(0.55f, 0.68f, 0.5f, 1f);

            if (HealthRatio <= criticalThreshold)
            {
                return Color.Lerp(
                    critical,
                    warning,
                    HealthRatio / criticalThreshold);
            }

            if (HealthRatio <= warningThreshold)
            {
                return Color.Lerp(
                    warning,
                    stable,
                    (HealthRatio - criticalThreshold) /
                    (warningThreshold - criticalThreshold));
            }

            return stable;
        }

        private static string FormatAmmunition(HitscanWeapon weapon)
        {
            return $"{weapon.CurrentMagazine:00}/{weapon.ReserveAmmo:000}";
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
