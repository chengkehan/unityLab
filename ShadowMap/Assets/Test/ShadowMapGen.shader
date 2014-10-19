Shader "XWorld/ShadowMapGen" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags {"Queue"="Transparent"}
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4x4 _x_lightVP;

			struct VertIn
			{
				float4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
			};

			struct FragIn
			{
				float4 pos : POSITION;
				fixed2 uv : TEXCOORD0;
			};

			FragIn vert(VertIn i)
			{
				FragIn o;
				o.pos = mul (UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.uv;
				return o;
			}

			fixed4 frag(FragIn i) : COLOR 
			{
				return fixed4(0.3, 0.3, 0.3, 1);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
