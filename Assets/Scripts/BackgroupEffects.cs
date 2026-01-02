using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BackgroupEffects
{
    public static void Tick(Context context)
    {
        if( !context.isMoving )
            return;
        
        Rect rect = CameraUtils.GetWorldRect(Camera.main);

        if( Rand.Chance(0.1f) )
        {   
            Entity particle = EntityMaker.MakeBackgroundParticle();
            particle.position = new(Rand.Range(rect.xMin, rect.xMax), rect.yMax + 2);
            particle.velocity = new Vector2(0, Rand.Range(-0.8f, -0.5f));

            context.entities.Add(particle);
        }
    }
}