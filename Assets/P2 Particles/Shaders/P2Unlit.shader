Shader "P2/UnlitParticles" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_AlphaMap("Alpha ", 2D) = "white"{}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Scale("Scale", Range(0,20)) = 0.5
		_ScaleSpeed("ScaleSpeed", Range(0,1)) = 0.5
		_Dimmer("Dimmer", range(0,1)) = 1

	}
		SubShader{
				LOD 100
				Cull Off
				ZWrite Off
				Tags{"Queue" = "Transparent+1" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "DisableBatching" = "True" }
				Pass{

						Blend[_SrcMode][_DstMode]
						CGPROGRAM
						#pragma vertex vert
						#pragma fragment frag

						//#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
						#pragma target 5.0

						#include "P2Render.hlsl"
						#include "UnityCG.cginc"
						#include "UnityLightingCommon.cginc" 
						#include "AutoLight.cginc"
						#include "ParticleTypes.cginc"
						#include "ColorSpaceConversion.hlsl"
						#include "Common.cginc"
						#include "noise.hlsl"
						#include "fbm.cginc"
						#include "BooleanOperators.hlsl"
						#include "RotationMatrix.hlsl"

							sampler2D _MainTex;
							sampler2D _AlphaMap;
							float4 _AlphaMap_ST;
							half _Scale;
							half _ScaleSpeed;
							half _ScaleFreq;
							StructuredBuffer<Particle> Particles;
							StructuredBuffer<MeshData> meshData;
							float3 objectPos;
							int MeshIndexCount;
							float _Softness;
							half _Particlize;
							half _NumParticles;
							float4x4 _ObjectTransform;
							half _Dimmer;
							float _MaxLife;
							float _FbmAmt;
							float4 _RemapFbm;
							float _FbmFreq;
							int _BlendEnum;

							sampler2D _UVParam1;
							sampler2D _UVParam2;

							sampler2D _ColorOverLife;

							bool _DebugVelocity;
							bool _CullLife;

							struct v2f
							{
								float4 pos : SV_POSITION;
								float2 uv : TEXCOORD0;
								float3 ambient : TEXCOORD1;
								float3 noise : TEXCOORD2;
								float4 color : TEXCOORD3;
								float2 pID : TEXCOORD4;
								SHADOW_COORDS(5)
							};

							struct VS_INPUT
							{
								float4 pos        : POSITION;
								float3 norm       : NORMAL;
								float2 uv         : TEXCOORD0;
								float4 col        : COLOR0;
								uint   id         : SV_VertexID;
							};

							v2f vert(VS_INPUT v)
							{

								uint pIndex = v.id / MeshIndexCount;
								uint mIndex = v.id % MeshIndexCount;

								Particle p = Particles[pIndex];



								float3 norm = normalize(p.position - _WorldSpaceCameraPos);
								float lat = acos(norm.y);
								float lon = atan2(norm.z, norm.x);

								float3 quadNorm = float3(0, 0, 1);
								float3 v1 = norm;
								float3 v2 = quadNorm;
								float angle = acos(dot(v2, v1));
								float3 axis = normalize(cross(v2, v1));

								float3x3 rot = rotationMatrix(axis, angle);


								float3 noise = lerp(1, snoise(_ScaleFreq*(p.position + _Time.xxy)), 1);

								_Scale = _Scale*length(p.position)*noise;

								float3 vert = _Scale*meshData[meshData[mIndex].index].vert;
								vert = mul(rot, vert);

								float3 localPosition = p.position + vert;

								float3 worldPosition = objectPos + localPosition;
								float3 worldNormal = v.norm;



								float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
								float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));

								//Optimization to clip verts of old particles
								if(_CullLife)
									localPosition += when_gt(p.age, _MaxLife)*float3(100000,100000, 100000);
								v2f o;

								o.pos = UnityObjectToClipPos(localPosition);

								o.uv = meshData[meshData[mIndex].index].uv;
								o.ambient = localPosition;
								o.noise = noise;
								o.pID.x = pIndex;
								o.pID.y = mIndex;
								//o.color = float4(p.velocity, p.age);
								o.color = tex2Dlod(_ColorOverLife, float4(smoothstep(0,_MaxLife, p.age), 0, 0, 0));
								//TRANSFER_SHADOW(o)
								return o;
							}


						float roundedRectangle(float2 uv, float2 size, float radius)
						{
							float2 st = uv * 2 - float2(1,1);
							float d = length(max(abs(st), size) - size) - radius;
							float x = saturate(pow(radius, max(_Softness, 0.0)));
							return smoothstep(0.9, 0.1, d * 5);
						}

						// https://thndl.com/square-shaped-shaders.html
						float hexagon(float2 uv)
						{
							float2 st = uv * 2 - float2(1,1);

							// Number of sides of your shape
							int N = 6;

							// Angle and radius from the current pixel
							float a = atan2(st.x, st.y) + UNITY_PI;
							float r = UNITY_TWO_PI / float(N);

							// Shaping function that modulate the distance
							float radius = .7;
							float d = cos(floor(.5 + a / r)*r - a)*length(st);
							return lerp(1, 1.0 - smoothstep(radius, radius + _Softness, d), _Particlize);
						} 
						 
						float circle(float2 uv)
						{
							half x = length(uv - 0.5) * 2;
							return 1 - smoothstep(1-_Softness, 1, x);
						}

						float remap(float value, float low1, float high1, float low2, float high2)
						{
							return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
						}

						fixed4 frag(v2f i) : SV_Target
						{ 
							float uv = TRANSFORM_TEX(i.uv, _AlphaMap);
							fixed4 albedo = tex2D(_MainTex, i.uv);
							float4 alpha = hexagon(i.uv)* tex2D(_AlphaMap, i.uv);

							fixed4 output = i.color;
							float shape = circle(i.uv);

							float3 camVec = _WorldSpaceCameraPos - i.pos;

							
							if (_DebugVelocity)
								output = i.color;

							float a = fbm(float3(i.ambient * 10 * _FbmFreq + _Time.xyz / 5));
							a = remap(a, _RemapFbm.x, _RemapFbm.y, _RemapFbm.z, _RemapFbm.w);
							a = lerp(1, a, _FbmAmt);

							a *= lerp(1, shape, _Particlize);

							if (_BlendEnum == BLEND_ADD || _BlendEnum == BLEND_SOFT_ADD) {
								output.rgb *= a*i.color.a;
							}
							else
							{
								output.a *= a;
							}

							
							clip(a);

							return output;
						}

						ENDCG
					}
		}
}
