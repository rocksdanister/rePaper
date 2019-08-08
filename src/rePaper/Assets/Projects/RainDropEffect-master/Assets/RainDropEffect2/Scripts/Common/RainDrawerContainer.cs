using UnityEngine;
using System.Collections;

[System.Serializable]
public class RainDrawerContainer<T> where T : UnityEngine.Component 
{
	public T Drawer; // Drawer controls mesh, render and shader
	public Transform transform;

	public RainDrawerContainer (string name, Transform parent) 
	{
		transform = RainDropTools.CreateHiddenObject (name, parent);
		this.Drawer = transform.gameObject.AddComponent <T> ();
	}
}
