using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

public class FrictionFlowRainController : MonoBehaviour
{

    public FrictionFlowRainVariables Variables { get; set; }
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
	public class FrictionFlowRainDrawerContainer : RainDrawerContainer<DropTrail>
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

        public FrictionFlowRainDrawerContainer(string name, Transform parent) : base(name, parent) { }
    }

    public List<FrictionFlowRainDrawerContainer> drawers = new List<FrictionFlowRainDrawerContainer>();

    Transform _dummy;
    Transform dummy
    {
        get
        {
            if (!_dummy)
                _dummy = RainDropTools.CreateHiddenObject("dummy", this.transform);
            return _dummy;
        }
    }

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
            FrictionFlowRainDrawerContainer container = new FrictionFlowRainDrawerContainer("Friction Flow RainDrawer " + i, this.transform);
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
                FrictionFlowRainDrawerContainer container = new FrictionFlowRainDrawerContainer("Friction Flow RainDrawer " + (drawers.Count() + i), this.transform);
                container.currentState = DrawState.Disabled;
                drawers.Add(container);
            }
        }

        // MaxRainSpawnCount was decreased
        if (diff < 0)
        {
            int rmcnt = -diff;
            List<FrictionFlowRainDrawerContainer> removeList = drawers.FindAll(x => x.currentState != DrawState.Playing).Take(rmcnt).ToList();
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


    private float GetProgress(FrictionFlowRainDrawerContainer dc)
    {
        return dc.TimeElapsed / dc.lifetime;
    }


    private void InitializeDrawer(FrictionFlowRainDrawerContainer dc)
    {
        dc.TimeElapsed = 0f;
        dc.lifetime = RainDropTools.Random(Variables.LifetimeMin, Variables.LifetimeMax);
        dc.acceleration = RainDropTools.Random(Variables.AccelerationMin, Variables.AccelerationMax);
        dc.transform.localPosition = RainDropTools.GetSpawnLocalPos(this.transform, camera, 0f, Variables.SpawnOffsetY);
        dc.startPos = dc.transform.localPosition;

        dc.acceleration = RainDropTools.Random(Variables.AccelerationMin, Variables.AccelerationMax);

        if (dc.Drawer.material == null)
        {
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
            dc.Drawer.material = mat;
        }
        
		dc.Drawer.lifeTime = dc.lifetime;
		dc.Drawer.vertexDistance = 0.01f;
		dc.Drawer.angleDivisions = 20;
        dc.Drawer.widthCurve = Variables.TrailWidth;
        dc.Drawer.widthMultiplier = RainDropTools.Random(Variables.SizeMinX, Variables.SizeMaxX);
		dc.Drawer.textureMode = LineTextureMode.Stretch;
        dc.Drawer.Clear();
        dc.Drawer.enabled = false;
    }


    private void Shuffle<T>(List<T> list)
    {
        int cnt = list.Count;
        while (cnt > 1)
        {
            cnt--;
            int k = RainDropTools.Random(0, cnt + 1);
            T value = list[k];
            list[k] = list[cnt];
            list[cnt] = value;
        }
    }


    private KeyValuePair<Vector3, float> PickRandomWeightedElement(Dictionary<Vector3, float> dictionary)
    {
        var kvList = dictionary.ToList();
        // If all the value is same, then we return a random element
        float firstVal = kvList[0].Value;
        if (kvList.FindAll(t => t.Value == firstVal).Count() == kvList.Count())
        {
            Shuffle(kvList);
            return kvList[0];
        }
        kvList.Sort((x, y) => x.Value.CompareTo(y.Value));
        //return RainDropTools.GetWeightedElement(kvList);
        return kvList.FirstOrDefault(x => x.Value == dictionary.Values.Max());
        //return new KeyValuePair<Vector3, float>(Vector3.zero, 0f);
    }


    private Vector3 GetNextPositionWithFriction(FrictionFlowRainDrawerContainer dc, float downValue, int resolution, int widthResolution, float dt)
    {
        dummy.parent = dc.Drawer.transform.parent;
        dummy.localRotation = dc.Drawer.transform.localRotation;
        dummy.localPosition = dc.Drawer.transform.localPosition;

        int texW = Variables.FrictionMap.width;
        int texH = Variables.FrictionMap.height;
        int iter = (int)(Mathf.Clamp(resolution * dt, 2, 5));
        //Vector3 frictionWay = dc.Drawer.transform.localPosition;
        Dictionary<Vector3, float> widthPixels = new Dictionary<Vector3, float>();

        // Get the gravity forced vector
        Vector3 downward = RainDropTools.GetGForcedScreenMovement(this.camera.transform, this.GForceVector);
        downward = downward.normalized;

        float angl = Mathf.Rad2Deg * Mathf.Atan2(downward.y, downward.x);

        //dummy.localPosition += downValue * new Vector3(downward.x, downward.y, 0f);
        dummy.localRotation = Quaternion.AngleAxis(angl + 90f, Vector3.forward);

        float step = downValue * (1f / iter) * 3f / widthResolution;
        int resol = Mathf.Clamp(2 * widthResolution, 2, 5);

        for (int i = 0; i < iter; i++)
        {
            dummy.localPosition += downValue * (1f / iter) * new Vector3(downward.x, downward.y, 0f);

            for (int j = 0; j <= resol; j++)
            {
                float ww = (j * step - (resol / 2f) * step);
                Vector3 downPos = dummy.TransformPoint(new Vector3(ww, 0f, 0f));
                Vector3 downVector2viewPoint = this.camera.WorldToViewportPoint(downPos);

                // Get the pixel grayscale
                float pixel = Variables.FrictionMap.GetPixel(
                    (int)(texW * downVector2viewPoint.x),
                    (int)(texH * -downVector2viewPoint.y)
                ).grayscale;

                // If never added to the list, we add it
                if (!widthPixels.ContainsKey(downPos))
                {
                    widthPixels.Add(downPos, 1.0f - pixel);
                }
            }
        }
        
        Vector3 frictionWay = PickRandomWeightedElement(widthPixels).Key;
        frictionWay = dc.Drawer.transform.parent.InverseTransformPoint(frictionWay);
        dummy.parent = null;

        return frictionWay;

        // OLD
        /*for (int i = 0; i < iter; i++)
        {
            float dv = downValue * ((float)i / iter);
            widthPixels.Clear();

            for (int j = 0; j <= 2*widthResolution; j++)
            {
                Vector3 downVector = frictionWay + (downward * dv);

                // TODO: Use normal vector of downward to search pixels
                if (Mathf.Abs(downward.y) > Mathf.Abs(downward.x))
                    downVector += Vector3.left * dv + Vector3.right * dv * ((float)j / widthResolution);
                else
                    downVector += Vector3.up * dv + Vector3.down * dv * ((float)j / widthResolution);
                // END OF todo

                Vector3 downVector2viewPoint = camera.WorldToViewportPoint(downVector);
                float pixel = Variables.FrictionMap.GetPixel(
                    (int)(texW * downVector2viewPoint.x),
                    (int)(texH * -downVector2viewPoint.y)
                ).grayscale;
                if (!widthPixels.ContainsKey(downVector))
                    widthPixels.Add(downVector, 1.0f-pixel);
            }

            Vector3 keyVector = PickRandomWeightedElement(widthPixels).Key;
            frictionWay = keyVector;
        }

        return frictionWay;*/
    }


    private void UpdateTransform(FrictionFlowRainDrawerContainer dc)
    {
		float progress = GetProgress(dc);
        float t = dc.TimeElapsed;
        Vector3 nextPos = GetNextPositionWithFriction(
            dc: dc,
            downValue: ((1 / 2f) * t * t * dc.acceleration * 0.1f) + (Variables.InitialVelocity * t * 0.01f),
            resolution: 150,
            widthResolution: 8,
            dt: Time.deltaTime);
        nextPos = new Vector3(nextPos.x, nextPos.y, 0f);
		nextPos += progress * new Vector3(GlobalWind.x, GlobalWind.y, 0f);
        dc.Drawer.vertexDistance = (1f * this.Distance * RainDropTools.GetCameraOrthographicSize(this.camera).y) / (Variables.Resolution * 10f);
        dc.transform.localPosition = nextPos;
    }


    private void UpdateShader(FrictionFlowRainDrawerContainer dc, int index)
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
    private void UpdateInstance(FrictionFlowRainDrawerContainer dc, int index)
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
}
