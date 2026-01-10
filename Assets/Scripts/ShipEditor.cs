using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShipEditor
{
    private static Vector2 mousePos;
    private static int startIndex = -1;
    private static EntityType entityType = EntityType.None;

    public static void DoShipEditor(int shipId, EntityType entityType, Context gameContext)
    {
        Find.Game.SetMode(GameMode.ShipEditor);

        startIndex = gameContext.entities.Count;
        mousePos = GetEntityRoot();

        foreach(Entity e in ShipUtils.CreateShipRoom(shipId, entityType, mousePos))
        {
            Entity entity = e;
            entity.isBeingPlaced = true;
            entity.sortingOrder += SortingOrder.BlueprintOffset;
            gameContext.entities.Add(entity);
        }
    }   

    private static Vector2 GetEntityRoot()
    {
        Vector2 worldPos = Input.mousePosition.ScreenToWorld();

        return new Vector2(
            Mathf.FloorToInt(worldPos.x), 
            Mathf.FloorToInt(worldPos.y));
    }

    public static void Update(Context context)
    {
        if( Find.Game.Mode != GameMode.ShipEditor )
            return;

        GridRenderer.CellSize = 1f;
        GridRenderer.MajorEvery = 5;
        GridRenderer.LineWidth = 0.03f;
        GridRenderer.Origin = Vector2.zero;
        GridRenderer.Z = 5f; // tweak if it draws on top/behind incorrectly
        GridRenderer.Draw(Camera.main);

        if( entityType == EntityType.None )
            return;

        Vector2 pos = GetEntityRoot();
        

        bool canPlace = CanPlace(context);

        Vector2 offset = pos - mousePos;

        for(int i = startIndex; i < context.entities.Count; i++)
        {
            Entity e = context.entities[i];
            e.position += offset;
            e.drawColorOverride = canPlace ? 
                new Color(0f, 1f, 0f, 0.5f) :  // green
                new Color(1f, 0f, 0f, 0.5f);   // red
            
            context.entities[i] = e;
        }

        mousePos = pos;

        if( canPlace && Input.GetKeyDown(KeyCode.Mouse0) )
        {
            for(int i = startIndex; i < context.entities.Count; i++)
            {
                Entity e = context.entities[i];
                e.drawColorOverride = null;
                e.isBeingPlaced = false;
                e.sortingOrder -= SortingOrder.BlueprintOffset;
                context.entities[i] = e;
            }

            Find.Game.SetMode(GameMode.Playing);
        }
    }

    public static void OnGUI(in Context context)
    {
        if( Find.Game.Mode != GameMode.ShipEditor )
            return;

        
    }

    public static bool CanPlace(Context context)
    {
        for(int i = startIndex; i < context.entities.Count; i++)
        {
            Entity e = context.entities[i];
            
            for(int j = 0; j < context.entities.Count; j++)
            {
                if( i == j )
                    continue;
                
                Entity otherEntity = context.entities[j];

                if( !CanOverlap(e, otherEntity) )
                    return false;
            }
            
        }

        return true;
    }

    private static bool CanOverlap(Entity entity, Entity existing)
    {
        if( existing.tags.HasAny(EntityTag.Room) )
        {
            if( existing.roomBounds.Contains(entity.position) )
                return false;   
        } 

        return true;
    }
}