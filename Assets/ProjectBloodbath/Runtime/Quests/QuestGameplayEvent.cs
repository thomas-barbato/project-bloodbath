using System;
using UnityEngine;

namespace ProjectBloodbath.Quests
{
    public static class QuestEventIdentifiers
    {
        public const string EnemyKilled = "enemy_killed";
        public const string ItemCollected = "item_collected";
    }

    public readonly struct QuestGameplayEvent
    {
        public QuestGameplayEvent(
            string eventIdentifier,
            string targetIdentifier,
            GameObject source,
            GameObject target,
            int amount = 1)
        {
            EventIdentifier = eventIdentifier;
            TargetIdentifier = targetIdentifier;
            Source = source;
            Target = target;
            Amount = Mathf.Max(1, amount);
        }

        public string EventIdentifier { get; }
        public string TargetIdentifier { get; }
        public GameObject Source { get; }
        public GameObject Target { get; }
        public int Amount { get; }
    }

    public static class QuestGameplayEvents
    {
        public static event Action<QuestGameplayEvent> Raised;

        public static void Publish(QuestGameplayEvent gameplayEvent)
        {
            if (string.IsNullOrWhiteSpace(gameplayEvent.EventIdentifier))
            {
                return;
            }

            Raised?.Invoke(gameplayEvent);
        }
    }
}
