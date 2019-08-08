using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear_Rotate : MonoBehaviour {

    public float angularVelocity = 100.0f;
    public Vector3 rotationAxis = Vector3.up;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.localRotation = Quaternion.AngleAxis(angularVelocity * Time.deltaTime, rotationAxis) * transform.localRotation;
    }
}

