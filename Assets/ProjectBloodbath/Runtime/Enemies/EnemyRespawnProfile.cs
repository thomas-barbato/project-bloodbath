using UnityEngine;

namespace ProjectBloodbath.Enemies
{
    public enum EnemyRespawnMode
    {
        NeverDuringSession,
        Timed
    }

    [CreateAssetMenu(
        fileName = "EnemyRespawnProfile",
        menuName = "Project Bloodbath/Enemies/Respawn Profile")]
    public sealed class EnemyRespawnProfile : ScriptableObject
    {
        [SerializeField] private EnemyRespawnMode mode =
            EnemyRespawnMode.NeverDuringSession;
        [SerializeField, Min(0.1f)] private float delay = 2.5f;

        public EnemyRespawnMode Mode => mode;
        public float Delay => delay;
        public bool RespawnsDuringSession => mode == EnemyRespawnMode.Timed;

        public void Configure(EnemyRespawnMode respawnMode, float respawnDelay)
        {
            mode = respawnMode;
            delay = Mathf.Max(0.1f, respawnDelay);
        }

        private void OnValidate()
        {
            delay = Mathf.Max(0.1f, delay);
        }
    }
}
