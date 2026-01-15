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
            if( context.entities[i].velocity != Vector2.zero )
            {
                Entity entity = context.entities[i];
                entity.position += entity.velocity;
                context.entities[i] = entity;
            }
            
            if( context.entities[i].rotationRate != 0f )
            {
                Entity entity = context.entities[i];
                entity.rotation += entity.rotationRate;
                context.entities[i] = entity;
            }
        }
    }

    public static void Update(Context context)
    {
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].velocity == Vector2.zero )
                continue;

            Debug.DrawLine(
                context.entities[i].position, 
                context.entities[i].position + context.entities[i].velocity * 10f,
                Color.red);
        }
    }
}