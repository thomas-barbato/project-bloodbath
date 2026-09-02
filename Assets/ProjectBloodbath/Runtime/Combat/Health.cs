using System;
using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maximum = 100f;

        private float current;

        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Died;

        public float Current => current;
        public float Maximum => maximum;
        public bool IsAlive => current > 0f;
        public bool IsInvulnerable { get; private set; }

        public void Configure(float maximumHealth)
        {
            maximum = Mathf.Max(1f, maximumHealth);
            current = maximum;
        }

        public void ApplyDamage(DamageInfo damage)
        {
            if (IsInvulnerable || !IsAlive || damage.Amount <= 0f)
            {
                return;
            }

            float previousHealth = current;
            current = Mathf.Max(0f, current - damage.Amount);
            CombatEvents.PublishCombatantDamaged(
                gameObject,
                damage,
                previousHealth,
                current);
            Damaged?.Invoke(damage);

            if (current <= 0f)
            {
                CombatEvents.PublishCombatantDied(gameObject, damage);
                Died?.Invoke(damage);
            }
        }

        public void RestoreFull()
        {
            current = maximum;
        }

        public void SetInvulnerable(bool invulnerable)
        {
            IsInvulnerable = invulnerable;
        }

        private void Awake()
        {
            RestoreFull();
        }

        private void OnValidate()
        {
            maximum = Mathf.Max(1f, maximum);
        }
    }
}
