using System;
using System.Collections.Generic;
using ProjectBloodbath.Input;
using ProjectBloodbath.Progression;
using ProjectBloodbath.Quests;
using ProjectBloodbath.Settings;
using ProjectBloodbath.World;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PrototypeMapPanel :
        MonoBehaviour,
        IPrototypeModalView
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PrototypeInterfaceCoordinator
            interfaceCoordinator;
        [SerializeField] private CharacterQuestJournal questJournal;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private string areaName =
            "LABORATOIRE DE MOUVEMENT";
        [SerializeField] private Vector2 worldCenter = Vector2.zero;
        [SerializeField] private Vector2 worldSize = new(44f, 44f);
        [SerializeField, Min(2f)] private float miniMapRadius = 22f;
        [SerializeField, Min(0.1f)] private float markerRefreshInterval = 1f;

        private readonly List<WorldMapMarker> markers = new();
        private readonly List<WorldMapGeometry> geometry = new();
        private readonly List<Rect> occupiedLabelRects = new();
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallStyle;
        private GUIStyle miniMapTitleStyle;
        private GUIStyle centerStyle;
        private GUIStyle playerMarkerStyle;
        private float nextMarkerRefresh;

        public bool IsOpen { get; private set; }
        public bool MiniMapVisible =>
            !IsOpen &&
            (interfaceCoordinator == null ||
             !interfaceCoordinator.HasOpenView);
        public int VisibleMarkerCount => markers.Count;
        public int VisibleGeometryCount => geometry.Count;

        private void Awake()
        {
            CacheReferences();
            RefreshMarkers();
        }

        private void OnDisable()
        {
            SetOpen(false);
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            if (IsOpen && !inputReader.enabled)
            {
                SetOpen(false);
                return;
            }

            if (inputReader.ConsumeWorldMapPressed())
            {
                SetOpen(!IsOpen);
            }

            if (IsOpen && inputReader.ConsumeMenuCancelPressed())
            {
                SetOpen(false);
            }

            if (Time.unscaledTime >= nextMarkerRefresh)
            {
                RefreshMarkers();
            }
        }

        public void SetOpen(bool open)
        {
            if (IsOpen == open)
            {
                return;
            }

            CacheReferences();
            if (open)
            {
                interfaceCoordinator?.Open(this);
                IsOpen = true;
                RefreshMarkers();
                if (interfaceCoordinator == null)
                {
                    ApplyFallbackInputState(true);
                }
                return;
            }

            IsOpen = false;
            if (interfaceCoordinator != null)
            {
                interfaceCoordinator.Close(this);
            }
            else
            {
                ApplyFallbackInputState(false);
            }
        }

        public void CloseFromCoordinator()
        {
            IsOpen = false;
        }

        public void RefreshMarkers()
        {
            markers.Clear();
            WorldMapMarker[] found =
                FindObjectsByType<WorldMapMarker>(
                    FindObjectsSortMode.None);
            for (int index = 0; index < found.Length; index++)
            {
                if (found[index] != null && found[index].isActiveAndEnabled)
                {
                    markers.Add(found[index]);
                }
            }

            geometry.Clear();
            WorldMapGeometry[] foundGeometry =
                FindObjectsByType<WorldMapGeometry>(
                    FindObjectsSortMode.None);
            for (int index = 0; index < foundGeometry.Length; index++)
            {
                if (foundGeometry[index] != null &&
                    foundGeometry[index].isActiveAndEnabled)
                {
                    geometry.Add(foundGeometry[index]);
                }
            }

            nextMarkerRefresh =
                Time.unscaledTime + markerRefreshInterval;
        }

        private void OnGUI()
        {
            if (!IsOpen && !MiniMapVisible)
            {
                return;
            }

            EnsureStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;

            float scale = Mathf.Max(
                0.5f,
                Mathf.Min(
                    Screen.width / ReferenceWidth,
                    Screen.height / ReferenceHeight));
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            if (IsOpen)
            {
                PrototypeInterfaceCursor.BeginFrame();
                DrawLargeMap(width, height);
                PrototypeInterfaceCursor.EndFrame();
            }
            else
            {
                DrawMiniMap(width);
            }

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawMiniMap(float width)
        {
            Rect frame = new(width - 286f, 36f, 244f, 244f);
            DrawMapBackground(frame, 0.9f);
            GUI.Label(
                new Rect(frame.x + 10f, frame.y + 7f, 26f, 30f),
                "N",
                titleStyle);
            GUI.Label(
                new Rect(frame.x + 40f, frame.y + 4f,
                    frame.width - 50f, 34f),
                areaName,
                miniMapTitleStyle);

            Rect content = new(
                frame.x + 10f,
                frame.y + 43f,
                frame.width - 20f,
                frame.height - 53f);
            DrawGrid(content, 4);
            DrawMiniMapGeometry(content);
            DrawMiniMapMarkers(content);
            DrawPlayerMarker(
                new Vector2(content.center.x, content.center.y),
                15f,
                true);
        }

        private void DrawLargeMap(float width, float height)
        {
            DrawRect(
                new Rect(0f, 0f, width, height),
                new Color(0.004f, 0.009f, 0.011f, 0.84f));
            Rect panel = new(
                width * 0.5f - 650f,
                height * 0.5f - 430f,
                1300f,
                860f);
            DrawMapBackground(panel, 0.98f);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 20f, 760f, 42f),
                areaName,
                titleStyle);
            Rect closeRect = new(
                panel.xMax - 70f,
                panel.y + 18f,
                40f,
                40f);
            PrototypeInterfaceCursor.RegisterInteractive(closeRect);
            if (GUI.Button(
                closeRect,
                "×",
                GUI.skin.button))
            {
                SetOpen(false);
            }

            Rect content = new(
                panel.x + 34f,
                panel.y + 78f,
                panel.width - 68f,
                panel.height - 132f);
            DrawRect(content, new Color(0.012f, 0.027f, 0.03f, 1f));
            DrawGrid(content, 8);
            DrawWorldMapGeometry(content);
            DrawWorldMapMarkers(content);
            DrawLegend(panel);
        }

        private void DrawMiniMapMarkers(Rect content)
        {
            Vector3 playerPosition = playerTransform.position;
            float halfWidth = content.width * 0.5f;
            float halfHeight = content.height * 0.5f;
            for (int index = 0; index < markers.Count; index++)
            {
                WorldMapMarker marker = markers[index];
                if (marker == null ||
                    !marker.ShowOnMiniMap ||
                    !marker.IsCurrentlyVisible)
                {
                    continue;
                }

                Vector3 offset = marker.transform.position - playerPosition;
                Vector2 normalized = new(
                    offset.x / miniMapRadius,
                    offset.z / miniMapRadius);
                if (Mathf.Abs(normalized.x) > 1f ||
                    Mathf.Abs(normalized.y) > 1f)
                {
                    continue;
                }

                Vector2 point = new(
                    content.center.x + normalized.x * halfWidth,
                    content.center.y - normalized.y * halfHeight);
                if (IsTrackedQuestObjective(marker))
                {
                    DrawMarker(
                        point,
                        16f,
                        new Color(0.58f, 0.74f, 0.61f, 1f));
                    DrawMarker(point, 8f, marker.Color);
                }
                else
                {
                    DrawMarker(point, 9f, marker.Color);
                }
            }
        }

        private void DrawWorldMapMarkers(Rect content)
        {
            occupiedLabelRects.Clear();
            for (int index = 0; index < markers.Count; index++)
            {
                WorldMapMarker marker = markers[index];
                if (marker == null ||
                    !marker.ShowOnWorldMap ||
                    !marker.IsCurrentlyVisible)
                {
                    continue;
                }

                Vector2 point = WorldToMapPoint(
                    marker.transform.position,
                    content);
                if (IsTrackedQuestObjective(marker))
                {
                    DrawMarker(
                        point,
                        22f,
                        new Color(0.58f, 0.74f, 0.61f, 1f));
                    DrawMarker(point, 11f, marker.Color);
                }
                else
                {
                    DrawMarker(point, 12f, marker.Color);
                }
                if (marker.MarkerType == WorldMapMarkerType.Loot)
                {
                    DrawHoverTarget(point, marker.DisplayName);
                }
                if (ShouldDisplayPermanentLabel(marker.MarkerType))
                {
                    float labelY = marker.MarkerType ==
                        WorldMapMarkerType.Quest
                            ? point.y + 10f
                            : point.y - 6f;
                    Rect labelRect = new(
                        point.x + 12f,
                        labelY,
                        280f,
                        28f);
                    while (OverlapsExistingLabel(labelRect))
                    {
                        labelRect.y += 26f;
                    }

                    occupiedLabelRects.Add(labelRect);
                    GUI.Label(
                        labelRect,
                        marker.DisplayName.ToUpperInvariant(),
                        smallStyle);
                }
            }

            Vector2 playerPoint = WorldToMapPoint(
                playerTransform.position,
                content);
            DrawPlayerMarker(playerPoint, 22f, true);

            DrawCurrentTooltip(content);
        }

        private static void DrawHoverTarget(
            Vector2 point,
            string tooltip)
        {
            GUI.Label(
                new Rect(point.x - 14f, point.y - 14f, 28f, 28f),
                new GUIContent(string.Empty, tooltip),
                GUIStyle.none);
        }

        private void DrawCurrentTooltip(Rect content)
        {
            if (string.IsNullOrWhiteSpace(GUI.tooltip))
            {
                return;
            }

            Vector2 mousePosition = GUI.matrix.inverse.MultiplyPoint(
                Event.current.mousePosition);
            const float tooltipWidth = 260f;
            const float tooltipHeight = 34f;
            float x = Mathf.Clamp(
                mousePosition.x + 18f,
                content.x,
                content.xMax - tooltipWidth);
            float y = Mathf.Clamp(
                mousePosition.y + 18f,
                content.y,
                content.yMax - tooltipHeight);
            Rect tooltipRect = new(
                x,
                y,
                tooltipWidth,
                tooltipHeight);
            DrawRect(
                tooltipRect,
                new Color(0.018f, 0.038f, 0.042f, 0.98f));
            DrawRect(
                new Rect(tooltipRect.x, tooltipRect.y, 3f,
                    tooltipRect.height),
                new Color(0.1f, 0.82f, 0.78f, 1f));
            GUI.Label(
                new Rect(tooltipRect.x + 12f, tooltipRect.y,
                    tooltipRect.width - 20f, tooltipRect.height),
                GUI.tooltip.ToUpperInvariant(),
                smallStyle);
        }

        private void DrawLegend(Rect panel)
        {
            float y = panel.yMax - 43f;
            DrawLegendItem(
                new Rect(panel.x + 40f, y, 210f, 24f),
                new Color(0.9f, 0.04f, 0.035f, 1f),
                "MONSTRES");
            DrawLegendItem(
                new Rect(panel.x + 266f, y, 210f, 24f),
                new Color(0.28f, 0.72f, 0.72f, 1f),
                "OBJETS AU SOL");
            DrawLegendItem(
                new Rect(panel.x + 492f, y, 210f, 24f),
                new Color(0.58f, 0.74f, 0.61f, 1f),
                "QUÊTES");
            DrawLegendItem(
                new Rect(panel.x + 718f, y, 210f, 24f),
                new Color(0.93f, 0.84f, 0.68f, 1f),
                "JOUEUR");
            DrawLegendItem(
                new Rect(panel.x + 944f, y,
                    panel.width - 984f, 24f),
                new Color(0.94f, 0.72f, 0.38f, 1f),
                "PNJ");
        }

        private void DrawLegendItem(
            Rect rect,
            Color markerColor,
            string label)
        {
            DrawMarker(
                new Vector2(rect.x + 7f, rect.center.y),
                11f,
                markerColor);
            GUI.Label(
                new Rect(rect.x + 20f, rect.y,
                    rect.width - 20f, rect.height),
                label,
                smallStyle);
        }

        private static bool ShouldDisplayPermanentLabel(
            WorldMapMarkerType markerType)
        {
            return markerType != WorldMapMarkerType.Hostile &&
                markerType != WorldMapMarkerType.Loot;
        }

        private bool OverlapsExistingLabel(Rect candidate)
        {
            for (int index = 0; index < occupiedLabelRects.Count; index++)
            {
                if (candidate.Overlaps(occupiedLabelRects[index]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsTrackedQuestObjective(WorldMapMarker marker)
        {
            QuestRuntimeState tracked = questJournal?.TrackedQuest;
            if (
                marker == null ||
                tracked?.Definition == null ||
                tracked.Status != QuestStatus.Active)
            {
                return false;
            }

            for (int index = 0;
                index < tracked.Definition.Objectives.Count;
                index++)
            {
                QuestObjectiveDefinition objective =
                    tracked.Definition.Objectives[index];
                if (
                    objective == null ||
                    tracked.GetObjectiveProgress(index) >=
                        objective.RequiredAmount)
                {
                    continue;
                }

                if (MatchesPickupObjective(marker, objective) ||
                    MatchesEnemyObjective(marker, objective))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesPickupObjective(
            WorldMapMarker marker,
            QuestObjectiveDefinition objective)
        {
            if (!string.Equals(
                objective.EventIdentifier,
                QuestEventIdentifiers.ItemCollected,
                StringComparison.Ordinal))
            {
                return false;
            }

            WorldPickup pickup = marker.GetComponentInParent<WorldPickup>();
            return pickup?.Definition != null &&
                MatchesTarget(
                    objective.TargetIdentifier,
                    pickup.Definition.Identifier);
        }

        private static bool MatchesEnemyObjective(
            WorldMapMarker marker,
            QuestObjectiveDefinition objective)
        {
            if (!string.Equals(
                objective.EventIdentifier,
                QuestEventIdentifiers.EnemyKilled,
                StringComparison.Ordinal))
            {
                return false;
            }

            QuestTargetIdentity identity =
                marker.GetComponentInParent<QuestTargetIdentity>();
            return identity != null &&
                MatchesTarget(
                    objective.TargetIdentifier,
                    identity.Identifier);
        }

        private static bool MatchesTarget(
            string expected,
            string actual)
        {
            return string.IsNullOrWhiteSpace(expected) ||
                string.Equals(expected, actual, StringComparison.Ordinal);
        }

        private void DrawMiniMapGeometry(Rect content)
        {
            Vector3 playerPosition = playerTransform.position;
            for (int index = 0; index < geometry.Count; index++)
            {
                WorldMapGeometry item = geometry[index];
                if (item == null || !item.ShowOnMiniMap)
                {
                    continue;
                }

                Rect rect = WorldBoundsToMiniMapRect(
                    item.WorldBounds,
                    playerPosition,
                    content);
                if (rect.width > 0f && rect.height > 0f)
                {
                    DrawRect(rect, item.Color);
                }
            }
        }

        private void DrawWorldMapGeometry(Rect content)
        {
            for (int index = 0; index < geometry.Count; index++)
            {
                WorldMapGeometry item = geometry[index];
                if (item == null || !item.ShowOnWorldMap)
                {
                    continue;
                }

                Bounds bounds = item.WorldBounds;
                Vector2 topLeft = WorldToMapPoint(
                    new Vector3(bounds.min.x, 0f, bounds.max.z),
                    content);
                Vector2 bottomRight = WorldToMapPoint(
                    new Vector3(bounds.max.x, 0f, bounds.min.z),
                    content);
                DrawRect(
                    Rect.MinMaxRect(
                        Mathf.Min(topLeft.x, bottomRight.x),
                        Mathf.Min(topLeft.y, bottomRight.y),
                        Mathf.Max(topLeft.x, bottomRight.x),
                        Mathf.Max(topLeft.y, bottomRight.y)),
                    item.Color);
            }
        }

        private Rect WorldBoundsToMiniMapRect(
            Bounds bounds,
            Vector3 playerPosition,
            Rect content)
        {
            float halfWidth = content.width * 0.5f;
            float halfHeight = content.height * 0.5f;
            float xMin = content.center.x +
                (bounds.min.x - playerPosition.x) /
                miniMapRadius * halfWidth;
            float xMax = content.center.x +
                (bounds.max.x - playerPosition.x) /
                miniMapRadius * halfWidth;
            float yMin = content.center.y -
                (bounds.max.z - playerPosition.z) /
                miniMapRadius * halfHeight;
            float yMax = content.center.y -
                (bounds.min.z - playerPosition.z) /
                miniMapRadius * halfHeight;
            return Rect.MinMaxRect(
                Mathf.Clamp(xMin, content.x, content.xMax),
                Mathf.Clamp(yMin, content.y, content.yMax),
                Mathf.Clamp(xMax, content.x, content.xMax),
                Mathf.Clamp(yMax, content.y, content.yMax));
        }

        private Vector2 WorldToMapPoint(Vector3 worldPosition, Rect content)
        {
            float safeWidth = Mathf.Max(1f, worldSize.x);
            float safeHeight = Mathf.Max(1f, worldSize.y);
            float normalizedX =
                (worldPosition.x - worldCenter.x) / safeWidth + 0.5f;
            float normalizedY =
                (worldPosition.z - worldCenter.y) / safeHeight + 0.5f;
            return new Vector2(
                Mathf.Lerp(content.x, content.xMax,
                    Mathf.Clamp01(normalizedX)),
                Mathf.Lerp(content.yMax, content.y,
                    Mathf.Clamp01(normalizedY)));
        }

        private void DrawPlayerMarker(
            Vector2 point,
            float size,
            bool rotateWithPlayer)
        {
            Matrix4x4 previous = GUI.matrix;
            if (rotateWithPlayer)
            {
                Matrix4x4 localRotation =
                    Matrix4x4.Translate(
                        new Vector3(point.x, point.y, 0f)) *
                    Matrix4x4.Rotate(
                        Quaternion.Euler(
                            0f,
                            0f,
                            playerTransform.eulerAngles.y)) *
                    Matrix4x4.Translate(
                        new Vector3(-point.x, -point.y, 0f));
                GUI.matrix = previous * localRotation;
            }

            Rect markerRect = new(
                point.x - size,
                point.y - size,
                size * 2f,
                size * 2f);
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 1f);
            GUI.Label(
                new Rect(
                    markerRect.x + 2f,
                    markerRect.y + 3f,
                    markerRect.width,
                    markerRect.height),
                "▲",
                playerMarkerStyle);
            GUI.color = new Color(0.88f, 0.86f, 0.72f, 1f);
            GUI.Label(markerRect, "▲", playerMarkerStyle);
            GUI.color = previousColor;
            GUI.matrix = previous;
        }

        private static void DrawMarker(
            Vector2 point,
            float size,
            Color color)
        {
            DrawRect(
                new Rect(
                    point.x - size * 0.5f,
                    point.y - size * 0.5f,
                    size,
                    size),
                color);
        }

        private static void DrawGrid(Rect content, int divisions)
        {
            Color gridColor = new(0.18f, 0.29f, 0.29f, 0.5f);
            for (int index = 1; index < divisions; index++)
            {
                float x = Mathf.Lerp(
                    content.x,
                    content.xMax,
                    index / (float)divisions);
                float y = Mathf.Lerp(
                    content.y,
                    content.yMax,
                    index / (float)divisions);
                DrawRect(new Rect(x, content.y, 1f, content.height), gridColor);
                DrawRect(new Rect(content.x, y, content.width, 1f), gridColor);
            }
        }

        private void CacheReferences()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (interfaceCoordinator == null)
            {
                interfaceCoordinator =
                    GetComponent<PrototypeInterfaceCoordinator>();
            }

            if (questJournal == null)
            {
                questJournal = GetComponent<CharacterQuestJournal>();
            }

            if (playerTransform == null)
            {
                playerTransform = transform;
            }
        }

        private void ApplyFallbackInputState(bool open)
        {
            inputReader?.SetGameplaySuppressed(open);
            Cursor.lockState = open
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = open;
            if (!open)
            {
                PrototypeInterfaceCursor.Reset();
            }
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.82f, 0.84f, 0.7f, 1f) }
            };
            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.84f, 0.72f, 1f) }
            };
            smallStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.65f, 0.76f, 0.69f, 1f) }
            };
            miniMapTitleStyle ??= new GUIStyle(smallStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            centerStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.84f, 0.72f, 1f) }
            };
            playerMarkerStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        private static void DrawMapBackground(Rect rect, float alpha)
        {
            DrawRect(rect, new Color(0.025f, 0.045f, 0.048f, alpha));
            PrototypeHudSkin.DrawTiledNotchedTexture(
                rect,
                new Color(0.48f, 0.54f, 0.53f, alpha * 0.82f),
                8f,
                256f);
            Rect trim = new(
                rect.x + 2f,
                rect.y + 2f,
                rect.width - 4f,
                rect.height - 4f);
            DrawRect(trim, new Color(0.25f, 0.38f, 0.38f, alpha));
            Rect interior = new(
                rect.x + 5f,
                rect.y + 5f,
                rect.width - 10f,
                rect.height - 10f);
            PrototypeHudSkin.DrawDisplayGlass(interior, alpha);
            PrototypeHudSkin.DrawTiledTexture(
                interior,
                new Color(0.68f, 0.72f, 0.7f, alpha * 0.24f),
                384f);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
