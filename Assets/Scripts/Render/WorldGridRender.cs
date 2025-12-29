using UnityEngine;

public static class GridRenderer
{
    // Cached
    private static Mesh quad;
    private static Material mat;
    private static readonly MaterialPropertyBlock mpb = new();

    // Public config
    public static float CellSize = 1f;     // 1 world unit per tile
    public static int MajorEvery = 5;      // thicker line every N cells
    public static float LineWidth = 0.03f; // world units
    public static Vector2 Origin = Vector2.zero;

    // Optional: set these if you want to control depth/order
    public static int RenderLayer = 0;
    public static float Z = 5f;            // set behind/above depending on your camera setup

    /// <summary>
    /// Call once (optional). If you don't, first Draw() will lazily initialize.
    /// </summary>
    public static void Init()
    {
        if (quad == null) quad = BuildQuad();
        if (mat == null)
        {
            mat = new Material(Shader.Find("Unlit/WorldGrid2D"));
            // Draw early (behind most transparents). Tweak as needed.
            mat.renderQueue = 2900;
        }
    }

    /// <summary>
    /// Draw a world-space grid covering the current orthographic camera view.
    /// Call this every frame in your render pass (eg. before drawing ship entities).
    /// </summary>
    public static void Draw(Camera cam)
    {
        if (cam == null) return;
        if (!cam.orthographic)
        {
            // This version is intended for ortho cameras.
            // You can extend it for perspective if you want.
            return;
        }

        Init();

        // Size quad to slightly exceed view
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float w = halfW * 2.2f;
        float h = halfH * 2.2f;

        // Center on camera XY
        Vector3 pos = new Vector3(cam.transform.position.x, cam.transform.position.y, Z);
        Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(w, h, 1f));

        mpb.Clear();
        mpb.SetFloat("_CellSize", Mathf.Max(1e-5f, CellSize));
        mpb.SetFloat("_MajorEvery", Mathf.Max(1, MajorEvery));
        mpb.SetFloat("_LineWidth", Mathf.Max(1e-5f, LineWidth));
        mpb.SetVector("_Origin", new Vector4(Origin.x, Origin.y, 0, 0));

        Graphics.DrawMesh(quad, matrix, mat, RenderLayer, cam, 0, mpb);
    }

    /// <summary>
    /// If you want to supply your own material (eg. you already created one in assets).
    /// </summary>
    public static void SetMaterial(Material material)
    {
        mat = material;
    }

    private static Mesh BuildQuad()
    {
        var m = new Mesh { name = "GridQuad" };
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