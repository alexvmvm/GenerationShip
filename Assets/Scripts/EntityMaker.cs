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
            sortingOrder = 2
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

    public static List<Entity> MakeShip(int width, int height)
    {
        var entities = new List<Entity>();
        
        var ship = new Entity();
        ship.entityType = EntityType.SHIP;
        ship.id = 0;

        var room = new Entity();
        room.entityType = EntityType.SHIP_ROOM;
        room.id = 1;
        room.parentId = ship.id;
        room.position = new Vector2(width/2f, height/2f);
        room.collisionSize = new Vector2(width, height);
        room.collisionLayer = CollisionLayer.Ship;
        room.collideWithMask = CollisionLayer.Asteroid;
        room.hitPoints = DamageTuning.RoomHitpoints;
        room.tags = EntityTag.Room;
        room.roomBounds = new Rect(0, 0, width, height);

        entities.Add(ship);
        entities.Add(room);

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Entity entity = new()
                {
                    entityType = EntityType.SHIP_FLOOR,
                    drawSize = Vector2.one,
                    position = new(x + 0.5f, y + 0.5f),
                    parentId = room.id  
                };

                entities.Add(entity);
            }
        }

        entities.Add(new Entity()
        {
           entityType = EntityType.SHIP_ENGINE,
           drawSize = Vector2.one,
           position = new(1.5f, 0.5f),
           sortingOrder = 1,
           parentId = room.id
        });

        entities.Add(new Entity()
        {
           entityType = EntityType.SHIP_ENGINE,
           drawSize = Vector2.one,
           position = new(width - 1.5f, 0.5f),
           sortingOrder = 1,
           parentId = room.id
        });

        return entities;
    }  
}
