using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectBloodbath.Progression;
using UnityEditor;

namespace ProjectBloodbath.Tests.Editor
{
    public sealed class SkillTreeAssetEditorTests
    {
        private const string Root =
            "Assets/ProjectBloodbath/Content/Progression/Skills";

        [Test]
        public void CatalogContainsTwelveCompleteClassTrees()
        {
            SkillTreeDefinition[] trees = LoadAll<SkillTreeDefinition>();

            Assert.That(trees, Has.Length.EqualTo(12));
            Assert.That(
                trees.GroupBy(tree => tree.CharacterClass)
                    .ToDictionary(group => group.Key, group => group.Count()),
                Is.EquivalentTo(new Dictionary<CharacterClassId, int>
                {
                    { CharacterClassId.Enforcer, 3 },
                    { CharacterClassId.Marine, 3 },
                    { CharacterClassId.Scientist, 3 },
                    { CharacterClassId.Engineer, 3 }
                }));

            HashSet<string> treeIds = new(StringComparer.Ordinal);
            HashSet<string> skillIds = new(StringComparer.Ordinal);
            foreach (SkillTreeDefinition tree in trees)
            {
                Assert.That(treeIds.Add(tree.Identifier), Is.True,
                    $"Identifiant d'arbre dupliqué : {tree.Identifier}");
                Assert.That(tree.TryValidateStructure(out string issue), Is.True,
                    issue);
                Assert.That(
                    tree.Skills.Count(skill => skill.SkillType == SkillType.Active),
                    Is.EqualTo(5),
                    tree.DisplayName);
                Assert.That(
                    tree.Skills.Count(skill => skill.SkillType == SkillType.Passive),
                    Is.EqualTo(5),
                    tree.DisplayName);
                foreach (SkillDefinition skill in tree.Skills)
                {
                    Assert.That(skillIds.Add(skill.Identifier), Is.True,
                        $"Identifiant de compétence dupliqué : {skill.Identifier}");
                }
            }

            Assert.That(skillIds, Has.Count.EqualTo(120));
        }

        [Test]
        public void RecursiveEffectsAndSecondaryResourcesAreBounded()
        {
            Assert.That(
                FindSkill("enforcer_slaughter_anchor").AppliesStatus("anchored"),
                Is.True);
            AssertRankValue("enforcer_retaliation_reactor", "maximum_guard_charges", 10f);
            AssertRankValue("enforcer_massacre_authority", "maximum_defensive_triggers_per_order", 1f);
            AssertRankValue("enforcer_massacre_authority", "maximum_assault_triggers_per_order", 1f);

            AssertRankValue("scientist_pyrophagous_residue", "maximum_catalyst_charges", 1f);
            AssertRankValue("scientist_pyrolytic_propagation", "maximum_secondary_chain_depth", 0f);
            AssertRankValue("scientist_phase_pressure", "maximum_pressure_charges", 1f);
            AssertRankValue("scientist_crystalline_fragility", "maximum_secondary_chain_depth", 0f);
            AssertRankValue("scientist_return_current", "maximum_return_charges", 1f);
            AssertRankValue("scientist_mobile_storm", "maximum_recentres", 1f);

            AssertRankValue("engineer_k9_scavenger", "maximum_active_main_chassis", 1f);
            AssertRankValue("engineer_field_replication", "replicated_unit_scrap_output", 0f);
            AssertRankValue("engineer_field_replication", "maximum_replications_per_second", 1f);
            AssertRankValue("engineer_kamikaze_protocol", "maximum_consumed_offensive_drones", 8f);
            AssertRankValue("engineer_viral_propagation", "maximum_secondary_chain_depth", 0f);
            AssertRankValue("engineer_zero_directive", "maximum_final_damage_percent", 600f);
        }

        [Test]
        public void KamikazeRequiresTwoDistinctOffensiveDroneSkills()
        {
            SkillDefinition skill = FindSkill("engineer_kamikaze_protocol");

            Assert.That(skill.PrerequisiteGroups, Has.Count.EqualTo(1));
            Assert.That(
                skill.PrerequisiteGroups[0].Mode,
                Is.EqualTo(SkillPrerequisiteMode.AtLeast));
            Assert.That(skill.PrerequisiteGroups[0].RequiredCount, Is.EqualTo(2));
            Assert.That(skill.PrerequisiteGroups[0].Prerequisites, Has.Count.EqualTo(3));
        }

        private static void AssertRankValue(
            string skillIdentifier,
            string valueIdentifier,
            float expectedAtRankTwenty)
        {
            SkillDefinition skill = FindSkill(skillIdentifier);
            Assert.That(
                skill.GetRankValue(valueIdentifier, 20),
                Is.EqualTo(expectedAtRankTwenty).Within(0.001f),
                $"{skillIdentifier}.{valueIdentifier}");
        }

        private static SkillDefinition FindSkill(string identifier)
        {
            SkillDefinition skill = LoadAll<SkillDefinition>()
                .SingleOrDefault(candidate =>
                    string.Equals(
                        candidate.Identifier,
                        identifier,
                        StringComparison.Ordinal));
            Assert.That(skill, Is.Not.Null, $"Compétence absente : {identifier}");
            return skill;
        }

        private static T[] LoadAll<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { Root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null)
                .ToArray();
        }
    }
}
