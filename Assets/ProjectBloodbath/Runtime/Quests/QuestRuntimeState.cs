using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBloodbath.Quests
{
    public enum QuestStatus
    {
        NotStarted,
        Active,
        ReadyToTurnIn,
        Completed
    }

    [Serializable]
    public sealed class QuestRuntimeState
    {
        [SerializeField] private QuestDefinition definition;
        [SerializeField] private QuestStatus status;
        [SerializeField] private List<int> objectiveProgress = new();

        public QuestRuntimeState(QuestDefinition questDefinition)
        {
            definition = questDefinition;
            status = QuestStatus.NotStarted;
            EnsureProgressSlots();
        }

        public QuestDefinition Definition => definition;
        public QuestStatus Status => status;

        public int GetObjectiveProgress(int objectiveIndex)
        {
            EnsureProgressSlots();
            return objectiveIndex >= 0 && objectiveIndex < objectiveProgress.Count
                ? objectiveProgress[objectiveIndex]
                : 0;
        }

        internal void Start()
        {
            EnsureProgressSlots();
            for (int index = 0; index < objectiveProgress.Count; index++)
            {
                objectiveProgress[index] = 0;
            }

            status = definition != null && definition.Objectives.Count == 0
                ? QuestStatus.ReadyToTurnIn
                : QuestStatus.Active;
        }

        internal bool Apply(
            QuestGameplayEvent gameplayEvent,
            out int changedObjectiveIndex,
            out bool becameReady)
        {
            changedObjectiveIndex = -1;
            becameReady = false;
            if (status != QuestStatus.Active || definition == null)
            {
                return false;
            }

            EnsureProgressSlots();
            for (int index = 0; index < definition.Objectives.Count; index++)
            {
                QuestObjectiveDefinition objective = definition.Objectives[index];
                if (objective == null || !objective.Matches(gameplayEvent))
                {
                    continue;
                }

                int previous = objectiveProgress[index];
                objectiveProgress[index] = Mathf.Min(
                    objective.RequiredAmount,
                    previous + gameplayEvent.Amount);
                if (objectiveProgress[index] == previous)
                {
                    continue;
                }

                changedObjectiveIndex = index;
                if (AllObjectivesComplete())
                {
                    status = QuestStatus.ReadyToTurnIn;
                    becameReady = true;
                }

                return true;
            }

            return false;
        }

        internal void Complete()
        {
            if (status == QuestStatus.ReadyToTurnIn)
            {
                status = QuestStatus.Completed;
            }
        }

        private bool AllObjectivesComplete()
        {
            if (definition == null)
            {
                return false;
            }

            for (int index = 0; index < definition.Objectives.Count; index++)
            {
                QuestObjectiveDefinition objective = definition.Objectives[index];
                if (
                    objective != null &&
                    objectiveProgress[index] < objective.RequiredAmount)
                {
                    return false;
                }
            }

            return true;
        }

        private void EnsureProgressSlots()
        {
            objectiveProgress ??= new List<int>();
            int requiredCount = definition?.Objectives.Count ?? 0;
            while (objectiveProgress.Count < requiredCount)
            {
                objectiveProgress.Add(0);
            }

            if (objectiveProgress.Count > requiredCount)
            {
                objectiveProgress.RemoveRange(
                    requiredCount,
                    objectiveProgress.Count - requiredCount);
            }
        }
    }
}
