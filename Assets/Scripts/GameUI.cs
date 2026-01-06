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

        if( !context.isMoving && Find.Game.Mode == GameMode.Playing )
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
    }

    // private static void DrawRoomUI(in Entity room)
    // {
    //     Vector2 screenPos = room.position.WorldToGUI();
    //     Rect rect = new Rect(screenPos.x, screenPos.y, 100, 20);
    //     float percentHit = room.hitPoints / (float)DamageTuning.RoomHitpoints;
        
    //     UI.ProgressBar(rect, percentHit, 
    //         backgroundColor: Color.white, 
    //         fillColor: Color.red,
    //         outlineColor: Color.white,
    //         labelText: $"{room.hitPoints}/{DamageTuning.RoomHitpoints}");
    // }
}