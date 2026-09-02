using System;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [Serializable]
    public sealed class EnemyLootEntry
    {
        [SerializeField] private WorldPickup pickupPrefab;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
        [SerializeField, Min(1)] private int minimumQuantity = 1;
        [SerializeField, Min(1)] private int maximumQuantity = 1;

        public WorldPickup PickupPrefab => pickupPrefab;
        public float DropChance => dropChance;
        public int MinimumQuantity => minimumQuantity;
        public int MaximumQuantity => maximumQuantity;

        public void Configure(
            WorldPickup prefab,
            float chance,
            int minimum,
            int maximum)
        {
            pickupPrefab = prefab;
            dropChance = Mathf.Clamp01(chance);
            minimumQuantity = Mathf.Max(1, minimum);
            maximumQuantity = Mathf.Max(minimumQuantity, maximum);
        }

        public bool RollDrop()
        {
            return pickupPrefab != null &&
                (dropChance >= 1f || UnityEngine.Random.value <= dropChance);
        }

        public int RollQuantity()
        {
            return UnityEngine.Random.Range(
                minimumQuantity,
                maximumQuantity + 1);
        }
    }

    [CreateAssetMenu(
        fileName = "EnemyLootProfile",
        menuName = "Project Bloodbath/Progression/Enemy Loot Profile")]
    public sealed class EnemyLootProfile : ScriptableObject
    {
        [SerializeField] private EnemyLootEntry[] entries =
            Array.Empty<EnemyLootEntry>();

        public EnemyLootEntry[] Entries => entries;

        public void Configure(params EnemyLootEntry[] configuredEntries)
        {
            entries = configuredEntries ?? Array.Empty<EnemyLootEntry>();
        }
    }
}
