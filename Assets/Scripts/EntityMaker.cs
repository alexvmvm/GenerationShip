using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityMaker
{
    public static Entity MakeAsteroid(bool large)
    {
        return new Entity()
        {
            id = -1,
            entityType = large ? EntityType.ASTEROID_LARGE : EntityType.ASTEROID_SMALL,
            drawSize = Vector2.one,
            cleanupIfNotVisible = true,
            collisionSize = large ? Vector2.one * 2 : Vector2.one,
            collisionLayer = CollisionLayer.Asteroid,
            collideWithMask = CollisionLayer.Ship
        };
    }

    public static Entity MakeAsteroidFragment(bool large)
    {
        return new Entity()
        {
            id = -1,
            entityType = large ? EntityType.ASTEROID_FRAGMENT_LARGE : EntityType.ASTEROID_FRAGMENT_SMALL,
            drawSize = Vector2.one,
            cleanupIfNotVisible = true,
            sortingOrder = SortingOrder.AsteroidFragment
        };
    }

    public static Entity MakeBackgroundParticle()
    {
        return new Entity()
        {
            id = -1,
            entityType = EntityType.BACKDROP_PARTICLE,
            drawSize = Vector2.one,
            cleanupIfNotVisible = true,
        };
    }

    public static Entity MakeShip(int width, int height, Context context)
    {
        var ship = new Entity();
        ship.entityType = EntityType.SHIP;
        ship.id = Rand.IntPositive;

        context.entities.Add(ship);

        Rect rect = new Rect(-width/2f, -height/2f, width, height);

        context.entities.AddRange(ShipUtils.CreateShipRoom(ship.id, EntityType.SHIP_ROOM_ENGINE, rect.position));

        return ship;
    }  
}
