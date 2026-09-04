using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "SkillTreeDefinition",
        menuName = "Project Bloodbath/Progression/Skill Tree")]
    public sealed class SkillTreeDefinition : ScriptableObject
    {
        public const int ExpectedSkillCount = 10;
        public const int ExpectedSkillCountPerType = 5;

        [SerializeField] private string identifier = "skill_tree";
        [SerializeField] private CharacterClassId characterClass;
        [SerializeField] private string displayName = "Arbre de compétences";
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private List<SkillDefinition> skills = new();

        public string Identifier => identifier;
        public CharacterClassId CharacterClass => characterClass;
        public string DisplayName => displayName;
        public string Description => description;
        public IReadOnlyList<SkillDefinition> Skills => skills;

        public void Configure(
            string treeIdentifier,
            CharacterClassId ownerClass,
            string treeDisplayName,
            string treeDescription,
            IReadOnlyList<SkillDefinition> definitions)
        {
            identifier = SkillDefinition.NormalizeIdentifier(
                treeIdentifier,
                "skill_tree");
            characterClass = ownerClass;
            displayName = string.IsNullOrWhiteSpace(treeDisplayName)
                ? "Arbre de compétences"
                : treeDisplayName.Trim();
            description = treeDescription?.Trim() ?? string.Empty;
            skills ??= new List<SkillDefinition>();
            skills.Clear();
            if (definitions == null)
            {
                return;
            }

            foreach (SkillDefinition definition in definitions)
            {
                if (definition != null && !skills.Contains(definition))
                {
                    skills.Add(definition);
                }
            }
        }

        public bool Contains(SkillDefinition definition)
        {
            return definition != null && skills.Contains(definition);
        }

        public SkillDefinition FindSkill(string skillIdentifier)
        {
            string normalized = SkillDefinition.NormalizeIdentifier(
                skillIdentifier,
                string.Empty);
            foreach (SkillDefinition skill in skills)
            {
                if (
                    skill != null &&
                    string.Equals(
                        skill.Identifier,
                        normalized,
                        StringComparison.Ordinal))
                {
                    return skill;
                }
            }

            return null;
        }

        public bool TryValidateStructure(out string issue)
        {
            if (skills.Count != ExpectedSkillCount)
            {
                issue = $"{displayName} contient {skills.Count} compétences " +
                    $"au lieu de {ExpectedSkillCount}.";
                return false;
            }

            int activeCount = 0;
            int passiveCount = 0;
            HashSet<string> identifiers = new(StringComparer.Ordinal);
            foreach (SkillDefinition skill in skills)
            {
                if (skill == null)
                {
                    issue = $"{displayName} contient une compétence vide.";
                    return false;
                }

                if (
                    skill.CharacterClass != characterClass ||
                    !string.Equals(
                        skill.TreeIdentifier,
                        identifier,
                        StringComparison.Ordinal))
                {
                    issue = $"{skill.DisplayName} n'appartient pas à " +
                        $"{displayName}.";
                    return false;
                }

                if (!identifiers.Add(skill.Identifier))
                {
                    issue = $"L'identifiant {skill.Identifier} est dupliqué.";
                    return false;
                }

                if (skill.SkillType == SkillType.Active)
                {
                    activeCount++;
                }
                else
                {
                    passiveCount++;
                }
            }

            if (
                activeCount != ExpectedSkillCountPerType ||
                passiveCount != ExpectedSkillCountPerType)
            {
                issue = $"{displayName} doit contenir cinq compétences " +
                    "actives et cinq passives.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private void OnValidate()
        {
            identifier = SkillDefinition.NormalizeIdentifier(
                identifier,
                "skill_tree");
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Arbre de compétences"
                : displayName.Trim();
            description = description?.Trim() ?? string.Empty;
        }
    }
}
