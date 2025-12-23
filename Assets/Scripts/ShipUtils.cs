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
    private static readonly Vector2[] AdjacentDirections = new Vector2[8]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
        Vector2.up + Vector2.left,
        Vector2.up + Vector2.right,
        Vector2.down + Vector2.left,
        Vector2.down + Vector2.right
    };

    private static ShipData GetShipData(int shipId, in List<Entity> entities)
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
                    ship.roomIds.Add(entities[i].id);
                    ship.roomRects.Add(entities[i].roomBounds);
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
        int width  = Rand.Range(6, 10);
        int height = Rand.Range(6, 10);

        Rect roomRect = GetBestRoomRect(shipId, width, height, context, out int roomId);

        // if we can't place a room something went wrong
        Debug.Assert(!roomRect.Equals(default));

        CreateRoom(shipId, roomId, roomRect, context);
        CreateWalls(shipId, context);
    }

    private static void CreateRoom(int shipId, int roomId, Rect roomRect, Context context)
    {
        var room = new Entity
        {
            entityType = EntityType.SHIP_ROOM,
            id = roomId,
            parentId = shipId,
            position = roomRect.position + new Vector2(roomRect.width / 2f, roomRect.height / 2f),
            collisionSize = new Vector2(roomRect.width, roomRect.height),
            collisionLayer = CollisionLayer.Ship,
            collideWithMask = CollisionLayer.Asteroid,
            hitPoints = DamageTuning.RoomHitpoints,
            tags = EntityTag.Room,
            roomBounds = roomRect
        };

        context.entities.Add(room);

        // place the floors
        for (int x = 0; x < roomRect.width; x++)
        {
            for(int y = 0; y < roomRect.height; y++)
            {
                Vector2 position = new(roomRect.x + x + 0.5f, roomRect.y + y + 0.5f);

                Entity entity = new()
                {
                    entityType = EntityType.SHIP_FLOOR,
                    drawSize = Vector2.one,
                    position = position,
                    parentId = room.id,
                    tags = EntityTag.Floor  
                };
                
                context.entities.Add(entity);
            }
        }
    }

    private static void CreateWalls(int shipId, Context context)
    {
        var ship = GetShipData(shipId, in context.entities);

        bool ShouldPlaceWall(Vector2 position)
        {
            for(int i = 0; i < AdjacentDirections.Length; i++)
            {
                if( !ship.structureByPosition.TryGetValue((position + AdjacentDirections[i]).ToVector2IntFloor(), out Entity entity) )
                    return true;
            }

            return false;
        }

        // place walls
        {
            for(int i = 0; i < context.entities.Count; i++)
            {
                if( context.entities[i].entityType != EntityType.SHIP_FLOOR )
                    continue;
                
                if( !ship.roomIds.Contains(context.entities[i].parentId) )
                    continue;

                if( ShouldPlaceWall(context.entities[i].position) )
                {
                    Entity floor = context.entities[i];
                    floor.entityType = EntityType.SHIP_WALL;
                    floor.tags = EntityTag.Floor;
                    context.entities[i] = floor;
                } 
            }
        }
    }

    private static Rect GetBestRoomRect(int shipId, int width, int height, Context context, out int roomId)
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

            var score = RoomPositionScore(rect, ship.roomRects);
            if( score > 0 && score > bestScore )
            {
                bestRect = rect;
                bestScore = score;
            }
        }

        roomId = ship.roomRects.Count;

        return bestRect;
    }

    private static float RoomPositionScore(Rect rect, in List<Rect> rooms)
    {
        Debug.Assert(!rect.Equals(default));

        const float ALIGN_BONUS = 2f;   // how much you reward perfect alignment
        const float ALIGN_MAX = 6f;     // diff at which bonus becomes 0
        const float CLOSENESS_WEIGHT = 10f; // weight it relative to your other terms

        float score = 0f;

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

            // distance score
            {
                float sumDist = 0f;
                for(int j = 0; j < rooms.Count; j++)
                {   
                    sumDist += Vector2.Distance(rooms[i].center, rect.center);
                }

                float distanceScore = 1f / (1f + sumDist);
                score += distanceScore * CLOSENESS_WEIGHT;
            }
            
        }

        return score;
    }
}