using System;
using System.Collections.Generic;
using UnityEngine;

public static class Render
{
    private static Material spriteMaterial;
    private static readonly MaterialPropertyBlock mpb = new();

    public static void DrawEntity(Entity e)
    {
        var sprite = GetSprite(e.entityType);
        if( sprite == null) 
            return;

        if( spriteMaterial == null )
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));

        Mesh mesh = GetMeshForSprite(sprite);

        Quaternion rot = Quaternion.Euler(0, 0, e.rotation);
        Vector3 worldPos = new(e.position.x, e.position.y, -e.sortingOrder);

        // NOTE: this scales the sprite mesh itself, not "fit inside drawSize".
        Vector3 scale = new(e.drawSize.x, e.drawSize.y, 1f);

        var matrix = Matrix4x4.TRS(worldPos, rot, scale);

        mpb.Clear();
        mpb.SetTexture("_MainTex", sprite.texture);
        mpb.SetColor("_Color", Color.white);

        // Extra safety for some SRP shaders:
        mpb.SetTexture("_BaseMap", sprite.texture);
        mpb.SetColor("_BaseColor", Color.white);

        Graphics.DrawMesh(mesh, matrix, spriteMaterial, 0, null, 0, mpb);

        DebugUtils.DrawRect(e.position, Vector2.one, Color.red);
    }

    private static Sprite GetSprite(EntityType type) => type switch
    {
        EntityType.TILE                 => ResourceCache.Sprite("Textures/floor"),
        EntityType.ENGINE               => ResourceCache.Sprite("Textures/engine"),
        EntityType.BACKDROP_PARTICLE    => ResourceCache.Sprite("Textures/particle"),
        EntityType.ASTEROID_SMALL       => ResourceCache.Sprite("Textures/asteroid-small"),
        EntityType.ASTEROID_LARGE       => ResourceCache.Sprite("Textures/asteroid-large"),
        _                               => ResourceCache.Sprite("Textures/placeholder")
    }; 

    private static readonly Dictionary<Sprite, Mesh> spriteMeshCache = new();
    private static Mesh GetMeshForSprite(Sprite sprite)
    {
        if (spriteMeshCache.TryGetValue(sprite, out var cached) && cached != null)
            return cached;

        var verts2 = sprite.vertices;
        var uvs = sprite.uv;
        var tris16 = sprite.triangles;

        var verts3 = new Vector3[verts2.Length];
        for (int i = 0; i < verts2.Length; i++)
            verts3[i] = verts2[i];

        var tris = new int[tris16.Length];
        for (int i = 0; i < tris16.Length; i++)
            tris[i] = tris16[i];

        var mesh = new Mesh { name = $"SpriteMesh_{sprite.name}" };
        mesh.SetVertices(verts3);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();

        spriteMeshCache[sprite] = mesh;
        return mesh;
    }
}