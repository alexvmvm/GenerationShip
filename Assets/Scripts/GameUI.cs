using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameUI
{
    public static void OnGUI(Context context)
    {
        if( Find.Game.Mode == GameMode.Playing )
        {
            if( context.isSuccess )
                DoSuccess();
            else if( context.isDestroyed )
                DoGameOver();
            else if( !context.isMoving )
            {
                const float Width = 600;
                const float Height = 150;

                var rect = new Rect(
                    Screen.width/2f - Width/2f, 
                    Screen.height - Height - UI.Gap, 
                    Width, Height);

                rect.SplitHorizontallyPercent(out Rect left, out Rect right);
                    
                DoChooseDestination(right);
                DoUpgradeShip(left);
            }
            else
            {
                DoRunProgress();
                DoPlayPause();
            }
        }
        else if( Find.Game.Mode is GameMode.Map or GameMode.ShipEditor )
        {
            if( Input.GetKeyDown(KeyCode.Escape) )
                Find.Game.SetMode(GameMode.Playing);
        }

        if( Find.Game.Mode is GameMode.Playing or GameMode.ShipEditor )
        {
            DoResources();
            DoHealthBar(context);
        }
    }

    private static void DoChooseDestination(Rect rect)
    {
        if( UI.Button(rect, "Choose destination") )
        {
            Find.Game.SetMode(GameMode.Map);
        }
    }

    private static void DoUpgradeShip(Rect rect)
    {
        if( UI.Button(rect, "Upgrade ship") )
        {
            Find.Game.SetMode(GameMode.ShipEditor);
        }
    }

    private static void DoGameOver()
    {
        const float Width = 200;
        const float Height = 200;

        var rect = new Rect(
            Screen.width/2f - Width/2f, 
            Screen.height/2f - Height/2f, 
            Width, Height);
        
        UI.WordWrap = true;
        UI.TextAlignment = TextAnchor.MiddleCenter;
        UI.Label(rect, "Your ship has been destroyed. The last remnants of humanity are lost forever.");
        UI.TextAlignment = TextAnchor.UpperLeft;
        UI.WordWrap = false;
    }

    private static void DoSuccess()
    {
        // text
        {
            const float Width = 200;
            const float Height = 200;

            var rect = new Rect(
                Screen.width/2f - Width/2f, 
                Screen.height/2f - Height/2f, 
                Width, Height);
            
            UI.WordWrap = true;
            UI.TextAlignment = TextAnchor.MiddleCenter;
            UI.Label(rect, "Your ship has reached the edge of the solar system.\nBeyond lies the vast intergalactic void—and with it, a fragile hope for humanity.");
            UI.TextAlignment = TextAnchor.UpperLeft;
            UI.WordWrap = false;
        }

        // reset
        {
            const float Width = 300;
            const float Height = 150;

            var rect = new Rect(
                Screen.width/2f - Width/2f, 
                Screen.height - Height - UI.Gap, 
                Width, Height);

            if( UI.Button(rect, "Restart") )
                Find.Game.Reset();
        }
    }

    private static void DoRunProgress()
    {
        const float Width = 300;
        const float Height = 40;

        var rect = new Rect(
            Screen.width/2f - Width/2f, 
            UI.Gap, 
            Width, Height);
        
        UI.ProgressBar(rect, Run.RunPercentComplete);
        
        Texture2D ship = ResourceCache.Texture("Textures/ship-icon");
        
        float width = Height * (ship.width / ship.height);
        Rect shipRect = new Rect(
            rect.x + rect.width * Run.RunPercentComplete - (width/2f), 
            UI.Gap, 
            width, 
            Height);
        
        UI.DrawTexture(shipRect, ship, tint: Color.gray);
    }

    private static void DoResources()
    {
        const float Width = 200;
        const float Height = 200;

        var rect = new Rect(
            Screen.width - Width - UI.Gap2x, 
            0, 
            Width, Height);
        
        UI.TextSize = TextSize.Large;
        UI.TextAlignment = TextAnchor.UpperRight;
        UI.Label(rect, Find.Game.Resources.ToString());
        UI.TextAlignment = TextAnchor.UpperLeft;
        UI.TextSize = TextSize.Small;
    }

    
    private static void DoPlayPause()
    {
        const float Size = 50;
        
        var pause = new Rect(Screen.width - Size, Screen.height - Size, Size, Size);
        if( UI.Button(pause, ResourceCache.Get<Texture2D>("Textures/pause")))
        {
            Find.Game.Pause();
        }

        var play = pause;
        play.x -= Size;
        if( UI.Button(play, ResourceCache.Get<Texture2D>("Textures/play")))
        {
            Find.Game.Play();
        }
    }

    private static List<Entity> tmpRooms = new();
    private static void DoHealthBar(Context context)
    {
        const float ScreenBufferLeft = 10f;
        const float ScreenBufferTop = 10f;
        const float BarGap = 3f;
        const float Width = 600f;
        const float Height = 30f;

        int shipId = Find.Game.ShipId;

        tmpRooms.Clear();

        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].parentId == shipId 
                && context.entities[i].tags.HasAny(EntityTag.Room)
                && !context.entities[i].isBeingPlaced )
            {
                tmpRooms.Add(context.entities[i]);
            }
        }

        int aliveCount = 0;
        int coreAliveCount = 0;
        for(int i = 0; i < tmpRooms.Count; i++)
        {
            if( tmpRooms[i].hitPoints <= 0 )
                continue;

            aliveCount++;
            if( tmpRooms[i].tags.HasAny(EntityTag.ShipCore) )
                coreAliveCount++;
        }

        if( aliveCount == 0 )
            return;

        int nonCoreAliveCount = aliveCount - coreAliveCount;
        float totalMax = DamageTuning.RoomHitpoints * aliveCount;
        if( totalMax <= 0f )
            return;

        float gap = BarGap;
        float totalGap = gap * (aliveCount - 1);
        float availableWidth = Mathf.Max(0f, Width - totalGap);

        var rect = new Rect(ScreenBufferLeft, ScreenBufferTop, Width, Height);
        float cursorX = rect.xMin;

        void DrawRoomBar(Entity room, bool isCore)
        {
            float maxHp = DamageTuning.RoomHitpoints;
            float segmentWidth = availableWidth * (maxHp / totalMax);
            float t = Mathf.Clamp01(room.hitPoints / maxHp);
            Color fill = isCore
                ? Color.Lerp(new Color(0.2f, 0.4f, 1f), new Color(0f, 0.9f, 1f), t)
                : Color.Lerp(Color.red, Color.green, t);


            var bar = new Rect(cursorX, rect.yMin, segmentWidth, rect.height);
            UI.ProgressBar(
                bar,
                t,
                backgroundColor: new Color(0f, 0f, 0f, 0.35f),
                fillColor: fill);

            cursorX += segmentWidth + gap;
        }

        for(int i = 0; i < tmpRooms.Count; i++)
        {
            Entity room = tmpRooms[i];
            if( room.hitPoints <= 0 )
                continue;
            if( !room.tags.HasAny(EntityTag.ShipCore) )
                continue;

            DrawRoomBar(room, isCore: true);
        }

        if( coreAliveCount > 0 && nonCoreAliveCount > 0 )
            cursorX -= gap;

        for(int i = 0; i < tmpRooms.Count; i++)
        {
            Entity room = tmpRooms[i];
            if( room.hitPoints <= 0 )
                continue;
            if( room.tags.HasAny(EntityTag.ShipCore) )
                continue;

            DrawRoomBar(room, isCore: false);
        }
    }
}
