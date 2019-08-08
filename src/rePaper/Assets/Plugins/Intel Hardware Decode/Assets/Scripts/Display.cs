using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour
{
    // Use this for initialization
    void Start ()
    {
        float height = (float)Camera.main.orthographicSize * 2.0f;
        float width = height * Screen.width / Screen.height;
        transform.localScale = new Vector3(width / 10, 1.0f, height / 10);
    }
}
