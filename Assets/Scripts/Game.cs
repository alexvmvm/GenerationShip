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
    ShipEditor,
    Map
}

public readonly struct Context
{
    public readonly List<Entity> entities;
    public readonly Rect worldScreenRect;
    public readonly bool isMoving;
    public readonly int targetNodeId;

    public Context(List<Entity> entities, Rect worldScreenRect, bool isMoving = false, int targetNodeId = -1)
    {
        this.entities = entities;
        this.worldScreenRect = worldScreenRect;
        this.isMoving = isMoving;
        this.targetNodeId = targetNodeId;
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
    private int seed;

    //Props
    public static int TicksGame => ticksGame;
    private Context Context => new(entities, Camera.main.GetWorldRect(), 
        Run.targetNodeId >= 0, 
        Run.targetNodeId);
    public GameMode Mode => gameMode;
    public bool DrawEntities => Mode != GameMode.Map;
    public int Seed => seed;
    public bool Ticking => Mode == GameMode.Playing;

    void Awake()
    {
        seed = Rand.Int;
    }

    void Start()
    {
        Entity ship = EntityMaker.MakeShip(6, 10, Context);
        shipId = ship.id;

        Map.CreateMap();
    }

    void Update()
    {
        // Gather input as fast as frames arrive
        GatherInput();

        // Run deterministic ticks at 60hz
        if( Ticking )
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
        Context context = Context;

        Collisions.Update(context);
        ShipEditor.Update(context);
        EntityRenderer.Update(context);
        Map.Update();
    }

    private readonly StringBuilder sb = new(1024);
    void OnGUI()
    {
        sb.Clear();
        sb.AppendLine($"Entities: {entities.Count}");

        GUI.Label(new Rect(10, 10, 600, 400), sb.ToString());

        Context context = Context;

        GameUI.OnGUI(context);
        Shields.OnGUI(in context);

        const int BtnWidth = 120;
        const int BtnHeight = 40;

        if( UI.Button(new Rect(Screen.width - BtnWidth - 10, Screen.height - BtnHeight - 10, BtnWidth, BtnHeight), "Add shield") )
            ShipEditor.DoShipEditor(shipId, EntityType.SHIP_ROOM_SHIELD, context);

        if( UI.Button(new Rect(Screen.width - BtnWidth - 10, Screen.height - 2 * BtnHeight - 20, BtnWidth, BtnHeight), "Add turret") )
            ShipEditor.DoShipEditor(shipId, EntityType.SHIP_ROOM_TURRET, context);

        if( UI.Button(new Rect(Screen.width - BtnWidth - 10, Screen.height - 3 * BtnHeight - 30, BtnWidth, BtnHeight), "Map") )
            Find.Game.SetMode(Find.Game.Mode == GameMode.Map ? GameMode.Playing : GameMode.Map);
    }

    void Tick()
    {
        CleanupEntities();
        ConsumeInput();

        var context = Context;
        
        Damage.Tick(context);
        Movement.Tick(context);
        Collisions.Tick(context);
        Asteroids.Tick(context);
        BackgroupEffects.Tick(context);
        Shields.Tick(context);
        Turrets.Tick(context);
        Run.Tick(context);
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

    public void SetMode(GameMode mode)
    {
        if( this.gameMode != mode )
            this.gameMode = mode;
    }
}
