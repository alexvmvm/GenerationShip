using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Movement
{
    public static void Tick(Context context)
    {
        // Update positions
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].velocity == Vector2.zero )
                continue;
            
            Entity entity = context.entities[i];
            entity.position += entity.velocity;
            context.entities[i] = entity;
        }
    }
}