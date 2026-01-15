using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GUIUtils
{
    public static Vector2 WorldToGUI(this Vector2 world, Camera cam = null)
    {
        cam = cam != null ? cam : Camera.main;

        Vector2 sp = cam.WorldToScreenPoint(world); // bottom-left origin
        return new Vector2(sp.x, Screen.height - sp.y);
    }
}
