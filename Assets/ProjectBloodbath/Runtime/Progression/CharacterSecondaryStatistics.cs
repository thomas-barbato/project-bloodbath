using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    public sealed class CharacterSecondaryStatistics : MonoBehaviour
    {
        private readonly Dictionary<string, List<SecondaryStatModifier>>
            modifiersBySource = new(StringComparer.Ordinal);

        [SerializeField] private List<SecondaryStatDefinition> definitions =
            new();

        public event Action ValuesChanged;

        public IReadOnlyList<SecondaryStatDefinition> Definitions =>
            definitions;

        public void Configure(
            IReadOnlyList<SecondaryStatDefinition> statDefinitions)
        {
            definitions.Clear();
            if (statDefinitions == null)
            {
                return;
            }

            foreach (SecondaryStatDefinition definition in statDefinitions)
            {
                if (definition != null && !definitions.Contains(definition))
                {
                    definitions.Add(definition);
                }
            }

            ValuesChanged?.Invoke();
        }

        public float GetValue(SecondaryStatDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }

            float flat = 0f;
            float additivePercent = 0f;
            float multiplicativePercent = 1f;
            foreach (List<SecondaryStatModifier> sourceModifiers in
                modifiersBySource.Values)
            {
                foreach (SecondaryStatModifier modifier in sourceModifiers)
                {
                    if (modifier?.Statistic != definition)
                    {
                        continue;
                    }

                    switch (modifier.Operation)
                    {
                        case SecondaryStatModifierOperation.Flat:
                            flat += modifier.Value;
                            break;
                        case SecondaryStatModifierOperation.AdditivePercent:
                            additivePercent += modifier.Value;
                            break;
                        case SecondaryStatModifierOperation.MultiplicativePercent:
                            multiplicativePercent *= 1f + modifier.Value;
                            break;
                    }
                }
            }

            float value =
                (definition.BaseValue + flat) *
                (1f + additivePercent) *
                multiplicativePercent;
            return definition.Clamp(value);
        }

        public float GetValue(string identifier, float fallbackValue = 0f)
        {
            SecondaryStatDefinition definition = FindDefinition(identifier);
            return definition == null ? fallbackValue : GetValue(definition);
        }

        public bool SetModifiers(
            string sourceIdentifier,
            IReadOnlyList<SecondaryStatModifier> modifiers)
        {
            if (string.IsNullOrWhiteSpace(sourceIdentifier))
            {
                return false;
            }

            string normalizedSource = sourceIdentifier.Trim();
            List<SecondaryStatModifier> validModifiers = new();
            if (modifiers != null)
            {
                foreach (SecondaryStatModifier modifier in modifiers)
                {
                    if (modifier?.Statistic != null)
                    {
                        validModifiers.Add(modifier);
                    }
                }
            }

            if (validModifiers.Count == 0)
            {
                bool removed = modifiersBySource.Remove(normalizedSource);
                if (removed)
                {
                    ValuesChanged?.Invoke();
                }

                return removed;
            }

            modifiersBySource[normalizedSource] = validModifiers;
            ValuesChanged?.Invoke();
            return true;
        }

        public bool RemoveModifiers(string sourceIdentifier)
        {
            if (
                string.IsNullOrWhiteSpace(sourceIdentifier) ||
                !modifiersBySource.Remove(sourceIdentifier.Trim()))
            {
                return false;
            }

            ValuesChanged?.Invoke();
            return true;
        }

        private SecondaryStatDefinition FindDefinition(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            foreach (SecondaryStatDefinition definition in definitions)
            {
                if (
                    definition != null &&
                    string.Equals(
                        definition.Identifier,
                        identifier,
                        StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
