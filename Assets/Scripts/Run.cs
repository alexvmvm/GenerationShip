using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Run
{
    public static int targetNodeId = -1;
    public static int runDurationTicks = -1;
    private static int runTicks = 0;
    private static int lastNodeId = -1;
    private static readonly List<int> visitedNodeIds = new();

    //Props
    public static int LastNodeId => lastNodeId;
    public static float RunPercentComplete => runTicks / (float)runDurationTicks;

    public static void Init(int targetId, int durationTicks)
    {
        targetNodeId = targetId;
        runDurationTicks = durationTicks;
        runTicks = 0;
    }

    public static void SetLastNodeId(int id)
    {
        lastNodeId = id;
    }

    public static bool VisitedNode(int id)
    {
        return visitedNodeIds.Contains(id);
    }

    public static void Reset()
    {
        visitedNodeIds.Clear();
        lastNodeId = -1;
        runDurationTicks = -1;
        runTicks = 0;
        targetNodeId = -1;
    }

    public static void Tick(Context context)
    {
        if( context.targetNodeId < 0 || runTicks >= runDurationTicks )
            return;

        runTicks++;
        
        if( runTicks >= runDurationTicks )
        {
            lastNodeId = context.targetNodeId;
            targetNodeId = -1;

            if( !visitedNodeIds.Contains(lastNodeId) )
                visitedNodeIds.Add(lastNodeId);
        }
    }
}