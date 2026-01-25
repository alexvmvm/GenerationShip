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
            DoResources();
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
            
        }

        var play = pause;
        play.x -= Size;
        if( UI.Button(play, ResourceCache.Get<Texture2D>("Textures/play")))
        {
            
        }
    }
}