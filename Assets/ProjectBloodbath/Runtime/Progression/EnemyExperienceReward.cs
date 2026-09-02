using ProjectBloodbath.Combat;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyExperienceReward : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private EnemyExperienceProfile profile;

        public EnemyExperienceProfile Profile => profile;
        public int LastGrantedAmount { get; private set; }

        public void Configure(
            Health enemyHealth,
            EnemyExperienceProfile experienceProfile)
        {
            health = enemyHealth;
            profile = experienceProfile;
        }

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        private void OnDied(DamageInfo finishingBlow)
        {
            LastGrantedAmount = 0;
            if (
                profile == null ||
                profile.ExperienceReward <= 0 ||
                finishingBlow.Source == null)
            {
                return;
            }

            CharacterProgression recipient = finishingBlow.Source
                .GetComponentInParent<CharacterProgression>();
            if (recipient == null)
            {
                return;
            }

            recipient.AddExperience(profile.ExperienceReward);
            LastGrantedAmount = profile.ExperienceReward;
        }
    }
}
