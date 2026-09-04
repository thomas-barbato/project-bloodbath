using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    public enum SkillInvestmentBlocker
    {
        None,
        MissingDefinition,
        UnavailableToCharacter,
        LevelLocked,
        MissingPrerequisite,
        MaximumRankReached,
        NoSkillPoints
    }

    [Serializable]
    public sealed class CharacterSkillRank
    {
        [SerializeField] private SkillDefinition definition;
        [SerializeField, Min(0)] private int investedRank;
        [SerializeField, Min(0)] private int equipmentBonusRank;

        public SkillDefinition Definition => definition;
        public int InvestedRank => investedRank;
        public int EquipmentBonusRank => equipmentBonusRank;
        public int EffectiveRank => investedRank <= 0
            ? 0
            : investedRank + equipmentBonusRank;

        public CharacterSkillRank(SkillDefinition skill)
        {
            definition = skill;
        }

        internal void IncreaseInvestedRank()
        {
            if (definition == null)
            {
                return;
            }

            investedRank = Mathf.Min(
                definition.MaximumInvestedRank,
                investedRank + 1);
        }

        internal void SetEquipmentBonusRank(int rank)
        {
            equipmentBonusRank = Mathf.Max(0, rank);
        }

        internal void Normalize()
        {
            investedRank = definition == null
                ? 0
                : Mathf.Clamp(
                    investedRank,
                    0,
                    definition.MaximumInvestedRank);
            equipmentBonusRank = Mathf.Max(0, equipmentBonusRank);
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterProgression))]
    public sealed class CharacterSkillProgression : MonoBehaviour
    {
        [SerializeField] private CharacterProgression progression;
        [SerializeField] private List<SkillTreeDefinition> availableTrees =
            new();
        [SerializeField, Min(0)] private int unspentSkillPoints;
        [SerializeField] private List<CharacterSkillRank> skillRanks = new();

        private CharacterProgression boundProgression;

        public event Action<int> SkillPointsChanged;
        public event Action<SkillDefinition, int, int> SkillRankChanged;

        public CharacterProgression Progression => progression;
        public IReadOnlyList<SkillTreeDefinition> AvailableTrees =>
            availableTrees;
        public IReadOnlyList<CharacterSkillRank> SkillRanks => skillRanks;
        public int UnspentSkillPoints => unspentSkillPoints;

        public void Configure(
            CharacterProgression characterProgression,
            IReadOnlyList<SkillTreeDefinition> trees,
            int availablePoints = 0)
        {
            progression = characterProgression;
            unspentSkillPoints = Mathf.Max(0, availablePoints);
            availableTrees.Clear();
            if (trees != null)
            {
                foreach (SkillTreeDefinition tree in trees)
                {
                    if (tree != null && !availableTrees.Contains(tree))
                    {
                        availableTrees.Add(tree);
                    }
                }
            }

            skillRanks.Clear();
            if (isActiveAndEnabled)
            {
                BindProgression();
            }

            SkillPointsChanged?.Invoke(unspentSkillPoints);
        }

        public SkillInvestmentBlocker GetInvestmentBlocker(
            SkillDefinition skill)
        {
            if (skill == null)
            {
                return SkillInvestmentBlocker.MissingDefinition;
            }

            if (!IsAvailable(skill))
            {
                return SkillInvestmentBlocker.UnavailableToCharacter;
            }

            if (
                progression == null ||
                progression.CurrentLevel < skill.UnlockLevel)
            {
                return SkillInvestmentBlocker.LevelLocked;
            }

            foreach (SkillPrerequisite prerequisite in skill.Prerequisites)
            {
                if (
                    prerequisite?.Skill == null ||
                    GetInvestedRank(prerequisite.Skill) <
                    prerequisite.RequiredInvestedRank)
                {
                    return SkillInvestmentBlocker.MissingPrerequisite;
                }
            }

            foreach (SkillPrerequisiteGroup group in skill.PrerequisiteGroups)
            {
                if (group != null && !group.IsSatisfiedBy(GetInvestedRank))
                {
                    return SkillInvestmentBlocker.MissingPrerequisite;
                }
            }

            if (GetInvestedRank(skill) >= skill.MaximumInvestedRank)
            {
                return SkillInvestmentBlocker.MaximumRankReached;
            }

            return unspentSkillPoints <= 0
                ? SkillInvestmentBlocker.NoSkillPoints
                : SkillInvestmentBlocker.None;
        }

        public bool TryInvestPoint(SkillDefinition skill)
        {
            if (GetInvestmentBlocker(skill) != SkillInvestmentBlocker.None)
            {
                return false;
            }

            CharacterSkillRank rank = GetOrCreateRank(skill);
            rank.IncreaseInvestedRank();
            unspentSkillPoints--;
            SkillRankChanged?.Invoke(
                skill,
                rank.InvestedRank,
                rank.EffectiveRank);
            SkillPointsChanged?.Invoke(unspentSkillPoints);
            return true;
        }

        public bool SetEquipmentBonusRank(
            SkillDefinition skill,
            int equipmentBonusRank)
        {
            if (skill == null || !IsAvailable(skill))
            {
                return false;
            }

            CharacterSkillRank rank = GetOrCreateRank(skill);
            int normalizedBonus = Mathf.Max(0, equipmentBonusRank);
            if (rank.EquipmentBonusRank == normalizedBonus)
            {
                return false;
            }

            rank.SetEquipmentBonusRank(normalizedBonus);
            SkillRankChanged?.Invoke(
                skill,
                rank.InvestedRank,
                rank.EffectiveRank);
            return true;
        }

        public int GetInvestedRank(SkillDefinition skill)
        {
            return FindRank(skill)?.InvestedRank ?? 0;
        }

        public int GetEffectiveRank(SkillDefinition skill)
        {
            return FindRank(skill)?.EffectiveRank ?? 0;
        }

        public float GetInvestedSynergyBonus(
            SkillDefinition skill,
            string affectedValueIdentifier,
            SkillSynergyOperation operation)
        {
            if (skill == null)
            {
                return 0f;
            }

            string normalizedValue = SkillDefinition.NormalizeIdentifier(
                affectedValueIdentifier,
                string.Empty);
            float total = 0f;
            foreach (SkillInvestedRankSynergy synergy in
                skill.InvestedRankSynergies)
            {
                if (
                    synergy?.SourceSkill == null ||
                    synergy.Operation != operation ||
                    !string.Equals(
                        synergy.AffectedValueIdentifier,
                        normalizedValue,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                total += GetInvestedRank(synergy.SourceSkill) *
                    synergy.BonusPerInvestedRank;
            }

            return total;
        }

        public bool IsAvailable(SkillDefinition skill)
        {
            if (skill == null)
            {
                return false;
            }

            foreach (SkillTreeDefinition tree in availableTrees)
            {
                if (tree != null && tree.Contains(skill))
                {
                    return true;
                }
            }

            return false;
        }

        private void Awake()
        {
            if (progression == null)
            {
                progression = GetComponent<CharacterProgression>();
            }

            NormalizeState();
        }

        private void OnEnable()
        {
            BindProgression();
        }

        private void OnDisable()
        {
            UnbindProgression();
        }

        private void OnValidate()
        {
            NormalizeState();
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
            if (boundProgression == null)
            {
                return;
            }

            boundProgression.LevelChanged -= OnLevelChanged;
            boundProgression = null;
        }

        private void OnLevelChanged(int newLevel)
        {
            if (progression?.Settings == null)
            {
                return;
            }

            int grantedPoints = progression.Settings.SkillPointsPerLevel;
            if (grantedPoints <= 0)
            {
                return;
            }

            unspentSkillPoints += grantedPoints;
            SkillPointsChanged?.Invoke(unspentSkillPoints);
        }

        private CharacterSkillRank GetOrCreateRank(SkillDefinition skill)
        {
            CharacterSkillRank rank = FindRank(skill);
            if (rank != null)
            {
                return rank;
            }

            rank = new CharacterSkillRank(skill);
            skillRanks.Add(rank);
            return rank;
        }

        private CharacterSkillRank FindRank(SkillDefinition skill)
        {
            if (skill == null)
            {
                return null;
            }

            foreach (CharacterSkillRank rank in skillRanks)
            {
                if (rank?.Definition == skill)
                {
                    return rank;
                }
            }

            return null;
        }

        private void NormalizeState()
        {
            unspentSkillPoints = Mathf.Max(0, unspentSkillPoints);
            availableTrees ??= new List<SkillTreeDefinition>();
            skillRanks ??= new List<CharacterSkillRank>();
            for (int index = skillRanks.Count - 1; index >= 0; index--)
            {
                CharacterSkillRank rank = skillRanks[index];
                if (rank?.Definition == null)
                {
                    skillRanks.RemoveAt(index);
                }
                else
                {
                    rank.Normalize();
                }
            }
        }
    }
}
