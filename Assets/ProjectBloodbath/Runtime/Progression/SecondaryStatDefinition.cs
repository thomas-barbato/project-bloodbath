using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "SecondaryStatDefinition",
        menuName = "Project Bloodbath/Progression/Secondary Stat")]
    public sealed class SecondaryStatDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "secondary_stat";
        [SerializeField] private string displayName = "Statistique secondaire";
        [SerializeField] private float baseValue;
        [SerializeField] private float minimumValue;
        [SerializeField] private float maximumValue = 9999f;

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public float BaseValue => baseValue;
        public float MinimumValue => minimumValue;
        public float MaximumValue => maximumValue;

        public void Configure(
            string statIdentifier,
            string statDisplayName,
            float initialValue,
            float minimum,
            float maximum)
        {
            identifier = string.IsNullOrWhiteSpace(statIdentifier)
                ? "secondary_stat"
                : statIdentifier.Trim();
            displayName = string.IsNullOrWhiteSpace(statDisplayName)
                ? "Statistique secondaire"
                : statDisplayName.Trim();
            minimumValue = Mathf.Min(minimum, maximum);
            maximumValue = Mathf.Max(minimum, maximum);
            baseValue = Clamp(initialValue);
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, minimumValue, maximumValue);
        }

        private void OnValidate()
        {
            Configure(
                identifier,
                displayName,
                baseValue,
                minimumValue,
                maximumValue);
        }
    }
}
