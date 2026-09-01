using UnityEngine;

namespace ProjectBloodbath.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            float amount,
            DamageType damageType,
            Vector3 point,
            Vector3 normal,
            Vector3 direction,
            float force,
            GameObject source)
        {
            Amount = amount;
            DamageType = damageType;
            Point = point;
            Normal = normal;
            Direction = direction;
            Force = force;
            Source = source;
        }

        public float Amount { get; }
        public DamageType DamageType { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public Vector3 Direction { get; }
        public float Force { get; }
        public GameObject Source { get; }
    }
}
