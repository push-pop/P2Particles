Shader "P2/BillboardLine" {

	Properties{

		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}

	}
		SubShader{
				LOD 100
				Cull Off
				ZWrite Off
		
				Tags{"Queue" = "Transparent+1"}// "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "DisableBatching" = "True" }
				Pass{

					Blend one one

					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					//#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
					#pragma target 5.0

					#include "UnityCG.cginc"
					#include "RotationMatrix.hlsl"
					#include "noise.cginc"
					

					struct MeshData
					{
						float3	vert;
						float2	uv;
						int		index;
						float3 norm;
					};

					struct Vec
					{
						float3 pos;
						float3 nPos;

						float3 vel;
//								float2 uv;
//								float life;
//								float a
					};
					  

					struct v2f
					{
						float4 pos : SV_POSITION;
						float2 uv : TEXCOORD0;
						float3 ambient : TEXCOORD1;
//								float3 noise : TEXCOORD2;
						float4 color : TEXCOORD3;
						float2 pID : TEXCOORD4;
//								SHADOW_COORDS(5)
					};

					struct VS_INPUT
					{
						float4 pos        : POSITION;
						float3 norm       : NORMAL;
						float2 uv         : TEXCOORD0;
						float4 col        : COLOR0;
						uint   id         : SV_VertexID;
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;
					float4 _Color;
					int MeshIndexCount;

					half _Scale;
					half _ScaleSpeed;
					half _ScaleFreq;

					float3 objectPos;

					StructuredBuffer<Vec> Particles;
					StructuredBuffer<MeshData> meshData;

					v2f vert(VS_INPUT v)
					{

						uint pIndex = v.id / MeshIndexCount;
						uint mIndex = v.id % MeshIndexCount;

						Vec p = Particles[pIndex];

						v2f o;

						o.uv = meshData[meshData[mIndex].index].uv;
						float3 norm = normalize(p.pos - _WorldSpaceCameraPos);
						float3 angle = p.pos - p.nPos;
						float3 axis = normalize(cross(norm, angle));

						//float3 vert = _Scale*meshData[meshData[mIndex].index].vert;// *float3(o.uv.x + p.vel.x * 100, o.uv.y + p.vel.y * 100, 0);
						float3 vert =  lerp(p.pos + axis * (.5-o.uv.y) *_Scale, p.nPos + axis * (.5-o.uv.y) *_Scale, o.uv.x);// meshData[meshData[mIndex].index].vert;
						//vert = mul(rot, vert);

						float3 localPosition =  vert;

						o.pos = UnityObjectToClipPos(localPosition);

						float u = o.uv.y;// ((cos(o.uv.y*6.28))*-.5) + 1.25;
						o.color = float4(vert.x*4, vert.y*4, vert.z*4, o.uv.y);// abs(p.vel.x) + .021, abs(p.vel.y) + .021, abs(p.vel.z) + .021, 1.0);//float4(1,1,1,1);//p.vel, p.age);
						return o;
					}


					fixed4 frag(v2f i) : SV_Target
					{
						float u = ((cos(i.color.a*6.28))*-.5) + .5;
						float n = abs(snoise(i.color.xyz));
						float n2 = abs(snoise(i.color.xyz+n*u*.3));
						u *= n2;
						return float4(u, u, u, 1.0) * _Color * _Color.a *5;// tex2D(_MainTex, i.uv)*i.color;
					}

					ENDCG
				}
		}
}
