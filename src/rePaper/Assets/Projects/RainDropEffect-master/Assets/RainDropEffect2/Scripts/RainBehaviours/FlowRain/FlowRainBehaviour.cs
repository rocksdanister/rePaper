using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RainDropEffect;

[ExecuteInEditMode]
public class FlowRainBehaviour : RainBehaviourBase {

	#region [ Internal Variables ]

	[HideInInspector]
	FlowRainController rainController
	{
		get;
		set;
	}

	#endregion


	#region [ Public Variables ]

	/// <summary>
	/// The variables.
	/// </summary>

	[SerializeField]
	public FlowRainVariables Variables;

	/// <summary>
	/// Gets the current draw call.
	/// </summary>
	/// <value>The current draw call.</value>

	public override int CurrentDrawCall 
	{
		get 
		{
			if (rainController == null) 
			{
				return 0;
			} 
			else 
			{
				return rainController.drawers.FindAll (x => x.Drawer.enabled).Count ();
			}
		}
	}

	/// <summary>
	/// Gets the max draw call.
	/// </summary>
	/// <value>The max draw call.</value>

	public override int MaxDrawCall
	{
		get
		{ 
			return Variables.MaxRainSpawnCount;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this instance is playing.
	/// </summary>
	/// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>

	public override bool IsPlaying {
		get 
		{
			if (rainController == null) 
			{
				return false;
			}
			else 
			{
				return rainController.IsPlaying;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether rain is shown on the screen.
	/// </summary>
	/// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>

	public override bool IsEnabled
	{
		get
		{ 
			return this.Alpha != 0f && this.CurrentDrawCall != 0;
		}
	}

	#endregion


	public override void Refresh ()
	{
		if (rainController != null)
		{
			DestroyImmediate (rainController.gameObject);
			rainController = null;
		}
		rainController = CreateController ();
		rainController.Refresh ();
		rainController.NoMoreRain = true;
	}


	public override void StartRain ()
	{
        if (rainController == null)
        {
            rainController = CreateController();
            rainController.Refresh();
        }
        rainController.NoMoreRain = false;
        rainController.Play();
    }


	public override void StopRain ()
	{
		if (rainController == null)
		{
			return;
		}
		rainController.NoMoreRain = true;
	}


	public override void StopRainImmidiate ()
	{
		if (rainController == null)
		{
			return;
		}
		DestroyImmediate (rainController.gameObject);
		rainController = null;
	}


	public override void ApplyFinalDepth (int depth)
	{
		if (rainController == null)
		{
			return;
		}
		rainController.RenderQueue = depth;
	}


	public override void ApplyGlobalWind (Vector2 globalWind)
	{
		if (rainController == null)
		{
			return;
		}
		rainController.GlobalWind = globalWind;
	}


	#region [ Internal Methods ]

	/// <summary>
	/// Unity's Start
	/// </summary>

	void Start ()
	{
		if (Application.isPlaying && Variables.AutoStart) 
		{
			this.StartRain ();
		}
	}

	/// <summary>
	/// Unity's update
	/// </summary>

	public override void Update ()
	{
        InitParams();

        if (rainController == null)
        {
            return;
        }

        rainController.ShaderType = this.ShaderType;
        rainController.Alpha = this.Alpha;
        rainController.Distance = this.Distance;
        rainController.GForceVector = this.GForceVector;
        rainController.UpdateController();
    }


	/// <summary>
	/// Creates the controller.
	/// </summary>
	/// <returns>The controller.</returns>

	FlowRainController CreateController ()
	{
		Transform tr = RainDropTools.CreateHiddenObject ("Controller", this.transform);
		FlowRainController con = tr.gameObject.AddComponent <FlowRainController> ();
		con.Variables = Variables;
		con.Alpha = 0f;
		con.NoMoreRain = false;
		con.camera = GetComponentInParent<Camera> ();
		return con;
	}


	/// <summary>
	/// (Internal) Initialize inspector params
	/// </summary>

	public void InitParams ()
	{
		if (Variables == null)
			return;

		if (Variables.MaxRainSpawnCount < 0)
			Variables.MaxRainSpawnCount = 0;
		if (Variables.SizeMinX > Variables.SizeMaxX) 
			swap (ref Variables.SizeMinX, ref Variables.SizeMaxX);
		if (Variables.fluctuationRateMin > Variables.fluctuationRateMax) 
			swap (ref Variables.fluctuationRateMin, ref Variables.fluctuationRateMax);
		if (Variables.LifetimeMin > Variables.LifetimeMax) 
			swap (ref Variables.LifetimeMin, ref Variables.LifetimeMax);
		if (Variables.AccelerationMin > Variables.AccelerationMax) 
			swap (ref Variables.AccelerationMin, ref Variables.AccelerationMax );
		if (Variables.EmissionRateMin > Variables.EmissionRateMax) 
			swap (ref Variables.EmissionRateMin, ref Variables.EmissionRateMax);
	}


	/// <summary>
	/// (Internal) Swap the specified a and b.
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>

	private void swap (ref float a, ref float b)
	{
		float tmp = a;
		a = b;
		b = tmp;
	}


	/// <summary>
	/// Swap this instance.
	/// </summary>

	private void swap (ref int a, ref int b)
	{
		int tmp = a;
		a = b;
		b = tmp;
	}

    #endregion


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Camera rainCam = GetComponentInParent<Camera>();

        if (rainCam == null)
            return;

        if (rainController != null)
        {
            foreach (var dc in rainController.drawers)
            {
                if (dc.IsEnable)
                    Gizmos.color = new Color(0.6f, 0.6f, 0.0f, 1f);
                else
                    Gizmos.color = new Color(1f, 1f, 1f, 0.4f);

				Gizmos.DrawWireSphere(dc.Drawer.transform.position, .5f);
            }
        }

        if (UnityEditor.Selection.Contains(gameObject))
        {
            float h = rainCam.orthographicSize * 2f;
            float w = h * rainCam.aspect;
            Vector3 p = transform.position + (Vector3.up * h * Variables.SpawnOffsetY);
            Gizmos.color = new Color(0.5f, 0.9f, 0.8f, 0.8f);
            Gizmos.DrawWireCube(p, new Vector3(w, h, rainCam.nearClipPlane - rainCam.nearClipPlane + 0.1f));
            Gizmos.color = new Color(0.5f, 0.9f, 0.8f, 0.2f);
            Gizmos.DrawCube(p, new Vector3(w, h, rainCam.farClipPlane - rainCam.nearClipPlane + 0.1f));
        }
    }
#endif
}
