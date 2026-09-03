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
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private Color panelColor =
            new(0.025f, 0.018f, 0.015f, 0.88f);
        [SerializeField] private Color borderColor =
            new(0.42f, 0.09f, 0.045f, 0.95f);
        [SerializeField] private Color accentColor =
            new(0.86f, 0.22f, 0.08f, 0.98f);
        [SerializeField] private Color textColor =
            new(0.9f, 0.82f, 0.68f, 1f);

        private GUIStyle smallLabelStyle;
        private GUIStyle valueStyle;
        private GUIStyle centeredStatusStyle;

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
        public string AbilityLabel => activeAbility?.Settings == null
            ? "COMPÉTENCE VIDE"
            : activeAbility.Settings.DisplayName.ToUpperInvariant();
        public string AbilityStatusLabel
        {
            get
            {
                if (activeAbility?.Settings == null)
                {
                    return "—";
                }

                if (!activeAbility.HasEnoughResource)
                {
                    return "ÉNERGIE INSUFFISANTE";
                }

                return activeAbility.CooldownRemaining > 0f
                    ? $"{activeAbility.CooldownRemaining:0.0}s"
                    : $"PRÊTE  •  {activeAbility.Settings.ResourceCost:0} énergie";
            }
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
            if (
                health == null ||
                weaponLoadout == null ||
                (interfaceCoordinator != null &&
                 interfaceCoordinator.HasOpenView))
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
            Rect panel = new(34f, height - 146f, 330f, 106f);
            DrawPanel(panel);
            GUI.Label(new Rect(panel.x + 16f, panel.y + 10f, 120f, 22f),
                "INTÉGRITÉ", smallLabelStyle);
            GUI.Label(new Rect(panel.x + 150f, panel.y + 6f, 160f, 28f),
                HealthLabel, valueStyle);

            Rect bar = new(panel.x + 16f, panel.y + 39f, panel.width - 32f, 12f);
            DrawRect(bar, new Color(0.08f, 0.035f, 0.025f, 1f));
            Color healthColor = Color.Lerp(
                new Color(0.64f, 0.025f, 0.015f, 1f),
                accentColor,
                HealthRatio);
            DrawRect(new Rect(bar.x + 2f, bar.y + 2f,
                (bar.width - 4f) * HealthRatio, bar.height - 4f), healthColor);

            GUI.Label(new Rect(panel.x + 16f, panel.y + 58f, 120f, 22f),
                "ÉNERGIE", smallLabelStyle);
            GUI.Label(new Rect(panel.x + 150f, panel.y + 54f, 160f, 28f),
                AbilityResourceLabel, valueStyle);
            Rect resourceBar = new(panel.x + 16f, panel.y + 85f,
                panel.width - 32f, 8f);
            DrawRect(resourceBar, new Color(0.025f, 0.045f, 0.07f, 1f));
            DrawRect(new Rect(resourceBar.x + 2f, resourceBar.y + 2f,
                (resourceBar.width - 4f) * AbilityResourceRatio,
                resourceBar.height - 4f),
                new Color(0.12f, 0.52f, 0.76f, 1f));
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
            DrawRect(reloadBar, new Color(0.08f, 0.035f, 0.025f, 1f));
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
            DrawRect(bar, new Color(0.055f, 0.025f, 0.018f, 1f));
            DrawRect(new Rect(
                bar.x,
                bar.y,
                bar.width * ExperienceRatio,
                bar.height),
                new Color(0.72f, 0.38f, 0.08f, 1f));
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
                    new Color(0.72f, 0.38f, 0.9f));
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
            Rect panel = new(width * 0.5f - 180f, height - 112f, 360f, 72f);
            DrawPanel(panel);
            GUI.Label(new Rect(panel.x + 16f, panel.y + 9f, 44f, 24f),
                "[F]", valueStyle);
            GUI.Label(new Rect(panel.x + 67f, panel.y + 9f, 270f, 22f),
                AbilityLabel, smallLabelStyle);
            GUI.Label(new Rect(panel.x + 67f, panel.y + 31f, 270f, 22f),
                AbilityStatusLabel, smallLabelStyle);

            float readiness = activeAbility?.Settings == null
                ? 0f
                : 1f - activeAbility.CooldownProgress;
            Rect bar = new(panel.x + 16f, panel.y + 57f,
                panel.width - 32f, 5f);
            DrawRect(bar, new Color(0.045f, 0.025f, 0.065f, 1f));
            DrawRect(new Rect(bar.x, bar.y, bar.width * readiness, bar.height),
                new Color(0.48f, 0.18f, 0.72f, 1f));
        }

        private void DrawStatus(
            float width,
            float height,
            string message,
            Color color)
        {
            Rect panel = new(width * 0.5f - 230f, height - 176f, 460f, 38f);
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
            Color pulse = new(0.38f, 0.08f, 0.62f, alpha);
            DrawRect(new Rect(0f, 0f, width, 14f), pulse);
            DrawRect(new Rect(0f, height - 14f, width, 14f), pulse);
            DrawRect(new Rect(0f, 0f, 14f, height), pulse);
            DrawRect(new Rect(width - 14f, 0f, 14f, height), pulse);
        }

        private void DrawPanel(Rect rect)
        {
            DrawRect(rect, borderColor);
            DrawRect(new Rect(rect.x + 2f, rect.y + 2f,
                rect.width - 4f, rect.height - 4f), panelColor);
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
