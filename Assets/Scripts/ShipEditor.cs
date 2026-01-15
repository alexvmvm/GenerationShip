using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum BuildCategory
{
    WEAPON,
    DEFENCE
}

public struct Buildable
{
    public string label;
    public int cost;
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
    private static Buildable? selectedBuildable;
    private static BuildCategory? category;

    private static List<Buildable> buildables = new();

    static ShipEditor()
    {
        buildables.Clear();
        buildables.Add(new Buildable
        {
            category   = BuildCategory.WEAPON,
            entityType = EntityType.SHIP_ROOM_TURRET,
            cost = 50,
            label = "turret"
        });
        buildables.Add(new Buildable
        {
            category   = BuildCategory.DEFENCE,
            entityType = EntityType.SHIP_ROOM_SHIELD,
            cost = 40,
            label = "shield"
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

        if( selectedBuildable is not Buildable )
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
    }

    public static void OnGUI(in Context context)
    {
        if( Find.Game.Mode != GameMode.ShipEditor )
        {
            selectedBuildable = null;
            category = null;
            ClearBuildEntities(context);
            return;
        }

        bool mouseOverEditorUI = false;
        Vector2 mousePosGUI = Event.current.mousePosition;

        var buildablesByCategory = buildables.GroupBy(e => e.category);

        const float CatBtnWidth = 200;
        const float CatBtnHeight = 60;

        float y = Screen.height/2f - buildablesByCategory.Count() * CatBtnHeight/2f;

        foreach(var cat in buildablesByCategory)
        {
            Rect rect = new Rect(UI.Gap, y, CatBtnWidth, CatBtnHeight);

            if( rect.Contains(mousePosGUI) )
                mouseOverEditorUI = true;

            if( UI.Button(rect, cat.Key.ToStringHuman()))
            {
                category = cat.Key;
            }

            y += rect.height;
        }   

        // exit btn
        {
            const float CloseBtnWidth = 150;
            const float CloseBtnHeight = 75;

            var closeRect = new Rect(
                Screen.width/2f - CloseBtnWidth/2f, 
                Screen.height - CloseBtnHeight,
                CloseBtnWidth,
                CloseBtnHeight);

            if( closeRect.Contains(mousePosGUI) )
                mouseOverEditorUI = true;

            if( UI.Button(closeRect, "Exit") )
                Find.Game.SetMode(GameMode.Playing);
        }

        if( category is BuildCategory selectedCategory )
            mouseOverEditorUI |= DoBuildableCategory(UI.Gap + CatBtnWidth, selectedCategory, context);

        if( Event.current.type == EventType.MouseDown 
            && Event.current.button == 0 
            && !mouseOverEditorUI )
        {
            TryPlace(context);
            Event.current.Use();
        }

        if( Event.current.type == EventType.MouseDown 
            && Event.current.button == 1 )
        {
            selectedBuildable = null;
            ClearBuildEntities(context);
            Event.current.Use();
        }
    }

    private static bool DoBuildableCategory(float x, BuildCategory category, Context context)
    {
        const float CatBtnWidth = 200;
        const float CatBtnHeight = 60;

        const float TipWidth = 200;
        const float TipHeight = 60;

        var selectedBuildables = buildables.Where(b => b.category == category);

        float y = Screen.height/2f - selectedBuildables.Count() * CatBtnHeight/2f;
        bool mouseOverEditorUI = false;

        foreach(Buildable buildable in selectedBuildables)
        {
            Rect rect = new Rect(x, y, CatBtnWidth, CatBtnHeight);

            if( rect.Contains(Event.current.mousePosition) )
                mouseOverEditorUI = true;

            if( UI.Button(rect, buildable.label.CapitalizeFirst()))
                SetBuildable(buildable, context);

            if( UI.MouseOver(rect) )
            {
                Rect mouseOverRect = new Rect(x + rect.width, y, TipWidth, TipHeight);
                UI.Box(mouseOverRect);

                mouseOverRect
                    .ContractBy(UI.Gap2x)
                    .SplitVerticallyAmount(out Rect labelRect, out Rect bottom, 20);
                
                UI.Label(labelRect, buildable.label.CapitalizeFirst());

                bottom.SplitVerticallyAmount(out Rect cost, out bottom, 20);
                UI.Label(cost, "Cost: " + buildable.cost.ToString());
            }
        }

        return mouseOverEditorUI;
    }

    private static void SetBuildable(Buildable buildable, Context context)
    {
        ClearBuildEntities(context);

        selectedBuildable = buildable;

        startIndex = context.entities.Count;
        mousePos = GetEntityRoot();

        foreach(Entity e in ShipUtils.CreateShipRoom(Find.Game.ShipId, buildable.entityType, mousePos))
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
        if( selectedBuildable is not Buildable buildable )
            return false;

        if( buildable.cost > Find.Game.Resources )
            return false;

        // check if we're going to overlap anything
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

    private static void TryPlace(Context context)
    {
        if( selectedBuildable is not Buildable buildable )
            return;

        if( !CanPlace(context) )
            return;

        for(int i = startIndex; i < context.entities.Count; i++)
        {
            Entity e = context.entities[i];
            e.drawColorOverride = null;
            e.isBeingPlaced = false;
            e.sortingOrder -= SortingOrder.BlueprintOffset;
            context.entities[i] = e;
        }

        startIndex = -1;

        Find.Game.SpendResource(buildable.cost);

        SetBuildable(buildable, context);
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
