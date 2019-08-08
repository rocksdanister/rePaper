using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugFPS : MonoBehaviour {

    float deltaTime = 0.0f;
    Text text;
    string tmp;

    private void Start()
    {
        text = GetComponent<Text>();
    }
    void OnGUI()
    {
        if (MenuController.menuController.userSettings.bvar2 == true)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 6 / 100);
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = h * 6 / 100;
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            tmp = string.Format("{0:0} fps", fps);
            //text.text = tmp;
            GUI.Label(rect, tmp, style);
        }
        
    }
}
