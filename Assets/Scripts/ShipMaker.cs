using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShipMaker
{
    public static List<Entity> MakeShip(int width, int height)
    {
        var entities = new List<Entity>();

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Entity entity = new()
                {
                    entityType = EntityType.TILE,
                    drawSize = Vector2.one,
                    position = new(x, y)  
                };

                entities.Add(entity);
            }
        }

        entities.Add(new Entity()
        {
           entityType = EntityType.ENGINE,
           drawSize = Vector2.one,
           position = new(1, 0),
           sortingOrder = 1
        });

        entities.Add(new Entity()
        {
           entityType = EntityType.ENGINE,
           drawSize = Vector2.one,
           position = new(width - 2, 0),
           sortingOrder = 1
        });

        return entities;
    }        
}
