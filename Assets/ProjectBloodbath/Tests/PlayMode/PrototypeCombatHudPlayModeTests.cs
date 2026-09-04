using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Prototype;
using ProjectBloodbath.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PrototypeCombatHudPlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Health health;
        private AbilityResource abilityResource;
        private HitscanWeapon rifle;
        private PrototypeWeaponLoadout loadout;
        private PrototypeCombatHud hud;
        private PrototypeShockwaveAbility ability;

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

            GameObject player = GameObject.Find("Player");
            Assert.That(player, Is.Not.Null);
            health = player.GetComponent<Health>();
            abilityResource = player.GetComponent<AbilityResource>();
            loadout = player.GetComponent<PrototypeWeaponLoadout>();
            hud = player.GetComponent<PrototypeCombatHud>();
            rifle = loadout?.ActiveRightRangedWeapon;
            ability = player.GetComponent<PrototypeShockwaveAbility>();

            Assert.That(health, Is.Not.Null);
            Assert.That(abilityResource, Is.Not.Null);
            Assert.That(loadout, Is.Not.Null);
            Assert.That(hud, Is.Not.Null);
            Assert.That(rifle, Is.Not.Null);
            Assert.That(ability, Is.Not.Null);
            Assert.That(
                Resources.Load<Texture2D>("RetroHudBiomedMetal"),
                Is.Not.Null,
                "La peau biomédicale du HUD doit être incluse dans les ressources.");
        }

        [UnityTest]
        public IEnumerator HudTracksHealthWeaponAmmunitionAndReload()
        {
            Assert.That(hud.HealthRatio, Is.EqualTo(1f));
            Assert.That(hud.HealthLabel, Is.EqualTo("100 / 100"));
            Assert.That(hud.AbilityResourceRatio, Is.EqualTo(1f));
            Assert.That(hud.AbilityResourceLabel, Is.EqualTo("100 / 100"));
            Assert.That(hud.WeaponLabel, Is.EqualTo("FUSILS PROTOTYPES"));
            Assert.That(
                hud.AmmunitionLabel,
                Is.EqualTo("D 12/048   G 12/048"));
            Assert.That(hud.ShowsAmmunition, Is.True);
            Assert.That(hud.AbilityLabel, Is.EqualTo("AUCUNE COMPÉTENCE"));
            Assert.That(
                hud.AbilityStatusLabel,
                Is.EqualTo("AUCUNE COMPÉTENCE ASSIGNÉE"));
            Assert.That(hud.AbilitySlotCapacity, Is.EqualTo(5));
            Assert.That(hud.IsAbilitySlotOccupied(0), Is.False);
            Assert.That(hud.IsAbilitySlotOccupied(1), Is.False);
            Assert.That(hud.IsAbilitySlotOccupied(4), Is.False);
            Assert.That(hud.LevelLabel, Is.EqualTo("NIVEAU 1"));
            Assert.That(hud.ExperienceLabel, Is.EqualTo("EXP 0 / 100"));
            Assert.That(hud.ExperienceRatio, Is.Zero);
            Assert.That(hud.AttributePointsLabel, Is.EqualTo("0 PT"));

            health.ApplyDamage(new DamageInfo(
                25f,
                DamageType.Ballistic,
                Vector3.zero,
                Vector3.back,
                Vector3.forward,
                0f,
                null));
            Assert.That(hud.HealthRatio, Is.EqualTo(0.75f));
            Assert.That(hud.HealthLabel, Is.EqualTo("75 / 100"));

            Assert.That(abilityResource.TrySpend(35f), Is.True);
            Assert.That(hud.AbilityResourceRatio, Is.EqualTo(0.65f));
            Assert.That(hud.AbilityResourceLabel, Is.EqualTo("65 / 100"));

            Assert.That(rifle.TryFire(), Is.True);
            Assert.That(rifle.TryStartReload(), Is.True);
            Assert.That(hud.ShowsReload, Is.True);

            loadout.SelectHandSet(PrototypeHandSetSlot.Secondary);
            Assert.That(hud.WeaponLabel, Is.EqualTo("ARME DE MÊLÉE"));
            Assert.That(hud.ShowsAmmunition, Is.False);
            Assert.That(hud.AmmunitionLabel, Is.EqualTo("—"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator HudSupportsFourDigitHealthAndEnergyValues()
        {
            health.Configure(9999f);
            abilityResource.Configure(9999f);

            Assert.That(hud.HealthLabel, Is.EqualTo("9999 / 9999"));
            Assert.That(hud.AbilityResourceLabel, Is.EqualTo("9999 / 9999"));

            health.ApplyDamage(new DamageInfo(
                876f,
                DamageType.Ballistic,
                Vector3.zero,
                Vector3.back,
                Vector3.forward,
                0f,
                null));
            Assert.That(abilityResource.TrySpend(876f), Is.True);

            Assert.That(hud.HealthLabel, Is.EqualTo("9123 / 9999"));
            Assert.That(hud.AbilityResourceLabel, Is.EqualTo("9123 / 9999"));
            yield return null;
        }
    }
}
