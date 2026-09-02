using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "CharacterStatDefinition",
        menuName = "Project Bloodbath/Progression/Character Stat")]
    public sealed class CharacterStatDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "stat";
        [SerializeField] private string displayName = "Statistique";

        public string Identifier => identifier;
        public string DisplayName => displayName;

        public void Configure(string statIdentifier, string statDisplayName)
        {
            identifier = string.IsNullOrWhiteSpace(statIdentifier)
                ? "stat"
                : statIdentifier.Trim();
            displayName = string.IsNullOrWhiteSpace(statDisplayName)
                ? "Statistique"
                : statDisplayName.Trim();
        }

        private void OnValidate()
        {
            Configure(identifier, displayName);
        }
    }
}
