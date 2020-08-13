Shader "Xslg/Terrain Mesh"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Control ("Control", 2D) = "white" {}
        _Splat1 ("_Splat1", 2D) = "white" {}
        _Splat2 ("_Splat2", 2D) = "white" {}
        _Splat3 ("_Splat3", 2D) = "white" {}
    }

    SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert
        #pragma shader_feature _SPLAT3 _SPLAT2

        sampler2D _Control;
        sampler2D _Splat1;
        sampler2D _Splat2;
        sampler2D _Splat3;
        fixed4 _Color;

        struct Input
        {
            float2 uv_Control;
            float2 uv_Splat1;
            float2 uv_Splat2;
            float2 uv_Splat3;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            half4 splat_control = tex2D(_Control, IN.uv_Control);
            half4 mixedDiffuse = half4(0,0,0,1);
            mixedDiffuse += splat_control.r * tex2D(_Splat1, IN.uv_Splat1);
            #if defined(_SPLAT2) || defined(_SPLAT3)
            mixedDiffuse += splat_control.g * tex2D(_Splat2, IN.uv_Splat2);
            #endif
            #if defined(_SPLAT3)
            mixedDiffuse += splat_control.b * tex2D(_Splat3, IN.uv_Splat3);
            #endif
            o.Albedo = mixedDiffuse.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }

    Fallback "Legacy Shaders/VertexLit"
}
