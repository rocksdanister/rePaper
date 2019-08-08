using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Label_Percent2 : MonoBehaviour {

    Text text;
    // Use this for initialization
    void Start()
    {
        text = GetComponent<Text>();
        //text.text = GameController.gameController.userSettings.sliderScale.ToString("F2");
    }

    public void SetSlider(float val)
    {
        text.text = val.ToString("F2");
    }

}
