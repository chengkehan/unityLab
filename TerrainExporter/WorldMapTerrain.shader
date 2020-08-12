Shader "Xslg/Terrain" {
    Properties{
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
    }

        CGINCLUDE
#pragma surface surf Lambert vertex:SplatmapVert finalcolor:SplatmapFinalColor exclude_path:prepass exclude_path:deferred noforwardadd nolightmap nodynlightmap nodirlightmap nolppv noshadowmask 
#pragma instancing_options assumeuniformscaling nomatrices
#pragma multi_compile_fog
#include "TerrainSplatmapCommon.cginc"



        void _SplatmapMix(Input IN, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
        {

#ifdef _ALPHATEST_ON
            ClipHoles(IN.tc.xy);
#endif

            // adjust splatUVs so the edges of the terrain tile lie on pixel centers
            float2 splatUV = (IN.tc.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
            splat_control = tex2D(_Control, splatUV);
            weight = dot(splat_control, half4(1, 1, 1, 1));

#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
            clip(weight == 0.0f ? -1 : 1);
#endif

            // Normalize weights before lighting and restore weights in final modifier functions so that the overal
            // lighting result can be correctly weighted.
            splat_control /= (weight + 1e-3f);

            float2 uvSplat0 = TRANSFORM_TEX(IN.tc.xy, _Splat0);
            float2 uvSplat1 = TRANSFORM_TEX(IN.tc.xy, _Splat1);
            float2 uvSplat2 = TRANSFORM_TEX(IN.tc.xy, _Splat2);
            //float2 uvSplat3 = TRANSFORM_TEX(IN.tc.xy, _Splat3);

            mixedDiffuse = 0.0f;
#ifdef TERRAIN_STANDARD_SHADER
            mixedDiffuse += splat_control.r * tex2D(_Splat0, uvSplat0) * half4(1.0, 1.0, 1.0, defaultAlpha.r);
            mixedDiffuse += splat_control.g * tex2D(_Splat1, uvSplat1) * half4(1.0, 1.0, 1.0, defaultAlpha.g);
            mixedDiffuse += splat_control.b * tex2D(_Splat2, uvSplat2) * half4(1.0, 1.0, 1.0, defaultAlpha.b);
            //mixedDiffuse += splat_control.a * tex2D(_Splat3, uvSplat3) * half4(1.0, 1.0, 1.0, defaultAlpha.a);
#else
            mixedDiffuse += splat_control.r * tex2D(_Splat0, uvSplat0);
            mixedDiffuse += splat_control.g * tex2D(_Splat1, uvSplat1);
            mixedDiffuse += splat_control.b * tex2D(_Splat2, uvSplat2);
            //mixedDiffuse += splat_control.a * tex2D(_Splat3, uvSplat3);
#endif

#ifdef _NORMALMAP
            mixedNormal = UnpackNormalWithScale(tex2D(_Normal0, uvSplat0), _NormalScale0) * splat_control.r;
            mixedNormal += UnpackNormalWithScale(tex2D(_Normal1, uvSplat1), _NormalScale1) * splat_control.g;
            mixedNormal += UnpackNormalWithScale(tex2D(_Normal2, uvSplat2), _NormalScale2) * splat_control.b;
            //mixedNormal += UnpackNormalWithScale(tex2D(_Normal3, uvSplat3), _NormalScale3) * splat_control.a;
            mixedNormal.z += 1e-5f; // to avoid nan after normalizing
#endif

#if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
            mixedNormal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
#endif

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
            float3 geomNormal = normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);
#ifdef _NORMALMAP
            float3 geomTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
            float3 geomBitangent = normalize(cross(geomTangent, geomNormal));
            mixedNormal = mixedNormal.x * geomTangent
                + mixedNormal.y * geomBitangent
                + mixedNormal.z * geomNormal;
#else
            mixedNormal = geomNormal;
#endif
            mixedNormal = mixedNormal.xzy;
#endif

        }






        void surf(Input IN, inout SurfaceOutput o)
    {
        half4 splat_control;
        half weight;
        fixed4 mixedDiffuse;
        _SplatmapMix(IN, splat_control, weight, mixedDiffuse, o.Normal);
        o.Albedo = mixedDiffuse.rgb;
        o.Alpha = weight;
    }
    ENDCG

        Category{
            Tags {
                "Queue" = "Geometry-99"
                "RenderType" = "Opaque"
            }
        // TODO: Seems like "#pragma target 3.0 _NORMALMAP" can't fallback correctly on less capable devices?
        // Use two sub-shaders to simulate different features for different targets and still fallback correctly.
        SubShader { // for sm3.0+ targets
            CGPROGRAM
                #pragma target 3.0
                //#pragma multi_compile_local __ _ALPHATEST_ON
                //#pragma multi_compile_local __ _NORMALMAP
            ENDCG

            UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
            UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
        }
        SubShader { // for sm2.0 targets
            CGPROGRAM
            ENDCG
        }
    }

        Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Diffuse-AddPass"
        Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/Diffuse-Base"
        Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Diffuse-BaseGen"
        Dependency "Details0" = "Hidden/TerrainEngine/Details/Vertexlit"
        Dependency "Details1" = "Hidden/TerrainEngine/Details/WavingDoublePass"
        Dependency "Details2" = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass"
        Dependency "Tree0" = "Hidden/TerrainEngine/BillboardTree"

        Fallback "Diffuse"
}
