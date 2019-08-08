// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

static inline float3 ComputeDiffuse (float3 lightWorldPos, float3 modelNormal, float3 lightColor, float minimun) {
	float3 L = normalize (lightWorldPos);
	float3 N = normalize ((mul(float4(modelNormal, 0.0), unity_WorldToObject).xyz));
	return lightColor * max (minimun, dot (N, L));
}

static inline half4 GrabPixel (sampler2D grab_texture, half4 uvgrab, half4 _grab_texel_size, half weight, half kern, half size) {
	return tex2Dproj(
		grab_texture, 
		UNITY_PROJ_COORD(half4(uvgrab.x + _grab_texel_size.x * kern * size, uvgrab.y, uvgrab.z, uvgrab.w))
	) * weight;
}

static inline fixed3 ComputeBlur (sampler2D grab_texture, half4 uvgrab, half4 _grab_texel_size, fixed alpha, half blur) {
	half3 blured = half3(0.0, 0.0, 0.0);
	half intensity = alpha * blur * 1000;

	half4 sum = half4(0,0,0,0);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.05, -4.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.09, -3.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.12, -2.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.15, -1.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.18,  0.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.15, +1.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.12, +2.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.09, +3.0, intensity);
	sum += GrabPixel(grab_texture, uvgrab, _grab_texel_size, 0.05, +4.0, intensity);
	return sum;
}