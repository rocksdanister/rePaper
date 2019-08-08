using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (RainCameraController))]
public class RainCameraControllerInspector : Editor {

	public override void OnInspectorGUI ()
	{
		var b = target as RainCameraController;

		if (Application.isPlaying)
		{
			GUI.color = Color.cyan;
			if (GUILayout.Button ("Refresh", GUILayout.Height (30))) 
			{
				b.Refresh ();
			}
			if (GUILayout.Button ("Stop Rain", GUILayout.Height (30)))
			{
				b.Stop ();
			}
			if (GUILayout.Button ("Start Rain", GUILayout.Height (30)))
			{
				b.Play ();
			}
		}

		GUI.color = Color.white;
		GUILayout.Label ("Max Draw Call: " + b.MaxDrawCall);
		//GUILayout.Box ("Draw calls are derived from 'MaxDrawCall' of 'RainBehaviourBase'.\nYou should not make the value large.", GUILayout.Width (EditorGUIUtility.currentViewWidth - 50));
		//GUILayout.Box ("'Use Cheap' means the values except distortion be ignored and not considered.'", GUILayout.Width (EditorGUIUtility.currentViewWidth - 50));
		DrawDefaultInspector ();
	}

}
