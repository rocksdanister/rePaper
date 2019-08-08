using UnityEngine;
using System.Collections;

public class FpsDisplay : MonoBehaviour
{
    float interval = 0.2f;
    float startTime = 0f;
    float dt = 0f;
    int flameCnt = 0;
    int fps = 0;

    void LateUpdate()
    {
        dt = Time.time - startTime;
        flameCnt += 1;
        if (dt >= interval)
        {
            fps = (int)(flameCnt / dt);
            flameCnt = 0;
            startTime = Time.time;
        }
    }

    void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, h - h / 10, w, h / 10);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h / 10;
        style.normal.textColor = Color.white;
        string text = string.Format("FPS:{0}", fps);
        GUI.Label(rect, text, style);
    }
}
