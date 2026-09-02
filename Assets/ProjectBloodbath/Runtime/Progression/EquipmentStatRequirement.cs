using System;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [Serializable]
    public sealed class EquipmentStatRequirement
    {
        [SerializeField] private CharacterStatDefinition statistic;
        [SerializeField, Min(0)] private int minimumValue;

        public CharacterStatDefinition Statistic => statistic;
        public int MinimumValue => minimumValue;

        public EquipmentStatRequirement(
            CharacterStatDefinition requiredStatistic,
            int requiredValue)
        {
            statistic = requiredStatistic;
            minimumValue = Mathf.Max(0, requiredValue);
        }

        public bool IsMetBy(CharacterStatistics statistics)
        {
            return
                statistic != null &&
                statistics != null &&
                statistics.GetValue(statistic) >= minimumValue;
        }
    }
}
