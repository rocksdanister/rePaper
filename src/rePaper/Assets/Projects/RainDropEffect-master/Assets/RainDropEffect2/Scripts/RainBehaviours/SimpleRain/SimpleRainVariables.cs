using UnityEngine;
using System.Collections;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

[System.Serializable]
public class SimpleRainVariables {

	public bool AutoStart = true;
	public bool PlayOnce = false;

	public Color OverlayColor = Color.gray;
	public Texture NormalMap;
	public Texture OverlayTexture;

	public bool AutoRotate = false;

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

	public AnimationCurve AlphaOverLifetime;

	[Range(0.0f, 20f)]
	public float SizeMinX = 0.75f;
	[Range(0.0f, 20f)]
	public float SizeMaxX = 0.75f;
	[Range(0.0f, 20f)]
	public float SizeMinY = 0.75f;
	[Range(0.0f, 20f)]
	public float SizeMaxY = 0.75f;
	public AnimationCurve SizeOverLifetime;

	[Range(0.0f, 200.0f)]
	public float DistortionValue;
	public AnimationCurve DistortionOverLifetime;

	[Range(0.0f, 2.0f)]
	public float ReliefValue;
	public AnimationCurve ReliefOverLifetime;

	[Range(0.0f, 2.0f)]
	public float Blur;
	public AnimationCurve BlurOverLifetime;

	public AnimationCurve PosYOverLifetime;

	[Range(0.0f, 5.0f)]
	public float Darkness;
}
