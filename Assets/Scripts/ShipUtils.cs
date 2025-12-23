using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShipAggregate
{
    public List<int> roomIds;   
}

public static class ShipUtils
{
    private static readonly List<Entity> tmpRooms = new();
    public static List<Entity> GetShipRooms(int shipId, in List<Entity> entities)
    {
        for(int i = 0; i < entities.Count; i++)
        {
            if( entities[i].tags.HasAny(EntityTag.Room) && entities[i].parentId == shipId )
                tmpRooms.Add(entities[i]);
        }

        return tmpRooms;
    }

    public static void AddShieldRoom(int shipId, in Context context)
    {
        int width  = Rand.Range(6, 10);
        int height = Rand.Range(6, 10);

        Rect roomRect = GetBestRoomRect(shipId, width, height, context, out int roomId);

        // if we can't place a room something went wrong
        Debug.Assert(!roomRect.Equals(default));

        CreateRoom(shipId, roomId, roomRect, context);
    }

    private static void CreateRoom(int shipId, int roomid, Rect bestRect, Context context)
    {
        var room = new Entity();
        room.entityType = EntityType.SHIP_ROOM;
        room.id = roomid;
        room.parentId = shipId;
        room.position = bestRect.position + new Vector2(bestRect.width/2f, bestRect.height/2f);
        room.collisionSize = new Vector2(bestRect.width, bestRect.height);
        room.collisionLayer = CollisionLayer.Ship;
        room.collideWithMask = CollisionLayer.Asteroid;
        room.hitPoints = DamageTuning.RoomHitpoints;
        room.tags = EntityTag.Room;
        room.roomBounds = bestRect;

        for(int x = 0; x < bestRect.width; x++)
        {
            for(int y = 0; y < bestRect.height; y++)
            {
                bool wall = x == 0 || x == bestRect.width - 1 || y == 0 || y == bestRect.height -1;

                Entity entity = new()
                {
                    entityType = wall ? EntityType.SHIP_WALL : EntityType.SHIP_FLOOR,
                    drawSize = Vector2.one,
                    position = new(bestRect.x + x + 0.5f, bestRect.y + y + 0.5f),
                    parentId = room.id,
                    tags = wall ? EntityTag.Wall : EntityTag.Floor  
                };

                context.entities.Add(entity);
            }
        }

        context.entities.Add(room);
    }

    private static Rect GetBestRoomRect(int shipId, int width, int height, Context context, out int roomId)
    {
        float bestScore = 0f;
        Rect  bestRect = default;

        var rooms = GetShipRooms(shipId, context.entities);

        int tries = 200;
        while( tries-- > 0 )
        {
            var room = rooms[Rand.Range(0, rooms.Count)];
            var roomRect = room.roomBounds;
            
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

            var score = RoomPositionScore(rect, rooms);
            if( score > 0 && score > bestScore )
            {
                bestRect = rect;
                bestScore = score;
            }
        }

        roomId = rooms.Count;

        return bestRect;
    }

    private static float RoomPositionScore(Rect rect, in List<Entity> rooms)
    {
        Debug.Assert(!rect.Equals(default));

        const float ALIGN_BONUS = 2f;   // how much you reward perfect alignment
        const float ALIGN_MAX = 6f;     // diff at which bonus becomes 0
        const float CLOSENESS_WEIGHT = 10f; // weight it relative to your other terms

        float score = 0f;

        // Cannot use if we overlap another room
        for(int i = 0; i < rooms.Count; i++)
        {
            Rect otherRect = rooms[i].roomBounds;
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
                    sumDist += Vector2.Distance(rooms[i].roomBounds.center, rect.center);
                }

                float distanceScore = 1f / (1f + sumDist);
                score += distanceScore * CLOSENESS_WEIGHT;
            }
            
        }

        return score;
    }
}