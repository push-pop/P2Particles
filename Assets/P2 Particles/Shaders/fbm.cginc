				float noise3D(float3 p)
{
	return frac(sin(dot(p ,float3(12.9898,78.233,126.7378))) * 43758.5453)*2.0-1.0;
}

float linear3D(float3 p)
{
	float3 p0 = floor(p);
	float3 p1x = float3(p0.x+1.0, p0.y, p0.z);
	float3 p1y = float3(p0.x, p0.y+1.0, p0.z);
	float3 p1z = float3(p0.x, p0.y, p0.z+1.0);
	float3 p1xy = float3(p0.x+1.0, p0.y+1.0, p0.z);
	float3 p1xz = float3(p0.x+1.0, p0.y, p0.z+1.0);
	float3 p1yz = float3(p0.x, p0.y+1.0, p0.z+1.0);
	float3 p1xyz = p0+1.0;
	
	float r0 = noise3D(p0);
	float r1x = noise3D(p1x);
	float r1y = noise3D(p1y);
	float r1z = noise3D(p1z);
	float r1xy = noise3D(p1xy);
	float r1xz = noise3D(p1xz);
	float r1yz = noise3D(p1yz);
	float r1xyz = noise3D(p1xyz);
	
	float a = lerp(r0, r1x, p.x-p0.x);
	float b = lerp(r1y, r1xy, p.x-p0.x);
	float ab = lerp(a, b, p.y-p0.y);
	float c = lerp(r1z, r1xz, p.x-p0.x);
	float d = lerp(r1yz, r1xyz, p.x-p0.x);
	float cd = lerp(c, d, p.y-p0.y);
	
	
	float res = lerp(ab, cd, p.z-p0.z);
	
	return res;
}

			float fbm(float3 p)
			{
			float f = 0.5000*linear3D(p*1.0); 
		  f+= 0.2500*linear3D(p*2.01); 
		  f+= 0.1250*linear3D(p*4.02); 
		  f+= 0.0625*linear3D(p*8.03);
		  f/= 0.9375;
	return f;
}