using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum BuildCategory
{
    WEAPON,
    DEFENCE
}

public struct Buildable
{
    public BuildCategory category;
    public EntityType entityType;
}

public static class BuildableUtils
{
    public static string ToStringHuman(this BuildCategory buildable) => buildable switch
    {
        BuildCategory.DEFENCE => "Defence",
        BuildCategory.WEAPON  => "Weapon",
        _                     => throw new NotImplementedException()
    };
}

public static class ShipEditor
{
    private static Vector2 mousePos;
    private static int startIndex = -1;
    private static EntityType? selectedEntityType;
    private static BuildCategory? category;

    private static List<Buildable> buildables = new();

    static ShipEditor()
    {
        buildables.Clear();
        buildables.Add(new Buildable
        {
            category   = BuildCategory.WEAPON,
            entityType = EntityType.SHIP_ROOM_TURRET
        });
        buildables.Add(new Buildable
        {
            category   = BuildCategory.DEFENCE,
            entityType = EntityType.SHIP_ROOM_SHIELD
        });
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

        if( selectedEntityType is not EntityType )
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

            startIndex = -1;

            Find.Game.SetMode(GameMode.Playing);
        }
    }

    public static void OnGUI(in Context context)
    {
        if( Find.Game.Mode != GameMode.ShipEditor )
        {
            selectedEntityType = null;
            category = null;
            ClearBuildEntities(context);
            
            return;
        }

        var buildablesByCategory = buildables.GroupBy(e => e.category);

        const float CatBtnWidth = 200;
        const float CatBtnHeight = 60;

        float y = Screen.height/2f - buildablesByCategory.Count() * CatBtnHeight/2f;

        foreach(var cat in buildablesByCategory)
        {
            Rect rect = new Rect(UI.Gap, y, CatBtnWidth, CatBtnHeight);

            if( UI.Button(rect, cat.Key.ToStringHuman()))
            {
                category = cat.Key;
            }

            y += rect.height;
        }   

        if( category is BuildCategory selectedCategory )
            DoBuildableCategory(UI.Gap + CatBtnWidth, selectedCategory, context);        
    }

    private static void DoBuildableCategory(float x, BuildCategory category, Context context)
    {
        const float CatBtnWidth = 200;
        const float CatBtnHeight = 60;

        var selectedBuildables = buildables.Where(b => b.category == category);

        float y = Screen.height/2f - selectedBuildables.Count() * CatBtnHeight/2f;

        foreach(Buildable buildable in selectedBuildables)
        {
            Rect rect = new Rect(x, y, CatBtnWidth, CatBtnHeight);

            if( UI.Button(rect, buildable.entityType.ToString()))
                SetEntityType(buildable.entityType, context);
        }
    }

    private static void SetEntityType(EntityType entityType, Context context)
    {
        if( selectedEntityType != null )
            ClearBuildEntities(context);

        selectedEntityType = entityType;

        startIndex = context.entities.Count;
        mousePos = GetEntityRoot();

        foreach(Entity e in ShipUtils.CreateShipRoom(Find.Game.ShipId, entityType, mousePos))
        {
            Entity entity = e;
            entity.isBeingPlaced = true;
            entity.sortingOrder += SortingOrder.BlueprintOffset;
            context.entities.Add(entity);
        }
    }

    private static void ClearBuildEntities(Context context)
    {
        if( startIndex < 0 )
            return;
        
        for(int i = context.entities.Count - 1; i >= startIndex; i--)
        {
            context.entities.RemoveAt(i);
        }

        startIndex = -1;
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