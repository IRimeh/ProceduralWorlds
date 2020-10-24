Shader "Custom/AtmosphereShader"
{
    Properties
    {
        [HDR]
        _Color ("Color", Color) = (1,1,1,1)
        _FresnelPow("Fresnel Power", Range(0, 16)) = 8
        _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200
        Cull Front

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 4.0

        fixed4 _Color;
        float _FresnelPow;
        float _Metallic;
        float _Smoothness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float fresnel = dot(normalize(IN.worldNormal), -IN.viewDir);
            fresnel = pow(saturate(fresnel), _FresnelPow);
            fresnel *= dot(IN.worldNormal, _WorldSpaceLightPos0);
            fresnel = saturate(fresnel);

            fixed4 c = _LightColor0;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Emission = _LightColor0 * _Color * saturate(pow(fresnel, 2));
            o.Alpha = fresnel;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
