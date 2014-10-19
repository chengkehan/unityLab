Shader "XWorld/ShadowMap" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _x_shadowMap;
			uniform float4x4 _x_lightVP;
			//uniform float _x_dist;

			struct VertIn
			{
				float4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
			};

			struct FragIn
			{
				float4 pos : POSITION;
				fixed2 uv : TEXCOORD0;
				fixed2 uv2 : TEXCOORD1;
			};

			FragIn vert(VertIn i)
			{
				FragIn o;
				o.pos = mul (UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.uv;

				float4 p = mul(_x_lightVP, mul(_Object2World, i.vertex));
				float num = _x_lightVP[3].xyz * i.vertex.xyz + _x_lightVP[3][3];
				num = 1 / num;
				o.uv2 = (p * num + 1) * 0.5;

				return o;
			}

			fixed4 frag(FragIn i) : COLOR 
			{
				fixed4 c = tex2D(_MainTex, i.uv.xy);
				
				
				float count = step(i.uv2.x, 0) + step(i.uv2.y, 0) + step(1, i.uv2.x) + step(1, i.uv2.y);
				
				if(count > 0)
				{
					return c;
				}
				
				
				fixed4 s = tex2D(_x_shadowMap, i.uv2.xy);
				return s * c + count;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
