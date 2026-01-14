using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct Node
{
    public int id;
    public Vector2 position;
}

public struct Link
{
    public int childId;
    public int parentId;
    public LinkType type;

    public Link(int childId, int parentId, LinkType type)
    {
        this.childId = childId;
        this.parentId = parentId;
        this.type = type;
    }
}

public enum LinkType
{
    None,
    AsteroidBelt,
    Pirates
}

public static class Map
{
    // Cached
    public static readonly List<Node> nodes = new();
    private static readonly List<Link> links = new();
    private static readonly List<int> availableNodes = new();
    private static readonly Dictionary<int, int> nodeIndexById = new();

    // Props
    public static List<int> AvailableNodes
    {
        get
        {
            CalculateAvailableNodes();
            return availableNodes;
        }
    }

    public static void CreateMap()
    {
        nodes.Clear();
        links.Clear();
        nodeIndexById.Clear();

        int lanes = 8;
        int rows = 8;

        var map = new int[lanes, rows];
        for (int x = 0; x < lanes; x++)
            for (int y = 0; y < rows; y++)
                map[x, y] = -1;

        int requiredLanes = 3;
        List<int> laneIndexes = new();

        Assert.IsTrue(requiredLanes < lanes);

        // Pick starting lanes (row 0)
        while (laneIndexes.Count < requiredLanes)
        {
            int idx = Rand.Range(0, lanes);
            if (laneIndexes.Contains(idx))
                continue;

            laneIndexes.Add(idx);
        }

        const float LaneGap = 4f;
        const float RowGap = 4f;

        float xOffset = -(lanes - 1) * 0.5f * LaneGap;
        float yPosition = new Vector2(Screen.width / 2f, 0f).ScreenToWorld().y + RowGap;

        // --- Create nodes ---
        for (int row = 0; row < rows; row++)
        {
            int nodeCountThisRow = row == 0 ? 1 : Math.Max(1, Rand.Range(requiredLanes - 1, requiredLanes + 1));

            laneIndexes.Clear();
            while (laneIndexes.Count < nodeCountThisRow)
            {
                int lane = Rand.Range(0, lanes);
                if (laneIndexes.Contains(lane))
                    continue;
                laneIndexes.Add(lane);
            }

            for (int i = 0; i < laneIndexes.Count; i++)
            {
                int lane = laneIndexes[i];

                var node = new Node();
                node.id = Rand.IntPositive; // (still recommend unique counter)
                node.position = new Vector2(
                    xOffset + lane * LaneGap,
                    yPosition + row * RowGap);

                map[lane, row] = node.id;

                nodeIndexById[node.id] = nodes.Count;
                nodes.Add(node);

                if( row == 0 && Run.LastNodeId < 0 )
                    Run.SetLastNodeId(node.id);
            }
        }

        // --- Create links child(row) -> parent(row+1) ---
        for (int row = 0; row < map.GetLength(1) - 1; row++)
        {
            // 1) each node in this row picks a closest parent in next row
            for (int lane = 0; lane < map.GetLength(0); lane++)
            {
                int childId = map[lane, row];
                if (childId < 0)
                    continue;

                int parentId = FindClosestNodeOnRow(map, row + 1, childId);
                if (parentId < 0)
                    continue;

                AddLink(childId, parentId);
            }

            // 2) ensure every parent in row+1 has at least one child in row
            for (int lane = 0; lane < map.GetLength(0); lane++)
            {
                int parentId = map[lane, row + 1];
                if (parentId < 0)
                    continue;

                if (HasAnyChildInRow(parentId, row, map))
                    continue;

                int childId = FindClosestNodeOnRow(map, row, parentId);
                if (childId < 0)
                    continue;

                AddLink(childId, parentId);
            }
        }
    }

    // --- Link helpers ---

