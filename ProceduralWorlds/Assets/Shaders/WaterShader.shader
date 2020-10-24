Shader "Custom/WaterShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DeepWaterColor("Deep Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(Refraction)]
        _RefractionTiling("Refraction Tiling", Range(0.001, 0.1)) = 0.01
        _RefractionStrength("Refraction Strength", Range(0.01, 0.1)) = 0.025
    }
    SubShader
    {
        GrabPass
        {
            "_GrabPass"
        }

        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert fullforwardshadows alpha:fade
        #pragma target 4.6

        sampler2D _CameraDepthTexture;
        sampler2D _GrabPass;
        sampler2D _MainTex;
        sampler2D _NormalMap;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _DeepWaterColor;

        float _RefractionTiling;
        float _RefractionStrength;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            float4 grabPos;
            float4 wpos;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.screenPos = ComputeScreenPos(v.vertex);
            o.grabPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
            o.wpos = mul(unity_ObjectToWorld, v.vertex);
        }

        float UnderWaterDepth(float4 scrPos)
        {
            float2 uvs = scrPos.xy / scrPos.w;
            float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvs));
            float surfaceDepth = scrPos.w;

            return (backgroundDepth - surfaceDepth) * 0.1;
        }

        float4 Triplanar(sampler2D Tex, float3 Pos, float3 Normal, float2 Tile, float Blend)
        {
            float3 uv = Pos * float3(Tile.x, Tile.y, Tile.x);
            float3 blend = pow(abs(Normal), Blend);
            blend /= dot(blend, 1.0);

            float4 x = tex2D(Tex, uv.zy);
            float4 y = tex2D(Tex, uv.xz);
            float4 z = tex2D(Tex, uv.xy);

            float4 result = (x * blend.x) + (y * blend.y) + (z * blend.z);

            return result;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 noise = Triplanar(_MainTex, IN.worldPos + float3(_Time.z, _Time.z, _Time.z), IN.worldNormal, _RefractionTiling, 8.0f);
            noise += Triplanar(_MainTex, IN.worldPos + float3(-_Time.z, -_Time.z, _Time.z), IN.worldNormal, _RefractionTiling, 8.0f);
            noise *= 0.5f;

            float4 offset = float4((noise.r - 0.2f) * _RefractionStrength, (noise.r - 0.2f) * _RefractionStrength, 0, 0);
            float4 grab = tex2Dproj(_GrabPass, IN.screenPos + offset);

            float depth = saturate(UnderWaterDepth(IN.screenPos));
            fixed4 col = lerp(_Color, _DeepWaterColor, saturate(depth * 2.0f));

            fixed4 c = lerp(grab, col, _Color.a);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
