using UnityEngine;
using System.Collections;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

[System.Serializable]
public class StaticRainVariables {

	public bool AutoStart = true;
	public bool FullScreen = true;

	public Color OverlayColor = Color.gray;
	public Texture OverlayTexture;
	public Texture NormalMap;

	[Range(0, 15f)]
	public float fadeTime = 2f;
	public AnimationCurve FadeinCurve;

	[Range(0.01f, 20f)]
	public float SizeX = 0f;
	[Range(0.01f, 20f)]
	public float SizeY = 0f;

	[Range(-2, 2f)]
	public float SpawnOffsetX = 0f;
	[Range(-2, 2f)]
	public float SpawnOffsetY = 0f;

	[Range(0.05f, 200.0f)]
	public float DistortionValue;

	[Range(0.0f, 2.0f)]
	public float ReliefValue;

	[Range(0.0f, 2.0f)]
	public float Blur;

	[Range(0.0f, 5.0f)]
	public float Darkness;
}
