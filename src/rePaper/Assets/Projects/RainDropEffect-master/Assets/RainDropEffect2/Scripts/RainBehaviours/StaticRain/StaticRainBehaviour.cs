using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RainDropEffect;

[ExecuteInEditMode]
public class StaticRainBehaviour : RainBehaviourBase {

	#region [ Internal Variables ]

	[HideInInspector]
	StaticRainController rainController
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
	public StaticRainVariables Variables;

	/// <summary>
	/// Gets the current draw call.
	/// </summary>
	/// <value>The current draw call.</value>

	public override int CurrentDrawCall 
	{
		get 
		{
			return rainController == null ? 0 : 1;
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
			return 1;
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
        Refresh(); // Work around TODO: fix initialize bug
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
	}


    #region [ Internal Methods ]

    /// <summary>
    /// Unity's Awake
    /// </summary>

    public override void Awake()
    {
        if (Application.isPlaying)
        {
            Refresh(); // Work around TODO: fix initialize bug
        }
    }

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
		InitParams ();

		if (rainController == null)
		{
			return;
		}

        rainController.ShaderType = this.ShaderType;
		rainController.Alpha = this.Alpha;
        rainController.VRMode = this.VRMode;
		rainController.UpdateController ();
	}


	/// <summary>
	/// Creates the controller.
	/// </summary>
	
	StaticRainController CreateController ()
	{
		Transform tr = RainDropTools.CreateHiddenObject ("Controller", this.transform);
		StaticRainController con = tr.gameObject.AddComponent <StaticRainController> ();
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
            if (rainController.staticDrawer.currentState == StaticRainController.DrawState.Playing)
                Gizmos.color = new Color(0.6f, 0.0f, 0.1f, 1f);
            else
                Gizmos.color = new Color(1f, 1f, 1f, 0.4f);

            Gizmos.DrawWireCube(rainController.staticDrawer.transform.position, new Vector3(1f, 1f, 0f));
        }

        if (UnityEditor.Selection.Contains(gameObject))
        {
            float h = rainCam.orthographicSize * 2f;
            float w = h * rainCam.aspect;
            Vector3 p = transform.position - (Vector3.up * h * Variables.SpawnOffsetY) - (Vector3.right * w * Variables.SpawnOffsetX);

            Vector3 size = new Vector3(
                 Variables.SizeX*2f,
                 Variables.SizeY*2f,
                 1f
             );
            if (Variables.FullScreen)
            {
                size = new Vector3(w, h, 1f);
            }

            Gizmos.color = new Color(0.5f, 0.9f, 0.8f, 0.8f);
            Gizmos.DrawWireCube(p, new Vector3(size.x, size.y, rainCam.nearClipPlane - rainCam.nearClipPlane + 0.1f));
            Gizmos.color = new Color(0.5f, 0.9f, 0.8f, 0.2f);
            Gizmos.DrawCube(p, new Vector3(size.x, size.y, rainCam.farClipPlane - rainCam.nearClipPlane + 0.1f));
        }
    }
#endif
}
