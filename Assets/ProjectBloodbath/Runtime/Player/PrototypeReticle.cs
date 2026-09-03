using UnityEngine;

namespace ProjectBloodbath.Player
{
    public enum ReticleShape
    {
        Cross = 0,
        Dot = 1,
        XCross = 3,
        Circle = 4,
        Chevron = 5
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeReticle : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float thickness = 2f;
        [SerializeField, Min(1f)] private float length = 10f;
        [SerializeField, Min(0f)] private float gap = 4f;
        [SerializeField] private Color color = new(0.75f, 0.95f, 0.85f, 0.9f);
        [SerializeField, Range(0.5f, 2f)] private float sizeMultiplier = 1f;
        [SerializeField] private ReticleShape shape = ReticleShape.Cross;

        public Color Color => color;
        public float SizeMultiplier => sizeMultiplier;
        public ReticleShape Shape => shape;

        public void ConfigureAppearance(
            float newSizeMultiplier,
            Color newColor,
            ReticleShape newShape)
        {
            sizeMultiplier = Mathf.Clamp(newSizeMultiplier, 0.5f, 2f);
            color = newColor;
            shape = newShape;
        }

        public static void DrawPreview(
            Rect area,
            ReticleShape previewShape,
            Color previewColor,
            float previewSize = 1f)
        {
            Color previousColor = GUI.color;
            GUI.color = previewColor;
            float centerX = area.center.x;
            float centerY = area.center.y;
            float previewThickness = 2f * previewSize;
            float previewLength = 8f * previewSize;
            float previewGap = 3f * previewSize;
            DrawShape(
                previewShape,
                centerX,
                centerY,
                previewThickness,
                previewLength,
                previewGap);
            GUI.color = previousColor;
        }

        private void OnGUI()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = color;

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;
            float scaledThickness = thickness * sizeMultiplier;
            float scaledLength = length * sizeMultiplier;
            float scaledGap = gap * sizeMultiplier;

            DrawShape(
                shape,
                centerX,
                centerY,
                scaledThickness,
                scaledLength,
                scaledGap);

            GUI.color = previousColor;
        }

        private static void DrawShape(
            ReticleShape reticleShape,
            float centerX,
            float centerY,
            float scaledThickness,
            float scaledLength,
            float scaledGap)
        {
            switch (reticleShape)
            {
                case ReticleShape.Dot:
                    DrawDot(centerX, centerY, scaledThickness);
                    break;
                case ReticleShape.XCross:
                    DrawXCross(
                        centerX,
                        centerY,
                        scaledThickness,
                        scaledLength,
                        scaledGap);
                    break;
                case ReticleShape.Circle:
                    DrawCircle(
                        centerX,
                        centerY,
                        scaledGap + scaledLength * 0.8f,
                        scaledThickness);
                    break;
                case ReticleShape.Chevron:
                    DrawChevron(
                        centerX,
                        centerY,
                        scaledThickness,
                        scaledLength + scaledGap);
                    break;
                default:
                    DrawCross(
                        centerX,
                        centerY,
                        scaledThickness,
                        scaledLength,
                        scaledGap);
                    break;
            }
        }

        private static void DrawCross(
            float centerX,
            float centerY,
            float lineThickness,
            float lineLength,
            float lineGap)
        {
            DrawRect(
                centerX - lineGap - lineLength,
                centerY - lineThickness * 0.5f,
                lineLength,
                lineThickness);
            DrawRect(
                centerX + lineGap,
                centerY - lineThickness * 0.5f,
                lineLength,
                lineThickness);
            DrawRect(
                centerX - lineThickness * 0.5f,
                centerY - lineGap - lineLength,
                lineThickness,
                lineLength);
            DrawRect(
                centerX - lineThickness * 0.5f,
                centerY + lineGap,
                lineThickness,
                lineLength);
        }

        private static void DrawXCross(
            float centerX,
            float centerY,
            float lineThickness,
            float lineLength,
            float lineGap)
        {
            Vector2 center = new(centerX, centerY);
            Vector2 rising = new Vector2(1f, -1f).normalized;
            Vector2 falling = new Vector2(1f, 1f).normalized;
            DrawArm(center, rising, lineGap, lineLength, lineThickness);
            DrawArm(center, -rising, lineGap, lineLength, lineThickness);
            DrawArm(center, falling, lineGap, lineLength, lineThickness);
            DrawArm(center, -falling, lineGap, lineLength, lineThickness);
        }

        private static void DrawArm(
            Vector2 center,
            Vector2 direction,
            float lineGap,
            float lineLength,
            float lineThickness)
        {
            Vector2 start = center + direction * lineGap;
            Vector2 end = start + direction * lineLength;
            DrawLine(start, end, lineThickness);
        }

        private static void DrawCircle(
            float centerX,
            float centerY,
            float radius,
            float lineThickness)
        {
            const int SegmentCount = 32;
            Vector2 center = new(centerX, centerY);
            Vector2 previous = center + Vector2.right * radius;
            for (int index = 1; index <= SegmentCount; index++)
            {
                float angle = index * Mathf.PI * 2f / SegmentCount;
                Vector2 next = center +
                    new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                DrawLine(previous, next, lineThickness);
                previous = next;
            }
        }

        private static void DrawChevron(
            float centerX,
            float centerY,
            float lineThickness,
            float lineLength)
        {
            Vector2 tip = new(centerX, centerY);
            Vector2 leftEnd = tip +
                new Vector2(-1f, 1f).normalized * lineLength;
            Vector2 rightEnd = tip +
                new Vector2(1f, 1f).normalized * lineLength;
            DrawLine(leftEnd, tip, lineThickness);
            DrawLine(tip, rightEnd, lineThickness);
        }

        private static void DrawDot(
            float centerX,
            float centerY,
            float lineThickness)
        {
            float dotSize = Mathf.Max(2f, lineThickness * 2.5f);
            DrawRect(
                centerX - dotSize * 0.5f,
                centerY - dotSize * 0.5f,
                dotSize,
                dotSize);
        }

        private static void DrawLine(
            Vector2 start,
            Vector2 end,
            float lineThickness)
        {
            Vector2 offset = end - start;
            float length = offset.magnitude;
            if (length <= 0.001f)
            {
                return;
            }

            float pixelSize = Mathf.Max(1f, lineThickness);
            int steps = Mathf.Max(
                1,
                Mathf.CeilToInt(length / (pixelSize * 0.7f)));
            for (int index = 0; index <= steps; index++)
            {
                Vector2 point = Vector2.Lerp(
                    start,
                    end,
                    index / (float)steps);
                DrawRect(
                    Mathf.Round(point.x - pixelSize * 0.5f),
                    Mathf.Round(point.y - pixelSize * 0.5f),
                    pixelSize,
                    pixelSize);
            }
        }

        private static void DrawRect(float x, float y, float width, float height)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
        }
    }
}
