using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraUtils 
{
    public static Rect GetWorldRect( this Camera camera )
    {
        // Get the screen corners in pixels
        Vector3 bottomLeft  = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        Vector3 topRight    = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.nearClipPlane));

        return new Rect(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y
        );
    }
}
