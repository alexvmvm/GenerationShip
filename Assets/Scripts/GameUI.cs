using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUI
{
    public static void OnGUI(Context context)
    {
        // Update positions
        // for(int i = 0; i < context.entities.Count; i++)
        // {
        //     switch(context.entities[i].entityType)
        //     {
        //         case EntityType.SHIP_ROOM:
        //             DrawRoomUI(context.entities[i]);
        //         break;
        //     }
        // }

        if( Find.Game.Mode == GameMode.Playing )
        {
            if( context.isDestroyed )
                DoGameOver();
            else if( !context.isMoving )
                DoChooseDestination();
            else
                DoRunProgress();
        }
    }

    private static void DoChooseDestination()
    {
        const float Width = 300;
        const float Height = 150;

        var rect = new Rect(
            Screen.width/2f - Width/2f, 
            Screen.height - Height - UI.Gap, 
            Width, Height);
        
        if( UI.Button(rect, "Choose destination") )
        {
            Find.Game.SetMode(GameMode.Map);
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
}