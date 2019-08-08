using UnityEngine;
using System.Collections;

public class AxisRotator : MonoBehaviour {

	public Vector3 Axis;
	public float smooth = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (Axis * Time.deltaTime * smooth);
	}
}
