using System;
using System.Collections.Generic;
using UnityEngine;

public static class MapUtils
{
    public static float Overscan = 1.05f; // >1 so it slightly exceeds edges
    public static Vector2 BackgroundCenter = Vector2.zero;
    public static float BackgroundWorldHeight = 60f;

    private static Material lineMat;
    private static Material nodeMat;
    private static Material backgroundMat;
    private static Material ringMat;
    private static Mesh lineQuad;
    private static Mesh quad;
    private static readonly MaterialPropertyBlock mpb = new();

    public static void DrawNodes(List<Node> nodes, Func<int, bool> highlight)
    {
        if (nodeMat == null)
        {
            nodeMat = new Material(Shader.Find("Sprites/Default"));
            nodeMat.renderQueue = 2910; // slightly above background
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

            Color c = highlight(d.id) ? Color.yellow : Color.white;

            mpb.SetColor("_Color", c);
            mpb.SetColor("_BaseColor", c);

            Graphics.DrawMesh(quad, mtx, nodeMat, 0, Camera.main, 0, mpb);
        }
    }

    public static void DrawLinks(List<Node> nodes)
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
                    DrawLink(nodes[j].position, n.position, width: 0.1f, color: new Color(1, 1, 1, 0.6f), z: 0f);
                }
            }            
        }
    }

    private static void DrawLink(Vector2 a, Vector2 b, float width, Color color, float z = 0f)
    {
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

        Graphics.DrawMesh(lineQuad, mtx, lineMat, 0, Camera.main, 0, mpb);
    }

    public static void DrawBackground(Camera cam)
    {
        if (cam == null)
            return;

        if (backgroundMat == null)
        {
            // Unlit color is simplest for a solid black quad
            backgroundMat = new Material(Shader.Find("Unlit/Color"));
            backgroundMat.renderQueue = 2900;
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

        Graphics.DrawMesh(quad, mtx, backgroundMat, 0, cam, 0, mpb);
    }    

    public static void DrawCircle(Vector2 center, float radius, float width, Color color, int segments = 32, float z = 0f)
    {
        if (ringMat == null)
        {
            ringMat = new Material(Shader.Find("Sprites/Default"));
            ringMat.renderQueue = 2915; // above nodes (nodes are 2910)
        }

        if (lineQuad == null)
            lineQuad = BuildQuad(); // reuse quad mesh

        segments = Mathf.Max(8, segments);
        float step = (Mathf.PI * 2f) / segments;

        // Draw as a loop of short "links"
        Vector2 prev = center + new Vector2(Mathf.Cos(0f), Mathf.Sin(0f)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float a = i * step;
            Vector2 next = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;

            DrawLinkWithMaterial(prev, next, width, color, ringMat, z);

            prev = next;
        }
    }

    // Same as DrawLink, but lets us pick a material (so circle can be layered above nodes cleanly)
    private static void DrawLinkWithMaterial(Vector2 a, Vector2 b, float width, Color color, Material material, float z = 0f)
    {
        if (material == null) return;

        if (lineQuad == null)
            lineQuad = BuildQuad();

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

        Graphics.DrawMesh(lineQuad, mtx, material, 0, Camera.main, 0, mpb);
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
}