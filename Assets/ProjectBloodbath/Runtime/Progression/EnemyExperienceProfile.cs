using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "EnemyExperienceProfile",
        menuName = "Project Bloodbath/Progression/Enemy Experience")]
    public sealed class EnemyExperienceProfile : ScriptableObject
    {
        [SerializeField] private string identifier = "enemy";
        [SerializeField, Min(0)] private int experienceReward = 10;

        public string Identifier => identifier;
        public int ExperienceReward => experienceReward;

        public void Configure(string profileIdentifier, int reward)
        {
            identifier = string.IsNullOrWhiteSpace(profileIdentifier)
                ? "enemy"
                : profileIdentifier.Trim();
            experienceReward = Mathf.Max(0, reward);
        }

        private void OnValidate()
        {
            Configure(identifier, experienceReward);
        }
    }
}
