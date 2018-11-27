Shader "Custom/LocalCubemap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha("Alpha", Range(0,1)) = 1

		[NoScaleOffset] _FakeReflection ("Fake Reflection", Cube) = "grey" {}
		_FakeReflectionCenter("Fake Reflection Center", Vector) = (0,0,0,0)
		_FakeReflectionSize("Fake Reflection Size", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityStandardUtils.cginc"
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 wPos : TEXCOORD2;
				float3 wNormal : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			UNITY_DECLARE_TEXCUBE(_FakeReflection);
			float3 _FakeReflectionCenter;
			float3 _FakeReflectionSize;
			half _Alpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.wNormal = unity_WorldToObject[1].xyz;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
// Forward may be different rely on model's local axis direction
				float3 wForward = normalize(unity_WorldToObject[1].xyz);
				float3 wNormal = normalize(i.wNormal);
				float3 wViewDir = normalize(i.wPos - _WorldSpaceCameraPos.xyz);
				float3 wRefl = normalize(reflect(wViewDir, wNormal));
// Calculate Reflection Vector
				wRefl = BoxProjectedCubemapDirection(wRefl, i.wPos, float4(_FakeReflectionCenter, 1), float4(_FakeReflectionCenter - _FakeReflectionSize, 1), float4(_FakeReflectionCenter + _FakeReflectionSize, 1));
				
// Calculate Fake Window Inner
// Cheap method
				//wRefl = wRefl - dot(wRefl, wForward) * wForward * 2;
// Expensive method
				/*float3 oRefl = mul((float3x3)unity_WorldToObject, wRefl);
				oRefl.y *= -1;
				wRefl = mul((float3x3)unity_ObjectToWorld, oRefl);*/
				
// Sample Fake Reflection
				half4 fakeC = UNITY_SAMPLE_TEXCUBE_LOD(_FakeReflection, wRefl, 0);

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return fixed4(fakeC.rgb, _Alpha);
			}
			ENDCG
		}
	}
}
