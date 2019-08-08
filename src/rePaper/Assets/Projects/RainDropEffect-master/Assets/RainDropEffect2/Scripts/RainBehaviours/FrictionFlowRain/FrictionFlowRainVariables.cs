using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

[System.Serializable]
public class FrictionFlowRainVariables {

	public bool AutoStart = true;
	public bool PlayOnce = false;

	public Color OverlayColor = Color.gray;
	public Texture NormalMap;
	public Texture OverlayTexture;
	public Texture2D FrictionMap;

	public float Duration = 1f;
    public float Delay = 0f;

    public int MaxRainSpawnCount = 30;

	[Range(-2, 2f)]
	public float SpawnOffsetY = 0f;

	[Range(0f, 10.0f)]
	public float LifetimeMin = 0.6f;
	[Range(0f, 10.0f)]
	public float LifetimeMax = 1.4f;

	[Range(0, 50f)]
	public int EmissionRateMax = 5;

	[Range(0, 50f)]
	public int EmissionRateMin = 2;

    [Range(5, 1024)]
    public int Resolution = 500;

	public AnimationCurve AlphaOverLifetime;

	[Range(0.0f, 20f)]
	public float SizeMinX = 0.75f;
	[Range(0.0f, 20f)]
	public float SizeMaxX = 0.75f;
	public AnimationCurve TrailWidth;

	[Range(0.0f, 200.0f)]
	public float DistortionValue;
	public AnimationCurve DistortionOverLifetime;

	[Range(0.0f, 2.0f)]
	public float ReliefValue;
	public AnimationCurve ReliefOverLifetime;

	[Range(0.0f, 20.0f)]
	public float Blur;
	public AnimationCurve BlurOverLifetime;

	[Range(0.0f, 5.0f)]
	public float Darkness;

	[Range(-40f, 40f)]
	public float InitialVelocity = 0.0f;

	[Range(-5f, 5f)]
	public float AccelerationMin = 0.06f;

	[Range(-5f, 5f)]
	public float AccelerationMax = 0.2f;

}
