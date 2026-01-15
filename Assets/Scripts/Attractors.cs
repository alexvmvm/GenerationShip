using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Attractors
{
    private const float MaxAttractionRange = 12f;
    private const float AbsorbRange = 2f;
    private const float MaxAttractionForce = 0.02f;
    private static List<Entity> attractors = new();

    public static void Tick(Context context)
    {
        attractors.Clear();
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].isResourceAttractor 
                && !context.entities[i].cleanup 
                && !context.entities[i].cleanupIfNotVisible )
            {
                attractors.Add(context.entities[i]);
            }
        }

        if( attractors.Count == 0 )
            return;

        // Update positions
        float maxRangeSq = MaxAttractionRange * MaxAttractionRange;
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].resourceCount > 0 )
            {
                if( context.entities[i].cleanup )
                    continue;

                Entity entity = context.entities[i];
                Vector2 attraction = Vector2.zero;
                float minDistance = float.MaxValue;

                for(int j = 0; j < attractors.Count; j++)
                {
                    Vector2 toAttractor = attractors[j].position - entity.position;
                    float distSq = toAttractor.sqrMagnitude;

                    if( distSq > maxRangeSq || distSq < Mathf.Epsilon )
                        continue;

                    float dist = Mathf.Sqrt(distSq);
                    float t = 1f - (dist / MaxAttractionRange);
                    float strength = MaxAttractionForce * t;
                    attraction += toAttractor * (strength / dist);

                    if( dist < minDistance )
                        minDistance = dist;
                }

                if( minDistance < AbsorbRange )
                {
                    entity.cleanup = true;
                    context.entities[i] = entity;
                    Find.Game.GainResource(entity.resourceCount);
                    continue;
                }

                if( attraction != Vector2.zero )
                {
                    entity.velocity += attraction;
                    context.entities[i] = entity;
                }
            }
        }
    }
}
