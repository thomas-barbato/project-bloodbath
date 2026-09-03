using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Quests
{
    public enum QuestCategory
    {
        Main,
        Secondary
    }

    public enum QuestPrerequisiteMode
    {
        All,
        Any
    }

    [Serializable]
    public sealed class QuestObjectiveDefinition
    {
        [SerializeField] private string eventIdentifier =
            QuestEventIdentifiers.EnemyKilled;
        [SerializeField] private string targetIdentifier = string.Empty;
        [SerializeField] private string description = "Objectif";
        [SerializeField, Min(1)] private int requiredAmount = 1;

        public string EventIdentifier => eventIdentifier;
        public string TargetIdentifier => targetIdentifier;
        public string Description => description;
        public int RequiredAmount => requiredAmount;

        public bool Matches(QuestGameplayEvent gameplayEvent)
        {
            return string.Equals(
                    eventIdentifier,
                    gameplayEvent.EventIdentifier,
                    StringComparison.Ordinal) &&
                (string.IsNullOrWhiteSpace(targetIdentifier) ||
                 string.Equals(
                     targetIdentifier,
                     gameplayEvent.TargetIdentifier,
                     StringComparison.Ordinal));
        }

        public void Configure(
            string objectiveEventIdentifier,
            string objectiveTargetIdentifier,
            string objectiveDescription,
            int amount)
        {
            eventIdentifier = objectiveEventIdentifier;
            targetIdentifier = objectiveTargetIdentifier;
            description = objectiveDescription;
            requiredAmount = amount;
            ValidateValues();
        }

        internal void ValidateValues()
        {
            eventIdentifier = string.IsNullOrWhiteSpace(eventIdentifier)
                ? QuestEventIdentifiers.EnemyKilled
                : eventIdentifier.Trim();
            targetIdentifier = targetIdentifier?.Trim() ?? string.Empty;
            description = string.IsNullOrWhiteSpace(description)
                ? "Objectif"
                : description.Trim();
            requiredAmount = Mathf.Max(1, requiredAmount);
        }
    }

    [CreateAssetMenu(
        fileName = "QuestDefinition",
        menuName = "Project Bloodbath/Quests/Quest")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "quest";
        [SerializeField] private string displayName = "Quête";
        [SerializeField] private QuestCategory category;
        [SerializeField, TextArea(2, 5)] private string openingDialogue =
            "Une nouvelle mission vous attend.";
        [SerializeField, TextArea(2, 5)] private string activeDialogue =
            "La mission est toujours en cours.";
        [SerializeField, TextArea(2, 5)] private string readyDialogue =
            "La mission est accomplie.";
        [SerializeField, TextArea(2, 5)] private string completedDialogue =
            "Cette mission a déjà été accomplie.";
        [SerializeField] private QuestObjectiveDefinition[] objectives =
            Array.Empty<QuestObjectiveDefinition>();
        [SerializeField, Min(0)] private int experienceReward;
        [SerializeField] private QuestDefinition[] prerequisiteQuests =
            Array.Empty<QuestDefinition>();
        [SerializeField] private QuestPrerequisiteMode prerequisiteMode =
            QuestPrerequisiteMode.All;

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public QuestCategory Category => category;
        public string OpeningDialogue => openingDialogue;
        public string ActiveDialogue => activeDialogue;
        public string ReadyDialogue => readyDialogue;
        public string CompletedDialogue => completedDialogue;
        public IReadOnlyList<QuestObjectiveDefinition> Objectives => objectives;
        public int ExperienceReward => experienceReward;
        public IReadOnlyList<QuestDefinition> PrerequisiteQuests =>
            prerequisiteQuests;
        public QuestPrerequisiteMode PrerequisiteMode => prerequisiteMode;

        public void Configure(
            string questIdentifier,
            string questDisplayName,
            QuestCategory questCategory,
            string openingText,
            string activeText,
            string readyText,
            string completedText,
            QuestObjectiveDefinition[] questObjectives,
            int rewardExperience,
            QuestDefinition[] prerequisites = null,
            QuestPrerequisiteMode requiredQuestMode =
                QuestPrerequisiteMode.All)
        {
            identifier = questIdentifier;
            displayName = questDisplayName;
            category = questCategory;
            openingDialogue = openingText;
            activeDialogue = activeText;
            readyDialogue = readyText;
            completedDialogue = completedText;
            objectives = questObjectives;
            experienceReward = rewardExperience;
            prerequisiteQuests = prerequisites;
            prerequisiteMode = requiredQuestMode;
            ValidateValues();
        }

        private void OnValidate()
        {
            ValidateValues();
        }

        private void ValidateValues()
        {
            identifier = string.IsNullOrWhiteSpace(identifier)
                ? "quest"
                : identifier.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Quête"
                : displayName.Trim();
            openingDialogue = openingDialogue?.Trim() ?? string.Empty;
            activeDialogue = activeDialogue?.Trim() ?? string.Empty;
            readyDialogue = readyDialogue?.Trim() ?? string.Empty;
            completedDialogue = completedDialogue?.Trim() ?? string.Empty;
            objectives ??= Array.Empty<QuestObjectiveDefinition>();
            prerequisiteQuests ??= Array.Empty<QuestDefinition>();
            for (int index = 0; index < objectives.Length; index++)
            {
                objectives[index]?.ValidateValues();
            }

            experienceReward = Mathf.Max(0, experienceReward);
        }
    }
}
