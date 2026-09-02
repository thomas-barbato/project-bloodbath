using ProjectBloodbath.Combat;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class PrototypeEnemyLootDropper : MonoBehaviour
    {
        [SerializeField] private EnemyLootProfile lootProfile;
        [SerializeField] private Transform dropOrigin;
        [SerializeField, Min(0f)] private float scatterRadius = 0.35f;
        [SerializeField, Min(0f)] private float verticalOffset = 0.2f;

        private Health health;

        public EnemyLootProfile LootProfile => lootProfile;
        public WorldPickup LastSpawnedPickup { get; private set; }
        public int LastDropCount { get; private set; }
        public int TotalDropCount { get; private set; }

        public void Configure(EnemyLootProfile profile)
        {
            lootProfile = profile;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health ??= GetComponent<Health>();
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        private void OnDied(DamageInfo finishingBlow)
        {
            SpawnLoot();
        }

        private void SpawnLoot()
        {
            LastDropCount = 0;
            if (lootProfile == null || lootProfile.Entries == null)
            {
                return;
            }

            Vector3 origin = dropOrigin == null
                ? transform.position
                : dropOrigin.position;

            foreach (EnemyLootEntry entry in lootProfile.Entries)
            {
                if (entry == null || !entry.RollDrop())
                {
                    continue;
                }

                Vector2 scatter = UnityEngine.Random.insideUnitCircle *
                    scatterRadius;
                Vector3 position = origin +
                    new Vector3(scatter.x, verticalOffset, scatter.y);
                WorldPickup pickup = Instantiate(
                    entry.PickupPrefab,
                    position,
                    Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
                pickup.name = $"{entry.PickupPrefab.name}_Drop";
                pickup.Configure(pickup.Definition, entry.RollQuantity());
                pickup.gameObject.SetActive(true);

                LastSpawnedPickup = pickup;
                LastDropCount++;
                TotalDropCount++;
            }
        }
    }
}
