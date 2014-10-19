Shader "SnailGame/Self-Illumin/CutOff Lightmap Double Sides Tint" {
	Properties {
	  _Color ("MainCol (RGBA)", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Illum ("Illumin (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f{
	    float4 pos : SV_POSITION;
	    half4 uvPacker : TEXCOORD0;
	    #ifdef LIGHTMAP_ON
	    half2 uv : TEXCOORD1; 
	    #endif
	};
	
	uniform fixed4 _Color;
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_ST;
	uniform sampler2D _Illum;
	uniform float4 _Illum_ST;
	uniform sampler2D unity_Lightmap;
	uniform float4 unity_LightmapST;
	
	uniform float _Cutoff;
	
	v2f vert( appdata_full v ) {
	    v2f o = (v2f)0;
	    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	    
	    o.uvPacker.xy = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
	    o.uvPacker.zw = v.texcoord * _Illum_ST.xy + _Illum_ST.zw;
	    
	    #ifdef LIGHTMAP_ON
	    o.uv = v.texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw;
	    #endif
	    return o;
	}
	
	fixed4 frag( v2f i ) : COLOR {
	    fixed4 col = tex2D( _MainTex, i.uvPacker.xy ) * _Color;
	    fixed e = tex2D( _Illum, i.uvPacker.zw ).a;
	    fixed3 emmision = col.rgb * e;
	    
	    #ifdef LIGHTMAP_ON
	    fixed3 lm = DecodeLightmap( tex2D( unity_Lightmap, i.uv) );
	    col.rgb *= lm;
	    #endif
	    
	    col.rgb += emmision;
	    clip( col.a - _Cutoff );
	    
	    return col;
	}
	
	ENDCG
	
	SubShader {
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
		LOD 180
		
		Pass {
		Cull Off
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON

		ENDCG
		}
	} 
	//FallBack "Diffuse"
}
