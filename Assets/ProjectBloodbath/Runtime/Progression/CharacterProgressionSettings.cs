using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "CharacterProgressionSettings",
        menuName = "Project Bloodbath/Progression/Character Progression")]
    public sealed class CharacterProgressionSettings : ScriptableObject
    {
        [SerializeField, Min(1)] private int maximumLevel = 50;
        [SerializeField, Min(1)] private int baseExperienceRequired = 100;
        [SerializeField, Min(1f)] private float growthMultiplier = 1.25f;
        [SerializeField, Min(0)] private int attributePointsPerLevel = 5;

        public int MaximumLevel => maximumLevel;
        public int BaseExperienceRequired => baseExperienceRequired;
        public float GrowthMultiplier => growthMultiplier;
        public int AttributePointsPerLevel => attributePointsPerLevel;

        public int GetExperienceRequiredForLevel(int level)
        {
            if (level >= maximumLevel)
            {
                return 0;
            }

            float scaledRequirement = baseExperienceRequired * Mathf.Pow(
                growthMultiplier,
                Mathf.Max(0, level - 1));
            return Mathf.Max(1, Mathf.RoundToInt(scaledRequirement));
        }

        public void Configure(
            int levelCap,
            int firstLevelRequirement,
            float requirementGrowth,
            int pointsPerLevel = 5)
        {
            maximumLevel = Mathf.Max(1, levelCap);
            baseExperienceRequired = Mathf.Max(1, firstLevelRequirement);
            growthMultiplier = Mathf.Max(1f, requirementGrowth);
            attributePointsPerLevel = Mathf.Max(0, pointsPerLevel);
        }

        private void OnValidate()
        {
            Configure(
                maximumLevel,
                baseExperienceRequired,
                growthMultiplier,
                attributePointsPerLevel);
        }
    }
}
