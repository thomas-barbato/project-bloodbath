using System.Collections.Generic;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader), typeof(CharacterInventory))]
    public sealed class PrototypeLootInteraction : MonoBehaviour
    {
        private const int CandidateBufferSize = 32;
        private const int SightBufferSize = 24;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private CharacterInventory inventory;
        [SerializeField] private Camera aimCamera;
        [SerializeField, Min(0.5f)] private float identificationRange = 8f;
        [SerializeField, Min(0.5f)] private float manualPickupRange = 3.25f;
        [SerializeField, Range(0.01f, 0.5f)]
        private float aimAssistScreenRadius = 0.16f;
        [SerializeField, Min(0.5f)] private float nearbyGroundAssistRange = 1.75f;
        [SerializeField, Range(-1f, 0f)] private float groundLookThreshold = -0.12f;
        [SerializeField, Min(0.1f)] private float notificationDuration = 2f;

        private readonly Collider[] candidateBuffer =
            new Collider[CandidateBufferSize];
        private readonly RaycastHit[] sightBuffer =
            new RaycastHit[SightBufferSize];
        private readonly HashSet<WorldPickup> evaluatedPickups = new();
        private GUIStyle hoverStyle;
        private GUIStyle notificationStyle;
        private float notificationUntil;

        public WorldPickup HoveredPickup { get; private set; }
        public string HoverLabel => HoveredPickup == null
            ? string.Empty
            : HoveredPickup.PickupMode == WorldPickupMode.Manual
                ? CanCollectHoveredPickup
                    ? $"{HoveredPickup.DisplayName}  •  [INTERAGIR]"
                    : $"{HoveredPickup.DisplayName}  •  APPROCHEZ-VOUS"
                : HoveredPickup.DisplayName;
        public float HoveredPickupDistance { get; private set; }
        public bool CanCollectHoveredPickup =>
            HoveredPickup != null &&
            HoveredPickupDistance <= manualPickupRange;
        public string LastNotificationText { get; private set; }
        public bool NotificationVisible =>
            !string.IsNullOrEmpty(LastNotificationText) &&
            Time.time < notificationUntil;

        public void Configure(
            PlayerInputReader reader,
            CharacterInventory characterInventory,
            Camera cameraComponent)
        {
            inputReader = reader;
            inventory = characterInventory;
            aimCamera = cameraComponent;
        }

        public void RefreshHoveredPickup()
        {
            HoveredPickup = null;
            HoveredPickupDistance = 0f;
            if (aimCamera == null)
            {
                return;
            }

            Vector3 origin = aimCamera.transform.position;
            int candidateCount = Physics.OverlapSphereNonAlloc(
                origin,
                identificationRange,
                candidateBuffer,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);
            evaluatedPickups.Clear();

            Vector2 pointerPosition = new(
                Screen.width * 0.5f,
                Screen.height * 0.5f);
            float screenReference = Mathf.Max(
                1f,
                Mathf.Min(Screen.width, Screen.height));
            float bestScore = float.PositiveInfinity;

            for (int index = 0; index < candidateCount; index++)
            {
                Collider candidate = candidateBuffer[index];
                WorldPickup pickup = candidate == null
                    ? null
                    : candidate.GetComponentInParent<WorldPickup>();
                if (
                    pickup == null ||
                    !pickup.gameObject.activeInHierarchy ||
                    !evaluatedPickups.Add(pickup))
                {
                    continue;
                }

                Vector3 targetPoint = candidate.bounds.center;
                Vector3 screenPoint = aimCamera.WorldToScreenPoint(targetPoint);
                if (screenPoint.z <= 0f)
                {
                    continue;
                }

                float screenDistance = Vector2.Distance(
                    pointerPosition,
                    new Vector2(screenPoint.x, screenPoint.y)) /
                    screenReference;
                Vector3 toPickup = targetPoint - origin;
                float worldDistance = toPickup.magnitude;
                bool usesNearbyGroundAssist = IsNearbyGroundCandidate(
                    targetPoint,
                    toPickup);
                if (
                    (screenDistance > aimAssistScreenRadius &&
                     !usesNearbyGroundAssist) ||
                    worldDistance <= 0.001f ||
                    !HasLineOfSight(origin, toPickup, pickup))
                {
                    continue;
                }

                float assistedScreenDistance = usesNearbyGroundAssist
                    ? Mathf.Min(screenDistance, aimAssistScreenRadius)
                    : screenDistance;
                float score =
                    assistedScreenDistance +
                    worldDistance / identificationRange * 0.04f;
                if (score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                HoveredPickup = pickup;
                HoveredPickupDistance = worldDistance;
            }
        }

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (inventory == null)
            {
                inventory = GetComponent<CharacterInventory>();
            }

            if (aimCamera == null)
            {
                aimCamera = GetComponentInChildren<Camera>(true);
            }
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.PickupCollected += OnPickupCollected;
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.PickupCollected -= OnPickupCollected;
            }

            HoveredPickup = null;
        }

        private void Update()
        {
            bool interactPressed = inputReader != null &&
                inputReader.ConsumeInteractPressed();
            RefreshHoveredPickup();
            if (
                HoveredPickup != null &&
                HoveredPickup.PickupMode == WorldPickupMode.Manual &&
                CanCollectHoveredPickup &&
                interactPressed)
            {
                HoveredPickup.TryCollect(inventory);
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (HoveredPickup != null)
            {
                Rect hoverRect = new(
                    Screen.width * 0.5f - 170f,
                    Screen.height * 0.5f + 26f,
                    340f,
                    34f);
                GUI.Label(hoverRect, HoverLabel, hoverStyle);
            }

            if (NotificationVisible)
            {
                Rect notificationRect = new(
                    Screen.width * 0.5f - 260f,
                    38f,
                    520f,
                    38f);
                GUI.Label(
                    notificationRect,
                    LastNotificationText,
                    notificationStyle);
            }
        }

        private void OnPickupCollected(string displayName, int quantity)
        {
            LastNotificationText = quantity > 1
                ? $"{displayName}  ×{quantity}"
                : displayName;
            notificationUntil = Time.time + notificationDuration;
        }

        private void EnsureStyles()
        {
            hoverStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.78f, 0.4f, 1f) }
            };
            notificationStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.94f, 0.84f, 0.65f, 1f) }
            };
        }

        private bool IsNearbyGroundCandidate(
            Vector3 targetPoint,
            Vector3 cameraToPickup)
        {
            if (
                aimCamera.transform.forward.y > groundLookThreshold ||
                Vector3.Distance(transform.position, targetPoint) >
                    nearbyGroundAssistRange)
            {
                return false;
            }

            Vector3 planarLook = aimCamera.transform.forward;
            planarLook.y = 0f;
            Vector3 planarOffset = cameraToPickup;
            planarOffset.y = 0f;
            return
                planarLook.sqrMagnitude > 0.001f &&
                planarOffset.sqrMagnitude > 0.001f &&
                Vector3.Dot(planarLook.normalized, planarOffset.normalized) > 0f;
        }

        private bool HasLineOfSight(
            Vector3 origin,
            Vector3 toPickup,
            WorldPickup intendedPickup)
        {
            float distance = toPickup.magnitude;
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                toPickup / distance,
                sightBuffer,
                distance + 0.05f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);
            Collider closestRelevantCollider = null;
            float closestRelevantDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider = sightBuffer[index].collider;
                if (
                    hitCollider == null ||
                    hitCollider.transform.root == transform.root ||
                    sightBuffer[index].distance >= closestRelevantDistance)
                {
                    continue;
                }

                closestRelevantCollider = hitCollider;
                closestRelevantDistance = sightBuffer[index].distance;
            }

            if (closestRelevantCollider == null)
            {
                return true;
            }

            WorldPickup hitPickup =
                closestRelevantCollider.GetComponentInParent<WorldPickup>();
            return hitPickup == intendedPickup;
        }
    }
}
