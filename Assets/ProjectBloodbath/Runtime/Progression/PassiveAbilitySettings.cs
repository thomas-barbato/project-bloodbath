using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [CreateAssetMenu(
        fileName = "PassiveAbilitySettings",
        menuName = "Project Bloodbath/Progression/Passive Ability Settings")]
    public sealed class PassiveAbilitySettings : ScriptableObject
    {
        [SerializeField] private string displayName = "Moisson sanglante";
        [SerializeField, Min(0f)] private float resourceRestoredPerKill = 20f;

        public string DisplayName => displayName;
        public float ResourceRestoredPerKill => resourceRestoredPerKill;

        private void OnValidate()
        {
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Compétence passive"
                : displayName.Trim();
            resourceRestoredPerKill = Mathf.Max(
                0f,
                resourceRestoredPerKill);
        }
    }
}
