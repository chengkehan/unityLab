Shader "T4MShaders/ShaderModel2/Diffuse/T4M 4 Textures" {
Properties {
	_Splat0 ("Layer 1", 2D) = "white" {}
	_Splat1 ("Layer 2", 2D) = "white" {}
	_Splat2 ("Layer 3", 2D) = "white" {}
	_Splat3 ("Layer 4", 2D) = "white" {}
	_Control ("Control (RGBA)", 2D) = "white" {}
	_MainTex ("Never Used", 2D) = "white" {}
}
                
SubShader {
	Tags {
   "SplatCount" = "4"
   "RenderType" = "Opaque"
   "Queue"="Transparent"
	}
CGPROGRAM
#include "UnityCG.cginc"
#pragma target 3.0
#pragma surface surf Lambert vertex:vert finalcolor:mycolor
#pragma exclude_renderers xbox360 ps3

struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	fixed2 p : TEXCOORD3;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
uniform sampler2D _x_shadowMap;
uniform float4x4 _x_lightVP;

void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
{
	float count = step(IN.p.x, 0) + step(IN.p.y, 0) + step(1, IN.p.x) + step(1, IN.p.y);
	if(count > 0)
	{
		return;
	}
    float4 s = tex2D(_x_shadowMap, IN.p.xy);
	color.rgb *= s.xyz;
}

void vert (inout appdata_full v, out Input data) {
	float4 p = mul(_x_lightVP, mul(_Object2World, v.vertex));
	float num = _x_lightVP[3].xyz * v.vertex.xyz + _x_lightVP[3][3];
	num = 1 / num;
	data.p = (p * num + 1) * 0.5;
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g);
}
ENDCG 
}
// Fallback to Diffuse
Fallback "Diffuse"
}
