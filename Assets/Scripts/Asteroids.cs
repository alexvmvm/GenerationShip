using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Asteroids
{
    public static void Tick(Context context)
    {
        Rect rect = CameraUtils.GetWorldRect(Camera.main);

        if( Rand.Chance(0.1f) )
        {
            bool large = Rand.Bool;
            context.entities.Add(new Entity()
            {
                entityType = large ? EntityType.ASTEROID_SMALL : EntityType.ASTEROID_LARGE,
                position = new(Rand.Range(rect.xMin, rect.xMax), rect.yMax + 2),
                drawSize = Vector2.one,
                cleanupIfNotVisible = true,
                velocity = new Vector2(0, Rand.Range(-0.1f, -0.05f))
            });
        }
    }
}