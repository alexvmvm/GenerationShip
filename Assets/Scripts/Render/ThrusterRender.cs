using UnityEngine;

public static class ThrusterRenderer
{
    static Material mat;
    static Mesh quad;
    static readonly MaterialPropertyBlock mpb = new();

    private static float PlumeLength = 2.5f;   // world units
    private static float PlumeWidth  = 0.8f;   // world units
    private static float PlumeYOffset = -0.25f;

    public static void DrawThruster(Vector2 enginePos, float engineRotationDeg, int sortingOrder)
    {
        if (mat == null) mat = new Material(Shader.Find("Unlit/ThrusterPlume"));
        if (quad == null) quad = BuildQuadBottomToTopUV();

        // In your game, engines point "down" visually. Adjust as needed:
        // We'll draw the plume extending "down" from engine in local space.
        Quaternion rot = Quaternion.Euler(0, 0, engineRotationDeg+180f);

        // Offset plume so its top touches the engine (plume starts just under engine center)
        Vector3 worldPos = new Vector3(enginePos.x, enginePos.y - PlumeLength + PlumeYOffset, sortingOrder * SortingOrder.LayerDelta);

        var mtx =
            Matrix4x4.TRS(worldPos, rot, new Vector3(PlumeWidth, PlumeLength, 1f)) *
            Matrix4x4.Translate(new Vector3(0f, -0.5f, 0f)); // move quad so y=1 is at engine

        mpb.Clear();
        mpb.SetColor("_Color", new Color(0.25f, 0.75f, 1f, 1f));
        mpb.SetFloat("_Intensity", 1.6f);
        mpb.SetFloat("_Core", 0.25f);
        mpb.SetFloat("_Noise", 0.25f);
        mpb.SetFloat("_TimeScale", 5f);

        Graphics.DrawMesh(quad, mtx, mat, 0, null, 0, mpb);
    }

    static Mesh BuildQuadBottomToTopUV()
    {
        // Vertices: y=-0.5..+0.5 in local. We'll treat UV.y=0 at bottom, 1 at top.
        var m = new Mesh { name = "ThrusterQuad" };
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