using System;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    public sealed class CharacterProgression : MonoBehaviour
    {
        [SerializeField] private CharacterProgressionSettings settings;
        [SerializeField, Min(1)] private int currentLevel = 1;
        [SerializeField, Min(0)] private int currentExperience;

        public event Action<int> LevelChanged;
        public event Action<int, int> ExperienceChanged;

        public CharacterProgressionSettings Settings => settings;
        public int CurrentLevel => currentLevel;
        public int CurrentExperience => currentExperience;
        public int ExperienceRequiredForNextLevel => settings == null
            ? 0
            : settings.GetExperienceRequiredForLevel(currentLevel);
        public bool IsAtMaximumLevel =>
            settings != null && currentLevel >= settings.MaximumLevel;
        public float ExperienceRatio
        {
            get
            {
                int requirement = ExperienceRequiredForNextLevel;
                return requirement <= 0
                    ? 1f
                    : Mathf.Clamp01((float)currentExperience / requirement);
            }
        }

        public void Configure(
            CharacterProgressionSettings progressionSettings,
            int level = 1,
            int experience = 0)
        {
            settings = progressionSettings;
            currentLevel = Mathf.Max(1, level);
            currentExperience = Mathf.Max(0, experience);
            NormalizeProgression();
        }

        public int AddExperience(int amount)
        {
            if (settings == null || amount <= 0 || IsAtMaximumLevel)
            {
                return 0;
            }

            currentExperience += amount;
            int gainedLevels = 0;
            int requirement = ExperienceRequiredForNextLevel;
            while (
                requirement > 0 &&
                currentExperience >= requirement &&
                !IsAtMaximumLevel)
            {
                currentExperience -= requirement;
                currentLevel++;
                gainedLevels++;
                LevelChanged?.Invoke(currentLevel);
                requirement = ExperienceRequiredForNextLevel;
            }

            if (IsAtMaximumLevel)
            {
                currentExperience = 0;
            }

            ExperienceChanged?.Invoke(
                currentExperience,
                ExperienceRequiredForNextLevel);
            return gainedLevels;
        }

        private void Awake()
        {
            NormalizeProgression();
        }

        private void OnValidate()
        {
            NormalizeProgression();
        }

        private void NormalizeProgression()
        {
            currentLevel = Mathf.Max(1, currentLevel);
            currentExperience = Mathf.Max(0, currentExperience);
            if (settings == null)
            {
                return;
            }

            currentLevel = Mathf.Min(currentLevel, settings.MaximumLevel);
            int requirement = ExperienceRequiredForNextLevel;
            while (
                requirement > 0 &&
                currentExperience >= requirement &&
                currentLevel < settings.MaximumLevel)
            {
                currentExperience -= requirement;
                currentLevel++;
                requirement = ExperienceRequiredForNextLevel;
            }

            if (currentLevel >= settings.MaximumLevel)
            {
                currentExperience = 0;
            }
        }
    }
}
