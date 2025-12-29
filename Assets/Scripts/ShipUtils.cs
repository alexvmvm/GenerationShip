using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShipData
{
    public List<int> roomIds;   
    public List<Rect> roomRects;
    public List<Entity> rooms;
    public Dictionary<Vector2Int, Entity> structureByPosition;
}

public static class ShipUtils
{
    public static ShipData GetShipData(int shipId, in List<Entity> entities)
    {
        var ship = new ShipData();
        ship.roomIds = new();
        ship.roomRects = new();
        ship.rooms = new();
        ship.structureByPosition = new();

        for(int i = 0; i < entities.Count; i++)
        {
            if( entities[i].parentId == shipId )
            {
                if( entities[i].tags.HasAny(EntityTag.Room) )
                {
                    Rect roomRect = entities[i].roomBounds;
                    
                    ship.roomIds.Add(entities[i].id);
                    ship.roomRects.Add(roomRect);
                    ship.rooms.Add(entities[i]);
                }
            }
        }

        for(int i = 0; i < entities.Count; i++)
        {
            if( !entities[i].tags.HasAny(EntityTag.Wall | EntityTag.Floor) )
                continue;
            
            if( !ship.roomIds.Contains(entities[i].parentId) )
                continue;
            
            Vector2Int p = entities[i].position.ToVector2IntFloor();

            ship.structureByPosition.Add(p, entities[i]);
        }

        return ship;
    }

