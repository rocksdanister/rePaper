using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

public class FlowRainController : MonoBehaviour
{

    public FlowRainVariables Variables { get; set; }
    [HideInInspector]
    public int RenderQueue { get; set; }
    public Camera camera { get; set; }
    public float Alpha { get; set; }
	public Vector2 GlobalWind { get; set; }
    public Vector3 GForceVector { get; set; }
    public bool NoMoreRain { get; set; }
    public RainDropTools.RainDropShaderType ShaderType { get; set; }
    public float Distance { get; set; }

    private int oldSpawnLimit = 0;
    private bool isOneShot = false;
    private bool isWaitingDelay = false;
    private float oneShotTimeleft = 0f;
    private float timeElapsed = 0f;
    private float interval = 0f;

    public bool IsPlaying
    {
        get
        {
            return drawers.FindAll(t => t.currentState == DrawState.Disabled).Count != drawers.Count;
        }
    }

    public enum DrawState
    {
        Playing,
        Disabled,
    }

    [System.Serializable]
	public class FlowRainDrawerContainer : RainDrawerContainer<DropTrail>
    {
        public DrawState currentState = DrawState.Disabled;
        public float initRnd = 0f;
        public float posXDt;
        public float rnd1;
        public float fluctuationRate = 5f;
        public float acceleration = 0.1f;

        public Vector3 startPos;
        public float TimeElapsed = 0f;
        public float lifetime = 0f;

        public bool IsEnable
        {
            get { return Drawer.material != null && Drawer.enabled == true; }
        }

        public FlowRainDrawerContainer(string name, Transform parent) : base(name, parent) { }
    }

    public List<FlowRainDrawerContainer> drawers = new List<FlowRainDrawerContainer>();


    /// <summary>
    /// Refresh this instance.
    /// </summary>

    public void Refresh()
    {
        foreach (var d in drawers)
        {
            DestroyImmediate(d.Drawer.gameObject);
        }

        drawers.Clear();

        for (int i = 0; i < Variables.MaxRainSpawnCount; i++)
        {
            FlowRainDrawerContainer container = new FlowRainDrawerContainer("Flow RainDrawer " + i, this.transform);
            container.currentState = DrawState.Disabled;
            drawers.Add(container);
        }
    }


    /// <summary>
    /// Play this instance.
    /// </summary>
    public void Play()
    {
        StartCoroutine(PlayDelay(Variables.Delay));
    }


    IEnumerator PlayDelay(float delay)
    {
        float t = 0f;
        while (t <= delay)
        {
            isWaitingDelay = true;
            t += Time.deltaTime;
            yield return null;
        }
        isWaitingDelay = false;

        if (drawers.Find(x => x.currentState == DrawState.Playing) != null)
        {
            yield break;
        }

        for (int i = 0; i < drawers.Count; i++)
        {
            InitializeDrawer(drawers[i]);
            drawers[i].currentState = DrawState.Disabled;
        }

        isOneShot = Variables.PlayOnce;
        if (isOneShot)
        {
            oneShotTimeleft = Variables.Duration;
        }

        yield break;
    }


    /// <summary>
    /// Update.
    /// </summary>
    public void UpdateController()
    {
        if (Variables == null)
        {
            return;
        }

        CheckSpawnNum();

        if (NoMoreRain)
        {
            timeElapsed = 0f;
        }
        else if (isOneShot)
        {
            oneShotTimeleft -= Time.deltaTime;
            if (oneShotTimeleft > 0f)
            {
                CheckSpawnTime();
            }
        }
        else if (!isWaitingDelay)
        {
            CheckSpawnTime();
        }

        for (int i = 0; i < drawers.Count; i++)
        {
            UpdateInstance(drawers[i], i);
        }
    }


