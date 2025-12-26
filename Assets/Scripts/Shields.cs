using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Shields
{
    private const int ShieldBarWidth = 40;
    private const int ShieldBarHeight = 10;
    private static readonly Vector2 Offset = new(0f, 20f);

    public static void OnGUI(in Context context)
    {
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].entityType != EntityType.SHIELD )
                continue;

            if( context.entities[i].cleanup || context.entities[i].cleanupIfNotVisible )
                continue;

            Vector2 pos = context.entities[i].position.WorldToGUI();

            Rect rect = new(
                pos.x - ShieldBarWidth/2f + Offset.x, 
                pos.y - ShieldBarHeight/2f + Offset.y, 
                ShieldBarWidth, 
                ShieldBarHeight);

            bool shieldUp = context.entities[i].hitPoints > 0;

            float progress = shieldUp ? 
                context.entities[i].hitPoints / (float)ShieldTuning.ShieldHitpoints :  
                context.entities[i].shieldChargeTicks / (float)ShieldTuning.ShieldRechargeTicks;

            Color fillColor = shieldUp ? Color.green : Color.red;

            UI.ProgressBar(rect, progress, fillColor: fillColor);
        }
    }

    public static void Tick(Context context)
    {
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].entityType != EntityType.SHIELD )
                continue;

            Entity e = context.entities[i];

            // charge the shield if it has zero hitpoints or wait and charge the shield if it
            // has some hitpoints left. Both types of charging should happen at the same rate.
            if( e.hitPoints > 0 )
            {
                if( e.hitPoints < ShieldTuning.ShieldHitpoints )
                {
                    int ticksPerHitpoint = Mathf.RoundToInt(ShieldTuning.ShieldRechargeTicks / (float)ShieldTuning.ShieldHitpoints);

                    e.shieldChargeTicks += 1;

                    if( e.shieldChargeTicks % ticksPerHitpoint == 0 )
                        e.hitPoints += 1;
                    
                    context.entities[i] = e;
                }
            }
            else
            {
                // recharge shields
                e.shieldChargeTicks += 1;
                if( e.shieldChargeTicks >= ShieldTuning.ShieldRechargeTicks )
                {
                    e.hitPoints = ShieldTuning.ShieldHitpoints;
                    e.shieldChargeTicks = 0;
                }
                context.entities[i] = e;
            }
        }
    }
}