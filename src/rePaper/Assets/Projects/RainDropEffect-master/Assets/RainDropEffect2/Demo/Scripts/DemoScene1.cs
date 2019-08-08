using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScene1 : MonoBehaviour {

    [SerializeField]
    List<RainCameraController> rainControllers;

    enum PlayMode
    {
        Home = 0,
        Rain = 1,
        Blood = 2,
        SplashIn = 3,
        SplashOut = 4,
        Frozen = 5,
    };

    PlayMode playMode = 0;
    float frozenValue = 0f;
    float rainAlpha = 1f;

    /*
    private void Awake()
    {
        // For mobile optimization, we should reduce the resolution on iOS & Android
#if UNITY_IOS || UNITY_ANDROID
		SetResolution (512);
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Application.targetFrameRate = 60;
#endif
    }


    private void SetResolution(int resolution)
    {
        float screenRate = Mathf.Min(1f, (float)resolution / Screen.height);
        int width = (int)(Screen.width * screenRate);
        int height = (int)(Screen.height * screenRate);
        Screen.SetResolution(width, height, true, 15);
    }
    */

    private void StopAll()
    {
        foreach (var con in rainControllers)
        {
            if(con !=null)
                con.StopImmidiate();
        }
    }


    private IEnumerator Start()
    {
        yield return null; // Since rains starts automatically, we have to wait for initialization.
        StopAll();
    }


    /*
    private void OnGUI()
    {
        int index = 0;
        foreach (var con in rainControllers)
        {
            if (GuiButton(string.Format("Rain[{0}]", index)))
            {
                StopAll();
                con.Play();
            }
            index++;
        }
    }


    private bool GuiButton(string buttonName)
    {
        return GUILayout.Button(buttonName, GUILayout.Height(40), GUILayout.Width(150));
    }
    */
}
