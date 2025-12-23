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
    public Vector2 centroid;
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

    public static void AddShieldRoom(int shipId, in Context context)
    {
        Rect roomRect = GetBestRoomRect(shipId, 6, 6, context);

        // if we can't place a room something went wrong
        Debug.Assert(!roomRect.Equals(default));

        Entity room = CreateRoom(shipId, roomRect, context, EntityTag.Shield);

        context.entities.Add(new Entity()
        {
           entityType = EntityType.SHIP_SHIELD,
           drawSize = Vector2.one,
           position = roomRect.center,
           sortingOrder = 1,
           parentId = room.id,
           tags = EntityTag.Shield
        });
    }

    public static Entity CreateRoom(int shipId, Rect roomRect, Context context, EntityTag tags)
    {
        var room = new Entity
        {
            entityType = EntityType.SHIP_ROOM,
            id = Rand.IntPositive,
            parentId = shipId,
            position = roomRect.position + new Vector2(roomRect.width / 2f, roomRect.height / 2f),
            collisionSize = new Vector2(roomRect.width, roomRect.height),
            collisionLayer = CollisionLayer.Ship,
            collideWithMask = CollisionLayer.Asteroid,
            hitPoints = DamageTuning.RoomHitpoints,
            tags = EntityTag.Room | tags,
            roomBounds = roomRect
        };

        context.entities.Add(room);

        // place the floors
        for (int x = 0; x < roomRect.width; x++)
        {
            for(int y = 0; y < roomRect.height; y++)
            {
                Vector2 position = new(roomRect.x + x + 0.5f, roomRect.y + y + 0.5f);

                Entity entity;

                if( x == 0 || y == 0 || x == roomRect.width - 1 || y == roomRect.height - 1)
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
                
                context.entities.Add(entity);
            }
        }

        return room;
    }

    private static Rect GetBestRoomRect(int shipId, int width, int height, Context context)
    {
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