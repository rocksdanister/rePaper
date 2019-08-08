//#define SHOW_HIDED
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RainDropEffect;

[ExecuteInEditMode]
public class RainCameraController : MonoBehaviour {

	#region [ Internal Variables ]

	Camera _cam;
	Camera cam 
	{
		get { 
			if (_cam == null)
				_cam = GetComponent<Camera> ();
			return _cam;
		}
	}

	List <RainBehaviourBase> _rainBehaviours;
	public List <RainBehaviourBase> rainBehaviours
	{
		get
		{ 
			if (_rainBehaviours == null) 
			{
				_rainBehaviours = GetComponentsInChildren <RainBehaviourBase> (false).ToList ();
			}
			return _rainBehaviours;
		}
	}

	#endregion


	#region [ Public Variables ]

	/// <summary>
	/// The render queue.
	/// </summary>

	public int RenderQueue = 3000;


	/// <summary>
	/// The alpha.
	/// </summary>

	[Range (0f, 1f)]
	public float Alpha = 1f;


	/// <summary>
	/// The global wind.
	/// </summary>

	public Vector2 GlobalWind = Vector3.zero;


    /// <summary>
    /// Gravity vector
    /// </summary>

    public Vector3 GForceVector = Vector3.down;


    /// <summary>
    /// Gets the current draw call.
    /// </summary>
    /// <value>The current draw call.</value>

    public int CurrentDrawCall 
	{
		get 
		{
			return rainBehaviours.Select (x => x.CurrentDrawCall).Sum ();
		}
	}

	/// <summary>
	/// Gets the max draw call.
	/// </summary>
	/// <value>The max draw call.</value>

	public int MaxDrawCall
	{
		get
		{ 
			return rainBehaviours.Select (x => x.MaxDrawCall).Sum ();
		}
	}

	/// <summary>
	/// Gets a value indicating whether this instance is playing.
	/// </summary>
	/// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>

	public bool IsPlaying {
		get 
		{
			return rainBehaviours.FindAll (x => x.IsPlaying).Count != 0;
		}
	}

    public RainDropTools.RainDropShaderType ShaderType;

    [Range(0.02f, 10f)]
    public float distance = 8.3f;

    public bool VRMode = false;

    #endregion


    /// <summary>
    /// Unity's awake
    /// </summary>

    void Awake ()
	{
		foreach (var beh in rainBehaviours)
		{
			beh.StopRainImmidiate ();
		}
	}


	/// <summary>
	/// Unity's start
	/// </summary>

	void Start () 
	{
		if (cam == null)
		{
			Debug.LogError ("You must add component (Camera)");
			return;
		}
	}


	/// <summary>
	/// Unity's Update
	/// </summary>

	void Update () 
	{
        if (cam == null)
            return;
        
		cam.orthographic = !VRMode;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = distance + 0.01f;

        if (transform.childCount != _rainBehaviours.Count ()) 
		{
			_rainBehaviours = null;
		}
		rainBehaviours.Sort ((a, b) => a.Depth - b.Depth);
		int cnt = 0;
        int behIndex = 0;
		foreach (var beh in rainBehaviours)
		{
            beh.transform.localRotation = Quaternion.Euler(Vector3.zero);
            beh.transform.localScale = Vector3.one;
            if (Application.isPlaying)
            {
				var frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
				var frustumWidth = frustumHeight * cam.aspect;
				cam.orthographicSize = frustumHeight * .5f;
				beh.transform.localPosition = Vector3.forward * distance;
                //beh.transform.localPosition = new Vector3(0f, 0f, 0.0001f * behIndex);
            }
            else
            {
                beh.transform.localPosition = Vector3.zero;
            }
            beh.ShaderType = this.ShaderType;
            beh.VRMode = this.VRMode;
            beh.Distance = this.distance;
            beh.ApplyFinalDepth (RenderQueue + cnt);
			beh.ApplyGlobalWind (GlobalWind);
            beh.GForceVector = this.GForceVector;
            beh.Alpha = this.Alpha;
			cnt += beh.MaxDrawCall;
            behIndex += 1;
        }
	}


	/// <summary>
	/// You can call this when you want to redraw rain
	/// </summary>

	public void Refresh ()
	{
		foreach (var beh in rainBehaviours)
		{
			beh.StopRainImmidiate ();
		}
		_rainBehaviours = GetComponentsInChildren <RainBehaviourBase> (false).ToList ();
		foreach (var beh in rainBehaviours)
		{
			beh.Refresh ();
		}
	}


	/// <summary>
	/// Starts the rain increasingly.
	/// </summary>

	public void Play ()
	{
		foreach (var beh in rainBehaviours)
		{
			beh.StartRain ();
            //Debug.Log("rainhere");
		}
	}


	/// <summary>
	/// Stops the rain gradually.
	/// </summary>

	public void Stop () 
	{
		foreach (var beh in rainBehaviours)
		{
			beh.StopRain ();
		}
	}


	/// <summary>
	/// Stops the rain immidiately.
	/// </summary>

	public void StopImmidiate () 
	{
		foreach (var beh in rainBehaviours)
		{
			beh.StopRainImmidiate ();
		}
	}


    private void OnDrawGizmos()
    {
        if (cam == null)
            return;

        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;

        Gizmos.color = new Color(0f, 0.1f, 0.7f, 0.1f);
        Gizmos.DrawCube(transform.position, new Vector3(w, h, cam.farClipPlane - cam.nearClipPlane));
        Gizmos.color = new Color(0f, 0.1f, 0.7f, 0.8f);
        Gizmos.DrawWireCube(transform.position, new Vector3(w, h, cam.farClipPlane - cam.nearClipPlane));
    }
}
