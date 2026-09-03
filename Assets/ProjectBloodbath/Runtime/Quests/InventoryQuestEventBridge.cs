using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Quests
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class InventoryQuestEventBridge : MonoBehaviour
    {
        [SerializeField] private CharacterInventory inventory;

        private void Awake()
        {
            inventory ??= GetComponent<CharacterInventory>();
        }

        private void OnEnable()
        {
            inventory ??= GetComponent<CharacterInventory>();
            if (inventory != null)
            {
                inventory.PickupDefinitionCollected += OnPickupCollected;
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.PickupDefinitionCollected -= OnPickupCollected;
            }
        }

        private void OnPickupCollected(
            WorldPickupDefinition definition,
            int quantity)
        {
            if (definition == null || quantity <= 0)
            {
                return;
            }

            QuestGameplayEvents.Publish(new QuestGameplayEvent(
                QuestEventIdentifiers.ItemCollected,
                definition.Identifier,
                gameObject,
                null,
                quantity));
        }
    }
}
