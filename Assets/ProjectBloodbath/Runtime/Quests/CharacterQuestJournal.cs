using System;
using System.Collections.Generic;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Quests
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterProgression))]
    public sealed class CharacterQuestJournal : MonoBehaviour
    {
        [SerializeField] private CharacterProgression characterProgression;
        [SerializeField] private List<QuestRuntimeState> questStates = new();
        [SerializeField] private QuestDefinition trackedQuestDefinition;

        public event Action<QuestRuntimeState> QuestStarted;
        public event Action<QuestRuntimeState, int> ObjectiveProgressChanged;
        public event Action<QuestRuntimeState> QuestReadyToTurnIn;
        public event Action<QuestRuntimeState> QuestCompleted;
        public event Action<QuestRuntimeState> TrackedQuestChanged;

        public IReadOnlyList<QuestRuntimeState> QuestStates => questStates;
        public QuestRuntimeState TrackedQuest
        {
            get
            {
                QuestRuntimeState state = FindState(trackedQuestDefinition);
                return IsTrackable(state) ? state : null;
            }
        }
        public int LastGrantedExperience { get; private set; }

        public QuestStatus GetStatus(QuestDefinition definition)
        {
            return FindState(definition)?.Status ?? QuestStatus.NotStarted;
        }

        public QuestRuntimeState GetState(QuestDefinition definition)
        {
            return FindState(definition);
        }

        public bool CanStartQuest(QuestDefinition definition)
        {
            return definition != null &&
                FindState(definition) == null &&
                ArePrerequisitesMet(definition);
        }

        public bool ArePrerequisitesMet(QuestDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            int validPrerequisiteCount = 0;
            int completedPrerequisiteCount = 0;
            for (
                int index = 0;
                index < definition.PrerequisiteQuests.Count;
                index++)
            {
                QuestDefinition prerequisite =
                    definition.PrerequisiteQuests[index];
                if (prerequisite == null)
                {
                    continue;
                }

                validPrerequisiteCount++;
                if (GetStatus(prerequisite) == QuestStatus.Completed)
                {
                    completedPrerequisiteCount++;
                }
            }

            if (validPrerequisiteCount == 0)
            {
                return true;
            }

            return definition.PrerequisiteMode == QuestPrerequisiteMode.All
                ? completedPrerequisiteCount == validPrerequisiteCount
                : completedPrerequisiteCount > 0;
        }

        public bool TryStartQuest(QuestDefinition definition)
        {
            if (!CanStartQuest(definition))
            {
                return false;
            }

            QuestRuntimeState state = new(definition);
            questStates.Add(state);
            state.Start();
            if (TrackedQuest == null)
            {
                SetTrackedQuest(state);
            }

            QuestStarted?.Invoke(state);
            if (state.Status == QuestStatus.ReadyToTurnIn)
            {
                QuestReadyToTurnIn?.Invoke(state);
            }

            return true;
        }

        public bool TryTurnInQuest(QuestDefinition definition)
        {
            QuestRuntimeState state = FindState(definition);
            LastGrantedExperience = 0;
            if (state?.Status != QuestStatus.ReadyToTurnIn)
            {
                return false;
            }

            state.Complete();
            if (characterProgression == null)
            {
                characterProgression = GetComponent<CharacterProgression>();
            }

            if (
                characterProgression != null &&
                definition.ExperienceReward > 0)
            {
                int experienceBefore = characterProgression.CurrentExperience;
                int levelBefore = characterProgression.CurrentLevel;
                characterProgression.AddExperience(definition.ExperienceReward);
                LastGrantedExperience = levelBefore == characterProgression.CurrentLevel
                    ? characterProgression.CurrentExperience - experienceBefore
                    : definition.ExperienceReward;
            }

            QuestCompleted?.Invoke(state);
            if (trackedQuestDefinition == definition)
            {
                SetTrackedQuest(FindFirstTrackableState());
            }

            return true;
        }

        public bool TryTrackQuest(QuestDefinition definition)
        {
            QuestRuntimeState state = FindState(definition);
            if (!IsTrackable(state))
            {
                return false;
            }

            SetTrackedQuest(state);
            return true;
        }

        public bool IsQuestTracked(QuestDefinition definition)
        {
            return definition != null &&
                TrackedQuest?.Definition == definition;
        }

        private void Awake()
        {
            if (characterProgression == null)
            {
                characterProgression = GetComponent<CharacterProgression>();
            }

            questStates ??= new List<QuestRuntimeState>();
        }

        private void OnEnable()
        {
            QuestGameplayEvents.Raised += OnGameplayEvent;
        }

        private void OnDisable()
        {
            QuestGameplayEvents.Raised -= OnGameplayEvent;
        }

        private void OnGameplayEvent(QuestGameplayEvent gameplayEvent)
        {
            if (
                gameplayEvent.Source == null ||
                gameplayEvent.Source.transform.root != transform.root)
            {
                return;
            }

            for (int index = 0; index < questStates.Count; index++)
            {
                QuestRuntimeState state = questStates[index];
                if (
                    state == null ||
                    !state.Apply(
                        gameplayEvent,
                        out int objectiveIndex,
                        out bool becameReady))
                {
                    continue;
                }

                ObjectiveProgressChanged?.Invoke(state, objectiveIndex);
                if (becameReady)
                {
                    QuestReadyToTurnIn?.Invoke(state);
                }
            }
        }

        private QuestRuntimeState FindState(QuestDefinition definition)
        {
            if (definition == null || questStates == null)
            {
                return null;
            }

            for (int index = 0; index < questStates.Count; index++)
            {
                QuestRuntimeState state = questStates[index];
                if (state?.Definition == definition)
                {
                    return state;
                }
            }

            return null;
        }

        private QuestRuntimeState FindFirstTrackableState()
        {
            QuestRuntimeState readyState = null;
            for (int index = 0; index < questStates.Count; index++)
            {
                QuestRuntimeState state = questStates[index];
                if (state?.Status == QuestStatus.Active)
                {
                    return state;
                }

                if (state?.Status == QuestStatus.ReadyToTurnIn)
                {
                    readyState ??= state;
                }
            }

            return readyState;
        }

        private void SetTrackedQuest(QuestRuntimeState state)
        {
            QuestDefinition nextDefinition = IsTrackable(state)
                ? state.Definition
                : null;
            if (trackedQuestDefinition == nextDefinition)
            {
                return;
            }

            trackedQuestDefinition = nextDefinition;
            TrackedQuestChanged?.Invoke(state);
        }

        private static bool IsTrackable(QuestRuntimeState state)
        {
            return state?.Definition != null &&
                (state.Status == QuestStatus.Active ||
                 state.Status == QuestStatus.ReadyToTurnIn);
        }
    }
}
