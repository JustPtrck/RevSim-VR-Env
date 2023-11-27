// Variables synced by WaveManager.cs
float4 _GerstnerWaves[10];
float _GerstnerAmount;
float4 _SineWaves[10];
float _SineAmount;
float4 _ImpactWaves[10];
float _ImpactAmount;
float _ImpactTimes[10];
float _YOffset;


float3 SineWave (float4 wave, float3 p, float time, inout float3 tangent, inout float3 binormal) {
	// Calculates and returns displacement vector at position "p" and for sinewave "wave" 
	// Updates "tangent" vector and "binormal" vector
	float2 direction = float2(cos(wave.x * PI / 180), sin(wave.x * PI / 180));
	float steepness = wave.y;
	float wavelength = wave.z;
	float speed = wave.w;
	if (wave.z = 0) wavelength = 1;

	float k = 2 * PI / wavelength;
	float c = sqrt(9.8 / k) * speed;
	float2 d = normalize(direction);
	float f = k * (dot(d, p.xz) - c * time);
	float a = steepness / k;
	
	tangent += float3(0, d.x * (steepness * cos(f)), 0);
	binormal += float3(0, d.y * (steepness * cos(f)), 0);

	return float3(0, a * sin(f), 0);
}


float3 GerstnerWave (float4 wave, float3 p, float time, inout float3 tangent, inout float3 binormal) {
	// Calculates and returns displacement vector at position "p" and for gerstnerwave "wave" 
	// Updates "tangent" vector and "binormal" vector
	float2 direction = float2(cos(wave.x * PI / 180), sin(wave.x * PI / 180));
	float steepness = wave.y;
	float wavelength = wave.z;
	float speed = wave.w;
	if (wave.z = 0) wavelength = 1;

	float k = 2 * PI / wavelength;
	float c = sqrt(9.8 / k) * speed;
	float2 d = normalize(direction);
	float f = k * (dot(d, p.xz) - c * time);
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

float3 ImpactWave (float4 wave, float3 p, float time, float impact_time, inout float3 tangent, inout float3 binormal) {
	float elapsed_time = time - impact_time;
	float steepness = wave.z;
	float wavelength = wave.w;
	float delay = 3/wavelength;
	float dist = distance(wave.xy, p.xz);
	float k = 2 * PI / (wavelength);
	float maxDist = steepness / k * wavelength;

	float2 d = normalize(p.xz - wave.xy);
	float c = sqrt(9.8 / k);
	float f = k * (dist - elapsed_time * c) ;
	float a = steepness / k * ((maxDist - dist) / maxDist);

	if (dist < maxDist * ( elapsed_time / (delay + elapsed_time) ) ){
		p = float3(
			d.x * a * cos(f), 
			a * sin(f), 
			d.y * a * cos(f)
		);
		tangent += float3(
			-d.x * d.x * (a * k * sin(f)),
			d.x * (a * k * cos(f)),
			-d.x * d.y * (a * k * sin(f))
		);
		binormal += float3(
			-d.x * d.y * (a * k * sin(f)),
			d.y * (a * k * cos(f)),
			-d.y * d.y * (a * k * sin(f))
		); 
		return p;
	}
	return 0;
}

void GetWaveDisplacement_float(float3 vertex_in, float time_in, out float3 out_pos, out float3 out_norm, out float out_y_offset)
{
	// Stacks wavecalculations at position "vertex_in" and returns the displaced position and the normal for this position
    float3 tangent = float3(1,0,0);
    float3 binormal =  float3(0,0,1);
    float3 p = float3(vertex_in.x, _YOffset, vertex_in.z);
	for (int g = 0; g < _GerstnerAmount; g++)
		p += GerstnerWave(_GerstnerWaves[g], vertex_in, time_in, tangent, binormal);
	for (int s = 0; s < _SineAmount; s++)
		p += SineWave(_SineWaves[s], vertex_in, time_in, tangent, binormal);
	for (int i = 0; i < _ImpactAmount; i++)
		p += ImpactWave(_ImpactWaves[i], vertex_in, time_in, _ImpactTimes[i], tangent, binormal);
	float3 normal = normalize(cross(binormal, tangent));
	out_pos = p;
	out_norm = normal;
	out_y_offset = _YOffset;
}