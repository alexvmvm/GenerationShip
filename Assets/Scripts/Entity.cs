using System;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType
{
    // misc
    SHIP,
    PROJECTILE,
    BACKDROP_PARTICLE,

    // asteroid
    ASTEROID_SMALL,
    ASTEROID_LARGE,
    ASTEROID_FRAGMENT_SMALL,
    ASTEROID_FRAGMENT_LARGE,

    // ship structures
    SHIP_FLOOR,
    SHIP_WALL,

    // ship buildings
    SHIP_ENGINE,
    SHIP_SHIELD,
    SHIP_RESOURCE_COLLECTOR,
    SHIELD,
    SHIP_TURRET,
    SHIP_TURRET_TOP,

    // ship rooms
    SHIP_ROOM_ENGINE,
    SHIP_ROOM_SHIELD,
    SHIP_ROOM_TURRET
}

[System.Flags]
public enum EntityTag : uint
{
    None        = 0,
    Room        = 1 << 2,
    Floor       = 1 << 3,
    Wall        = 1 << 4,
    Engine      = 1 << 5,
    Shield      = 1 << 6,
    Turret      = 1 << 7
}

public static class EntityTagUtils
{
    public static bool HasAny(this EntityTag value, EntityTag mask) => (value & mask) != 0;
    public static bool HasAll(this EntityTag value, EntityTag mask) => (value & mask) == mask;
}

[Flags]
public enum CollisionLayer : uint
{
    None      = 0,
    Ship      = 1 << 0,
    Asteroid  = 1 << 1,
}

public enum CollisionType
{
    SQAURE,
    CIRCLE
}   

public struct Entity
{
    // identity
    public int id;
    public int parentId;
    public EntityType entityType;
    public EntityTag tags;
    
    // position
    public Vector2 position;
    public float rotation;
    public Vector2 velocity;
    public float rotationRate;

    // collision
    public CollisionType collisionType;
    public Vector2 collisionSize;
    public float collisionRadius;
    public CollisionLayer collisionLayer; 
    public CollisionLayer collideWithMask; 

    // draw
    public Vector2 drawSize;
    public int sortingOrder;
    public Color? drawColorOverride;

    // misc
    public int damageFlashTicks; 
    public int hitPoints;
    public Rect roomBounds;
    public bool isBeingPlaced;

    // shield
    public float shieldRadius;    
    public int shieldChargeTicks;
    public float shieldHitLastTick;

    // cleanup
    public bool cleanupIfNotVisible;
    public bool cleanup;
}