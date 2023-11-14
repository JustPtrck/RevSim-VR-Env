// Variables synced by WaveManager.cs
float4 _Waves[5];
int _WaveAmount;
int _WaveType;
float _YOffset;
float _SyncedTime;


float3 SineWave (float4 wave, float3 p, inout float3 tangent, inout float3 binormal) {
	float2 direction = float2(cos(wave.x * PI / 180), sin(wave.x * PI / 180));
	float steepness = wave.y;
	float wavelength = wave.z;
	float speed = wave.w;
	if (wave.z = 0) wavelength = 1;

	float k = 2 * PI / wavelength;
	float c = sqrt(9.8 / k) * speed;
	float2 d = normalize(direction);
	float f = k * (dot(d, p.xz) - c * _SyncedTime);
	float a = steepness / k;
	
	tangent += float3(0, d.x * (steepness * cos(f)), 0);
	binormal += float3(0, d.y * (steepness * cos(f)), 0);

	return float3(0, a * sin(f), 0);
}


float3 GerstnerWave (float4 wave, float3 p, inout float3 tangent, inout float3 binormal) {
	float2 direction = float2(cos(wave.x * PI / 180), sin(wave.x * PI / 180));
	float steepness = wave.y;
	float wavelength = wave.z;
	float speed = wave.w;
	if (wave.z = 0) wavelength = 1;

	float k = 2 * PI / wavelength;
	float c = sqrt(9.8 / k) * speed;
	float2 d = normalize(direction);
	float f = k * (dot(d, p.xz) - c * _SyncedTime);
	float a = steepness / k;

	tangent += float3(
		-d.x * d.x * (steepness * sin(f)),
		d.x * (steepness * cos(f)),
		-d.x * d.y * (steepness * sin(f))
	);
	binormal += float3(
		-d.x * d.y * (steepness * sin(f)),
		d.y * (steepness * cos(f)),
		-d.y * d.y * (steepness * sin(f))
	);
	return float3(
		d.x * (a * cos(f)),
		a * sin(f),
		d.y * (a * cos(f))
	);
}



void GetWaveDisplacement_half(float3 vertex_in, out float3 out_pos, out float3 out_norm, out float out_y_offset)
{
    float3 tangent = float3(1,0,0);
    float3 binormal =  float3(0,0,1);
    float3 p = float3(vertex_in.x, _YOffset, vertex_in.z);
	for (int i = 0; i < _WaveAmount; i++)
	{
		if (_WaveType == 0) p += SineWave(_Waves[i], vertex_in, tangent, binormal);
		else if (_WaveType == 1) p += GerstnerWave(_Waves[i], vertex_in, tangent, binormal);
	}
	float3 normal = normalize(cross(binormal, tangent));
	out_pos = p;
	out_norm = normal;
	out_y_offset = _YOffset;
}
