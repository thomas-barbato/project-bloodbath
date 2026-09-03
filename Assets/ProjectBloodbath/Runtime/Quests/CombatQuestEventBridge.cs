using ProjectBloodbath.Combat;
using UnityEngine;

namespace ProjectBloodbath.Quests
{
    [DisallowMultipleComponent]
    public sealed class CombatQuestEventBridge : MonoBehaviour
    {
        private void OnEnable()
        {
            CombatEvents.CombatantDied += OnCombatantDied;
        }

        private void OnDisable()
        {
            CombatEvents.CombatantDied -= OnCombatantDied;
        }

        private void OnCombatantDied(CombatDeathEvent death)
        {
            GameObject source = death.FinishingBlow.Source;
            if (
                source == null ||
                source.transform.root != transform.root ||
                death.Target == null ||
                death.Target.transform.root == transform.root)
            {
                return;
            }

            QuestTargetIdentity identity = death.Target
                .GetComponentInParent<QuestTargetIdentity>();
            if (identity == null)
            {
                return;
            }

            QuestGameplayEvents.Publish(new QuestGameplayEvent(
                QuestEventIdentifiers.EnemyKilled,
                identity.Identifier,
                source,
                death.Target));
        }
    }
}
