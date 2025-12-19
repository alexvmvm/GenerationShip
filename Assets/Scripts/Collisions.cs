using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Collisions
{
    public static void Tick(Context context)
    {
        // Update positions
        for(int i = 0; i < context.entities.Count; i++)
        {
            Entity a = context.entities[i];

            if( a.collisionSize == Vector2.zero 
                || a.collisionLayer == CollisionLayer.None
                || a.collideWithMask == CollisionLayer.None )
            {
                continue;
            }
            
            for(int j = 0; j < context.entities.Count; j++)
            {
                if( i == j )
                    continue;

                Entity b = context.entities[j];

                if( !ShouldCollide(a, b) )
                    continue;

                if( !Collides(a, b) )
                    continue;
                
                Collide(ref a, ref b, context);

                context.entities[i] = a;
                context.entities[j] = b;
            }

        }
    }

    private static bool ShouldCollide(in Entity a, in Entity b)
    {
        return ((a.collideWithMask & b.collisionLayer) != 0) &&
            ((b.collideWithMask & a.collisionLayer) != 0);
    }

    public static bool Collides(in Entity a, in Entity b)
    {
        Vector2 aHalf = a.collisionSize * 0.5f;
        Vector2 bHalf = b.collisionSize * 0.5f;

        // Separating Axis Theorem for AABBs (2D)
        if (Mathf.Abs(a.position.x - b.position.x) > (aHalf.x + bHalf.x)) return false;
        if (Mathf.Abs(a.position.y - b.position.y) > (aHalf.y + bHalf.y)) return false;

        return true;
    }

    private static void Collide(ref Entity a, ref Entity b, Context context)
    {
        bool aIsShip = a.entityType == EntityType.SHIP_ROOM;
        bool bIsShip = b.entityType == EntityType.SHIP_ROOM;

        bool aIsAsteroid = a.entityType == EntityType.ASTEROID_SMALL || a.entityType == EntityType.ASTEROID_LARGE;
        bool bIsAsteroid = b.entityType == EntityType.ASTEROID_SMALL || b.entityType == EntityType.ASTEROID_LARGE;

        // asteroid hit ship
        if (aIsShip && bIsAsteroid)
        {
            b.cleanup = true;
            DoRoomHit(ref a, context);
        }
        else if (bIsShip && aIsAsteroid)
        {
            a.cleanup = true;
            DoRoomHit(ref b, context);
        }
            
    }

    private static void DoRoomHit(ref Entity room, Context context)
    {
        room.damageFlashTicks = 5;

        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].parentId == room.id && 
                (context.entities[i].entityType == EntityType.SHIP_FLOOR || 
                 context.entities[i].entityType == EntityType.SHIP_ENGINE) )
            {
                Entity floor = context.entities[i];
                floor.damageFlashTicks = 6;
                context.entities[i] = floor;
            }
        }
    }

    public static void Update(Context context)
    {
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].collisionSize == Vector2.zero )
                continue;
            
            DebugUtils.DrawRect(
                context.entities[i].position, 
                context.entities[i].collisionSize, 
                Color.green);
        }
    }
}