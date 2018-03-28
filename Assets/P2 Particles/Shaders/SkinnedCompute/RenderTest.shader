// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SkinnedMesh/RenderTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry GS_Main
			// make fog work
			#pragma multi_compile_fog
			#pragma target 5.0
			

			#include "PointData.cginc"
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				uint id : SV_VertexID;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv :  TEXCOORD0;
			};

			struct g2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _Positions;
			float4 _MainTex_ST;
			StructuredBuffer<PointData> _PointBuffer;
			
			// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void GS_Main(point v2g p[1], inout TriangleStream<g2f> triStream)
				{
					float3 up = float3(0, 1, 0);
					float3 look = _WorldSpaceCameraPos - p[0].vertex;
					look.y = 0;
					look = normalize(look);
					float3 right = cross(up, look);
					
					float halfS = .01;//_Size;
							
					float4 v[4];
					v[0] = float4(p[0].vertex + halfS * right - halfS * up, 1.0f);
					v[1] = float4(p[0].vertex + halfS * right + halfS * up, 1.0f);
					v[2] = float4(p[0].vertex - halfS * right - halfS * up, 1.0f);
					v[3] = float4(p[0].vertex - halfS * right + halfS * up, 1.0f);

                    g2f pIn;
 
                    pIn.vertex = UnityObjectToClipPos(v[0]);
                    pIn.uv = float2(1.0f, 0.0f);
                    triStream.Append(pIn);
 
                    pIn.vertex = UnityObjectToClipPos(v[1]);
                    pIn.uv = float2(1.0f, 1.0f);
                    triStream.Append(pIn);
 
                    pIn.vertex = UnityObjectToClipPos(v[2]);
                    pIn.uv = float2(0.0f, 0.0f);
                    triStream.Append(pIn);
 
                    pIn.vertex = UnityObjectToClipPos(v[3]);
                    pIn.uv = float2(0.0f, 1.0f);
                    triStream.Append(pIn);
			}


			v2g vert (appdata v)
			{
				PointData p = _PointBuffer[v.id];

				float4 worldPos = float4(p.CurrentPosition, 1);

				v2g o;
//				o.vertex = UnityObjectToClipPos(pos.xyz);
				o.vertex = worldPos;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col = fixed4(0,1,1,1);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
