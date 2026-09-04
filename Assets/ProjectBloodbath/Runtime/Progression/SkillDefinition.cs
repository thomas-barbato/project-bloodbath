using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    public enum CharacterClassId
    {
        Enforcer,
        Marine,
        Scientist,
        Engineer
    }

    public enum SkillType
    {
        Active,
        Passive
    }

    public enum SkillTargetMode
    {
        None,
        Self,
        Reticle,
        Target,
        CursorArea,
        Line,
        Cone,
        Aura,
        Movement
    }

    public enum SkillSynergyOperation
    {
        AdditiveValue,
        AdditivePercent
    }

    public enum SkillPrerequisiteMode
    {
        All,
        Any,
        AtLeast
    }

    [Serializable]
    public sealed class SkillRankValue
    {
        [SerializeField] private string identifier = "value";
        [SerializeField] private float valueAtRankOne;
        [SerializeField] private float valueAtNaturalCap;
        [SerializeField] private float valuePerEquipmentRank;
        [SerializeField] private bool wholeNumber;
        [SerializeField] private bool clampMinimum;
        [SerializeField] private float minimumValue;
        [SerializeField] private bool clampMaximum;
        [SerializeField] private float maximumValue;

        public string Identifier => identifier;
        public float ValueAtRankOne => valueAtRankOne;
        public float ValueAtNaturalCap => valueAtNaturalCap;
        public float ValuePerEquipmentRank => valuePerEquipmentRank;
        public bool IsWholeNumber => wholeNumber;

        public SkillRankValue(
            string valueIdentifier,
            float rankOneValue,
            float naturalCapValue,
            float equipmentRankStep = 0f,
            bool useWholeNumbers = false,
            float? minimum = null,
            float? maximum = null)
        {
            identifier = SkillDefinition.NormalizeIdentifier(
                valueIdentifier,
                "value");
            valueAtRankOne = rankOneValue;
            valueAtNaturalCap = naturalCapValue;
            valuePerEquipmentRank = equipmentRankStep;
            wholeNumber = useWholeNumbers;
            clampMinimum = minimum.HasValue;
            minimumValue = minimum.GetValueOrDefault();
            clampMaximum = maximum.HasValue;
            maximumValue = maximum.GetValueOrDefault();
        }

        public float Evaluate(int effectiveRank, int naturalRankCap)
        {
            if (effectiveRank <= 0)
            {
                return 0f;
            }

            int normalizedCap = Mathf.Max(1, naturalRankCap);
            int naturalRank = Mathf.Min(effectiveRank, normalizedCap);
            float interpolation = normalizedCap <= 1
                ? 1f
                : (float)(naturalRank - 1) / (normalizedCap - 1);
            float value = Mathf.LerpUnclamped(
                valueAtRankOne,
                valueAtNaturalCap,
                interpolation);
            value += Mathf.Max(0, effectiveRank - normalizedCap) *
                valuePerEquipmentRank;

            if (clampMinimum)
            {
                value = Mathf.Max(minimumValue, value);
            }

            if (clampMaximum)
            {
                value = Mathf.Min(maximumValue, value);
            }

            return wholeNumber ? Mathf.Round(value) : value;
        }
    }

    [Serializable]
    public sealed class SkillPrerequisite
    {
        [SerializeField] private SkillDefinition skill;
        [SerializeField, Min(1)] private int requiredInvestedRank = 1;

        public SkillDefinition Skill => skill;
        public int RequiredInvestedRank => requiredInvestedRank;

        public SkillPrerequisite(
            SkillDefinition requiredSkill,
            int investedRank = 1)
        {
            skill = requiredSkill;
            requiredInvestedRank = Mathf.Max(1, investedRank);
        }
    }

    [Serializable]
    public sealed class SkillPrerequisiteGroup
    {
        [SerializeField] private SkillPrerequisiteMode mode;
        [SerializeField, Min(1)] private int requiredCount = 1;
        [SerializeField] private List<SkillPrerequisite> prerequisites = new();

        public SkillPrerequisiteMode Mode => mode;
        public int RequiredCount => GetRequiredCount();
        public IReadOnlyList<SkillPrerequisite> Prerequisites => prerequisites;

        public SkillPrerequisiteGroup(
            SkillPrerequisiteMode prerequisiteMode,
            IReadOnlyList<SkillPrerequisite> requiredSkills,
            int minimumRequiredCount = 1)
        {
            mode = prerequisiteMode;
            requiredCount = Mathf.Max(1, minimumRequiredCount);
            prerequisites = new List<SkillPrerequisite>();
            if (requiredSkills == null)
            {
                return;
            }

            foreach (SkillPrerequisite prerequisite in requiredSkills)
            {
                if (prerequisite != null)
                {
                    prerequisites.Add(prerequisite);
                }
            }
        }

        public bool IsSatisfiedBy(Func<SkillDefinition, int> rankResolver)
        {
            if (prerequisites == null || prerequisites.Count == 0)
            {
                return true;
            }

            if (rankResolver == null)
            {
                return false;
            }

            int matches = 0;
            foreach (SkillPrerequisite prerequisite in prerequisites)
            {
                if (
                    prerequisite?.Skill != null &&
                    rankResolver(prerequisite.Skill) >=
                    prerequisite.RequiredInvestedRank)
                {
                    matches++;
                }
            }

            return matches >= GetRequiredCount();
        }

        private int GetRequiredCount()
        {
            int count = prerequisites?.Count ?? 0;
            if (count == 0)
            {
                return 0;
            }

            return mode switch
            {
                SkillPrerequisiteMode.All => count,
                SkillPrerequisiteMode.Any => 1,
                _ => Mathf.Clamp(requiredCount, 1, count)
            };
        }
    }

    [Serializable]
    public sealed class SkillInvestedRankSynergy
    {
        [SerializeField] private SkillDefinition sourceSkill;
        [SerializeField] private string affectedValueIdentifier = "damage";
        [SerializeField] private SkillSynergyOperation operation;
        [SerializeField] private float bonusPerInvestedRank;

        public SkillDefinition SourceSkill => sourceSkill;
        public string AffectedValueIdentifier => affectedValueIdentifier;
        public SkillSynergyOperation Operation => operation;
        public float BonusPerInvestedRank => bonusPerInvestedRank;

        public SkillInvestedRankSynergy(
            SkillDefinition source,
            string affectedValue,
            float bonusPerRank,
            SkillSynergyOperation synergyOperation =
                SkillSynergyOperation.AdditivePercent)
        {
            sourceSkill = source;
            affectedValueIdentifier = SkillDefinition.NormalizeIdentifier(
                affectedValue,
                "damage");
            bonusPerInvestedRank = bonusPerRank;
            operation = synergyOperation;
        }
    }

    [CreateAssetMenu(
        fileName = "SkillDefinition",
        menuName = "Project Bloodbath/Progression/Skill")]
    public sealed class SkillDefinition : ScriptableObject
    {
        public const int DefaultMaximumInvestedRank = 20;

        [Header("Identity")]
        [SerializeField] private string identifier = "skill";
        [SerializeField] private CharacterClassId characterClass;
        [SerializeField] private string treeIdentifier = "skill_tree";
        [SerializeField] private string displayName = "Compétence";
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private Sprite icon;

        [Header("Progression")]
        [SerializeField] private SkillType skillType;
        [SerializeField, Min(1)] private int maximumInvestedRank =
            DefaultMaximumInvestedRank;
        [SerializeField, Min(1)] private int unlockLevel = 1;
        [SerializeField] private List<SkillPrerequisite> prerequisites = new();
        [SerializeField] private List<SkillPrerequisiteGroup>
            prerequisiteGroups = new();

        [Header("Activation")]
        [SerializeField] private SkillTargetMode targetMode;
        [SerializeField, Min(0f)] private float resourceCost;
        [SerializeField] private List<string> tags = new();

        [Header("Effects")]
        [SerializeField] private List<SkillRankValue> rankValues = new();
        [SerializeField] private List<string> appliedStatusIdentifiers = new();
        [SerializeField] private List<string> consumedStatusIdentifiers = new();
        [SerializeField] private List<SkillInvestedRankSynergy>
            investedRankSynergies = new();

        public string Identifier => identifier;
        public CharacterClassId CharacterClass => characterClass;
        public string TreeIdentifier => treeIdentifier;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public SkillType SkillType => skillType;
        public int MaximumInvestedRank => maximumInvestedRank;
        public int UnlockLevel => unlockLevel;
        public IReadOnlyList<SkillPrerequisite> Prerequisites => prerequisites;
        public IReadOnlyList<SkillPrerequisiteGroup> PrerequisiteGroups =>
            prerequisiteGroups;
        public SkillTargetMode TargetMode => targetMode;
        public float ResourceCost => resourceCost;
        public IReadOnlyList<string> Tags => tags;
        public IReadOnlyList<SkillRankValue> RankValues => rankValues;
        public IReadOnlyList<string> AppliedStatusIdentifiers =>
            appliedStatusIdentifiers;
        public IReadOnlyList<string> ConsumedStatusIdentifiers =>
            consumedStatusIdentifiers;
        public IReadOnlyList<SkillInvestedRankSynergy>
            InvestedRankSynergies => investedRankSynergies;

        public void Configure(
            string skillIdentifier,
            CharacterClassId ownerClass,
            string ownerTreeIdentifier,
            string skillDisplayName,
            string skillDescription,
            SkillType type,
            int requiredLevel,
            SkillTargetMode targeting,
            float energyCost = 0f,
            IReadOnlyList<SkillPrerequisite> requiredSkills = null,
            IReadOnlyList<string> skillTags = null,
            IReadOnlyList<SkillRankValue> values = null,
            IReadOnlyList<string> appliedStatuses = null,
            IReadOnlyList<string> consumedStatuses = null,
            IReadOnlyList<SkillInvestedRankSynergy> synergies = null,
            int naturalRankCap = DefaultMaximumInvestedRank,
            IReadOnlyList<SkillPrerequisiteGroup> prerequisiteSets = null)
        {
            identifier = NormalizeIdentifier(skillIdentifier, "skill");
            characterClass = ownerClass;
            treeIdentifier = NormalizeIdentifier(
                ownerTreeIdentifier,
                "skill_tree");
            displayName = string.IsNullOrWhiteSpace(skillDisplayName)
                ? "Compétence"
                : skillDisplayName.Trim();
            description = skillDescription?.Trim() ?? string.Empty;
            skillType = type;
            maximumInvestedRank = Mathf.Max(1, naturalRankCap);
            unlockLevel = Mathf.Max(1, requiredLevel);
            targetMode = type == SkillType.Passive
                ? SkillTargetMode.None
                : targeting;
            resourceCost = type == SkillType.Passive
                ? 0f
                : Mathf.Max(0f, energyCost);

            ReplaceList(prerequisites, requiredSkills);
            ReplaceList(prerequisiteGroups, prerequisiteSets);
            ReplaceIdentifiers(tags, skillTags);
            ReplaceList(rankValues, values);
            ReplaceIdentifiers(appliedStatusIdentifiers, appliedStatuses);
            ReplaceIdentifiers(consumedStatusIdentifiers, consumedStatuses);
            ReplaceList(investedRankSynergies, synergies);
        }

        public bool HasTag(string tagIdentifier)
        {
            return ContainsIdentifier(tags, tagIdentifier);
        }

        public bool AppliesStatus(string statusIdentifier)
        {
            return ContainsIdentifier(
                appliedStatusIdentifiers,
                statusIdentifier);
        }

        public bool ConsumesStatus(string statusIdentifier)
        {
            return ContainsIdentifier(
                consumedStatusIdentifiers,
                statusIdentifier);
        }

        public float GetRankValue(string valueIdentifier, int effectiveRank)
        {
            string normalized = NormalizeIdentifier(
                valueIdentifier,
                string.Empty);
            foreach (SkillRankValue value in rankValues)
            {
                if (
                    value != null &&
                    string.Equals(
                        value.Identifier,
                        normalized,
                        StringComparison.Ordinal))
                {
                    return value.Evaluate(
                        effectiveRank,
                        maximumInvestedRank);
                }
            }

            return 0f;
        }

        private void OnValidate()
        {
            identifier = NormalizeIdentifier(identifier, "skill");
            treeIdentifier = NormalizeIdentifier(
                treeIdentifier,
                "skill_tree");
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Compétence"
                : displayName.Trim();
            description = description?.Trim() ?? string.Empty;
            maximumInvestedRank = Mathf.Max(1, maximumInvestedRank);
            unlockLevel = Mathf.Max(1, unlockLevel);
            resourceCost = skillType == SkillType.Passive
                ? 0f
                : Mathf.Max(0f, resourceCost);
            if (skillType == SkillType.Passive)
            {
                targetMode = SkillTargetMode.None;
            }

            NormalizeIdentifiers(tags);
            NormalizeIdentifiers(appliedStatusIdentifiers);
            NormalizeIdentifiers(consumedStatusIdentifiers);
        }

        internal static string NormalizeIdentifier(
            string value,
            string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return value.Trim().ToLowerInvariant().Replace(' ', '_');
        }

        private static bool ContainsIdentifier(
            IReadOnlyList<string> identifiers,
            string candidate)
        {
            string normalized = NormalizeIdentifier(candidate, string.Empty);
            foreach (string identifierValue in identifiers)
            {
                if (string.Equals(
                    identifierValue,
                    normalized,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ReplaceList<T>(
            List<T> destination,
            IReadOnlyList<T> source)
        {
            destination ??= new List<T>();
            IReadOnlyList<T> sourceSnapshot =
                ReferenceEquals(destination, source)
                    ? new List<T>(destination)
                    : source;
            destination.Clear();
            if (sourceSnapshot == null)
            {
                return;
            }

            foreach (T item in sourceSnapshot)
            {
                if (item != null)
                {
                    destination.Add(item);
                }
            }
        }

        private static void ReplaceIdentifiers(
            List<string> destination,
            IReadOnlyList<string> source)
        {
            destination ??= new List<string>();
            IReadOnlyList<string> sourceSnapshot =
                ReferenceEquals(destination, source)
                    ? new List<string>(destination)
                    : source;
            destination.Clear();
            if (sourceSnapshot == null)
            {
                return;
            }

            foreach (string value in sourceSnapshot)
            {
                string normalized = NormalizeIdentifier(value, string.Empty);
                if (
                    !string.IsNullOrEmpty(normalized) &&
                    !destination.Contains(normalized))
                {
                    destination.Add(normalized);
                }
            }
        }

        private static void NormalizeIdentifiers(List<string> identifiers)
        {
            if (identifiers == null)
            {
                return;
            }

            for (int index = identifiers.Count - 1; index >= 0; index--)
            {
                string normalized = NormalizeIdentifier(
                    identifiers[index],
                    string.Empty);
                if (string.IsNullOrEmpty(normalized))
                {
                    identifiers.RemoveAt(index);
                }
                else
                {
                    identifiers[index] = normalized;
                }
            }
        }
    }
}
