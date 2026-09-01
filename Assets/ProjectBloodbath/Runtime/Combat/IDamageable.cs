namespace ProjectBloodbath.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void ApplyDamage(DamageInfo damage);
    }
}
