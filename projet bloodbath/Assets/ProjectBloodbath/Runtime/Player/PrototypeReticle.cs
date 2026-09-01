using UnityEngine;

namespace ProjectBloodbath.Player
{
    [DisallowMultipleComponent]
    public sealed class PrototypeReticle : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float thickness = 2f;
        [SerializeField, Min(1f)] private float length = 10f;
        [SerializeField, Min(0f)] private float gap = 4f;
        [SerializeField] private Color color = new(0.75f, 0.95f, 0.85f, 0.9f);

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
            DrawRect(centerX - gap - length, centerY - thickness * 0.5f, length, thickness);
            DrawRect(centerX + gap, centerY - thickness * 0.5f, length, thickness);
            DrawRect(centerX - thickness * 0.5f, centerY - gap - length, thickness, length);
            DrawRect(centerX - thickness * 0.5f, centerY + gap, thickness, length);

            GUI.color = previousColor;
        }

        private static void DrawRect(float x, float y, float width, float height)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
        }
    }
}
