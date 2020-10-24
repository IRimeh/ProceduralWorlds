// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "Custom/WorldShader"
{
    Properties
    {
        _ColorGradient ("Color Gradient", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [HDR]
        _FresnelCol("Fresnel Color", Color) = (1,1,1,1)
        _FresnelPow("Fresnel Power", Range(0, 16)) = 8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 4.6

        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
        #include "Lighting.cginc"
        //#include "UnityDeferredLibrary.cginc"

        sampler2D _ColorGradient;
        half _Glossiness;
        half _Metallic;

        float _MinHeight;
        float _MaxHeight;

        float _FresnelPow;
        fixed4 _FresnelCol;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
            float4 pos;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float posDist = length(IN.worldPos.xyz);
            float ratio = (posDist - _MinHeight) / (_MaxHeight - _MinHeight);

            float3 approximateNormal = lerp(normalize(IN.worldPos), normalize(IN.worldNormal), 0.5f);
            float fresnel = dot(approximateNormal, -IN.viewDir);
            fresnel = pow(saturate(fresnel), _FresnelPow);
            fresnel *= dot(normalize(IN.worldPos), normalize(_WorldSpaceLightPos0));
            fresnel = saturate(fresnel);

            fixed4 c = tex2D (_ColorGradient, float2(ratio, 0));
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = fresnel * _FresnelCol;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