    private static void AddLink(int childId, int parentId)
    {
        // Avoid duplicates
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].childId == childId && links[i].parentId == parentId)
                return;
        }
        links.Add(new Link(childId, parentId, LinkType.AsteroidBelt));
    }

    private static bool HasLink(int childId, int parentId)
    {
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].childId == childId && links[i].parentId == parentId)
                return true;
        }
        return false;
    }

    private static bool HasAnyChildInRow(int parentId, int childRow, int[,] map)
    {
        // Look at all nodes in that row and see if any links to parentId
        for (int lane = 0; lane < map.GetLength(0); lane++)
        {
            int childId = map[lane, childRow];
            if (childId < 0)
                continue;

            if (HasLink(childId, parentId))
                return true;
        }
        return false;
    }

    private static bool IsParentOf(int parentCandidateId, int childId)
    {
        // parentCandidateId is a valid move if there is a link child->parentCandidate
        return HasLink(childId, parentCandidateId);
    }

    private static bool IsRootNode(int nodeId)
    {
        // Root means: no child points to this node as a parent (i.e. indegree==0)
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].parentId == nodeId)
                return false;
        }
        return true;
    }

    // --- Node helpers ---

    private static int FindClosestNodeOnRow(int[,] map, int row, int rootId)
    {
        int bestId = -1;
        float bestDistance = float.MaxValue;

        int rootIndex = IndexOf(rootId);
        if (rootIndex < 0)
            return -1;

        Vector2 root = nodes[rootIndex].position;

        for (int l = 0; l < map.GetLength(0); l++)
        {
            int id = map[l, row];
            if (id < 0)
                continue;

            int index = IndexOf(id);
            if (index < 0)
                continue;

            float distance = Vector2.Distance(root, nodes[index].position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestId = id;
            }
        }

        return bestId;
    }

    private static int IndexOf(int id)
    {
        if (nodeIndexById.TryGetValue(id, out int idx))
            return idx;

        // fallback
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].id == id)
                return i;
        }
        return -1;
    }

    private static void CalculateAvailableNodes()
    {
        availableNodes.Clear();

        for (int i = 0; i < nodes.Count; i++)
        {
            int nodeId = nodes[i].id;

            bool canUse;
            if (Run.LastNodeId >= 0)
            {
                // Can use if nodeId is a parent of lastNodeId
                canUse = IsParentOf(nodeId, Run.LastNodeId);
            }
            else
            {
                // First pick: roots only
                canUse = IsRootNode(nodeId);
            }

            if (canUse)
                availableNodes.Add(nodeId);
        }
    }

    private static bool TryGetShipPosition(out Vector2 vector, out float rotation)
    {
        vector = default;
        rotation= default;
        
        if( Run.LastNodeId < 0 )
            return false;

        vector = nodes[IndexOf(Run.LastNodeId)].position;

        if( Run.targetNodeId < 0 )
            return true;
        
        Vector2 end = nodes[IndexOf(Run.targetNodeId)].position;
        vector = Vector2.Lerp(vector, end, Run.RunPercentComplete);
        rotation = Vector2.SignedAngle(Vector2.up, end - vector);
        return true;
    }

    public static void Update(Context context)
    {
        if (Find.Game.Mode != GameMode.Map)
            return;

        CalculateAvailableNodes();

        MapUtils.DrawBackground(Camera.main);
        MapUtils.DrawNodes(nodes, nodeId => GetNodeColor(context, nodeId));
        MapUtils.DrawLinks(nodes, links, link => GetLinkColor(context, link));

        if( TryGetShipPosition(out Vector2 shipPos, out float rotation) )
            MapUtils.DrawShip(shipPos, rotation);
        
        if( !context.isMoving )
            DoStationaryMapUI(context);
    }

    private static Color GetNodeColor(Context context, int nodeId)
    {
        if( Run.VisitedNode(nodeId) )
            return Color.green;

        if( !context.isMoving && availableNodes.Contains(nodeId) )
            return Color.yellow;
        
        return Color.white;
    }

    private static Color GetLinkColor(Context context, Link link)
    {
        if( Run.VisitedNode(link.childId) && Run.VisitedNode(link.parentId) )
            return Color.green;
        
        return Color.white;
    }

    private static void DoStationaryMapUI(Context context)
    {
        Vector2 mousePosWorld = Input.mousePosition.ScreenToWorld();

        for (int i = 0; i < availableNodes.Count; i++)
        {
            int idx = IndexOf(availableNodes[i]);
            if (idx < 0) 
                continue;

            Node node = nodes[idx];

            if (Vector2.Distance(node.position, mousePosWorld) < 1)
            {
                MapUtils.DrawCircle(node.position, 1f, 0.1f, Color.yellow);

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Run.Init(node.id, 5);
                    Find.Game.SetMode(GameMode.Playing);
                }
            }
        }
    }
}