
Shader "P2/Replacement"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{

			Tags { "P2Replacement" = "Source" }

		Pass
		{
			ZTest Always ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma target 5.0

			#include "UnityCG.cginc"
			#include "PointData.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
				uint vid : SV_VERTEXID;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
				half id : TEXCOORD1;
			};

			RWStructuredBuffer<PointData> _PointBuffer : register(u1);

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.position = float4(v.texcoord.x * 2 - 1, 0, 0, 1);
				o.texcoord = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent = half4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
				o.id= v.vid;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				PointData p = _PointBuffer[i.id];

				p.LastPosition = p.CurrentPosition;
				p.CurrentPosition = i.texcoord;
				p.Normal = i.normal;
				p.Tangent = i.tangent;

				_PointBuffer[i.id] = p;

				discard;

				return float4(p.CurrentPosition, 1);

			}
			ENDCG
		}
	}
}
