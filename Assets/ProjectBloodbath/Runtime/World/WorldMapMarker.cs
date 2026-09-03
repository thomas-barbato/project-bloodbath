using ProjectBloodbath.Combat;
using UnityEngine;

namespace ProjectBloodbath.World
{
    public enum WorldMapMarkerType
    {
        PointOfInterest,
        Quest,
        Hub,
        NonPlayerCharacter,
        Door,
        Loot,
        Hostile
    }

    [DisallowMultipleComponent]
    public sealed class WorldMapMarker : MonoBehaviour
    {
        [SerializeField] private WorldMapMarkerType markerType =
            WorldMapMarkerType.PointOfInterest;
        [SerializeField] private string displayName = "Point d'intérêt";
        [SerializeField] private bool showOnMiniMap = true;
        [SerializeField] private bool showOnWorldMap = true;
        [SerializeField] private Color color =
            new(0.9f, 0.25f, 0.07f, 1f);

        private Health health;

        public WorldMapMarkerType MarkerType => markerType;
        public string DisplayName => displayName;
        public bool ShowOnMiniMap => showOnMiniMap;
        public bool ShowOnWorldMap => showOnWorldMap;
        public Color Color => color;
        public bool IsCurrentlyVisible
        {
            get
            {
                if (markerType != WorldMapMarkerType.Hostile)
                {
                    return isActiveAndEnabled;
                }

                if (health == null)
                {
                    health = GetComponentInParent<Health>();
                }

                return isActiveAndEnabled &&
                    (health == null || health.IsAlive);
            }
        }

        public void Configure(
            WorldMapMarkerType type,
            string markerDisplayName,
            bool visibleOnMiniMap,
            bool visibleOnWorldMap,
            Color markerColor)
        {
            markerType = type;
            displayName = string.IsNullOrWhiteSpace(markerDisplayName)
                ? "Point d'intérêt"
                : markerDisplayName.Trim();
            showOnMiniMap = visibleOnMiniMap;
            showOnWorldMap = visibleOnWorldMap;
            color = markerColor;
        }
    }
}
