using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "InventoryResourceDefinition",
        menuName = "Project Bloodbath/Progression/Inventory Resource")]
    public sealed class InventoryResourceDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "resource";
        [SerializeField] private string displayName = "Ressource";
        [SerializeField, Min(1)] private int maximumCarried = 100;

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public int MaximumCarried => maximumCarried;

        public void Configure(
            string resourceIdentifier,
            string resourceDisplayName,
            int maximum)
        {
            identifier = resourceIdentifier;
            displayName = resourceDisplayName;
            maximumCarried = maximum;
            ValidateValues();
        }

        private void OnValidate()
        {
            ValidateValues();
        }

        private void ValidateValues()
        {
            identifier = string.IsNullOrWhiteSpace(identifier)
                ? "resource"
                : identifier.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Ressource"
                : displayName.Trim();
            maximumCarried = Mathf.Max(1, maximumCarried);
        }
    }
}
