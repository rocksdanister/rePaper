//#define SHOW_HIDED
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RainDropTools : MonoBehaviour {

    public enum RainDropShaderType
    {
        Expensive,
        Cheap,
        NoDistortion
    }

    public static string SHADER_FORWARD = "RainDrop/Internal/RainDistortion (Forward)";
    public static string SHADER_CHEAP = "RainDrop/Internal/RainDistortion (Mobile)";
    public static string SHADER_NO_DISTORTION = "RainDrop/Internal/RainNoDistortion";
    //public static string SHADER_DEFFERED = "RainDrop/Internal/RainDistortion (Deffered)";



    /// <summary>
    /// Get the shader name from RainDropShaderType
    /// </summary>
    /// <param name="shaderType"></param>
    /// <returns></returns>
    public static string GetShaderName (RainDropShaderType shaderType)
    {
        switch (shaderType)
        {
            case RainDropShaderType.Expensive:
                return SHADER_FORWARD;
            case RainDropShaderType.Cheap:
                return SHADER_CHEAP;
            case RainDropShaderType.NoDistortion:
                return SHADER_NO_DISTORTION;
            default:
                return "";
        }
    }


    /// <summary>
    /// Creates the rain material.
    /// </summary>
    /// <param name="shaderType"></param>
    /// <param name="renderQueue"></param>
    /// <returns></returns>

    public static Material CreateRainMaterial (RainDropShaderType shaderType, int renderQueue) 
	{
		var shader = Shader.Find (GetShaderName (shaderType));
		var mat = new Material (shader);
		mat.renderQueue = renderQueue;
		return mat;
	}
    
    
	/// <summary>
    /// Apply the values to a material of rain object
    /// </summary>
    /// <param name="material"></param>
    /// <param name="shaderType"></param>
    /// <param name="normalMap"></param>
    /// <param name="overlayTexture"></param>
    /// <param name="distortionValue"></param>
    /// <param name="overlayColor"></param>
    /// <param name="reliefValue"></param>
    /// <param name="blur"></param>
    /// <param name="darkness"></param>

	public static void ApplyRainMaterialValue (
		Material material, 
		RainDropShaderType shaderType, 
		Texture normalMap,
		Texture overlayTexture=null, 
		float distortionValue=0f,
		Color? overlayColor=null, 
		float reliefValue=0f,
		float blur=0f,
		float darkness=0f
	) {
        // Apply shader values
        switch (shaderType)
        {
            case RainDropShaderType.Expensive:
                material.SetColor("_Color", overlayColor ?? Color.white);
                material.SetFloat("_Strength", distortionValue);
                //material.SetFloat("_HeightOffset", 0.5f);
                material.SetFloat("_Relief", reliefValue);
                if (blur != 0f)
                {
                    material.EnableKeyword("BLUR");
                    material.SetFloat("_Blur", blur);
                }
                else
                {
                    material.DisableKeyword("BLUR");
                    material.SetFloat("_Blur", blur);
                }
                material.SetFloat("_Darkness", darkness);
                material.SetTexture("_Distortion", normalMap);
                material.SetTexture("_ReliefTex", overlayTexture);
                break;
            case RainDropShaderType.Cheap:
                material.SetFloat("_Strength", distortionValue);
                material.SetTexture("_Distortion", normalMap);
                break;
            case RainDropShaderType.NoDistortion:
                material.SetTexture("_MainTex", overlayTexture);
                material.SetTexture("_Distortion", normalMap);
                material.SetColor("_Color", overlayColor ?? Color.white);
                material.SetFloat("_Darkness", darkness);
                material.SetFloat("_Relief", reliefValue);
                break;
            default:
                break;
        }
	}


	/// <summary>
	/// Creates the quad.
	/// </summary>
	/// <returns>The quad.</returns>

	public static Mesh CreateQuadMesh () 
	{
		Vector3[] verts = new Vector3[]
		{
			new Vector3( 1, 1,  0),
			new Vector3( 1, -1, 0),
			new Vector3(-1, 1,  0),
			new Vector3(-1, -1, 0),
		};

		Vector2[] uvs = new Vector2[]
		{
			new Vector2(1, 1),
			new Vector2(1, 0),
			new Vector2(0, 1),
			new Vector2(0, 0),
		};

		int[] triangles = new int[]
		{
			0, 1, 2,
			2, 1, 3
		};

		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		mesh.name = "Rain Mesh";
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		mesh.MarkDynamic();
		mesh.RecalculateBounds ();
		return mesh;
	}


	/// <summary>
	/// Creates the hidden object.
	/// </summary>
	/// <returns>The hidden object.</returns>
	/// <param name="name">Name.</param>
	/// <param name="parent">Parent.</param>

	public static Transform CreateHiddenObject (string name, Transform parent)
	{
		#if (UNITY_EDITOR && !SHOW_HIDED)
		GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags (name, HideFlags.HideAndDontSave);
		#else
		GameObject go = new GameObject ();
		#endif
		go.name = name;
		go.transform.parent = parent;
		go.transform.localScale = Vector3.one;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.Euler (Vector3.zero);
		return go.transform;
	}


	/// <summary>
	/// Random the specified min and max.
	/// </summary>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>

	public static float Random (float min, float max)
	{
		return UnityEngine.Random.Range (min, max);
	}


	/// <summary>
	/// Random the specified min and max.
	/// </summary>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>

	public static int Random (int min, int max)
	{
		return UnityEngine.Random.Range (min, max);
	}


	/// <summary>
	/// Destroies all the children.
	/// </summary>
	/// <param name="t">T.</param>

	public static void DestroyChildren (Transform t)
	{
		foreach (Transform child in t)
		{
			Destroy(child.gameObject);
		}
	}


	/// <summary>
	/// Destroies all the children immediately.
	/// </summary>
	/// <param name="t">T.</param>

	public static void DestroyChildrenImmediate (Transform t)
	{
		foreach (Transform child in t)
		{
			DestroyImmediate(child.gameObject);
		}
	}


    /// <summary>
    /// Get the Camera's Orthographic Size
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>

    public static Vector2 GetCameraOrthographicSize(Camera cam)
    {
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        return new Vector2(w, h);
    }


    /// <summary>
    /// Get Spawn Position by camera and offset position
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="cam"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    /// <returns></returns>

    public static Vector3 GetSpawnLocalPos(Transform parent, Camera cam, float offsetX, float offsetY)
    {
        Vector2 camSize = GetCameraOrthographicSize(cam);
        Vector3 p = new Vector3(
            Random(-camSize.x / 2f, camSize.x / 2f),
            Random(-camSize.y / 2f, camSize.y / 2f),
            0f
            );
        p = cam.transform.rotation * p + parent.position;
        p.x += camSize.x * offsetX;
        p.y += camSize.y * offsetY;
        Vector3 localPos = parent.InverseTransformPoint(p);
        return localPos;
    }


    /// <summary>
    /// Get the g-forced screen movement
    /// That is to say, we gets the rotation vector that applies gravity
    /// </summary>
    /// <param name="screenTransform"></param>
    /// <param name="GForce"></param>
    /// <returns></returns>

    public static Vector3 GetGForcedScreenMovement(Transform screenTransform, Vector3 GForce)
    {
        Vector3 projY = Vector3.Project(GForce, screenTransform.up);
        Vector3 projX = Vector3.Project(GForce, screenTransform.right);
        Vector3 projZ = Vector3.Project(GForce, screenTransform.forward);

        Vector3 relativePointY = screenTransform.InverseTransformPoint(screenTransform.position + projY);
        Vector3 relativePointX = screenTransform.InverseTransformPoint(screenTransform.position + projX);
        Vector3 relativePointZ = screenTransform.InverseTransformPoint(screenTransform.position + projZ);

        return new Vector3(relativePointX.x, relativePointY.y, relativePointZ.z);
    }


    /// <summary>
    /// Get an element from a weighted KeyValuePair list
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>

    public static KeyValuePair<T1, T2> GetWeightedElement<T1, T2>(List<KeyValuePair<T1, T2>> list) where T2 : IComparable
    {
        if (list.Count == 0)
        {
            return list.FirstOrDefault();
        }

        float totalweight = (float)list.Sum(t => Convert.ToDouble(t.Value));
        float choice = RainDropTools.Random(0f, totalweight);
        float sum = 0;

        foreach (var obj in list)
        {
            for (float i = sum; i < Convert.ToDouble(obj.Value) + sum; i++)
            {
                if (i >= choice)
                {
                    return obj;
                }
            }
            sum += (float)Convert.ToDouble(obj.Value);
        }

        return list.First();
    }
}
