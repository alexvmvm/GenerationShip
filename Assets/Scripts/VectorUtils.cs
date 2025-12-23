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
}
