using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

public class SimpleRainController : MonoBehaviour
{
    public SimpleRainVariables Variables { get; set; }
    [HideInInspector]
    public int RenderQueue { get; set; }
    public Camera camera { get; set; }
    public float Alpha { get; set; }
	public Vector2 GlobalWind { get; set; }
    public Vector3 GForceVector { get; set; }
    public bool NoMoreRain { get; set; }
    public RainDropTools.RainDropShaderType ShaderType { get; set; }

    private int oldSpawnLimit = 0;
    private bool isOneShot = false;
    private float oneShotTimeleft = 0f;
    private float timeElapsed = 0f;
    private float interval = 0f;
    private bool isWaitingDelay = false;

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
    public class SimpleRainDrawerContainer : RainDrawerContainer<RainDrawer>
    {
        public DrawState currentState = DrawState.Disabled;
        public Vector3 startSize;
        public Vector3 startPos;
        public float TimeElapsed = 0f;
        public float lifetime = 0f;

        public SimpleRainDrawerContainer(string name, Transform parent) : base(name, parent) { }
    }

    public List<SimpleRainDrawerContainer> drawers = new List<SimpleRainDrawerContainer>();

    /// <summary>
    /// Refresh this instance.
    /// </summary>

    public void Refresh()
    {
        foreach (var d in drawers)
        {
            d.Drawer.Hide();
            DestroyImmediate(d.Drawer.gameObject);
        }

        drawers.Clear();

        for (int i = 0; i < Variables.MaxRainSpawnCount; i++)
        {
            SimpleRainDrawerContainer container = new SimpleRainDrawerContainer("Simple RainDrawer " + i, this.transform);
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

        for (int i = 0; i < drawers.Count(); i++)
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
                SimpleRainDrawerContainer container = new SimpleRainDrawerContainer("Simple RainDrawer " + (drawers.Count() + i), this.transform);
                container.currentState = DrawState.Disabled;
                drawers.Add(container);
            }
        }

        // MaxRainSpawnCount was decreased
        if (diff < 0)
        {
            int rmcnt = -diff;
            List<SimpleRainDrawerContainer> removeList = drawers.FindAll(x => x.currentState != DrawState.Playing).Take(rmcnt).ToList();
            if (removeList.Count() < rmcnt)
            {
                removeList.AddRange(drawers.FindAll(x => x.currentState == DrawState.Playing).Take(rmcnt - removeList.Count()));
            }

            foreach (var rem in removeList)
            {
                rem.Drawer.Hide();
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


    private float GetProgress(SimpleRainDrawerContainer dc)
    {
        return dc.TimeElapsed / dc.lifetime;
    }


    private void InitializeDrawer(SimpleRainDrawerContainer dc)
    {
        dc.TimeElapsed = 0f;
        dc.lifetime = RainDropTools.Random(Variables.LifetimeMin, Variables.LifetimeMax);
        dc.transform.localPosition = RainDropTools.GetSpawnLocalPos(this.transform, camera, 0f, Variables.SpawnOffsetY);
        dc.startPos = dc.transform.localPosition;
        dc.startSize = new Vector3(
            RainDropTools.Random(Variables.SizeMinX, Variables.SizeMaxX),
            RainDropTools.Random(Variables.SizeMinY, Variables.SizeMaxY),
            1f
        );
		dc.transform.localEulerAngles += Vector3.forward * (Variables.AutoRotate ? UnityEngine.Random.Range(0f, 179.9f) : 0f);
        dc.Drawer.NormalMap = Variables.NormalMap;
        dc.Drawer.ReliefTexture = Variables.OverlayTexture;
        dc.Drawer.Darkness = Variables.Darkness;
        dc.Drawer.Hide();
    }


    private void UpdateShader(SimpleRainDrawerContainer dc, int index)
    {
        float progress = GetProgress(dc);
        dc.Drawer.RenderQueue = RenderQueue + index;
        dc.Drawer.NormalMap = Variables.NormalMap;
        dc.Drawer.ReliefTexture = Variables.OverlayTexture;
        dc.Drawer.OverlayColor = new Color(
            Variables.OverlayColor.r,
            Variables.OverlayColor.g,
            Variables.OverlayColor.b,
            Variables.OverlayColor.a * Variables.AlphaOverLifetime.Evaluate(progress) * Alpha
        );
        dc.Drawer.DistortionStrength = Variables.DistortionValue * Variables.DistortionOverLifetime.Evaluate(progress) * Alpha;
        dc.Drawer.ReliefValue = Variables.ReliefValue * Variables.ReliefOverLifetime.Evaluate(progress) * Alpha;
        dc.Drawer.Blur = Variables.Blur * Variables.BlurOverLifetime.Evaluate(progress) * Alpha;
        dc.Drawer.Darkness = Variables.Darkness * Alpha;
        dc.transform.localScale = dc.startSize * Variables.SizeOverLifetime.Evaluate(progress);
        // old
        //dc.transform.localPosition = dc.startPos + Vector3.up * Variables.PosYOverLifetime.Evaluate(progress);
        Vector3 gforced = RainDropTools.GetGForcedScreenMovement(this.camera.transform, this.GForceVector);
        gforced = gforced.normalized;
        dc.transform.localPosition += new Vector3(-gforced.x, -gforced.y, 0f) * 0.01f * Variables.PosYOverLifetime.Evaluate(progress);
        dc.transform.localPosition += progress * new Vector3(GlobalWind.x, GlobalWind.y, 0f);
        dc.transform.localPosition = new Vector3(dc.transform.localPosition.x, dc.transform.localPosition.y, 0f);
        dc.Drawer.ShaderType = this.ShaderType;
        dc.Drawer.Show();
    }


    /// <summary>
    /// Update rain variables
    /// </summary>
    /// <param name="i">The index.</param>
    private void UpdateInstance(SimpleRainDrawerContainer dc, int index)
    {
        if (dc.currentState == DrawState.Playing)
        {
            if (GetProgress(dc) >= 1.0f)
            {
                dc.Drawer.Hide();
                dc.currentState = DrawState.Disabled;
            }
            else
            {
                dc.TimeElapsed += Time.deltaTime;
                UpdateShader(dc, index);
            }
        }
    }
}
