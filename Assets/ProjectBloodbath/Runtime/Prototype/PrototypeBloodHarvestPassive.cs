using System;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AbilityResource))]
    public sealed class PrototypeBloodHarvestPassive : MonoBehaviour
    {
        [SerializeField] private AbilityResource abilityResource;
        [SerializeField] private PassiveAbilitySettings settings;

        private float feedbackUntil;

        public event Action<float> Triggered;

        public PassiveAbilitySettings Settings => settings;
        public int TriggerCount { get; private set; }
        public float LastRestoredAmount { get; private set; }
        public float FeedbackRemaining => Mathf.Max(
            0f,
            feedbackUntil - Time.time);

        public void Configure(
            AbilityResource resource,
            PassiveAbilitySettings passiveSettings)
        {
            abilityResource = resource;
            settings = passiveSettings;
        }

        private void Awake()
        {
            if (abilityResource == null)
            {
                abilityResource = GetComponent<AbilityResource>();
            }
        }

        private void OnEnable()
        {
            CombatEvents.CombatantDied += OnCombatantDied;
        }

        private void OnDisable()
        {
            CombatEvents.CombatantDied -= OnCombatantDied;
        }

        private void OnCombatantDied(CombatDeathEvent death)
        {
            GameObject target = death.Target;
            GameObject source = death.FinishingBlow.Source;
            if (
                settings == null ||
                abilityResource == null ||
                target == null ||
                source == null ||
                target.transform.root == transform.root ||
                source.transform.root != transform.root)
            {
                return;
            }

            float resourceBefore = abilityResource.Current;
            abilityResource.Restore(settings.ResourceRestoredPerKill);
            LastRestoredAmount = abilityResource.Current - resourceBefore;
            if (LastRestoredAmount <= 0f)
            {
                return;
            }

            TriggerCount++;
            feedbackUntil = Time.time + 1.1f;
            Triggered?.Invoke(LastRestoredAmount);
        }
    }
}
