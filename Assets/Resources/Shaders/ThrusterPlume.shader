Shader "Unlit/ThrusterPlume"
{
    Properties
    {
        _Color("Color", Color) = (0.2,0.7,1,1)
        _Intensity("Intensity", Range(0,5)) = 1.5
        _Core("Core", Range(0,1)) = 0.4
        _Noise("Noise", Range(0,1)) = 0.25
        _TimeScale("TimeScale", Range(0,10)) = 4
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One        // additive glow
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Intensity;
            float _Core;
            float _Noise;
            float _TimeScale;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // tiny hash noise
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV: (0,0) bottom, (1,1) top if you set it that way
                float2 uv = i.uv;

                // Shape: wider near top? (invert if you want)
                float y = uv.y; // 0..1
                float x = abs(uv.x - 0.5) * 2.0; // 0 center -> 1 edge

                // Base plume: narrow at origin, fades out with y
                float width = lerp(0.75, 0.15, y); // wide at top, narrow at bottom
                float body = smoothstep(width, 0.0, x);

                // Fade along length
                float lengthFade = smoothstep(1.0, 0.0, y);

                // Core bright center
                float core = smoothstep(_Core, 0.0, x) * smoothstep(0.25, 0.0, y);

                // Flicker noise
                float t = _Time.y * _TimeScale;
                float n = hash21(float2(uv.x * 20.0, uv.y * 30.0 + t));
                float flicker = lerp(1.0 - _Noise, 1.0, n);

                float a = (body * lengthFade + core) * flicker;
                fixed4 col = _Color * (_Intensity * a);
                col.a = 1; // ignored in additive
                return col;
            }
            ENDCG
        }
    }
}