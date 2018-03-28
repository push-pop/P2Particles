Shader "P2/LitParticles" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
	  _Color("Color", Color) = (1,1,1,1)

	  _EmissionColor("Emission", Color) = (0,0,0)


      _Metallic("Metallic", Range(0,1)) = 1.0
      _Smoothness("Smoothness", Range(0,1)) = 1.0
				_ScaleSpeed("ScaleSpeed", Range(0,200)) = 0.5

      _BumpScale("Scale", Float) = 1.0
      _BumpMap("Normal Map", 2D) = "bump" {}
	  _NumSides("Num Sides", Range(1,10)) = 5.0
	  _Falloff("Falloff", Float) = 0.5
	  _Strobe("Strobe", Float) = 0
    }
    SubShader {
	LOD 100
      Cull Back

	  Tags { "RenderType" = "Opaque" }
      CGPROGRAM
	#pragma target 5.0

      #pragma surface surf Standard vertex:vert

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
	float4 _Color;
	half _Scale;
	half _ScaleSpeed;
	half _ScaleFreq;
	
	#ifdef SHADER_API_D3D11
	StructuredBuffer<Particle> Particles;
	StructuredBuffer<MeshData> meshData;
	#endif
	
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
	sampler2D _BumpMap;
	
	bool _DebugVelocity;
	
	  struct Input
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 ambient : TEXCOORD1;
			float3 noise : TEXCOORD2;
			float4 color : TEXCOORD3;
			float2 pID : TEXCOORD4;
			SHADOW_COORDS(5)
		};

		struct appdata {
			float4 vertex        : POSITION;
			float3 normal       : NORMAL;
			float4 tangent		: TANGENT;
			float2 uv         : TEXCOORD0;
			float4 col        : COLOR0;
			uint   id         : SV_VertexID;
        };

      void vert (inout appdata v, out Input o) {
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


			float3 vert = _Scale*meshData[meshData[mIndex].index].vert;
			//vert = mul(rot, vert);

			float3 localPosition = p.position + vert;

			float3 worldPosition = objectPos + localPosition;
			float3 worldNormal = v.normal;



			float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
			float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));

			//Optimization to clip verts of old particles
			//localPosition += when_gt(p.age, _MaxLife)*float3(100000,100000, 100000);
			 
		   UNITY_INITIALIZE_OUTPUT(Input,o);

			o.pos = UnityObjectToClipPos(localPosition);

			o.uv = meshData[meshData[mIndex].index].uv;
			o.ambient = localPosition;
			o.noise = float3(0,0,0);
			o.pID.x = pIndex;
			o.pID.y = mIndex;
			o.color = float4(p.velocity, p.age);

			//o.pos.z = .3;
			v.vertex = o.pos;
//			v.vertex = float4(o.pos.xyz+ float3(0,0,0), 1);
      }
	   

      void surf (Input IN, inout SurfaceOutputStandard o) {
			float hue = fmod(10*_Time.x + 0.02*(IN.pID.x), 1.0f);

			float3 rgb = HUEtoRGB(.5);

			//float alpha = pow(saturate(1 - x), exp(3 - _Softness * 6));
			//alpha = x;
			o.Albedo = rgb;
			o.Normal =  UnpackNormal (tex2D(_BumpMap, IN.ambient.xy));
			o.Smoothness = 1;
			o.Metallic = 0;
			//o.Alpha  = pow(saturate(1 - x), exp(3 - _Shape * 6));
			//o.Emission = _Strobe*rgb;
			//o.Emission = float3(1,0,0);
			o.Alpha = 1;
			//if(alpha <0.7)discard;
      }
      ENDCG
    } 
    Fallback "Diffuse" 
  }