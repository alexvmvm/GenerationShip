using System;
using UnityEngine;

public enum EntityType
{
    SHIP,
    SHIP_FLOOR,
    SHIP_ENGINE,
    SHIP_ROOM,
    BACKDROP_PARTICLE,
    ASTEROID_SMALL,
    ASTEROID_LARGE,
    ASTEROID_FRAGMENT_SMALL,
    ASTEROID_FRAGMENT_LARGE
}

[Flags]
public enum CollisionLayer : uint
{
    None      = 0,
    Ship      = 1 << 0,
    Asteroid  = 1 << 1,
}


public struct Entity
{
    // identity
    public int id;
    public int parentId;
    public EntityType entityType;
    
    // position
    public Vector2 position;
    public float rotation;
    public Vector2 velocity;
    public float rotationRate;

    // collision
    public Vector2 collisionSize;
    public CollisionLayer collisionLayer; 
    public CollisionLayer collideWithMask; 

    // draw
    public Vector2 drawSize;
    public int sortingOrder;

    // misc
    public int damageFlashTicks; 
    public int hitPoints;

    // cleanup
    public bool cleanupIfNotVisible;
    public bool cleanup;
}