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
}
