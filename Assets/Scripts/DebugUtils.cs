using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugUtils
{
    public static void DrawRect(Vector2 center, Vector2 size, Color color, float duration = 0f)
    {
        Vector2 h = size * 0.5f;
        Vector3 a = new(center.x - h.x, center.y - h.y, 0);
        Vector3 b = new(center.x + h.x, center.y - h.y, 0);
        Vector3 c = new(center.x + h.x, center.y + h.y, 0);
        Vector3 d = new(center.x - h.x, center.y + h.y, 0);

        Debug.DrawLine(a, b, color, duration);
        Debug.DrawLine(b, c, color, duration);
        Debug.DrawLine(c, d, color, duration);
        Debug.DrawLine(d, a, color, duration);
    }

    public static void DrawCircle(Vector2 center, float radius, Color color, float duration = 0f, int segments = 32)
    {
        if (radius <= 0f || segments < 3)
            return;

        float step = 2f * Mathf.PI / segments;

        Vector3 prev = new Vector3(
            center.x + Mathf.Cos(0f) * radius,
            center.y + Mathf.Sin(0f) * radius,
            0f);

        for (int i = 1; i <= segments; i++)
        {
            float a = step * i;
            Vector3 cur = new Vector3(
                center.x + Mathf.Cos(a) * radius,
                center.y + Mathf.Sin(a) * radius,
                0f);

            Debug.DrawLine(prev, cur, color, duration);
            prev = cur;
        }
    }
}
