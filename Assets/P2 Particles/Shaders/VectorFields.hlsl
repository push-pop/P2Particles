struct VectorFieldInfo
{
	float3 center;
	float3 resolution;
	float3 boundingMinimum;
	float3 boundingMaximum;
	float fieldScale;
	float forceScale;
};

struct VectorInfo
{
	float3 position;
	float3 direction;
};

StructuredBuffer<VectorFieldInfo> _vFieldInfo;

Texture3D<float4> _vectorField;
SamplerState sampler_vectorField;


float IsInsideVField(float3 p)
{
	VectorFieldInfo info = _vFieldInfo[0];

	float3 pos = (1 / info.fieldScale)*(p - info.center);

	return when_lt(pos.x, info.boundingMaximum.x)
		*when_gt(pos.x, info.boundingMinimum.x)
		*when_lt(pos.y, info.boundingMaximum.y)
		*when_gt(pos.y, info.boundingMinimum.y)
		*when_lt(pos.z, info.boundingMaximum.z)
		*when_gt(pos.z, info.boundingMinimum.z);
}

float3 SampleVectorField(float3 p)
{
	VectorFieldInfo info = _vFieldInfo[0];

	float rangeX = abs(info.boundingMaximum.x - info.boundingMinimum.x) / 2.0;
	float rangeY = abs(info.boundingMaximum.y - info.boundingMinimum.y) / 2.0;
	float rangeZ = abs(info.boundingMaximum.z - info.boundingMinimum.z) / 2.0;

	float3 pos = (1 / info.fieldScale)*(p - info.center);

	float t = ((0.5*pos.x + 0.5 )/ rangeX);
	float u = ((0.5*pos.y + 0.5 )/ rangeY);
	float v = ((0.5*pos.z + 0.5 )/ rangeZ);

	float3 tuv = float3(t, u, v);

	float4 force = info.forceScale*_vectorField.SampleLevel(sampler_vectorField, tuv, 0);

	return force.rgb;
}