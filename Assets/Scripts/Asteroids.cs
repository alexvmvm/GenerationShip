using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Asteroids
{
    public static void Tick(Context context)
    {
        if( !context.isMoving )
            return;

        Rect rect = CameraUtils.GetWorldRect(Camera.main);

        if( Rand.Chance(0.1f) )
        {
            Entity asteroid = EntityMaker.MakeAsteroid(large: Rand.Bool);
            asteroid.position = new(Rand.Range(rect.xMin, rect.xMax), rect.yMax + 2);
            asteroid.velocity = new Vector2(0, Rand.Range(-0.1f, -0.05f));

            context.entities.Add(asteroid);
        }
    }
}