using System;
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
            DoAsteroidDestruction(b, context);
            DoRoomHit(ref a, context);
        }
        else if (bIsShip && aIsAsteroid)
        {
            a.cleanup = true;
            DoAsteroidDestruction(a, context);
            DoRoomHit(ref b, context);
        }
            
    }

    private static void DoAsteroidDestruction(in Entity asteroid, Context context)
    {
        bool large = asteroid.entityType == EntityType.ASTEROID_LARGE;
        int count = large ? Rand.Range(6, 10) : Rand.Range(2, 5);

        for(int i = 0; i < count; i++)
        {
            bool largeFragment = large && Rand.Chance(0.5f);

            Entity fragment = EntityMaker.MakeAsteroidFragment(large: largeFragment);
            fragment.velocity = asteroid.velocity.Rotate(Rand.Range(-30, 30f)) * Rand.Range(0.5f, 1f);
            fragment.rotationRate = Rand.Range(0.5f, 2f);
            fragment.position = asteroid.position + new Vector2(
                Rand.Range(-0.5f, 0.5f), 
                Rand.Range(-0.5f, 0.5f));

            context.entities.Add(fragment);
        }
    }

    private static void DoRoomHit(ref Entity room, Context context)
    {
        room.damageFlashTicks = 5;
        room.hitPoints -= 10;
        room.hitPoints = Math.Max(0, room.hitPoints);

        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].parentId == room.id && 
                context.entities[i].tags.HasAny(EntityTag.Floor | EntityTag.Wall | EntityTag.Engine) )
            {
                Entity floor = context.entities[i];
                floor.damageFlashTicks = 6;
                context.entities[i] = floor;
            }
        }

        if( room.hitPoints <= 0 )
        {
            room.collideWithMask = CollisionLayer.None;
            room.collisionLayer = CollisionLayer.None;
            room.collisionSize = default;
            room.cleanup = true;

            for(int i = 0; i < context.entities.Count; i++)
            {
                if( context.entities[i].parentId != room.id )
                    continue;

                Entity e = context.entities[i];
                e.cleanupIfNotVisible = true;
                e.sortingOrder -= 10;

                if( Rand.Chance(0.1f) )
                {
                    e.velocity = new Vector2(0, -0.05f).Rotate(Rand.Range(-15, 15));
                    e.rotationRate = Rand.Bool ? 1 : 0;
                }
                else
                {
                    e.velocity = new Vector2(0, -0.05f).Rotate(Rand.Range(-1, 1));
                }
                
                context.entities[i] = e;
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