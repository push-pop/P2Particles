float4 when_eq(float4 x, float4 y) {
	return 1.0 - abs(sign(x - y));
}

float4 when_neq(float4 x, float4 y) {
	return abs(sign(x - y));
}

float4 when_gt(float4 x, float4 y) {
	return max(sign(x - y), 0.0);
}

float4 when_lt(float4 x, float4 y) {
	return max(sign(y - x), 0.0);
}

float4 when_ge(float4 x, float4 y) {
	return 1.0 - when_lt(x, y);
}

float4 when_le(float4 x, float4 y) {
	return 1.0 - when_gt(x, y);
}

float4 and(float4 a, float4 b) {
	return a * b;
}

float4 or(float4 a, float4 b) {
	return min(a + b, 1.0);
}

#ifdef SHADER_API_D3D11

float4 xor(float4 a, float4 b) {
	return (a + b) % 2.0;
}

#endif

float4 not(float4 a) {
	return 1.0 - a;
} 

//float3 when_gt(float3 x, float3 y) {
//	return max(sign(x - y), 0.0);
//}
//
//float3 when_lt(float3 x, float3 y) {
//	return max(sign(y - x), 0.0);
//}
//
//float3 when_ge(float3 x, float3 y) {
//	return 1.0 - when_lt(x, y);
//}
//
//float3 when_le(float3 x, float3 y) {
//	return 1.0 - when_gt(x, y);
//}