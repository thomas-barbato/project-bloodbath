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

        public void Configure(float maximumHealth)
        {
            maximum = Mathf.Max(1f, maximumHealth);
            current = maximum;
        }

        public void ApplyDamage(DamageInfo damage)
        {
            if (!IsAlive || damage.Amount <= 0f)
            {
                return;
            }

            current = Mathf.Max(0f, current - damage.Amount);
            Damaged?.Invoke(damage);

            if (current <= 0f)
            {
                Died?.Invoke(damage);
            }
        }

        public void RestoreFull()
        {
            current = maximum;
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
