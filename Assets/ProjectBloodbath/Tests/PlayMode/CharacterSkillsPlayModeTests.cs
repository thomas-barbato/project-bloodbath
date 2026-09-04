using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectBloodbath.Progression;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class CharacterSkillsPlayModeTests
    {
        private readonly List<ScriptableObject> temporaryAssets = new();

        private GameObject character;
        private CharacterProgression progression;
        private CharacterSkillProgression skills;
        private ActiveSkillBar skillBar;
        private CharacterProgressionSettings progressionSettings;
        private SkillTreeDefinition tree;
        private SkillDefinition terminalBurst;
        private SkillDefinition predatoryCadence;
        private SkillDefinition combatReload;
        private SkillDefinition ballisticMobility;
        private SkillDefinition breechSweep;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            progressionSettings = CreateAsset<CharacterProgressionSettings>();
            progressionSettings.Configure(99, 100, 1f, 5, 1);

            terminalBurst = CreateSkill(
                "marine_terminal_burst",
                "Rafale terminale",
                SkillType.Active,
                1,
                SkillTargetMode.Reticle,
                values: new[]
                {
                    new SkillRankValue(
                        "cooldown_seconds",
                        8f,
                        5f,
                        -0.1f,
                        minimum: 1f)
                });
            predatoryCadence = CreateSkill(
                "marine_predatory_cadence",
                "Cadence prédatrice",
                SkillType.Passive,
                1,
                SkillTargetMode.None);
            terminalBurst.Configure(
                terminalBurst.Identifier,
                CharacterClassId.Marine,
                "marine_doctrine_saturation",
                terminalBurst.DisplayName,
                string.Empty,
                SkillType.Active,
                1,
                SkillTargetMode.Reticle,
                10f,
                values: terminalBurst.RankValues,
                synergies: new[]
                {
                    new SkillInvestedRankSynergy(
                        predatoryCadence,
                        "weapon_damage_percent",
                        1.5f)
                });
            combatReload = CreateSkill(
                "marine_combat_reload",
                "Rechargement de combat",
                SkillType.Active,
                6,
                SkillTargetMode.Movement);
            ballisticMobility = CreateSkill(
                "marine_ballistic_mobility",
                "Mobilité balistique",
                SkillType.Passive,
                6,
                SkillTargetMode.None,
                prerequisiteGroups: new[]
                {
                    new SkillPrerequisiteGroup(
                        SkillPrerequisiteMode.Any,
                        new[]
                        {
                            new SkillPrerequisite(terminalBurst),
                            new SkillPrerequisite(combatReload)
                        })
                });
            breechSweep = CreateSkill(
                "marine_breech_sweep",
                "Balayage de culasse",
                SkillType.Active,
                12,
                SkillTargetMode.Cone,
                prerequisiteGroups: new[]
                {
                    new SkillPrerequisiteGroup(
                        SkillPrerequisiteMode.Any,
                        new[]
                        {
                            new SkillPrerequisite(predatoryCadence),
                            new SkillPrerequisite(ballisticMobility)
                        })
                },
                synergies: new[]
                {
                    new SkillInvestedRankSynergy(
                        combatReload,
                        "weapon_damage_percent",
                        2f)
                });

            tree = CreateAsset<SkillTreeDefinition>();
            tree.Configure(
                "marine_doctrine_saturation",
                CharacterClassId.Marine,
                "Doctrine de saturation",
                string.Empty,
                new[]
                {
                    terminalBurst,
                    predatoryCadence,
                    combatReload,
                    ballisticMobility,
                    breechSweep
                });

            character = new GameObject("SkillTestCharacter");
            progression = character.AddComponent<CharacterProgression>();
            skills = character.AddComponent<CharacterSkillProgression>();
            skillBar = character.AddComponent<ActiveSkillBar>();
            progression.Configure(progressionSettings, 1, 0);
            skills.Configure(progression, new[] { tree }, 0);
            skillBar.Configure(skills);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(character);
            foreach (ScriptableObject asset in temporaryAssets)
            {
                Object.Destroy(asset);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator LevelUpsGrantOneSkillPointEach()
        {
            int gainedLevels = progression.AddExperience(200);

            Assert.That(gainedLevels, Is.EqualTo(2));
            Assert.That(progression.CurrentLevel, Is.EqualTo(3));
            Assert.That(skills.UnspentSkillPoints, Is.EqualTo(2));
            yield break;
        }

        [UnityTest]
        public IEnumerator AnyPrerequisiteUsesInvestedRankInsteadOfEquipmentRank()
        {
            progression.Configure(progressionSettings, 6, 0);
            skills.Configure(progression, new[] { tree }, 2);

            Assert.That(
                skills.SetEquipmentBonusRank(terminalBurst, 5),
                Is.True);
            Assert.That(skills.GetInvestedRank(terminalBurst), Is.Zero);
            Assert.That(skills.GetEffectiveRank(terminalBurst), Is.Zero);
            Assert.That(
                skills.GetInvestmentBlocker(ballisticMobility),
                Is.EqualTo(SkillInvestmentBlocker.MissingPrerequisite));

            Assert.That(skills.TryInvestPoint(terminalBurst), Is.True);
            Assert.That(skills.GetInvestedRank(terminalBurst), Is.EqualTo(1));
            Assert.That(skills.GetEffectiveRank(terminalBurst), Is.EqualTo(6));
            Assert.That(skills.TryInvestPoint(ballisticMobility), Is.True);
            yield break;
        }

        [UnityTest]
        public IEnumerator LevelSixActiveCanBeAnIndependentEntryPoint()
        {
            progression.Configure(progressionSettings, 6, 0);
            skills.Configure(progression, new[] { tree }, 1);

            Assert.That(
                skills.GetInvestmentBlocker(combatReload),
                Is.EqualTo(SkillInvestmentBlocker.None));
            Assert.That(skills.TryInvestPoint(combatReload), Is.True);
            yield break;
        }

        [UnityTest]
        public IEnumerator PassiveCanUnlockAnActiveThroughAnyPrerequisite()
        {
            progression.Configure(progressionSettings, 12, 0);
            skills.Configure(progression, new[] { tree }, 2);

            Assert.That(
                skills.GetInvestmentBlocker(breechSweep),
                Is.EqualTo(SkillInvestmentBlocker.MissingPrerequisite));
            Assert.That(skills.TryInvestPoint(predatoryCadence), Is.True);
            Assert.That(skills.TryInvestPoint(breechSweep), Is.True);
            yield break;
        }

        [UnityTest]
        public IEnumerator UnequippedActiveStillGrantsHardPointSynergy()
        {
            progression.Configure(progressionSettings, 12, 0);
            skills.Configure(progression, new[] { tree }, 2);

            Assert.That(skills.TryInvestPoint(combatReload), Is.True);
            Assert.That(skillBar.GetSkill(0), Is.Null);
            Assert.That(
                skills.GetInvestedSynergyBonus(
                    breechSweep,
                    "weapon_damage_percent",
                    SkillSynergyOperation.AdditivePercent),
                Is.EqualTo(2f));
            yield break;
        }

        [UnityTest]
        public IEnumerator AtLeastGroupRequiresTheConfiguredNumberOfSkills()
        {
            SkillDefinition advancedSkill = CreateSkill(
                "marine_advanced_test",
                "Test avancé",
                SkillType.Passive,
                6,
                SkillTargetMode.None,
                prerequisiteGroups: new[]
                {
                    new SkillPrerequisiteGroup(
                        SkillPrerequisiteMode.AtLeast,
                        new[]
                        {
                            new SkillPrerequisite(terminalBurst),
                            new SkillPrerequisite(predatoryCadence),
                            new SkillPrerequisite(combatReload)
                        },
                        2)
                });
            tree.Configure(
                tree.Identifier,
                tree.CharacterClass,
                tree.DisplayName,
                tree.Description,
                new[]
                {
                    terminalBurst,
                    predatoryCadence,
                    combatReload,
                    ballisticMobility,
                    breechSweep,
                    advancedSkill
                });
            progression.Configure(progressionSettings, 6, 0);
            skills.Configure(progression, new[] { tree }, 3);

            Assert.That(skills.TryInvestPoint(terminalBurst), Is.True);
            Assert.That(
                skills.GetInvestmentBlocker(advancedSkill),
                Is.EqualTo(SkillInvestmentBlocker.MissingPrerequisite));
            Assert.That(skills.TryInvestPoint(combatReload), Is.True);
            Assert.That(
                skills.GetInvestmentBlocker(advancedSkill),
                Is.EqualTo(SkillInvestmentBlocker.None));
            Assert.That(skills.TryInvestPoint(advancedSkill), Is.True);
            yield break;
        }

        [UnityTest]
        public IEnumerator NaturalRankStopsAtTwentyAndEquipmentCanExceedIt()
        {
            progression.Configure(progressionSettings, 30, 0);
            skills.Configure(progression, new[] { tree }, 25);

            for (int rank = 1; rank <= 20; rank++)
            {
                Assert.That(skills.TryInvestPoint(terminalBurst), Is.True);
            }

            Assert.That(skills.TryInvestPoint(terminalBurst), Is.False);
            Assert.That(
                skills.GetInvestmentBlocker(terminalBurst),
                Is.EqualTo(SkillInvestmentBlocker.MaximumRankReached));
            Assert.That(
                skills.SetEquipmentBonusRank(terminalBurst, 5),
                Is.True);
            Assert.That(skills.GetInvestedRank(terminalBurst), Is.EqualTo(20));
            Assert.That(skills.GetEffectiveRank(terminalBurst), Is.EqualTo(25));
            Assert.That(
                terminalBurst.GetRankValue("cooldown_seconds", 25),
                Is.EqualTo(4.5f).Within(0.001f));
            yield break;
        }

        [UnityTest]
        public IEnumerator SynergiesIgnoreEquipmentBonusRanks()
        {
            progression.Configure(progressionSettings, 30, 0);
            skills.Configure(progression, new[] { tree }, 5);

            Assert.That(skills.TryInvestPoint(predatoryCadence), Is.True);
            Assert.That(skills.TryInvestPoint(predatoryCadence), Is.True);
            Assert.That(
                skills.SetEquipmentBonusRank(predatoryCadence, 8),
                Is.True);

            float synergy = skills.GetInvestedSynergyBonus(
                terminalBurst,
                "weapon_damage_percent",
                SkillSynergyOperation.AdditivePercent);
            Assert.That(synergy, Is.EqualTo(3f));
            yield break;
        }

        [UnityTest]
        public IEnumerator ActiveBarHasFiveDistinctLearnedActiveSlots()
        {
            skills.Configure(progression, new[] { tree }, 2);

            Assert.That(skillBar.Capacity, Is.EqualTo(5));
            Assert.That(skillBar.TryAssign(0, terminalBurst), Is.False);
            Assert.That(skills.TryInvestPoint(terminalBurst), Is.True);
            Assert.That(skillBar.TryAssign(4, terminalBurst), Is.True);
            Assert.That(skillBar.GetSkill(4), Is.SameAs(terminalBurst));

            Assert.That(skillBar.TryAssign(2, terminalBurst), Is.True);
            Assert.That(skillBar.GetSkill(4), Is.Null);
            Assert.That(skillBar.GetSkill(2), Is.SameAs(terminalBurst));

            Assert.That(skills.TryInvestPoint(predatoryCadence), Is.True);
            Assert.That(skillBar.TryAssign(0, predatoryCadence), Is.False);
            Assert.That(
                skillBar.GetAssignmentBlocker(0, predatoryCadence),
                Is.EqualTo(SkillAssignmentBlocker.PassiveSkill));
            Assert.That(skillBar.TryAssign(5, terminalBurst), Is.False);
            yield break;
        }

        private SkillDefinition CreateSkill(
            string identifier,
            string displayName,
            SkillType type,
            int unlockLevel,
            SkillTargetMode targetMode,
            IReadOnlyList<SkillPrerequisite> prerequisites = null,
            IReadOnlyList<SkillRankValue> values = null,
            IReadOnlyList<SkillPrerequisiteGroup> prerequisiteGroups = null,
            IReadOnlyList<SkillInvestedRankSynergy> synergies = null)
        {
            SkillDefinition skill = CreateAsset<SkillDefinition>();
            skill.Configure(
                identifier,
                CharacterClassId.Marine,
                "marine_doctrine_saturation",
                displayName,
                string.Empty,
                type,
                unlockLevel,
                targetMode,
                requiredSkills: prerequisites,
                values: values,
                synergies: synergies,
                prerequisiteSets: prerequisiteGroups);
            return skill;
        }

        private T CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            temporaryAssets.Add(asset);
            return asset;
        }
    }
}
