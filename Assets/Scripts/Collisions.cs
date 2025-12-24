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
        return ((a.collideWithMask & b.collisionLayer) != 0) && ((b.collideWithMask & a.collisionLayer) != 0);
    }

    public static bool Collides(in Entity a, in Entity b)
    {
        // Fast reject: if both are squares, do AABB/AABB
        if (a.collisionType == CollisionType.SQAURE && b.collisionType == CollisionType.SQAURE)
            return CollidesAabbAabb(a.position, a.collisionSize, b.position, b.collisionSize);

        // Circle/Circle
        if (a.collisionType == CollisionType.CIRCLE && b.collisionType == CollisionType.CIRCLE)
            return CollidesCircleCircle(a.position, a.collisionRadius, b.position, b.collisionRadius);

        // Circle/AABB (either order)
        if (a.collisionType == CollisionType.CIRCLE && b.collisionType == CollisionType.SQAURE)
            return CollidesCircleAabb(a.position, a.collisionRadius, b.position, b.collisionSize);

        if (a.collisionType == CollisionType.SQAURE && b.collisionType == CollisionType.CIRCLE)
            return CollidesCircleAabb(b.position, b.collisionRadius, a.position, a.collisionSize);

        // Fallback (shouldn't happen)
        return false;
    }

    private static bool CollidesAabbAabb(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize)
    {
        Vector2 aHalf = aSize * 0.5f;
        Vector2 bHalf = bSize * 0.5f;

        if (Mathf.Abs(aPos.x - bPos.x) > (aHalf.x + bHalf.x)) return false;
        if (Mathf.Abs(aPos.y - bPos.y) > (aHalf.y + bHalf.y)) return false;
        return true;
    }

    private static bool CollidesCircleCircle(Vector2 aPos, float aR, Vector2 bPos, float bR)
    {
        float r = aR + bR;
        return (aPos - bPos).sqrMagnitude <= r * r;
    }

    private static bool CollidesCircleAabb(Vector2 cPos, float cR, Vector2 bPos, Vector2 bSize)
    {
        Vector2 half = bSize * 0.5f;

        // Closest point on the box to the circle center
        float closestX = Mathf.Clamp(cPos.x, bPos.x - half.x, bPos.x + half.x);
        float closestY = Mathf.Clamp(cPos.y, bPos.y - half.y, bPos.y + half.y);
        Vector2 closest = new Vector2(closestX, closestY);

        return (cPos - closest).sqrMagnitude <= cR * cR;
    }

    private static void Collide(ref Entity a, ref Entity b, Context context)
    {
        DoEntityCollision(ref a, context);
        DoEntityCollision(ref b, context);            
    }

    private static void DoEntityCollision(ref Entity entity, Context context)
    {
        switch(entity.entityType)
        {
            case EntityType.ASTEROID_SMALL:
            case EntityType.ASTEROID_LARGE:
                DoEntityCollision_Asteroid(ref entity, context);
            break;
            case EntityType.SHIP_ROOM:
                DoEntityCollision_Room(ref entity, context);
            break;
            case EntityType.SHIELD:
                DoEntityCollision_Shield(ref entity, context);
            break;
            default:
                Debug.LogError($"Collision with {entity.entityType} not supported.");
            break;
        }
    }

    private static void DoEntityCollision_Room(ref Entity room, Context context)
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

    private static void DoEntityCollision_Asteroid(ref Entity asteroid, Context context)
    {
        asteroid.cleanup = true;

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

    private static void DoEntityCollision_Shield(ref Entity shield, Context context)
    {
        
    }

    public static void Update(Context context)
    {
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].collisionSize == Vector2.zero && 
                context.entities[i].collisionRadius == 0f )
            {
                continue;
            }

            switch(context.entities[i].collisionType)
            {
                case CollisionType.SQAURE:
                DebugUtils.DrawRect(
                    context.entities[i].position, 
                    context.entities[i].collisionSize, 
                    Color.green);
                break;
                case CollisionType.CIRCLE:
                DebugUtils.DrawCircle(
                    context.entities[i].position, 
                    context.entities[i].collisionRadius, 
                    Color.green);
                break;
            }
            
            
        }
    }
}