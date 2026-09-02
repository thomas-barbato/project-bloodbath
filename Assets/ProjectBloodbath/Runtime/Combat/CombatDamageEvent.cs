using UnityEngine;

namespace ProjectBloodbath.Combat
{
    public readonly struct CombatDamageEvent
    {
        public CombatDamageEvent(
            GameObject target,
            DamageInfo damage,
            float previousHealth,
            float currentHealth)
        {
            Target = target;
            Damage = damage;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
        }

        public GameObject Target { get; }
        public DamageInfo Damage { get; }
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public float AppliedAmount =>
            Mathf.Max(0f, PreviousHealth - CurrentHealth);
        public bool IsLethal => PreviousHealth > 0f && CurrentHealth <= 0f;
    }
}
