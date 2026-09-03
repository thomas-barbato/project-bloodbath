using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class WeaponMarkState : MonoBehaviour
    {
        private sealed class ActiveMark
        {
            public int Stacks;
            public float ExpiresAt;
        }

        private readonly Dictionary<WeaponMarkEffectSettings, ActiveMark>
            activeMarks = new();
        private readonly List<WeaponMarkEffectSettings> expiredMarks = new();
        private Health health;

        public event Action<WeaponMarkEffectSettings, int> MarkChanged;

        public static WeaponMarkState GetOrAdd(Health targetHealth)
        {
            if (targetHealth == null)
            {
                return null;
            }

            WeaponMarkState state = targetHealth.GetComponent<WeaponMarkState>();
            return state != null
                ? state
                : targetHealth.gameObject.AddComponent<WeaponMarkState>();
        }

        public int ApplyMark(
            WeaponMarkEffectSettings effect,
            int addedStacks = 1)
        {
            if (effect == null || addedStacks <= 0 || !health.IsAlive)
            {
                return 0;
            }

            if (
                !activeMarks.TryGetValue(effect, out ActiveMark mark) ||
                Time.time >= mark.ExpiresAt)
            {
                mark = new ActiveMark();
                activeMarks[effect] = mark;
            }

            mark.Stacks = Mathf.Min(
                effect.MaximumStacks,
                mark.Stacks + addedStacks);
            mark.ExpiresAt = Time.time + effect.Duration;
            MarkChanged?.Invoke(effect, mark.Stacks);
            return mark.Stacks;
        }

        public int GetStacks(WeaponMarkEffectSettings effect)
        {
            if (
                effect == null ||
                !activeMarks.TryGetValue(effect, out ActiveMark mark))
            {
                return 0;
            }

            if (Time.time < mark.ExpiresAt)
            {
                return mark.Stacks;
            }

            RemoveMark(effect);
            return 0;
        }

        public int ConsumeMark(WeaponMarkEffectSettings effect)
        {
            int stacks = GetStacks(effect);
            if (stacks > 0)
            {
                RemoveMark(effect);
            }

            return stacks;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health ??= GetComponent<Health>();
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }

            ClearMarks();
        }

        private void Update()
        {
            expiredMarks.Clear();
            foreach (KeyValuePair<WeaponMarkEffectSettings, ActiveMark> pair in
                activeMarks)
            {
                if (pair.Key == null || Time.time >= pair.Value.ExpiresAt)
                {
                    expiredMarks.Add(pair.Key);
                }
            }

            for (int index = 0; index < expiredMarks.Count; index++)
            {
                RemoveMark(expiredMarks[index]);
            }
        }

        private void OnDied(DamageInfo damage)
        {
            ClearMarks();
        }

        private void RemoveMark(WeaponMarkEffectSettings effect)
        {
            if (effect != null && activeMarks.Remove(effect))
            {
                MarkChanged?.Invoke(effect, 0);
            }
        }

        private void ClearMarks()
        {
            if (activeMarks.Count == 0)
            {
                return;
            }

            expiredMarks.Clear();
            expiredMarks.AddRange(activeMarks.Keys);
            activeMarks.Clear();
            for (int index = 0; index < expiredMarks.Count; index++)
            {
                WeaponMarkEffectSettings effect = expiredMarks[index];
                if (effect != null)
                {
                    MarkChanged?.Invoke(effect, 0);
                }
            }
        }
    }
}
