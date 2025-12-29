using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public static class Find
{
    //Props
    public static Game Game => GetObjectCached(ref game);

    //Private cache
    private static Game game;

    public static void Reset()
    {
        game = null;
    }
    
    
    private static T GetObjectCached<T>(ref T obj, bool includeInactive = false) where T : MonoBehaviour
    {
        return obj ??= Object.FindObjectOfType<T>(includeInactive);
    }
}
