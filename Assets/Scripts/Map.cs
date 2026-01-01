using System;
using System.Collections.Generic;
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
    // Config
    public static string BackgroundSpritePath = "Textures/map-background"; // set to your sprite path
    public static float Overscan = 1.05f; // >1 so it slightly exceeds edges

    public static Vector2 BackgroundCenter = Vector2.zero;
    public static float BackgroundWorldHeight = 60f;

    // Cached
    private static Material mat;
    private static Material dotMat;
    private static Material lineMat;
    private static Mesh lineQuad;
    private static Mesh quad;
    private static readonly MaterialPropertyBlock mpb = new();
    private static List<Node> nodes = new();

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

    private static void DrawBackground(Camera cam)
    {
        if (cam == null)
            return;

        if (mat == null)
        {
            // Unlit color is simplest for a solid black quad
            mat = new Material(Shader.Find("Unlit/Color"));
            mat.renderQueue = 2900;
        }
        if (quad == null)
            quad = BuildQuad();

        // Fixed world size, but ensure it still covers the camera view.
        float h = Mathf.Max(0.01f, BackgroundWorldHeight) * Overscan;
        float w = h; // square by default (no texture aspect to preserve)

        if (cam.orthographic)
        {
            float viewH = cam.orthographicSize * 2f;
            float viewW = viewH * cam.aspect;

            if (h < viewH * Overscan) h = viewH * Overscan;
            if (w < viewW * Overscan) w = viewW * Overscan;
        }

        float z = 0f;
        var pos = new Vector3(BackgroundCenter.x, BackgroundCenter.y, z);
        var mtx = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(w, h, 1f));

        mpb.Clear();
        mpb.SetColor("_Color", Color.black);

        Graphics.DrawMesh(quad, mtx, mat, 0, cam, 0, mpb);
    }

    private static void DrawNodes(Camera camera)
    {
        if (dotMat == null)
        {
            dotMat = new Material(Shader.Find("Sprites/Default"));
            dotMat.renderQueue = 2910; // slightly above background
        }

        if (quad == null) 
            quad = BuildQuad();

        mpb.Clear();
        mpb.SetTexture("_MainTex", Texture2D.whiteTexture);
        mpb.SetTexture("_BaseMap", Texture2D.whiteTexture);
        
        for(int i = 0; i < nodes.Count; i++)
        {
            var d = nodes[i];

            var mtx = Matrix4x4.TRS(d.position, Quaternion.identity, new Vector3(1f, 1f, 1f));

            Color c = Color.yellow;

            mpb.SetColor("_Color", c);
            mpb.SetColor("_BaseColor", c);

            Graphics.DrawMesh(quad, mtx, dotMat, 0, camera, 0, mpb);
        }
    }

    public static void DrawLinks(Camera cam)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (n.parents.NullOrEmpty()) 
                continue;

            for (int j = 0; j < nodes.Count; j++)
            {
                if (n.parents.Contains(nodes[j].id))
                {                    
                    // your quad-line renderer from earlier
                    DrawLink(cam, nodes[j].position, n.position, width: 0.1f, color: new Color(1, 1, 1, 0.6f), z: 0f);
                }
            }            
        }
    }

    private static void DrawLink(Camera cam, Vector2 a, Vector2 b, float width, Color color, float z = 0f)
    {
        if (cam == null) return;

        if (lineMat == null)
        {
            lineMat = new Material(Shader.Find("Sprites/Default"));
            lineMat.renderQueue = 2905; // above background/dots, below nodes (tweak as needed)
        }

        if (lineQuad == null)
            lineQuad = BuildQuad(); // reuse your existing quad builder

        Vector2 d = b - a;
        float len = d.magnitude;
        if (len <= 1e-5f) return;

        float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        Vector2 mid = (a + b) * 0.5f;

        var mtx = Matrix4x4.TRS(
            new Vector3(mid.x, mid.y, z),
            Quaternion.Euler(0f, 0f, angle),
            new Vector3(len, width, 1f));

        mpb.Clear();
        mpb.SetColor("_Color", color);

        Graphics.DrawMesh(lineQuad, mtx, lineMat, 0, cam, 0, mpb);
    }

    private static Mesh BuildQuad()
    {
        var m = new Mesh { name = "MapQuad" };
        m.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
        };
        m.uv = new[]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1),
        };
        m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateBounds();
        return m;
    }

    public static void Update()
    {
        if( Find.Game.Mode != GameMode.Map )
            return;
        
        DrawBackground(Camera.main);
        DrawNodes(Camera.main);
        DrawLinks(Camera.main);
    }
}