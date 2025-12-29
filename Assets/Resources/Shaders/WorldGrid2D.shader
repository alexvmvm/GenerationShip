Shader "Unlit/WorldGrid2D"
{
    Properties
    {
        _MinorColor ("Minor Color", Color) = (0.4,0.4,0.4,0.20)
        _MajorColor ("Major Color", Color) = (0.6,0.6,0.6,0.30)
        _CellSize   ("Cell Size (World)", Float) = 1
        _MajorEvery ("Major Every N", Float) = 5
        _LineWidth  ("Line Width (World)", Float) = 0.03
        _Origin     ("Grid Origin (World)", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _MinorColor;
            fixed4 _MajorColor;
            float _CellSize;
            float _MajorEvery;
            float _LineWidth;
            float4 _Origin;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // distance to nearest grid line for coordinate x (in world units)
            float lineDist(float x, float cell)
            {
                float fx = frac(x / cell);
                float d = min(fx, 1.0 - fx) * cell;
                return d;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.world.xy - _Origin.xy;

                float cell = max(_CellSize, 1e-5);
                float majorCell = cell * max(_MajorEvery, 1.0);

                float dx = lineDist(p.x, cell);
                float dy = lineDist(p.y, cell);
                float dMinor = min(dx, dy);

                float dxM = lineDist(p.x, majorCell);
                float dyM = lineDist(p.y, majorCell);
                float dMajor = min(dxM, dyM);

                float minor = smoothstep(_LineWidth, 0.0, dMinor);
                float major = smoothstep(_LineWidth, 0.0, dMajor);

                fixed4 col = _MinorColor * minor;
                col = lerp(col, _MajorColor, major); // major overrides a bit
                col.a = max(col.a, _MinorColor.a * minor);
                return col;
            }
            ENDCG
        }
    }
}