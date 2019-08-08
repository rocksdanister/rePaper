using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

public class StaticRainController : MonoBehaviour
{
    public StaticRainVariables Variables { get; set; }
    [HideInInspector]
    public int RenderQueue { get; set; }
    public Camera camera { get; set; }
    public float Alpha { get; set; }
    public bool NoMoreRain { get; set; }
    public RainDropTools.RainDropShaderType ShaderType { get; set; }
    public bool VRMode { get; set; }

    public bool IsPlaying
    {
        get
        {
            return staticDrawer.currentState == DrawState.Playing;
        }
    }

    public enum DrawState
    {
        Playing,
        Disabled,
    }

    [System.Serializable]
    public class StaticRainDrawerContainer : RainDrawerContainer<RainDrawer>
    {
        public DrawState currentState = DrawState.Disabled;
        public float TimeElapsed = 0f;

        public StaticRainDrawerContainer(string name, Transform parent) : base(name, parent) { }
    }

    public StaticRainDrawerContainer staticDrawer;


    /// <summary>
    /// Refresh this instance.
    /// </summary>

    public void Refresh()
    {
        if (staticDrawer != null)
        {
            DestroyImmediate(staticDrawer.Drawer.gameObject);
        }
        staticDrawer = new StaticRainDrawerContainer("Static RainDrawer", this.transform);
        staticDrawer.currentState = DrawState.Disabled;
        InitializeInstance(staticDrawer);
    }


    public void Play()
    {
        if (staticDrawer.currentState == DrawState.Playing)
        {
            return;
        }

        InitializeInstance(staticDrawer);
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

        UpdateInstance(staticDrawer);
    }


    private float GetProgress(StaticRainDrawerContainer dc)
    {
        return dc.TimeElapsed / Variables.fadeTime;
    }


    /// <summary>
    /// Initializes the rain instance.
    /// </summary>
    private void InitializeInstance(StaticRainDrawerContainer dc)
    {
        // Initialization
        dc.TimeElapsed = 0f;
        dc.Drawer.NormalMap = Variables.NormalMap;
        dc.Drawer.ReliefTexture = Variables.OverlayTexture;
        dc.Drawer.Hide();
    }


    /// <summary>
    /// Update rain variables
    /// </summary>
    /// <param name="dc">Dc.</param>
    private void UpdateInstance(StaticRainDrawerContainer dc)
    {
        AnimationCurve fadeCurve = Variables.FadeinCurve;

        // Update time
        if (!NoMoreRain)
        {
            dc.TimeElapsed = Mathf.Min(Variables.fadeTime, dc.TimeElapsed + Time.deltaTime);
        }
        else
        {
            dc.TimeElapsed = Mathf.Max(0f, dc.TimeElapsed - Time.deltaTime);
        }

        if (dc.TimeElapsed == 0f)
        {
            dc.Drawer.Hide();
            dc.currentState = DrawState.Disabled;
            return;
        }
        else
        {
            dc.currentState = DrawState.Playing;
        }

        if (Variables.FullScreen)
        {
            Vector2 orthSize = RainDropTools.GetCameraOrthographicSize(this.camera);
            Vector3 targetScale = new Vector3(
                orthSize.x / 2f,
                orthSize.y / 2f,
                0f
                );
            if (VRMode)
            {
                targetScale += Vector3.one * 0.02f;
            }
            dc.transform.localScale = targetScale;
            dc.transform.localPosition = Vector3.zero;
        }
        else
        {
            dc.transform.localScale = new Vector3(
                Variables.SizeX,
                Variables.SizeY,
                1f
            );

            Vector3 p = camera.ScreenToWorldPoint(
            new Vector3(
                -Screen.width * Variables.SpawnOffsetX + Screen.width / 2,
                -Screen.height * Variables.SpawnOffsetY + Screen.height / 2,
                0f
            ));
            dc.transform.localPosition = transform.InverseTransformPoint(p);
            dc.transform.localPosition -= Vector3.forward * dc.transform.localPosition.z;
        }

        float progress = GetProgress(dc);
        dc.Drawer.RenderQueue = RenderQueue;
        dc.Drawer.NormalMap = Variables.NormalMap;
        dc.Drawer.ReliefTexture = Variables.OverlayTexture;
        dc.Drawer.OverlayColor = new Color(
            Variables.OverlayColor.r,
            Variables.OverlayColor.g,
            Variables.OverlayColor.b,
            Variables.OverlayColor.a * fadeCurve.Evaluate(progress) * Alpha
        );
        dc.Drawer.DistortionStrength = Variables.DistortionValue * fadeCurve.Evaluate(progress) * Alpha;
        dc.Drawer.ReliefValue = Variables.ReliefValue * fadeCurve.Evaluate(progress) * Alpha;
        dc.Drawer.Blur = Variables.Blur * fadeCurve.Evaluate(progress) * Alpha;
        dc.Drawer.Darkness = Variables.Darkness;
        dc.Drawer.ShaderType = ShaderType;
        dc.Drawer.Show();
    }
}