    private void CheckSpawnNum()
    {
        int diff = Variables.MaxRainSpawnCount - drawers.Count();

        // MaxRainSpawnCount was increased
        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                FlowRainDrawerContainer container = new FlowRainDrawerContainer("Flow RainDrawer " + (drawers.Count() + i), this.transform);
                container.currentState = DrawState.Disabled;
                drawers.Add(container);
            }
        }

        // MaxRainSpawnCount was decreased
        if (diff < 0)
        {
            int rmcnt = -diff;
            List<FlowRainDrawerContainer> removeList = drawers.FindAll(x => x.currentState != DrawState.Playing).Take(rmcnt).ToList();
            if (removeList.Count() < rmcnt)
            {
                removeList.AddRange(drawers.FindAll(x => x.currentState == DrawState.Playing).Take(rmcnt - removeList.Count()));
            }

            foreach (var rem in removeList)
            {
                rem.Drawer.Clear();
                DestroyImmediate(rem.Drawer.gameObject);
            }

            drawers.RemoveAll(x => x.Drawer == null);
        }
    }


    private void CheckSpawnTime()
    {
		if (interval == 0f) 
		{
			interval = Variables.Duration / RainDropTools.Random(Variables.EmissionRateMin, Variables.EmissionRateMax);
		}

		timeElapsed += Time.deltaTime;
		if (timeElapsed >= interval)
		{
			int spawnNum = (int) Mathf.Min ((timeElapsed / interval), Variables.MaxRainSpawnCount - drawers.FindAll (x => x.currentState == DrawState.Playing).Count ());
			for (int i = 0; i < spawnNum; i++)
			{
				Spawn();
			}
			interval = Variables.Duration / RainDropTools.Random(Variables.EmissionRateMin, Variables.EmissionRateMax);
			timeElapsed = 0f;
		}
    }


    private void Spawn()
    {
        var spawnRain = drawers.Find(x => x.currentState == DrawState.Disabled);
        if (spawnRain == null)
        {
            //Debug.LogError ("Spawn limit!");
            return;
        }

        InitializeDrawer(spawnRain);
        spawnRain.currentState = DrawState.Playing;
    }


    private float GetProgress(FlowRainDrawerContainer dc)
    {
        return dc.TimeElapsed / dc.lifetime;
    }


    private void InitializeDrawer(FlowRainDrawerContainer dc)
    {
        dc.TimeElapsed = 0f;
        dc.lifetime = RainDropTools.Random(Variables.LifetimeMin, Variables.LifetimeMax);
        dc.fluctuationRate = RainDropTools.Random(Variables.fluctuationRateMin, Variables.fluctuationRateMax);
        dc.acceleration = RainDropTools.Random(Variables.AccelerationMin, Variables.AccelerationMax);
        dc.transform.localPosition = RainDropTools.GetSpawnLocalPos(this.transform, camera, 0f, Variables.SpawnOffsetY);
        dc.startPos = dc.transform.localPosition;

        dc.acceleration = RainDropTools.Random(Variables.AccelerationMin, Variables.AccelerationMax);
        Material mat = RainDropTools.CreateRainMaterial(ShaderType, RenderQueue);
        RainDropTools.ApplyRainMaterialValue(
            mat,
            ShaderType,
            Variables.NormalMap,
            Variables.OverlayTexture,
            Variables.DistortionValue,
            Variables.OverlayColor,
            Variables.ReliefValue,
            Variables.Blur,
            Variables.Darkness
        );
		dc.Drawer.lifeTime = dc.lifetime;
		dc.Drawer.vertexDistance = 0.01f;
		dc.Drawer.angleDivisions = 20;
        dc.Drawer.material = mat;
        dc.Drawer.widthCurve = Variables.TrailWidth;
        dc.Drawer.widthMultiplier = RainDropTools.Random(Variables.SizeMinX, Variables.SizeMaxX);
        dc.Drawer.textureMode = LineTextureMode.Stretch;
        dc.Drawer.vertexDistance = (1f * this.Distance * RainDropTools.GetCameraOrthographicSize(this.camera).y) / (Variables.Resolution * 10f);
        dc.Drawer.Clear();
        dc.Drawer.enabled = false;
    }


    private void UpdateTransform(FlowRainDrawerContainer dc)
    {
        Action initRnd = () =>
        {
            dc.rnd1 = RainDropTools.Random(-0.1f * Variables.Amplitude, 0.1f * Variables.Amplitude);
            dc.posXDt = 0f;
        };

        if (dc.posXDt == 0f)
        {
            StartCoroutine(
                Wait(
                    0.01f,
                    0.01f,
                    (int)(1f / dc.fluctuationRate * 100),
                    () =>
                    {
                        initRnd();
                    }
                )
            );
        }

        dc.posXDt += 0.01f * Variables.Smooth * Time.deltaTime;

        if (dc.rnd1 == 0f)
        {
            initRnd();
        }

        float t = dc.TimeElapsed;

        Vector3 downward = RainDropTools.GetGForcedScreenMovement(this.camera.transform, this.GForceVector);
        downward = -downward.normalized;

        Vector3 nextPos = new Vector3(
            Vector3.Slerp(dc.transform.localPosition, dc.transform.localPosition + downward * dc.rnd1, dc.posXDt).x,
            dc.startPos.y - downward.y * (1 / 2f) * t * t * dc.acceleration - Variables.InitialVelocity * t,
            0.001f // TODO: Work around
        );

        dc.transform.localPosition = nextPos;

        dc.transform.localPosition += GetProgress(dc) * new Vector3(GlobalWind.x, GlobalWind.y, 0f);
    }


    private void UpdateShader(FlowRainDrawerContainer dc, int index)
    {
        float progress = GetProgress(dc);
        dc.Drawer.material.renderQueue = RenderQueue + index;

        // Update shader if needed
        if (dc.Drawer.material.shader.name != RainDropTools.GetShaderName(ShaderType))
        {
            dc.Drawer.material = RainDropTools.CreateRainMaterial(ShaderType, RenderQueue + index);
        }

        float distortionValue = Variables.DistortionValue * Variables.DistortionOverLifetime.Evaluate(progress) * Alpha;
        float reliefValue = Variables.ReliefValue * Variables.ReliefOverLifetime.Evaluate(progress) * Alpha;
        float blurValue = Variables.Blur * Variables.BlurOverLifetime.Evaluate(progress) * Alpha;
        Color overlayColor = new Color(
            Variables.OverlayColor.r,
            Variables.OverlayColor.g,
            Variables.OverlayColor.b,
            Variables.OverlayColor.a * Variables.AlphaOverLifetime.Evaluate(progress) * Alpha
        );

        switch (ShaderType)
        {
            case RainDropTools.RainDropShaderType.Expensive:
                if (distortionValue == 0f && reliefValue == 0f && overlayColor.a == 0f && blurValue == 0f)
                {
                    dc.Drawer.enabled = false;
                    return;
                }
                break;
            case RainDropTools.RainDropShaderType.Cheap:
                if (distortionValue == 0f)
                {
                    dc.Drawer.enabled = false;
                    return;
                }
                break;
            case RainDropTools.RainDropShaderType.NoDistortion:
                if (reliefValue == 0f && overlayColor.a == 0f)
                {
                    dc.Drawer.enabled = false;
                    return;
                }
                break;
        }

        RainDropTools.ApplyRainMaterialValue(
            dc.Drawer.material,
            ShaderType,
            Variables.NormalMap,
            Variables.OverlayTexture,
            distortionValue,
            overlayColor,
            reliefValue,
            blurValue,
            Variables.Darkness * Alpha
        );
        dc.Drawer.enabled = true;
    }


    /// <summary>
    /// Update rain variables
    /// </summary>
    /// <param name="i">The index.</param>
    private void UpdateInstance(FlowRainDrawerContainer dc, int index)
    {
        if (dc.currentState == DrawState.Playing)
        {
            if (GetProgress(dc) >= 1.0f)
            {
                dc.Drawer.Clear();
                dc.currentState = DrawState.Disabled;
            }
            else
            {
                dc.TimeElapsed += Time.deltaTime;
                UpdateTransform(dc);
                UpdateShader(dc, index);
            }
        }
    }


    IEnumerator Wait(float atLeast = 0.5f, float step = 0.1f, int rndMax = 20, Action callBack = null)
    {
        float elapsed = 0f;
        while (elapsed < atLeast)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        while (RainDropTools.Random(0, rndMax) != 0)
        {
            elapsed = 0f;
            while (elapsed < step)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (callBack != null)
        {
            callBack();
        }
        yield break;
    }
}
