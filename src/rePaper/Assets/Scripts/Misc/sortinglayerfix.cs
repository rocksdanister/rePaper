using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sortinglayerfix : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Renderer obj = this.GetComponent<Renderer>();
        obj.sortingLayerName = "RainDrop_Window";
        obj.sortingOrder = 0;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
