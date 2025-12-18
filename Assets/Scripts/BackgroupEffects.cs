using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BackgroupEffects
{
    public static void Tick(Context context)
    {
        Rect rect = CameraUtils.GetWorldRect(Camera.main);

        if( Rand.Chance(0.1f) )
        {            
            context.entities.Add(new Entity()
            {
                entityType = EntityType.BACKDROP_PARTICLE,
                position = new(Rand.Range(rect.xMin, rect.xMax), rect.yMax + 2),
                drawSize = Vector2.one,
                cleanupIfNotVisible = true,
                velocity = new Vector2(0, Rand.Range(-0.8f, -0.5f))
            });
        }
    }
}