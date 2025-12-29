using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils
{
    public static Vector2Int ToVector2IntFloor(this Vector2 position)
    {
        return new(
                Mathf.FloorToInt(position.x), 
                Mathf.FloorToInt(position.y));
    }

    public static Vector2Int ToVector2IntFloor(this Vector3 position)
        => ToVector2IntFloor((Vector2)position);

    public static Vector2 ScreenToWorld(this Vector2 pos)
        => Camera.main.ScreenToWorldPoint(pos);

    public static Vector3 ScreenToWorld(this Vector3 pos)
        => Camera.main.ScreenToWorldPoint(pos);
}
