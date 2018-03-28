Shader "Superbright/HDRParticles" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "black" {}
		_Rotation ("Rotation", Range(0, 360)) = 0
		_Scale("Scale", Range(0,20)) = 0.5
		_ScaleSpeed("ScaleSpeed", Range(0,200)) = 0.5
		_Falloff("Falloff", Range(0,1)) = 0.5
		_Particlize("Particlize", Range(0,1))=0.0
		_Softness("Softness", Range(0,1))=0.0

	}
		SubShader{
				LOD 100
				Cull Off
			Blend OneMinusDstColor One
			Tags{ "Queue" = "Transparent+1" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "DisableBatching" = "True" }
			ZWrite Off
			AlphaTest Greater .01
			//Blend SrcAlpha OneMinusSrcAlpha
				Pass{

				Tags{ "LightMode" = "ForwardBase"  "DisableBatching" = "True" }

				CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
//#pragma surface surf Lambert
#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
	#pragma target 5.0

	#include "UnityCG.cginc"
	#include "UnityLightingCommon.cginc" 
	#include "AutoLight.cginc"
	#include "ParticleTypes.cginc"
#include "ColorSpaceConversion.hlsl"
#include "Common.cginc"
				sampler2D _MainTex;
				float4 _MainTex_ST;
				half _VertexScale;
				half _Scale;
				half _ScaleSpeed;
				float _Rotation;

				StructuredBuffer<Particle> Particles;
				StructuredBuffer<MeshData> meshData;
				float3 objectPos;
				int MeshIndexCount;
				float3 cameraPos;
				float _Particlize;
				float _Softness;
				sampler3D _vectorField;
				float _UpdateUV;
				float4x4 _ObjectTransform;
				float _Opacity;

				struct SurfaceOutput
				{
					fixed3 Albedo;  // diffuse color
					fixed3 Normal;  // tangent space normal, if written
					fixed3 Emission;
					half Specular;  // specular power in 0..1 range
					fixed Alpha;    // alpha for transparencies
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float3 uv : TEXCOORD0;
					float2 uv_sprite : TEXCOORD1;
					float3 color : TEXCOORD3;
					uint pID : TEXCOORD4;
					SHADOW_COORDS(5)
				};

				float3x3 rotationMatrix(float3 axis, float angle)
				{
					axis = normalize(axis);
					float s = sin(angle);
					float c = cos(angle);
					float oc = 1.0 - c;

					return float3x3(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
						oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
						oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
				}
		
				float4x4 rotationMatrix4(float3 axis, float angle)
				{
					axis = normalize(axis);
					float s = sin(angle);
					float c = cos(angle);
					float oc = 1.0 - c;

					return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
						oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
						oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
						0.0,0.0,0.0,1.0);
				}

				void rotate2D(inout float2 v, float r)
				{
					float s, c;
					sincos(r, s, c);
					v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
				}

				struct VS_INPUT
				{
					float4 pos        : POSITION;
					float3 norm       : NORMAL;
					float2 uv         : TEXCOORD0;
					float4 col        : COLOR0;
					uint   id         : SV_VertexID;
				};

				float3 RotateAroundYInDegrees (float3 vertex, float degrees)
				{
					float alpha = degrees * UNITY_PI / 180.0;
					float sina, cosa;
					sincos(alpha, sina, cosa);
					float2x2 m = float2x2(cosa, -sina, sina, cosa);
					return float3(mul(m, vertex.xz), vertex.y).xzy;
				}

				float2 ToRadialCoords(float3 position)
				{
					float3 norm = normalize(position);
					float lat = acos(norm.y);
					float lon = atan2(norm.z, norm.x);
					float2 sphereCoords =float2(lon, lat)* float2(0.5/UNITY_PI, 1.0/UNITY_PI);
					return float2(0.5,1.0) - sphereCoords;
				}


			v2f vert(VS_INPUT v)
			{

				uint pIndex = v.id / MeshIndexCount;
				uint mIndex = v.id % MeshIndexCount;

				Particle p = Particles[pIndex];
				
				float3 norm = normalize(p.position - objectPos);
				float lat = acos(norm.y);
				float lon = atan2(norm.z, norm.x);
				float2 rad = ToRadialCoords(p.position);

				float3 quadNorm = float3(0, 0, 1);
				float3 v1 = norm;
				float3 v2 = quadNorm;
				float angle = acos(dot(v2, v1));
				float3 axis = normalize(cross(v2, v1));

				float3x3 rot = rotationMatrix(axis, angle);

				float3 vert = meshData[meshData[mIndex].index].vert;
				vert *= _Scale*_VertexScale*0.5;

				vert = mul(rot, vert);
				
				float3 localPosition = p.position*_VertexScale + vert -objectPos;
				//localPosition = RotateAroundYInDegrees(localPosition, -_Rotation+90);
				//localPosition = mul(rotationMatrix4(float3(1, 0, 0), -90), localPosition);
				localPosition = mul(_ObjectTransform, localPosition);


				float3 worldPosition = localPosition +objectPos;
				float3 worldNormal = v.norm; 
				float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));

				 
				v2f o;
				o.pos = UnityObjectToClipPos(worldPosition);
				
				//USE uv at emission point
				float2 tc = p.uv;

				//use uv at current point
				if(_UpdateUV)
					tc = ToRadialCoords(localPosition);

				tc.y = 1-tc.y;
				tc.x = tc.x;
				//tc = TRANSFORM_TEX(tc, _MainTex);

				o.uv.xy = float2(tc.x, tc.y);
				o.uv_sprite = meshData[meshData[mIndex].index].uv;
				o.pID.x = pIndex;
				o.color = float4(o.uv, 1);
				//TRANSFER_SHADOW(o)
				return o;
			}

			//---------------------------------------------------------
			// draw rectangle frame with rounded edges
			//---------------------------------------------------------
			float roundedRectangle(float2 pos, float2 size, float radius)
			{
				float d = length(max(abs( pos), size) - size) - radius;
				//float x = saturate(pow(radius, max(_Softness, 0.0)));
				return smoothstep(0.66, 0.33, d  * 5.0);
			}

			// https://thndl.com/square-shaped-shaders.html
			float hexagon(float2 uv)
			{
				float2 st = uv *2 - float2(1,1);

				// Number of sides of your shape
				int N = 6;

				// Angle and radius from the current pixel
				float a = atan2(st.x, st.y) + UNITY_PI;
				float r = UNITY_TWO_PI / float(N);

				// Shaping function that modulate the distance
				float radius = .7;
				float d = cos(floor(.5 + a / r)*r - a)*length(st);
				return 1.0 - smoothstep(radius, radius + _Softness, d);
			}

			fixed4 frag(v2f i) : SV_Target
				{
					Particle p = Particles[i.pID];



			float3 rgb = tex2D(_MainTex, i.uv).rgb;

			fixed4 output = fixed4(rgb, 1);

			float2 halfRes = lerp(float2(.4, .4), float2(.01, .01), _Particlize);

			float b = roundedRectangle((i.uv_sprite -float2(0.5,0.5)), halfRes, 0.1);
			b = hexagon(i.uv_sprite);
			//output.rgb = float3(1, 1, 0);
			output.rgb*=(b*_Opacity);
			return output;
		}

			ENDCG
		}
		}
}
