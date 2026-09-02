using System;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    public sealed class AbilityResource : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximum = 100f;

        private float current;

        public event Action<float, float> Changed;

        public float Current => current;
        public float Maximum => maximum;
        public float Ratio => maximum > 0f
            ? Mathf.Clamp01(current / maximum)
            : 0f;

        public void Configure(float maximumResource)
        {
            maximum = Mathf.Max(1f, maximumResource);
            RestoreFull();
        }

        public bool TrySpend(float amount)
        {
            if (amount <= 0f || current < amount)
            {
                return false;
            }

            current -= amount;
            Changed?.Invoke(current, maximum);
            return true;
        }

        public void Restore(float amount)
        {
            if (amount <= 0f || current >= maximum)
            {
                return;
            }

            current = Mathf.Min(maximum, current + amount);
            Changed?.Invoke(current, maximum);
        }

        public void RestoreFull()
        {
            current = maximum;
            Changed?.Invoke(current, maximum);
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
