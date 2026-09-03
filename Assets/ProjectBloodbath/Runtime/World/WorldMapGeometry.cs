using UnityEngine;

namespace ProjectBloodbath.World
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class WorldMapGeometry : MonoBehaviour
    {
        [SerializeField] private Collider sourceCollider;
        [SerializeField] private bool showOnMiniMap = true;
        [SerializeField] private bool showOnWorldMap = true;
        [SerializeField] private Color color =
            new(0.28f, 0.24f, 0.2f, 0.95f);

        public bool ShowOnMiniMap => showOnMiniMap;
        public bool ShowOnWorldMap => showOnWorldMap;
        public Color Color => color;
        public Bounds WorldBounds
        {
            get
            {
                if (sourceCollider == null)
                {
                    sourceCollider = GetComponent<Collider>();
                }

                return sourceCollider == null
                    ? new Bounds(transform.position, Vector3.zero)
                    : sourceCollider.bounds;
            }
        }

        private void Awake()
        {
            if (sourceCollider == null)
            {
                sourceCollider = GetComponent<Collider>();
            }
        }

        public void Configure(
            bool visibleOnMiniMap,
            bool visibleOnWorldMap,
            Color geometryColor)
        {
            sourceCollider = GetComponent<Collider>();
            showOnMiniMap = visibleOnMiniMap;
            showOnWorldMap = visibleOnWorldMap;
            color = geometryColor;
        }
    }
}
