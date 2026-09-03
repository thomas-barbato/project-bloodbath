using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    public static class PrototypeInterfaceCursor
    {
        private static readonly Vector2Int[] CursorShape =
        {
            new(3, 2),
            new(3, 25),
            new(9, 19),
            new(14, 30),
            new(19, 27),
            new(14, 17),
            new(24, 17)
        };

        private static Texture2D interactiveCursor;
        private static bool hasInteractiveTarget;
        private static bool isUsingInteractiveCursor;

        public static void BeginFrame()
        {
            hasInteractiveTarget = false;
        }

        public static void RegisterInteractive(Rect rect, bool enabled = true)
        {
            Vector2 pointerPosition = GUI.matrix.inverse.MultiplyPoint3x4(
                Event.current.mousePosition);
            if (
                enabled &&
                rect.Contains(pointerPosition))
            {
                hasInteractiveTarget = true;
            }
        }

        public static void EndFrame()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (hasInteractiveTarget == isUsingInteractiveCursor)
            {
                return;
            }

            isUsingInteractiveCursor = hasInteractiveTarget;
            Cursor.SetCursor(
                hasInteractiveTarget ? GetInteractiveCursor() : null,
                hasInteractiveTarget ? new Vector2(3f, 2f) : Vector2.zero,
                CursorMode.Auto);
        }

        public static void Reset()
        {
            hasInteractiveTarget = false;
            if (!isUsingInteractiveCursor)
            {
                return;
            }

            isUsingInteractiveCursor = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private static Texture2D GetInteractiveCursor()
        {
            if (interactiveCursor != null)
            {
                return interactiveCursor;
            }

            const int size = 32;
            interactiveCursor = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false)
            {
                name = "PrototypeInteractiveCursor",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[size * size];
            Color32 fill = new(190, 224, 207, 255);
            Color32 outline = new(20, 39, 43, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!IsInsideShape(x + 0.5f, y + 0.5f))
                    {
                        pixels[(size - 1 - y) * size + x] =
                            new Color32(0, 0, 0, 0);
                        continue;
                    }

                    bool edge =
                        !IsInsideShape(x - 1f, y) ||
                        !IsInsideShape(x + 1f, y) ||
                        !IsInsideShape(x, y - 1f) ||
                        !IsInsideShape(x, y + 1f);
                    pixels[(size - 1 - y) * size + x] =
                        edge ? outline : fill;
                }
            }

            interactiveCursor.SetPixels32(pixels);
            interactiveCursor.Apply(false, true);
            return interactiveCursor;
        }

        private static bool IsInsideShape(float x, float y)
        {
            bool inside = false;
            int previous = CursorShape.Length - 1;
            for (int current = 0;
                 current < CursorShape.Length;
                 current++)
            {
                Vector2Int currentPoint = CursorShape[current];
                Vector2Int previousPoint = CursorShape[previous];
                bool crosses =
                    currentPoint.y > y != previousPoint.y > y &&
                    x <
                    (previousPoint.x - currentPoint.x) *
                    (y - currentPoint.y) /
                    (previousPoint.y - currentPoint.y) +
                    currentPoint.x;
                if (crosses)
                {
                    inside = !inside;
                }

                previous = current;
            }

            return inside;
        }
    }
}
