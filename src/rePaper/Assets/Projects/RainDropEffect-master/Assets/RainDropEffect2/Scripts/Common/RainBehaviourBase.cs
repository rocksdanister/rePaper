using UnityEngine;
using System.Collections;

/// <summary>
/// ABSTRACT Rain base.
/// </summary>
public abstract class RainBehaviourBase : MonoBehaviour {

	/// <summary>
	/// Rendering Queue
	/// </summary>

	public int Depth;


	/// <summary>
	/// The alpha value.
	/// </summary>

	[HideInInspector]
	public float Alpha;


    /// <summary>
    /// The shader type.
    /// </summary>

    [HideInInspector]
    public RainDropTools.RainDropShaderType ShaderType;


    /// <summary>
    /// Whether current mode is VR or not
    /// </summary>

    [HideInInspector]
    public bool VRMode;


    /// <summary>
    /// Rain distance from camera
    /// </summary>

    [HideInInspector]
    public float Distance;


    /// <summary>
    /// G-force vector
    /// </summary>

    [HideInInspector]
    public Vector3 GForceVector;


    /// <summary>
    /// Gets a value indicating whether this instance is playing.
    /// </summary>
    /// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>

    public virtual bool IsPlaying
	{
		get
		{ 
			return false;
		}
	}


	/// <summary>
	/// Gets a value indicating whether rain is shown on the screen.
	/// </summary>
	/// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>

	public virtual bool IsEnabled
	{
		get
		{ 
			return false;
		}
	}


	/// <summary>
	/// Gets the current draw call.
	/// </summary>
	/// <value>The current draw call.</value>

	public virtual int CurrentDrawCall 
	{
		get 
		{
			return 0;
		}
	}


	/// <summary>
	/// Gets the max draw call.
	/// </summary>
	/// <value>The max draw call.</value>

	public virtual int MaxDrawCall
	{
		get
		{ 
			return 0;
		}
	}


	/// <summary>
	/// You can call this when you want to redraw rain
	/// </summary>

	public virtual void Refresh ()
	{
		return;
	}


	/// <summary>
	/// Starts the rain increasingly.
	/// </summary>

	public virtual void StartRain ()
	{
		return;
	}


	/// <summary>
	/// Stops the rain gradually.
	/// </summary>

	public virtual void StopRain () 
	{
		return;
	}


	/// <summary>
	/// Stops the rain immidiately.
	/// </summary>

	public virtual void StopRainImmidiate () 
	{
		return;
	}


	/// <summary>
	/// Applies the final depth.
	/// </summary>
	/// <param name="depth">Depth.</param>

	public virtual void ApplyFinalDepth (int depth)
	{
		return;
	}


    /// <summary>
    /// Applies the global wind
    /// </summary>
    /// <param name="globalWind"></param>

    public virtual void ApplyGlobalWind(Vector2 globalWind)
    {
        return;
    }


    /// <summary>
    /// Unity's Awake
    /// </summary>

    public virtual void Awake () {
		return;
	}

	/// <summary>
	/// Unity's Update
	/// </summary>

	public virtual void Update () 
	{
		return;
	}
}