    public static List<Entity> CreateShipRoom(int shipId, EntityType entityType, Vector2 root)
    {
        Debug.Assert(
            entityType == EntityType.SHIP_ROOM_SHIELD || 
            entityType == EntityType.SHIP_ROOM_TURRET || 
            entityType == EntityType.SHIP_ROOM_ENGINE, 
            "non-room entity type passed to CreateRoom");

        var (width, height) = RoomSize(entityType);

        var rect = new Rect(root.x, root.y, width, height);

        var entities = new List<Entity>();

        var room = new Entity
        {
            entityType = entityType,
            id = Rand.IntPositive,
            parentId = shipId,
            position = rect.position + (rect.size/2f),
            collisionSize = new Vector2(width, height),
            collisionLayer = CollisionLayer.Ship,
            collideWithMask = CollisionLayer.Asteroid,
            hitPoints = DamageTuning.RoomHitpoints,
            tags = RoomTags(entityType),
            roomBounds = rect
        };

        entities.Add(room);

        // place the floors
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector2 position = new(rect.x + x + 0.5f, rect.y + y + 0.5f);

                Entity entity;

                if( x == 0 || y == 0 || x == rect.width - 1 || y == rect.height - 1)
                {
                    entity = new()
                    {
                        entityType = EntityType.SHIP_WALL,
                        drawSize = Vector2.one,
                        position = position,
                        parentId = room.id,
                        tags = EntityTag.Wall  
                    };
                }
                else
                {
                    entity = new()
                    {
                        entityType = EntityType.SHIP_FLOOR,
                        drawSize = Vector2.one,
                        position = position,
                        parentId = room.id,
                        tags = EntityTag.Floor  
                    };
                }
                
                entities.Add(entity);
            }
        }

        entities.AddRange(GetRoomBuildings(entityType, rect, room.id));

        return entities;
    }

    private static List<Entity> GetRoomBuildings(EntityType roomType, Rect rect, int roomId)
    {
        var entities = new List<Entity>();

        switch(roomType)
        {
            case EntityType.SHIP_ROOM_ENGINE:
            
                entities.Add(new Entity()
                {
                    entityType = EntityType.SHIP_ENGINE,
                    drawSize = Vector2.one,
                    position = new(rect.xMin + 1.5f, rect.yMin + 0.5f),
                    sortingOrder = 1,
                    parentId = roomId,
                    tags = EntityTag.Engine
                });

                entities.Add(new Entity()
                {
                    entityType = EntityType.SHIP_ENGINE,
                    drawSize = Vector2.one,
                    position = new(rect.xMax - 1.5f, rect.yMin + 0.5f),
                    sortingOrder = 1,
                    parentId = roomId,
                    tags = EntityTag.Engine
                });

            break;
            case EntityType.SHIP_ROOM_TURRET:

                entities.Add(new Entity()
                {
                    id = Rand.IntPositive,
                    entityType = EntityType.SHIP_TURRET,
                    drawSize = Vector2.one,
                    position = rect.center,
                    sortingOrder = 1,
                    parentId = roomId,
                    tags = EntityTag.Turret
                });

                entities.Add(new Entity()
                {
                    entityType = EntityType.SHIP_TURRET_TOP,
                    drawSize = Vector2.one,
                    position = rect.center,
                    sortingOrder = 2,
                    parentId = roomId,
                    tags = EntityTag.Shield,
                    hitPoints = 100,
                    shieldRadius = 8,
                });

            break;
            case EntityType.SHIP_ROOM_SHIELD:

                entities.Add(new Entity()
                {
                    entityType = EntityType.SHIP_SHIELD,
                    drawSize = Vector2.one,
                    position = rect.center,
                    sortingOrder = 1,
                    parentId = roomId,
                    tags = EntityTag.Shield
                });

                entities.Add(new Entity()
                {
                    entityType = EntityType.SHIELD,
                    drawSize = Vector2.one,
                    position = rect.center,
                    sortingOrder = 1,
                    parentId = roomId,
                    tags = EntityTag.Shield,
                    hitPoints = 100,
                    shieldRadius = 8,
                    collisionType = CollisionType.CIRCLE,
                    collisionRadius = 8,
                    collisionLayer = CollisionLayer.Ship,   
                    collideWithMask = CollisionLayer.Asteroid,
                });

            break;
        }

        return entities;
    }

    private static (int width, int height) RoomSize(EntityType type) => type switch
    {
        EntityType.SHIP_ROOM_SHIELD => (6, 6),
        EntityType.SHIP_ROOM_TURRET => (8, 8),
        EntityType.SHIP_ROOM_ENGINE => (6, 8),
        _ => throw new NotImplementedException("Unknown room type " + type) 
    };

    private static EntityTag RoomTags(EntityType type) => type switch
    {
        EntityType.SHIP_ROOM_SHIELD => EntityTag.Room | EntityTag.Shield,
        EntityType.SHIP_ROOM_TURRET => EntityTag.Room | EntityTag.Turret,
        EntityType.SHIP_ROOM_ENGINE => EntityTag.Room | EntityTag.Engine,
        _ => throw new NotImplementedException("Unknown room type " + type) 
    };

    public static Rect GetBestRoomRect(int shipId, EntityType roomType, Context context)
    {
        var (width, height) = RoomSize(roomType);

        float bestScore = 0f;
        Rect  bestRect = default;

        var ship = GetShipData(shipId, context.entities);

        int tries = 200;
        while( tries-- > 0 )
        {
            var roomRect = ship.roomRects[Rand.Range(0, ship.roomRects.Count)];
            
            Rect rect;
            if( Rand.Bool )
            {
                int x = Rand.Range(Mathf.FloorToInt(roomRect.xMin), Mathf.FloorToInt(roomRect.xMax));
                int y = Rand.Bool ? Mathf.FloorToInt(roomRect.yMin) - height : Mathf.FloorToInt(roomRect.yMax);

                rect = new Rect(x, y, width, height);
            }
            else
            {
                int x = Rand.Bool ? Mathf.FloorToInt(roomRect.xMin) - width : Mathf.FloorToInt(roomRect.xMax);
                int y = Rand.Range(Mathf.FloorToInt(roomRect.yMin), Mathf.FloorToInt(roomRect.yMax));

                rect = new Rect(x, y, width, height);
            }

            var score = RoomPositionScore(rect, ship);
            if( score > 0 && score > bestScore )
            {
                bestRect = rect;
                bestScore = score;
            }
        }

        return bestRect;
    }

    private static float RoomPositionScore(Rect rect, ShipData shipData)
    {
        Debug.Assert(!rect.Equals(default));

        const float ALIGN_BONUS = 2f;   // how much you reward perfect alignment
        const float ALIGN_MAX = 6f;     // diff at which bonus becomes 0
        const float CLOSENESS_WEIGHT = 10f; // weight it relative to your other terms

        float score = 0f;
        
        var rooms = shipData.roomRects;

        // Cannot use if we overlap another room
        for(int i = 0; i < rooms.Count; i++)
        {
            Rect otherRect = rooms[i];
            if( otherRect.Overlaps(rect) )
                return 0f;

            score += RectUtils.AdjacentCellCount(rect, otherRect);

            // alignent score
            {
                float xDiff = Mathf.Abs(rect.center.x - otherRect.center.x);
                float yDiff = Mathf.Abs(rect.center.y - otherRect.center.y);

                float xAlign = Mathf.Clamp01(1f - (xDiff / ALIGN_MAX)); // 1 at 0, 0 at ALIGN_MAX+
                float yAlign = Mathf.Clamp01(1f - (yDiff / ALIGN_MAX));

                score += ALIGN_BONUS * (xAlign + yAlign);
            }

            var centroid = Vector2.zero;
            for(int j = 0; j < rooms.Count; j++)
            {
                centroid += rooms[i].center;
            }
            centroid /= rooms.Count;

            // distance score
            {
                float distanceScore = 1f / (1f + Vector2.Distance(centroid, rect.center));
                score += distanceScore * CLOSENESS_WEIGHT;
            }
            
        }

        return score;
    }
}