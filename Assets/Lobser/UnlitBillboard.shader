Shader "P2/UnlitParticles" {
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
//						#include "UnityLightingCommon.cginc" 
//						#include "AutoLight.cginc"
//						#include "ParticleTypes.cginc"
//						#include "ColorSpaceConversion.hlsl"
//						#include "Common.cginc"
//						#include "noise.hlsl"
//						#include "fbm.cginc"
//						#include "BooleanOperators.hlsl"
					#include "RotationMatrix.hlsl"

					

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

						float3 norm = normalize(p.pos - _WorldSpaceCameraPos);
						float lat = acos(norm.y);
						float lon = atan2(norm.z, norm.x);

						float3 quadNorm = float3(0, 0, 1);
						float3 v1 = norm;
						float3 v2 = quadNorm;
						float angle = acos(dot(v2, v1));
						float3 axis = normalize(cross(v2, v1));

						float3x3 rot = rotationMatrix(axis, angle);

//						float3 noise = lerp(1, snoise(_ScaleFreq*(p.position + _Time.xxy)), 1);
//						_Scale = _Scale*length(p.pos);//*noise;

						float3 vert = _Scale*meshData[meshData[mIndex].index].vert;
						vert = mul(rot, vert);

						float3 localPosition = p.pos + vert;

//						float3 worldPosition = objectPos + localPosition;
//						float3 worldNormal = v.norm;
//						float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
//						float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));

//						Optimization to clip verts of old particles
//						localPosition += when_gt(p.age, _MaxLife)*float3(100000,100000, 100000);

						v2f o;

						o.pos = UnityObjectToClipPos(localPosition);
						o.uv = meshData[meshData[mIndex].index].uv;
						o.ambient = localPosition;
//						o.noise = noise;
						o.pID.x = pIndex;
						o.pID.y = mIndex;
						o.color = float4(1,1,1,1);//p.vel, p.age);
						//TRANSFER_SHADOW(o)
						return o;
					}


					fixed4 frag(v2f i) : SV_Target
					{
						return tex2D(_MainTex, i.uv);
					}

					ENDCG
				}
		}
}
