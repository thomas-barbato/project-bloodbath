using System;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    public enum SecondaryStatModifierOperation
    {
        Flat,
        AdditivePercent,
        MultiplicativePercent
    }

    [Serializable]
    public sealed class SecondaryStatModifier
    {
        [SerializeField] private SecondaryStatDefinition statistic;
        [SerializeField] private SecondaryStatModifierOperation operation;
        [SerializeField] private float value;

        public SecondaryStatDefinition Statistic => statistic;
        public SecondaryStatModifierOperation Operation => operation;
        public float Value => value;

        public SecondaryStatModifier(
            SecondaryStatDefinition modifiedStatistic,
            SecondaryStatModifierOperation modifierOperation,
            float modifierValue)
        {
            statistic = modifiedStatistic;
            operation = modifierOperation;
            value = modifierValue;
        }
    }
}
