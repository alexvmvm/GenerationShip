using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

[Flags]
public enum Cardinal
{
    NORTH = 1 << 0,
    EAST  = 1 << 1,
    SOUTH = 1 << 2,
    WEST  = 1 << 3,
}


public enum InputType
{
    MOVE_PLAYER
}

public struct InputEvent
{
    public InputType inputType;
    public Cardinal direction;
}

public enum GameMode
{
    Playing,
    ShipEditor
}

public readonly struct Context
{
    public readonly List<Entity> entities;
    public readonly Rect worldScreenRect;

    public Context(List<Entity> entities, Rect worldScreenRect)
    {
        this.entities = entities;
        this.worldScreenRect = worldScreenRect;
    }
}

public class Game : MonoBehaviour
{
    // Config
    public const int TicksPerRealSecond = 60;
    static readonly float TICK_DT = 1f / TicksPerRealSecond;

    //Working vars
    private readonly List<Entity> entities = new();    
    private readonly Queue<InputEvent> events = new();
    private float tickAcc;
    private static int ticksGame;
    private int shipId;
    private GameMode gameMode = GameMode.Playing;

    //Props
    public static int TicksGame => ticksGame;
    private Context GameContext => new(entities, Camera.main.GetWorldRect());
    public GameMode Mode => gameMode;

    void Start()
    {
        Entity ship = EntityMaker.MakeShip(6, 10, GameContext);

        shipId = ship.id;
    }

    void Update()
    {
        // Gather input as fast as frames arrive
        GatherInput();

        // Run deterministic ticks at 60hz
        if( gameMode == GameMode.Playing)
        {
            tickAcc += Time.deltaTime;
            while (tickAcc >= TICK_DT)
            {
                Tick();
                tickAcc -= TICK_DT;
                ticksGame++;
            }
        }

        // Render / presentation (per-frame)
        DrawEntities();

        Context context = GameContext;

        Collisions.Update(context);
        ShipEditor.Update(context);
    }

    private readonly StringBuilder sb = new(1024);
    void OnGUI()
    {
        sb.Clear();
        sb.AppendLine($"Entities: {entities.Count}");

        GUI.Label(new Rect(10, 10, 600, 400), sb.ToString());

        Context context = GameContext;

        GameUI.OnGUI(context);
        Shields.OnGUI(in context);

        const int BtnWidth = 120;
        const int BtnHeight = 40;

        if( UI.Button(new Rect(Screen.width - BtnWidth - 10, Screen.height - BtnHeight - 10, BtnWidth, BtnHeight), "Add shield") )
            ShipEditor.DoShipEditor(shipId, EntityType.SHIP_ROOM_SHIELD, context);

        if( UI.Button(new Rect(Screen.width - BtnWidth - 10, Screen.height - 2 * BtnHeight - 20, BtnWidth, BtnHeight), "Add turret") )
            ShipEditor.DoShipEditor(shipId, EntityType.SHIP_ROOM_TURRET, context);
    }



    void Tick()
    {
        CleanupEntities();
        ConsumeInput();

        var context = GameContext;
        
        Damage.Tick(context);
        Movement.Tick(context);
        Collisions.Tick(context);
        Asteroids.Tick(context);
        BackgroupEffects.Tick(context);
        Shields.Tick(context);
        Turrets.Tick(context);
    }

    void GatherInput()
    {
        Cardinal move;

        if( Input.GetKeyDown(KeyCode.RightArrow) )
            move = Cardinal.EAST;
        else if( Input.GetKeyDown(KeyCode.LeftArrow) )
            move = Cardinal.WEST;
        else if( Input.GetKeyDown(KeyCode.UpArrow) )
            move = Cardinal.NORTH;
        else if( Input.GetKeyDown(KeyCode.DownArrow) )
            move = Cardinal.SOUTH;
        else
            return;

        InputEvent evnt = new()
        {
            direction = move,
            inputType = InputType.MOVE_PLAYER
        };

        this.events.Enqueue(evnt);
    }

    void ConsumeInput()
    {
        while( events.Count > 0 )
        {
            InputEvent e = events.Dequeue();
        }
    }

    void CleanupEntities()
    {
        Rect rect = CameraUtils.GetWorldRect(Camera.main).ExpandBy(20);

        for(int i = entities.Count - 1; i >= 0; i--)
        {
            if( entities[i].cleanup || (entities[i].cleanupIfNotVisible && !rect.Contains(entities[i].position)) )
                entities.RemoveAt(i);
        }
    }

    void DrawEntities()
    {
        for(int i = entities.Count - 1; i >= 0; i--)
        {
            Render.DrawEntity(entities[i]);    
        }
    }

    public void SetMode(GameMode mode)
    {
        this.gameMode = mode;
    }
}
