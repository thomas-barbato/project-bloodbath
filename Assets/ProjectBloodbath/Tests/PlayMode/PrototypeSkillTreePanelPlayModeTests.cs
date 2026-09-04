using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeSkillTreePanelPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Keyboard keyboard;
        private PlayerInputReader inputReader;
        private PrototypeSkillTreePanel panel;
        private CharacterProgression progression;
        private CharacterSkillProgression skills;
        private ActiveSkillBar skillBar;
        private PrototypeCombatHud hud;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                ScenePath,
                LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null);
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            GameObject.Find("PrototypeEnemy")?.SetActive(false);
            GameObject.Find("PrototypeSkirmisher")?.SetActive(false);

            GameObject player = GameObject.Find("Player");
            Assert.That(player, Is.Not.Null);
            inputReader = player.GetComponent<PlayerInputReader>();
            panel = player.GetComponent<PrototypeSkillTreePanel>();
            progression = player.GetComponent<CharacterProgression>();
            skills = player.GetComponent<CharacterSkillProgression>();
            skillBar = player.GetComponent<ActiveSkillBar>();
            hud = player.GetComponent<PrototypeCombatHud>();

            Assert.That(inputReader, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            Assert.That(progression, Is.Not.Null);
            Assert.That(skills, Is.Not.Null);
            Assert.That(skillBar, Is.Not.Null);
            Assert.That(hud, Is.Not.Null);
            Assert.That(skills.AvailableTrees.Count, Is.GreaterThan(0));

            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            panel?.SetOpen(false);
            if (keyboard != null && keyboard.added)
            {
                SetKeys();
                InputSystem.RemoveDevice(keyboard);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator KOpensTreeAndKeepsOnlyTheLiveSkillBarVisible()
        {
            Assert.That(panel.IsOpen, Is.False);

            SetKeys(Key.K);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(panel.IsOpen, Is.True,
                "K doit ouvrir le panneau de compétences.");
            Assert.That(inputReader.GameplaySuppressed, Is.True,
                "L'arbre ouvert doit suspendre les commandes de gameplay.");
            Assert.That(hud.KeepsAbilityBarVisibleForOpenView, Is.True,
                "Le HUD doit identifier l'arbre comme vue conservant la barre.");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));

            SetKeys(Key.K);
            yield return null;
            SetKeys();
            yield return null;

            Assert.That(panel.IsOpen, Is.False,
                "Une seconde pression sur K doit fermer le panneau.");
            Assert.That(inputReader.GameplaySuppressed, Is.False,
                "Fermer l'arbre doit restaurer les commandes de gameplay.");
            Assert.That(hud.KeepsAbilityBarVisibleForOpenView, Is.False,
                "La barre seule ne doit plus être forcée après fermeture.");
        }

        [UnityTest]
        public IEnumerator AssignedActiveSkillUsesItsLiveImprovedRank()
        {
            SkillDefinition active = FindSkill(SkillType.Active);
            Assert.That(active, Is.Not.Null);
            Assert.That(panel.SelectSkill(IndexOfSkill(active)), Is.True);

            GainOneLevel();
            GainOneLevel();
            Assert.That(skills.UnspentSkillPoints, Is.EqualTo(2));

            Assert.That(panel.TryInvestSelectedPoint(), Is.True);
            Assert.That(panel.TryAssignSelectedToSlot(2), Is.True);
            Assert.That(skillBar.GetSkill(2), Is.SameAs(active));
            Assert.That(skillBar.GetEffectiveRank(2), Is.EqualTo(1));
            Assert.That(hud.GetAbilitySlotEffectiveRank(2), Is.EqualTo(1));

            Assert.That(panel.TryInvestSelectedPoint(), Is.True);
            Assert.That(skillBar.GetSkill(2), Is.SameAs(active),
                "L'amélioration ne doit pas exiger une nouvelle affectation.");
            Assert.That(skillBar.GetEffectiveRank(2), Is.EqualTo(2),
                "La barre doit résoudre le rang courant, pas une copie figée.");
            Assert.That(hud.GetAbilitySlotEffectiveRank(2), Is.EqualTo(2),
                "Le HUD doit relire le rang effectif courant.");
            yield break;
        }

        [UnityTest]
        public IEnumerator PassiveSkillCannotBeAssignedToTheBar()
        {
            SkillDefinition passive = FindSkill(SkillType.Passive);
            Assert.That(passive, Is.Not.Null);
            Assert.That(panel.SelectSkill(IndexOfSkill(passive)), Is.True);

            GainOneLevel();
            Assert.That(panel.TryAssignSelectedToSlot(0), Is.False);
            Assert.That(skillBar.GetSkill(0), Is.Null);
            yield break;
        }

        [UnityTest]
        public IEnumerator DoctrineOfSaturationUsesVersionTwoTopology()
        {
            SkillTreeDefinition tree = skills.AvailableTrees[0];
            Assert.That(tree.TryValidateStructure(out string issue),
                Is.True, issue);
            Assert.That(tree.Skills.Count, Is.EqualTo(10));

            SkillDefinition reload = tree.FindSkill("marine_combat_reload");
            SkillDefinition mobility = tree.FindSkill(
                "marine_ballistic_mobility");
            SkillDefinition sweep = tree.FindSkill("marine_breech_sweep");
            Assert.That(reload, Is.Not.Null);
            Assert.That(mobility, Is.Not.Null);
            Assert.That(sweep, Is.Not.Null);
            Assert.That(reload.Prerequisites, Is.Empty,
                "Rechargement de combat doit rester une racine indépendante.");
            Assert.That(reload.PrerequisiteGroups, Is.Empty);
            Assert.That(mobility.PrerequisiteGroups.Count, Is.EqualTo(1));
            Assert.That(
                mobility.PrerequisiteGroups[0].Mode,
                Is.EqualTo(SkillPrerequisiteMode.Any));
            Assert.That(sweep.SkillType, Is.EqualTo(SkillType.Active));
            yield break;
        }

        [UnityTest]
        public IEnumerator DoctrineOfSaturationUsesRevisedCombatIdentities()
        {
            SkillTreeDefinition tree = skills.AvailableTrees[0];
            SkillDefinition terminal = tree.FindSkill("marine_terminal_burst");
            SkillDefinition cadence = tree.FindSkill(
                "marine_predatory_cadence");
            SkillDefinition mobility = tree.FindSkill(
                "marine_ballistic_mobility");
            SkillDefinition sweep = tree.FindSkill("marine_breech_sweep");
            SkillDefinition feed = tree.FindSkill("marine_brutal_feed");
            SkillDefinition doubleTrigger = tree.FindSkill(
                "marine_double_trigger");
            SkillDefinition rain = tree.FindSkill("marine_rain_of_casings");
            SkillDefinition storm = tree.FindSkill("marine_breech_storm");
            SkillDefinition hunter = tree.FindSkill(
                "marine_adrenaline_hunter");

            Assert.That(
                terminal.GetRankValue("burst_duration_seconds", 1),
                Is.EqualTo(0.28f).Within(0.001f));
            AssertSynergy(
                terminal,
                cadence,
                "weapon_damage_percent",
                1.5f);

            Assert.That(
                mobility.GetRankValue(
                    "maximum_ballistic_momentum_charges",
                    20),
                Is.Zero,
                "L'Élan ne doit plus être une seconde ressource à charges.");
            Assert.That(
                mobility.GetRankValue(
                    "saturation_loss_reduction_percent",
                    20),
                Is.EqualTo(60f).Within(0.001f));

            Assert.That(
                sweep.GetRankValue("cone_angle_degrees", 20),
                Is.EqualTo(95f).Within(0.001f));
            Assert.That(
                sweep.GetRankValue("maximum_hits_per_target", 20),
                Is.EqualTo(3f).Within(0.001f));
            Assert.That(
                feed.GetRankValue("retained_saturation_percent", 20),
                Is.EqualTo(80f).Within(0.001f));

            Assert.That(
                doubleTrigger.GetRankValue("weapon_damage_percent", 20),
                Is.EqualTo(520f).Within(0.001f));
            Assert.That(doubleTrigger.ConsumesStatus("riddled"), Is.True);
            Assert.That(
                doubleTrigger.GetRankValue(
                    "riddled_detonation_weapon_damage_percent",
                    20),
                Is.EqualTo(200f).Within(0.001f));

            Assert.That(
                rain.GetRankValue("internal_cooldown_seconds", 20),
                Is.EqualTo(0.25f).Within(0.001f));
            Assert.That(
                storm.GetRankValue("reload_speed_bonus_percent", 20),
                Is.EqualTo(60f).Within(0.001f));
            Assert.That(
                storm.GetRankValue("duration_seconds", 20),
                Is.EqualTo(9f).Within(0.001f));

            Assert.That(hunter.AppliesStatus("riddled"), Is.False);
            Assert.That(
                hunter.GetRankValue("explosion_damage_percent", 20),
                Is.Zero,
                "Chasseur ne doit plus dupliquer le rôle d'explosion de zone.");
            Assert.That(
                hunter.GetRankValue("adrenaline_duration_seconds", 20),
                Is.EqualTo(6f).Within(0.001f));
            yield break;
        }

        [UnityTest]
        public IEnumerator OrdnanceOfRuptureUsesRevisedHeavyWeaponLoop()
        {
            SkillTreeDefinition tree = FindTree("marine_ordnance_rupture");
            Assert.That(tree, Is.Not.Null);
            Assert.That(tree.TryValidateStructure(out string issue),
                Is.True, issue);
            Assert.That(panel.SelectTree(IndexOfTree(tree)), Is.True);
            Assert.That(panel.SelectedTree, Is.SameAs(tree));

            SkillDefinition striker = tree.FindSkill(
                "marine_overcharged_striker");
            SkillDefinition denseCore = tree.FindSkill(
                "marine_dense_core_ammunition");
            SkillDefinition line = tree.FindSkill("marine_demolition_line");
            SkillDefinition mount = tree.FindSkill("marine_hydraulic_mount");
            SkillDefinition anchor = tree.FindSkill("marine_anchor_shot");
            SkillDefinition stoppingMass = tree.FindSkill(
                "marine_stopping_mass");
            SkillDefinition seismic = tree.FindSkill(
                "marine_seismic_impact");
            SkillDefinition chamber = tree.FindSkill(
                "marine_sacrificial_chamber");
            SkillDefinition overload = tree.FindSkill(
                "marine_cannon_overload");
            SkillDefinition architecture = tree.FindSkill(
                "marine_siege_architecture");

            Assert.That(striker.Prerequisites, Is.Empty);
            Assert.That(denseCore.Prerequisites, Is.Empty);
            Assert.That(line.Prerequisites.Count, Is.EqualTo(1));
            Assert.That(line.Prerequisites[0].Skill, Is.SameAs(denseCore));
            Assert.That(seismic.PrerequisiteGroups.Count, Is.EqualTo(1));
            Assert.That(
                seismic.PrerequisiteGroups[0].Mode,
                Is.EqualTo(SkillPrerequisiteMode.Any));
            Assert.That(chamber.PrerequisiteGroups.Count, Is.EqualTo(1));
            Assert.That(architecture.PrerequisiteGroups.Count, Is.EqualTo(1));

            Assert.That(
                mount.GetRankValue(
                    "preparation_time_reduction_percent",
                    20),
                Is.EqualTo(25f).Within(0.001f));
            Assert.That(
                mount.GetRankValue("brace_grace_duration_seconds", 20),
                Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(
                anchor.GetRankValue("anchor_duration_seconds", 20),
                Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(
                stoppingMass.GetRankValue("stagger_per_fracture_percent", 20),
                Is.EqualTo(12f).Within(0.001f));
            Assert.That(
                chamber.GetRankValue("projectile_speed_bonus_percent", 20),
                Is.Zero,
                "Chambre ne doit plus faciliter indirectement la visée.");
            Assert.That(
                chamber.GetRankValue("energy_cost_reduction_percent", 20),
                Is.Zero,
                "Un chargeur sans réserve ne doit pas offrir un coût inutile.");

            Assert.That(
                overload.GetRankValue("weapon_damage_percent", 20),
                Is.EqualTo(900f).Within(0.001f));
            Assert.That(overload.ConsumesStatus("fracture"), Is.True);
            Assert.That(overload.ConsumesStatus("primer"), Is.True);
            Assert.That(
                architecture.GetRankValue(
                    "heavy_skill_cooldown_recovery_percent",
                    20),
                Is.EqualTo(25f).Within(0.001f));
            Assert.That(
                architecture.GetRankValue("overkill_wave_damage_percent", 20),
                Is.Zero,
                "Architecture ne doit plus produire une attaque automatique sans rapport avec le protocole.");

            AssertSynergy(
                striker,
                denseCore,
                "weapon_damage_percent",
                1.5f);
            AssertSynergy(
                seismic,
                line,
                "weapon_damage_percent",
                1.5f);
            AssertSynergy(
                architecture,
                overload,
                "siege_protocol_duration_percent",
                0.5f);
            yield break;
        }

        [UnityTest]
        public IEnumerator ControlledDevastationUsesBoundedExplosiveRotation()
        {
            SkillTreeDefinition tree = FindTree(
                "marine_controlled_devastation");
            Assert.That(tree, Is.Not.Null);
            Assert.That(tree.TryValidateStructure(out string issue),
                Is.True, issue);
            Assert.That(panel.SelectTree(IndexOfTree(tree)), Is.True);
            Assert.That(panel.SelectedTree, Is.SameAs(tree));

            SkillDefinition grenade = tree.FindSkill(
                "marine_m13_skinner_grenade");
            SkillDefinition belt = tree.FindSkill("marine_demolition_belt");
            SkillDefinition mine = tree.FindSkill("marine_scavenger_mine");
            SkillDefinition compounds = tree.FindSkill("marine_pit_compounds");
            SkillDefinition breach = tree.FindSkill("marine_breach_charge");
            SkillDefinition shrapnel = tree.FindSkill(
                "marine_industrial_shrapnel");
            SkillDefinition rocket = tree.FindSkill(
                "marine_thermobaric_rocket");
            SkillDefinition reaction = tree.FindSkill("marine_chain_reaction");
            SkillDefinition crown = tree.FindSkill("marine_charge_crown");
            SkillDefinition protocol = tree.FindSkill(
                "marine_scorched_earth_protocol");

            Assert.That(grenade.Prerequisites, Is.Empty);
            Assert.That(belt.Prerequisites, Is.Empty);
            Assert.That(mine.Prerequisites, Is.Empty);
            Assert.That(compounds.Prerequisites.Count, Is.EqualTo(1));
            Assert.That(compounds.Prerequisites[0].Skill, Is.SameAs(grenade));
            AssertAnyPrerequisiteGroup(breach, belt, compounds);
            AssertAnyPrerequisiteGroup(shrapnel, mine, grenade);
            AssertAnyPrerequisiteGroup(rocket, grenade, breach);
            AssertAnyPrerequisiteGroup(reaction, shrapnel, compounds);
            AssertAnyPrerequisiteGroup(crown, mine, breach);
            Assert.That(protocol.Prerequisites.Count, Is.EqualTo(1));
            Assert.That(protocol.Prerequisites[0].Skill, Is.SameAs(reaction));

            Assert.That(
                compounds.GetRankValue("maximum_unstable_mix_charges", 20),
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(
                compounds.GetRankValue("maximum_escalation_charges", 20),
                Is.Zero,
                "Composés de fosse ne doit pas créer une ressource empilable.");
            Assert.That(
                mine.GetRankValue("maximum_active_mines", 20),
                Is.EqualTo(4f).Within(0.001f));
            Assert.That(
                shrapnel.GetRankValue("maximum_secondary_chain_depth", 20),
                Is.Zero);
            Assert.That(
                rocket.GetRankValue("explosion_radius_metres", 20),
                Is.EqualTo(7.5f).Within(0.001f));
            Assert.That(
                rocket.GetRankValue("burning_zone_duration_seconds", 20),
                Is.EqualTo(6f).Within(0.001f));
            Assert.That(
                reaction.GetRankValue(
                    "reaction_damage_per_primer_percent",
                    20),
                Is.EqualTo(100f).Within(0.001f));
            Assert.That(
                reaction.GetRankValue("maximum_secondary_chain_depth", 20),
                Is.Zero);
            Assert.That(
                crown.GetRankValue("projectile_count", 20),
                Is.EqualTo(9f).Within(0.001f));
            Assert.That(
                crown.GetRankValue("maximum_hits_per_target", 20),
                Is.EqualTo(3f).Within(0.001f));
            Assert.That(
                protocol.GetRankValue(
                    "required_distinct_explosive_skills",
                    20),
                Is.EqualTo(3f).Within(0.001f));
            Assert.That(
                protocol.GetRankValue("scorched_earth_duration_seconds", 20),
                Is.EqualTo(10f).Within(0.001f));
            Assert.That(
                protocol.GetRankValue("maximum_escalation_charges", 20),
                Is.Zero,
                "Terre brûlée doit ouvrir une fenêtre, pas accumuler une seconde ressource.");

            AssertSynergy(
                grenade,
                belt,
                "weapon_damage_percent",
                1.5f);
            AssertSynergy(
                mine,
                reaction,
                "weapon_damage_percent",
                1.5f);
            AssertSynergy(
                protocol,
                compounds,
                "scorched_earth_duration_percent",
                0.5f);
            yield break;
        }

        private void GainOneLevel()
        {
            int previousLevel = progression.CurrentLevel;
            progression.AddExperience(
                progression.ExperienceRequiredForNextLevel);
            Assert.That(
                progression.CurrentLevel,
                Is.EqualTo(previousLevel + 1));
        }

        private SkillDefinition FindSkill(SkillType type)
        {
            foreach (SkillDefinition skill in skills.AvailableTrees[0].Skills)
            {
                if (skill != null && skill.SkillType == type)
                {
                    return skill;
                }
            }

            return null;
        }

        private int IndexOfSkill(SkillDefinition target)
        {
            for (int index = 0;
                index < skills.AvailableTrees[0].Skills.Count;
                index++)
            {
                if (skills.AvailableTrees[0].Skills[index] == target)
                {
                    return index;
                }
            }

            return -1;
        }

        private SkillTreeDefinition FindTree(string identifier)
        {
            foreach (SkillTreeDefinition tree in skills.AvailableTrees)
            {
                if (tree != null && tree.Identifier == identifier)
                {
                    return tree;
                }
            }

            return null;
        }

        private int IndexOfTree(SkillTreeDefinition target)
        {
            for (int index = 0; index < skills.AvailableTrees.Count; index++)
            {
                if (skills.AvailableTrees[index] == target)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void AssertSynergy(
            SkillDefinition target,
            SkillDefinition source,
            string valueIdentifier,
            float expectedBonus)
        {
            foreach (SkillInvestedRankSynergy synergy in
                target.InvestedRankSynergies)
            {
                if (
                    synergy.SourceSkill == source &&
                    synergy.AffectedValueIdentifier == valueIdentifier)
                {
                    Assert.That(
                        synergy.BonusPerInvestedRank,
                        Is.EqualTo(expectedBonus).Within(0.001f));
                    return;
                }
            }

            Assert.Fail(
                $"Synergie absente : {source.DisplayName} vers " +
                $"{target.DisplayName} ({valueIdentifier}).");
        }

        private static void AssertAnyPrerequisiteGroup(
            SkillDefinition skill,
            SkillDefinition first,
            SkillDefinition second)
        {
            Assert.That(skill.PrerequisiteGroups, Has.Count.EqualTo(1));
            SkillPrerequisiteGroup group = skill.PrerequisiteGroups[0];
            Assert.That(group.Mode, Is.EqualTo(SkillPrerequisiteMode.Any));
            Assert.That(group.Prerequisites, Has.Count.EqualTo(2));
            Assert.That(
                group.Prerequisites[0].Skill == first ||
                group.Prerequisites[1].Skill == first,
                Is.True,
                $"Le groupe OU de {skill.DisplayName} doit inclure {first.DisplayName}.");
            Assert.That(
                group.Prerequisites[0].Skill == second ||
                group.Prerequisites[1].Skill == second,
                Is.True,
                $"Le groupe OU de {skill.DisplayName} doit inclure {second.DisplayName}.");
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
    }
}
