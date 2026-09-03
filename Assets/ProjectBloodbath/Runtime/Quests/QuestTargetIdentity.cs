using UnityEngine;

namespace ProjectBloodbath.Quests
{
    [DisallowMultipleComponent]
    public sealed class QuestTargetIdentity : MonoBehaviour
    {
        [SerializeField] private string identifier = "quest_target";

        public string Identifier => identifier;

        public void Configure(string targetIdentifier)
        {
            identifier = string.IsNullOrWhiteSpace(targetIdentifier)
                ? "quest_target"
                : targetIdentifier.Trim();
        }

        private void OnValidate()
        {
            identifier = string.IsNullOrWhiteSpace(identifier)
                ? "quest_target"
                : identifier.Trim();
        }
    }
}
