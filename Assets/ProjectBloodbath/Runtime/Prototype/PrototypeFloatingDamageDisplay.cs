using System.Collections.Generic;
using ProjectBloodbath.Combat;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    public sealed class PrototypeFloatingDamageDisplay : MonoBehaviour
    {
        private const string PreferenceKey =
            "ProjectBloodbath.Options.ShowDamageNumbers";

        private sealed class FloatingNumber
        {
            public Transform Target;
            public Vector3 LocalHeadAnchor;
            public Vector3 LastWorldAnchor;
            public float HorizontalOffset;
            public float StartedAt;
            public string Text;
        }

        [SerializeField] private Camera worldCamera;
        [SerializeField] private FloatingDamageSettings settings;
        [SerializeField] private bool damageNumbersVisible = true;
        [SerializeField] private bool loadSavedPreference = true;

        private readonly List<FloatingNumber> numbers = new();
        private Transform playerRoot;
        private GUIStyle numberStyle;
        private int spawnSequence;

        public FloatingDamageSettings Settings => settings;
        public bool DamageNumbersVisible => damageNumbersVisible;
        public int ActiveNumberCount => numbers.Count;
        public int SpawnCount { get; private set; }
        public Vector3 LastSpawnWorldPosition { get; private set; }

        public void Configure(
            Camera displayCamera,
            FloatingDamageSettings displaySettings)
        {
            worldCamera = displayCamera;
            settings = displaySettings;
        }

        public void SetDamageNumbersVisible(bool visible)
        {
            SetDamageNumbersVisible(visible, true);
        }

        public void SetDamageNumbersVisible(bool visible, bool savePreference)
        {
            damageNumbersVisible = visible;
            if (!visible)
            {
                numbers.Clear();
            }

            if (savePreference)
            {
                PlayerPrefs.SetInt(PreferenceKey, visible ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void Awake()
        {
            playerRoot = transform.root;
            if (worldCamera == null)
            {
                worldCamera = GetComponentInChildren<Camera>(true);
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (loadSavedPreference && PlayerPrefs.HasKey(PreferenceKey))
            {
                damageNumbersVisible = PlayerPrefs.GetInt(PreferenceKey) != 0;
            }
        }

        private void OnEnable()
        {
            CombatEvents.CombatantDamaged += OnCombatantDamaged;
        }

        private void OnDisable()
        {
            CombatEvents.CombatantDamaged -= OnCombatantDamaged;
            numbers.Clear();
        }

        private void Update()
        {
            if (settings == null)
            {
                numbers.Clear();
                return;
            }

            for (int index = numbers.Count - 1; index >= 0; index--)
            {
                if (Time.time - numbers[index].StartedAt >= settings.Duration)
                {
                    numbers.RemoveAt(index);
                }
            }
        }

        private void OnGUI()
        {
            if (
                !damageNumbersVisible ||
                settings == null ||
                worldCamera == null ||
                numbers.Count == 0)
            {
                return;
            }

            EnsureStyle();
            for (int index = 0; index < numbers.Count; index++)
            {
                DrawNumber(numbers[index]);
            }
        }

        private void OnCombatantDamaged(CombatDamageEvent damageEvent)
        {
            if (
                !damageNumbersVisible ||
                settings == null ||
                damageEvent.Target == null ||
                damageEvent.Damage.Source == null ||
                damageEvent.AppliedAmount <= 0f)
            {
                return;
            }

            Transform sourceRoot = damageEvent.Damage.Source.transform.root;
            Transform targetRoot = damageEvent.Target.transform.root;
            if (
                sourceRoot != playerRoot ||
                targetRoot == playerRoot ||
                damageEvent.Target.GetComponentInParent<
                    PrototypeEnemyController>() == null)
            {
                return;
            }

            Vector3 worldAnchor = FindHeadAnchor(
                damageEvent.Target.transform,
                settings.HeadPadding);
            AddNumber(
                damageEvent.Target.transform,
                worldAnchor,
                damageEvent.AppliedAmount);
        }

        private void AddNumber(
            Transform target,
            Vector3 worldAnchor,
            float amount)
        {
            while (numbers.Count >= settings.MaximumVisibleNumbers)
            {
                numbers.RemoveAt(0);
            }

            float horizontalOffset = ((spawnSequence % 3) - 1) * 0.12f;
            spawnSequence++;
            numbers.Add(new FloatingNumber
            {
                Target = target,
                LocalHeadAnchor = target.InverseTransformPoint(worldAnchor),
                LastWorldAnchor = worldAnchor,
                HorizontalOffset = horizontalOffset,
                StartedAt = Time.time,
                Text = Mathf.RoundToInt(amount).ToString()
            });
            SpawnCount++;
            LastSpawnWorldPosition = worldAnchor;
        }

        private void DrawNumber(FloatingNumber number)
        {
            float progress = Mathf.Clamp01(
                (Time.time - number.StartedAt) / settings.Duration);
            float easedRise = 1f - (1f - progress) * (1f - progress);
            Vector3 anchor = number.LastWorldAnchor;
            if (number.Target != null)
            {
                anchor = number.Target.TransformPoint(number.LocalHeadAnchor);
                number.LastWorldAnchor = anchor;
            }

            Vector3 worldPosition =
                anchor +
                Vector3.up * settings.RiseDistance * easedRise +
                worldCamera.transform.right * number.HorizontalOffset;
            Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f)
            {
                return;
            }

            float fade = Mathf.Clamp01((1f - progress) / 0.4f);
            float scale = Mathf.Lerp(1.1f, 0.9f, progress);
            numberStyle.fontSize = Mathf.Max(
                8,
                Mathf.RoundToInt(settings.FontSize * scale));

            Rect labelRect = new(
                screenPosition.x - 70f,
                Screen.height - screenPosition.y - 26f,
                140f,
                52f);
            DrawOutlinedLabel(labelRect, number.Text, fade);
        }

        private void DrawOutlinedLabel(Rect rect, string text, float alpha)
        {
            Color damageColor = settings.DamageColor;
            Color outlineColor = settings.OutlineColor;
            damageColor.a *= alpha;
            outlineColor.a *= alpha;

            numberStyle.normal.textColor = outlineColor;
            GUI.Label(new Rect(rect.x - 2f, rect.y, rect.width, rect.height),
                text, numberStyle);
            GUI.Label(new Rect(rect.x + 2f, rect.y, rect.width, rect.height),
                text, numberStyle);
            GUI.Label(new Rect(rect.x, rect.y - 2f, rect.width, rect.height),
                text, numberStyle);
            GUI.Label(new Rect(rect.x, rect.y + 2f, rect.width, rect.height),
                text, numberStyle);

            numberStyle.normal.textColor = damageColor;
            GUI.Label(rect, text, numberStyle);
        }

        private void EnsureStyle()
        {
            if (numberStyle != null)
            {
                return;
            }

            numberStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        private static Vector3 FindHeadAnchor(
            Transform target,
            float padding)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            Renderer headRenderer = null;
            Bounds combinedBounds = default;
            bool hasBounds = false;

            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer candidate = renderers[index];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.name == "Head")
                {
                    headRenderer = candidate;
                }

                if (!hasBounds)
                {
                    combinedBounds = candidate.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(candidate.bounds);
                }
            }

            if (headRenderer != null)
            {
                Bounds headBounds = headRenderer.bounds;
                return new Vector3(
                    headBounds.center.x,
                    headBounds.max.y + padding,
                    headBounds.center.z);
            }

            if (hasBounds)
            {
                return new Vector3(
                    combinedBounds.center.x,
                    combinedBounds.max.y + padding,
                    combinedBounds.center.z);
            }

            return target.position + Vector3.up * (1.8f + padding);
        }
    }
}
