using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;



public enum EntityType
{
    TILE,
    ENGINE,
    BACKDROP_PARTICLE,
    ASTEROID_SMALL,
    ASTEROID_LARGE
}

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

public struct Entity
{
    public Vector2 position;
    public float rotation;
    public EntityType entityType;
    public Vector2 drawSize;
    public Vector2 velocity;
    public int sortingOrder;
    public bool cleanupIfNotVisible;
    public bool cleanup;
}

public struct InputEvent
{
    public InputType inputType;
    public Cardinal direction;
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
    const float TICK_RATE = 60f;
    static readonly float TICK_DT = 1f / TICK_RATE;

    //Working vars
    private readonly List<Entity> entities = new();    
    private readonly Queue<InputEvent> events = new();
    private float tickAcc;
    private static int ticksGame;

    //Props
    public static int TicksGame => ticksGame;

    void Start()
    {
        entities.AddRange(ShipMaker.MakeShip(6, 10));
    }

    void Update()
    {
        // Gather input as fast as frames arrive
        GatherInput();

        // Run deterministic ticks at 60hz
        tickAcc += Time.deltaTime;
        while (tickAcc >= TICK_DT)
        {
            Tick();
            tickAcc -= TICK_DT;
            ticksGame++;
        }

        // Render / presentation (per-frame)
        DrawEntities();
    }

    private readonly StringBuilder sb = new(1024);
    void OnGUI()
    {
        sb.Clear();
        sb.AppendLine($"Entities: {entities.Count}");

        GUI.Label(new Rect(10, 10, 600, 400), sb.ToString());
    }



    void Tick()
    {
        CleanupEntities();
        ConsumeInput();

        var context = new Context(entities, Camera.main.GetWorldRect());

        Movement.Tick(context);
        Asteroids.Tick(context);
        BackgroupEffects.Tick(context);
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
}
