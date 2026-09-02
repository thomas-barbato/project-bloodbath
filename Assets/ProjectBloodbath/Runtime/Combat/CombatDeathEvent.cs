using System;
using UnityEngine;

namespace ProjectBloodbath.Combat
{
    public readonly struct CombatDeathEvent
    {
        public CombatDeathEvent(
            GameObject target,
            DamageInfo finishingBlow)
        {
            Target = target;
            FinishingBlow = finishingBlow;
        }

        public GameObject Target { get; }
        public DamageInfo FinishingBlow { get; }
    }

    public static class CombatEvents
    {
        public static event Action<CombatDamageEvent> CombatantDamaged;
        public static event Action<CombatDeathEvent> CombatantDied;

        internal static void PublishCombatantDamaged(
            GameObject target,
            DamageInfo damage,
            float previousHealth,
            float currentHealth)
        {
            CombatantDamaged?.Invoke(new CombatDamageEvent(
                target,
                damage,
                previousHealth,
                currentHealth));
        }

        internal static void PublishCombatantDied(
            GameObject target,
            DamageInfo finishingBlow)
        {
            CombatantDied?.Invoke(new CombatDeathEvent(
                target,
                finishingBlow));
        }
    }
}
