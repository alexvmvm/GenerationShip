using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct Ship
{
    public int shipId;
    
}

public static class ShipUtils
{
    private static readonly List<Entity> rooms = new();
    public static List<Entity> GetShipRooms(int shipId, in List<Entity> entities)
    {
        for(int i = 0; i < entities.Count; i++)
        {
            if( entities[i].tags.HasAny(EntityTag.Room) && entities[i].parentId == shipId )
                rooms.Add(entities[i]);
        }

        return rooms;
    }

    public static void AddShieldRoom(int shipId, in Context context)
    {
        var rooms = GetShipRooms(shipId, context.entities);

        const int RoomWidth = 4;
        const int RoomHeight = 4;

        float bestScore = 0f;
        Rect  bestRect = default;

        for(int i = 0; i < rooms.Count; i++)
        {
            var roomRect = rooms[i].roomBounds;

            for(int x = Mathf.FloorToInt(roomRect.xMin); x < roomRect.xMax; x++)
            {
                var above = new Rect(x, Mathf.FloorToInt(roomRect.yMin) - RoomHeight, RoomWidth, RoomHeight);
                var aboveScore = RoomPositionScore(above, rooms);
                if( aboveScore > 0f && aboveScore > bestScore )
                {
                    bestRect = above;
                    bestScore = aboveScore;
                }
                
                var below = new Rect(x, Mathf.FloorToInt(roomRect.yMax), RoomWidth, RoomHeight);
                var belowScore = RoomPositionScore(below, rooms);
                if( belowScore > 0 && belowScore > bestScore )
                {
                    bestRect = below;
                    bestScore = belowScore;
                }
            }
        }

        Debug.Assert(!bestRect.Equals(default));

        var room = new Entity();
        room.entityType = EntityType.SHIP_ROOM;
        room.id = rooms.Count;
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
                Entity entity = new()
                {
                    entityType = EntityType.SHIP_FLOOR,
                    drawSize = Vector2.one,
                    position = new(bestRect.x + x + 0.5f, bestRect.y + y + 0.5f),
                    parentId = room.id  
                };

                context.entities.Add(entity);
            }
        }

        context.entities.Add(room);
    }

    private static float RoomPositionScore(Rect rect, in List<Entity> rooms)
    {
        Debug.Assert(!rect.Equals(default));

        float score = 0f;

        // Cannot use if we overlap another room
        for(int i = 0; i < rooms.Count; i++)
        {
            if( rooms[i].roomBounds.Overlaps(rect) )
                return 0f;

            score += RectUtils.AdjacentCellCount(rect, rooms[i].roomBounds);
        }

        return score;
    }
}