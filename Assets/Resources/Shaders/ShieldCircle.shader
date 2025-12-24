Shader "Unlit/ShieldCircle"
{
    Properties
    {
        _Color("Color", Color) = (0.2, 0.7, 1, 1)
        _EdgeWidth("Edge Width", Range(0.0, 0.5)) = 0.08
        _EdgeAlpha("Edge Alpha", Range(0.0, 1.0)) = 0.35
        _FillAlpha("Fill Alpha", Range(0.0, 1.0)) = 0.02
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

            fixed4 _Color;
            float _EdgeWidth;
            float _EdgeAlpha;
            float _FillAlpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV distance from center (0.5,0.5). Radius in UV space is 0.5.
                float2 d = i.uv - 0.5;
                float dist = length(d); // 0 at center, ~0.707 at corners

                // Circle radius in UV = 0.5
                float r = 0.5;

                // Edge band: [r - _EdgeWidth, r]
                float edge = smoothstep(r - _EdgeWidth, r, dist);

                // Mask outside circle
                float outside = step(r, dist);

                // Alpha: small fill + stronger edge, nothing outside
                float a = lerp(_FillAlpha, _EdgeAlpha, edge);
                a *= (1.0 - outside);

                fixed4 col = _Color;
                col.a *= a;
                return col;
            }
            ENDCG
        }
    }
}