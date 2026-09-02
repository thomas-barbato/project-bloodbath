using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [CreateAssetMenu(
        fileName = "FloatingDamageSettings",
        menuName = "Project Bloodbath/UI/Floating Damage Settings")]
    public sealed class FloatingDamageSettings : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float duration = 0.9f;
        [SerializeField, Min(0f)] private float riseDistance = 0.85f;
        [SerializeField, Min(0f)] private float headPadding = 0.25f;
        [SerializeField, Min(8)] private int fontSize = 30;
        [SerializeField, Min(1)] private int maximumVisibleNumbers = 24;
        [SerializeField] private Color damageColor =
            new(1f, 0.68f, 0.28f, 1f);
        [SerializeField] private Color outlineColor =
            new(0.08f, 0.015f, 0.01f, 0.95f);

        public float Duration => duration;
        public float RiseDistance => riseDistance;
        public float HeadPadding => headPadding;
        public int FontSize => fontSize;
        public int MaximumVisibleNumbers => maximumVisibleNumbers;
        public Color DamageColor => damageColor;
        public Color OutlineColor => outlineColor;

        private void OnValidate()
        {
            duration = Mathf.Max(0.1f, duration);
            riseDistance = Mathf.Max(0f, riseDistance);
            headPadding = Mathf.Max(0f, headPadding);
            fontSize = Mathf.Max(8, fontSize);
            maximumVisibleNumbers = Mathf.Max(1, maximumVisibleNumbers);
        }
    }
}
