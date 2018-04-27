
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
				float3 texcoord : TEXCOORD0;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
				uint vid : SV_VERTEXID;
			};

			struct v2f
			{
				float3 position : TEXCOORD0;
				float3 texcoord : TEXCOORD1;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
				half id : TEXCOORD2;
			};

			RWStructuredBuffer<PointData> _PointBuffer : register(u1);

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
			PointData p = _PointBuffer[v.vid];

				v2f o;
				o.position = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.texcoord = v.texcoord;
				//o.texcoord = v.texcoord;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent = half4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
				o.id= v.vid;

				p.LastPosition = p.CurrentPosition;
				p.CurrentPosition = o.position;
				p.Normal = o.normal;
				p.Tangent = o.tangent;
				p.uv = o.texcoord;

				_PointBuffer[o.id] = p;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				


				discard;
				return 0;
				//return float4(p.CurrentPosition, 1);

			}
			ENDCG
		}
	}
}
