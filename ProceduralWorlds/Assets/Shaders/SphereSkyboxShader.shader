Shader "Unlit/SphereSkyboxShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        [Header(Sun)]
        [HDR]_SunColor("Sun Color", Color) = (1,1,1,1)
        _SunSize("Sun Size", Range(-1, 1)) = 0.5
        [HDR]_SunGlowColor("Sun Glow Color", Color) = (1,1,1,1)
        _SunGlowInnerEdge("Sun Glow Inner Edge", Range(0, 1)) = 0.5
        _SunGlowOuterEdge("Sun Glow Outer Edge", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 pos : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _SunColor;
            float _SunSize;
            fixed4 _SunGlowColor;
            float _SunGlowInnerEdge;
            float _SunGlowOuterEdge;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pos = v.vertex;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(mul(unity_ObjectToWorld, i.pos) - _WorldSpaceCameraPos);

                //Sun
                float4 sun = _SunColor * 100;
                float sunSize = 1.1 - (_SunSize * 0.1);
                float sunVal = smoothstep(sunSize * 0.95, sunSize, dot(viewDir, _WorldSpaceLightPos0));

                float outerEdge = (1 - _SunGlowOuterEdge);
                float innerEdge = max(outerEdge, 10 * (1 - _SunGlowInnerEdge));
                float sunglow = smoothstep(outerEdge, innerEdge, dot(viewDir, _WorldSpaceLightPos0));

                sun *= sunVal;
                sun += _SunGlowColor * sunglow;


                //Sky
                fixed4 sky = _Color;

                return sky + sun;
            }
            ENDCG
        }
    }
}
