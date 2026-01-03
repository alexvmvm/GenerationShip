using System;
using System.Collections.Generic;
using UnityEngine;

public static class Turrets
{
    public static void Tick(Context context)
    {
        if( !context.isMoving )
            return;

        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].entityType != EntityType.SHIP_TURRET_TOP )
                continue;

            if( context.entities[i].cleanup || context.entities[i].cleanupIfNotVisible )
                continue;

            Entity e = context.entities[i];
            e.rotation += 1;
            context.entities[i] = e;

            if( Game.TicksGame % 30 == 0 )
            {
                var proj = new Entity();
                proj.entityType = EntityType.PROJECTILE;
                proj.velocity = Vector2.down.Rotate(e.rotation) * 0.5f;
                proj.rotation = e.rotation;
                proj.drawSize = Vector2.one;
                proj.position = e.position;
                proj.hitPoints = 10;
                proj.collisionType = CollisionType.CIRCLE;
                proj.collisionRadius = 0.25f;
                proj.collisionLayer = CollisionLayer.Ship;
                proj.collideWithMask = CollisionLayer.Asteroid;
                proj.cleanupIfNotVisible = true;
                context.entities.Add(proj);
            }
        }
    }
}