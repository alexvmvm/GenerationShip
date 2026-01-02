using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public struct Node
{
    public int id;
    public Vector2 position;
    public List<int> parents;
}

public static class Map
{
    // Cached
    private static int lastNodeId = -1;
    private static readonly List<Node> nodes = new();
    private static readonly List<int> availableNodes = new();

    public static void CreateMap()
    {
        nodes.Clear();

        int lanes = 8;
        int rows = 8;

        var map = new int[lanes, rows];
        for(int x = 0; x < lanes; x++)
            for(int y = 0; y < rows; y++)
                map[x, y] = -1;

        int requiredLanes = 3;
        List<int> laneIndexes = new();

        Assert.IsTrue(requiredLanes < lanes);

        // Pick starting lanes (row 0)
        while(laneIndexes.Count < requiredLanes)
        {
            int idx = Rand.Range(0, lanes);
            if( laneIndexes.Contains(idx) )
                continue;

            laneIndexes.Add(idx);
        }

        const float LaneGap = 4f;
        const float RowGap  = 4f;

        float xOffset = -(lanes - 1) * 0.5f * LaneGap;
        float yPosition = new Vector2(Screen.width/2f, 0f).ScreenToWorld().y + RowGap;

        for(int row = 0; row < rows; row++)
        {
            int nodeCountThisRow = Math.Max(1, Rand.Range(requiredLanes-1, requiredLanes+1));

            // Pick unique lanes for this row
            laneIndexes.Clear();
            while(laneIndexes.Count < nodeCountThisRow)
            {
                int lane = Rand.Range(0, lanes);
                if (laneIndexes.Contains(lane))
                    continue;
                laneIndexes.Add(lane);
            }

            // Create nodes in those lanes
            for(int i = 0; i < laneIndexes.Count; i++)
            {
                int lane = laneIndexes[i];

                var node = new Node();
                node.id = Rand.IntPositive;
                node.parents = new();
                node.position = new Vector2(
                    xOffset + lane * LaneGap,
                    yPosition + row * RowGap);

                map[lane, row] = node.id;
                nodes.Add(node);
            }
        }
        
        for(int row = 0; row < map.GetLength(1) - 1; row++)
        {            
            // look for the best parent nodes
            for(int lane = 0; lane < map.GetLength(0); lane++)
            {                
                int idx = map[lane, row];
                if( idx < 0 )
                    continue;

                int parentId = FindClosestNodeOnRow(map, row+1, idx);
                if( parentId < 0 )
                    continue;

                int index = IndexOf(idx);
                Node n = nodes[index];
                n.parents.Add(parentId);
                nodes[index] = n;
            }

            // check if any are not connected
            for(int lane = 0; lane < map.GetLength(0); lane++)
            {
                int parentId = map[lane, row+1];
                if( parentId < 0 )
                    continue;
                
                bool hasChild = false;
                for(int l = 0; l < map.GetLength(0); l++)
                {
                    int id = map[l, row];
                    if( id < 0 )
                        continue;
                    
                    int childIndex = IndexOf(id);
                    if( nodes[childIndex].parents.Contains(parentId) )
                    {
                        hasChild = true;
                        break;
                    }
                }

                if( hasChild )
                    continue;
                
                int childId = FindClosestNodeOnRow(map, row, parentId);
                if( childId < 0 )
                    continue;
                
                int index = IndexOf(childId);
                Node n = nodes[index];
                n.parents.Add(parentId);
                nodes[index] = n;
            }
        }
    }

    private static int FindClosestNodeOnRow(int[,] map, int row, int rootId)
    {
        int bestIndex = -1;
        float bestDistance = float.MaxValue;
        Vector2 root = nodes[IndexOf(rootId)].position;
    
        for(int l = 0; l < map.GetLength(0); l++)
        {
            int id = map[l, row];
            if( id < 0 )
                continue;

            int index = IndexOf(id);

            var distance = Vector2.Distance(root, nodes[index].position);
            if( distance < bestDistance )
            {
                bestDistance = distance;
                bestIndex = map[l, row];
            }
        }

        return bestIndex;
    }

    private static int IndexOf(int id)
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            if( nodes[i].id == id )
                return i;
        }

        return -1;
    }

    private static void CalculateAvailableNodes()
    {
        availableNodes.Clear();

        static bool CanUseNode(Node node)
        {
            // check if this node is a parent of the current node 
            // if we are at a node
            if( lastNodeId >= 0 )
            {
                Node lastNode = nodes[IndexOf(lastNodeId)];
                return lastNode.parents.NotNullAndContains(node.id);
            }
                
            // check if this node is a root node since we
            // haven't picked a node yet
            for(int i = 0; i < nodes.Count; i++)
            {
                if( nodes[i].id == node.id )
                    continue;
                
                // not a root node since there is a child node
                if( nodes[i].parents.NotNullAndContains(node.id) )
                    return false;
            }

            return true;
        }

        for(int i = 0; i < nodes.Count; i++)
        {
            if( CanUseNode(nodes[i]) )
                availableNodes.Add(nodes[i].id);
        }
    }

    public static void Update()
    {
        if( Find.Game.Mode != GameMode.Map )
            return;

        CalculateAvailableNodes();
        
        MapUtils.DrawBackground(Camera.main);
        MapUtils.DrawNodes(nodes, n => availableNodes.Contains(n));
        MapUtils.DrawLinks(nodes);

        Vector2 mousePosWorld = Input.mousePosition.ScreenToWorld();

        for(int i = 0; i < availableNodes.Count; i++)
        {
            int idx = IndexOf(availableNodes[i]);
            Node node = nodes[idx];

            if( Vector2.Distance(node.position, mousePosWorld) < 1 )
            {
                MapUtils.DrawCircle(node.position, 1f, 0.1f, Color.yellow);

                if( Input.GetKeyDown(KeyCode.Mouse0) )
                {
                    lastNodeId = node.id;
                }
            }
                
        }
    }
}