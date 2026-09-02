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

        public event Action<int> AttributePointsChanged;
        public event Action<CharacterStatDefinition, int> StatChanged;

        public IReadOnlyList<CharacterStatValue> Statistics => statistics;
        public int UnspentAttributePoints => unspentAttributePoints;

        public void Configure(
            CharacterProgression characterProgression,
            IReadOnlyList<CharacterStatDefinition> definitions,
            int initialValue,
            int availablePoints = 0)
        {
            progression = characterProgression;
            unspentAttributePoints = Mathf.Max(0, availablePoints);
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
            return value?.BaseValue ?? 0;
        }

        public bool TrySpendAttributePoints(
            CharacterStatDefinition definition,
            int amount)
        {
            CharacterStatValue value = FindValue(definition);
            if (
                value == null ||
                amount <= 0 ||
                unspentAttributePoints < amount)
            {
                return false;
            }

            unspentAttributePoints -= amount;
            value.Increase(amount);
            StatChanged?.Invoke(definition, value.BaseValue);
            AttributePointsChanged?.Invoke(unspentAttributePoints);
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
            AttributePointsChanged?.Invoke(unspentAttributePoints);
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
