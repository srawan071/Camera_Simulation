Shader "Custom/StandardInstancedColor"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 2.0
        #pragma multi_compile_instancing

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 albedoTex = tex2D(_MainTex, IN.uv_MainTex);
            float4 tint = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            o.Albedo = albedoTex.rgb * tint.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = tint.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
