using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [Serializable]
    public sealed class CharacterStatValue
    {
        [SerializeField] private CharacterStatDefinition definition;
        [SerializeField, Min(0)] private int baseValue;

        public CharacterStatDefinition Definition => definition;
        public int BaseValue => baseValue;

        public CharacterStatValue(
            CharacterStatDefinition statDefinition,
            int initialValue)
        {
            definition = statDefinition;
            baseValue = Mathf.Max(0, initialValue);
        }

        public void Increase(int amount)
        {
            baseValue = Mathf.Max(0, baseValue + amount);
        }
    }

    [DisallowMultipleComponent]
    public sealed class CharacterStatistics : MonoBehaviour
    {
        [SerializeField] private CharacterProgression progression;
        [SerializeField, Min(0)] private int unspentAttributePoints;
        [SerializeField] private List<CharacterStatValue> statistics = new();

        private CharacterProgression boundProgression;
        private readonly Dictionary<CharacterStatDefinition, int>
            pendingAttributeIncreases = new();
        private int pendingAttributePointCount;

        public event Action<int> AttributePointsChanged;
        public event Action<CharacterStatDefinition, int> StatChanged;
        public event Action PendingAttributeChangesChanged;

        public IReadOnlyList<CharacterStatValue> Statistics => statistics;
        public int UnspentAttributePoints => Mathf.Max(
            0,
            unspentAttributePoints - pendingAttributePointCount);
        public int PendingAttributePointCount => pendingAttributePointCount;
        public bool HasPendingAttributeChanges =>
            pendingAttributePointCount > 0;

        public void Configure(
            CharacterProgression characterProgression,
            IReadOnlyList<CharacterStatDefinition> definitions,
            int initialValue,
            int availablePoints = 0)
        {
            progression = characterProgression;
            unspentAttributePoints = Mathf.Max(0, availablePoints);
            pendingAttributeIncreases.Clear();
            pendingAttributePointCount = 0;
            statistics.Clear();
            if (definitions != null)
            {
                foreach (CharacterStatDefinition definition in definitions)
                {
                    if (definition != null)
                    {
                        statistics.Add(new CharacterStatValue(
                            definition,
                            initialValue));
                    }
                }
            }

            if (isActiveAndEnabled)
            {
                BindProgression();
            }
        }

        public int GetValue(CharacterStatDefinition definition)
        {
            CharacterStatValue value = FindValue(definition);
            return value == null
                ? 0
                : value.BaseValue + GetPendingIncrease(definition);
        }

        public int GetPendingIncrease(CharacterStatDefinition definition)
        {
            return
                definition != null &&
                pendingAttributeIncreases.TryGetValue(
                    definition,
                    out int increase)
                    ? increase
                    : 0;
        }

        public bool TrySpendAttributePoints(
            CharacterStatDefinition definition,
            int amount)
        {
            CharacterStatValue value = FindValue(definition);
            if (
                value == null ||
                amount <= 0 ||
                UnspentAttributePoints < amount)
            {
                return false;
            }

            pendingAttributeIncreases.TryGetValue(
                definition,
                out int currentIncrease);
            pendingAttributeIncreases[definition] =
                currentIncrease + amount;
            pendingAttributePointCount += amount;
            StatChanged?.Invoke(definition, GetValue(definition));
            AttributePointsChanged?.Invoke(UnspentAttributePoints);
            PendingAttributeChangesChanged?.Invoke();
            return true;
        }

        public bool CommitPendingAttributePoints()
        {
            if (!HasPendingAttributeChanges)
            {
                return false;
            }

            foreach (KeyValuePair<CharacterStatDefinition, int> pending in
                pendingAttributeIncreases)
            {
                CharacterStatValue value = FindValue(pending.Key);
                if (value == null)
                {
                    continue;
                }

                value.Increase(pending.Value);
            }

            unspentAttributePoints = Mathf.Max(
                0,
                unspentAttributePoints - pendingAttributePointCount);
            pendingAttributeIncreases.Clear();
            pendingAttributePointCount = 0;
            AttributePointsChanged?.Invoke(UnspentAttributePoints);
            PendingAttributeChangesChanged?.Invoke();
            return true;
        }

        public bool CancelPendingAttributePoints()
        {
            if (!HasPendingAttributeChanges)
            {
                return false;
            }

            List<CharacterStatDefinition> changedDefinitions = new(
                pendingAttributeIncreases.Keys);
            pendingAttributeIncreases.Clear();
            pendingAttributePointCount = 0;
            foreach (CharacterStatDefinition definition in changedDefinitions)
            {
                StatChanged?.Invoke(definition, GetValue(definition));
            }

            AttributePointsChanged?.Invoke(UnspentAttributePoints);
            PendingAttributeChangesChanged?.Invoke();
            return true;
        }

        private void Awake()
        {
            if (progression == null)
            {
                progression = GetComponent<CharacterProgression>();
            }
        }

        private void OnEnable()
        {
            BindProgression();
        }

        private void OnDisable()
        {
            UnbindProgression();
        }

        private void BindProgression()
        {
            if (boundProgression == progression)
            {
                return;
            }

            UnbindProgression();
            boundProgression = progression;
            if (boundProgression != null)
            {
                boundProgression.LevelChanged += OnLevelChanged;
            }
        }

        private void UnbindProgression()
        {
            if (boundProgression != null)
            {
                boundProgression.LevelChanged -= OnLevelChanged;
                boundProgression = null;
            }
        }

        private void OnLevelChanged(int newLevel)
        {
            if (progression == null || progression.Settings == null)
            {
                return;
            }

            unspentAttributePoints +=
                progression.Settings.AttributePointsPerLevel;
            AttributePointsChanged?.Invoke(UnspentAttributePoints);
        }

        private CharacterStatValue FindValue(
            CharacterStatDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            foreach (CharacterStatValue value in statistics)
            {
                if (value?.Definition == definition)
                {
                    return value;
                }
            }

            return null;
        }
    }
}
