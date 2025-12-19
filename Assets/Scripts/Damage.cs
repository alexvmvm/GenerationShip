using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Damage
{
    public static void Tick(Context context)
    {
        for(int i = 0; i < context.entities.Count; i++)
        {
            if( context.entities[i].damageFlashTicks <= 0 )
                continue;
            
            Entity e = context.entities[i];
            e.damageFlashTicks -= 1;
            context.entities[i] = e;
        }
    }
}