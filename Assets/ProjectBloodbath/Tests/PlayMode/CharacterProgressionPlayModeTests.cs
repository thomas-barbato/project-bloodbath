using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class CharacterProgressionPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private GameObject player;
        private CharacterProgression progression;
        private CharacterStatistics statistics;
        private CharacterSkillProgression skills;
        private ActiveSkillBar skillBar;
        private GameObject enemy;
        private Health enemyHealth;
        private EnemyExperienceReward reward;
        private GameObject playerWeaponSource;
        private GameObject foreignSource;

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

            GameObject.Find("PrototypeSkirmisher")?.SetActive(false);
            player = GameObject.Find("Player");
            enemy = GameObject.Find("PrototypeEnemy");
            Assert.That(player, Is.Not.Null);
            Assert.That(enemy, Is.Not.Null);

            progression = player.GetComponent<CharacterProgression>();
            statistics = player.GetComponent<CharacterStatistics>();
            skills = player.GetComponent<CharacterSkillProgression>();
            skillBar = player.GetComponent<ActiveSkillBar>();
            enemyHealth = enemy.GetComponent<Health>();
            reward = enemy.GetComponent<EnemyExperienceReward>();
            Assert.That(progression, Is.Not.Null);
            Assert.That(progression.Settings, Is.Not.Null);
            Assert.That(statistics, Is.Not.Null);
            Assert.That(skills, Is.Not.Null);
            Assert.That(skillBar, Is.Not.Null);
            Assert.That(enemyHealth, Is.Not.Null);
            Assert.That(reward, Is.Not.Null);
            Assert.That(reward.Profile, Is.Not.Null);

            PrototypeEnemyController enemyController =
                enemy.GetComponent<PrototypeEnemyController>();
            Assert.That(enemyController, Is.Not.Null);
            enemyController.enabled = false;
            playerWeaponSource = new GameObject("ProgressionPlayerSource");
            playerWeaponSource.transform.SetParent(player.transform);
            foreignSource = new GameObject("ProgressionForeignSource");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(playerWeaponSource);
            Object.Destroy(foreignSource);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerKillGrantsConfiguredExperienceOnlyToPlayer()
        {
            int initialExperience = progression.CurrentExperience;
            KillEnemy(playerWeaponSource);

            Assert.That(
                progression.CurrentExperience,
                Is.EqualTo(initialExperience + reward.Profile.ExperienceReward));
            Assert.That(
                reward.LastGrantedAmount,
                Is.EqualTo(reward.Profile.ExperienceReward));

            enemyHealth.RestoreFull();
            KillEnemy(foreignSource);
            Assert.That(
                progression.CurrentExperience,
                Is.EqualTo(initialExperience + reward.Profile.ExperienceReward));
            Assert.That(reward.LastGrantedAmount, Is.Zero);
            yield break;
        }

        [UnityTest]
        public IEnumerator ExcessExperienceCarriesAcrossSeveralLevels()
        {
            int levelOneRequirement =
                progression.ExperienceRequiredForNextLevel;
            int levelTwoRequirement = progression.Settings
                .GetExperienceRequiredForLevel(2);
            int remainder = 7;

            int gainedLevels = progression.AddExperience(
                levelOneRequirement + levelTwoRequirement + remainder);

            Assert.That(gainedLevels, Is.EqualTo(2));
            Assert.That(progression.CurrentLevel, Is.EqualTo(3));
            Assert.That(progression.CurrentExperience, Is.EqualTo(remainder));
            Assert.That(
                statistics.UnspentAttributePoints,
                Is.EqualTo(progression.Settings.AttributePointsPerLevel * 2));
            yield break;
        }

        [UnityTest]
        public IEnumerator ProgressionSupportsLevelNinetyNineAndSkillPoints()
        {
            Assert.That(progression.Settings.MaximumLevel, Is.EqualTo(99));
            Assert.That(progression.Settings.SkillPointsPerLevel, Is.EqualTo(1));
            Assert.That(
                progression.Settings.GetExperienceRequiredForLevel(98),
                Is.GreaterThan(0));
            Assert.That(
                progression.Settings.GetExperienceRequiredForLevel(99),
                Is.Zero);
            yield break;
        }

        [UnityTest]
        public IEnumerator MarineSkillTreesAreWiredAndStructurallyValid()
        {
            Assert.That(skills.AvailableTrees, Has.Count.EqualTo(3));
            Assert.That(
                skills.AvailableTrees[0].Identifier,
                Is.EqualTo("marine_doctrine_saturation"));
            Assert.That(
                skills.AvailableTrees[1].Identifier,
                Is.EqualTo("marine_ordnance_rupture"));
            Assert.That(
                skills.AvailableTrees[2].Identifier,
                Is.EqualTo("marine_controlled_devastation"));
            foreach (SkillTreeDefinition tree in skills.AvailableTrees)
            {
                Assert.That(tree, Is.Not.Null);
                Assert.That(
                    tree.CharacterClass,
                    Is.EqualTo(CharacterClassId.Marine));
                Assert.That(
                    tree.TryValidateStructure(out string issue),
                    Is.True,
                    issue);
            }

            Assert.That(skillBar.Capacity, Is.EqualTo(5));
            yield break;
        }

        private void KillEnemy(GameObject source)
        {
            enemyHealth.ApplyDamage(new DamageInfo(
                enemyHealth.Maximum,
                DamageType.Ballistic,
                enemy.transform.position,
                Vector3.up,
                Vector3.forward,
                0f,
                source));
            Assert.That(enemyHealth.IsAlive, Is.False);
        }
    }
}
