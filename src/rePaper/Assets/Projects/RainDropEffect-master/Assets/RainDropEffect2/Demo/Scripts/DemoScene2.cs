using UnityEngine;
using System.Collections;

public class DemoScene2 : MonoBehaviour {

	[SerializeField]
	RainCameraController frozenRain;

	enum PlayMode {
		None = 0,
		Blood = 1,
		SplashIn = 2,
		SplashOut = 3,
		Frozen = 4,
	};

	PlayMode playMode = 0;
	float frozenValue = 0f;
    float rainAlpha = 1f;

    /*
    private void Awake () 
	{
        // For mobile optimization, we should reduce the resolution on iOS & Android
#if UNITY_IOS || UNITY_ANDROID
		SetResolution (512);
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Application.targetFrameRate = 60;
#endif
    }


    private void SetResolution (int resolution)
	{
		float screenRate = Mathf.Min (1f, (float)resolution/Screen.height);
		int width = (int)(Screen.width * screenRate);
		int height = (int)(Screen.height * screenRate);
		Screen.SetResolution (width, height, true, 15);
	}
    */

    private void StopAll () 
	{
		//bloodRainController.Reset ();
		// You can stop and clear effects by Refresh ()
		//splashInRain.StopImmidiate ();
		//splashOutRain.StopImmidiate();
		frozenRain.StopImmidiate();
		//splashInAudio.Stop ();
		//splashOutAudio.Stop ();
		//damageAudio.Stop ();
		//windAudio.Stop ();
	}

    /*
    private void OnGUI () 
	{
		if (playMode != PlayMode.None) {
			if (GuiButton ("GoBack")) {
				StopAll ();
				playMode = PlayMode.None;
			}
		}
		else 
		{
			if (GuiButton ("Blood")) 
			{
				playMode = PlayMode.Blood;
			}

			if (GuiButton ("Splash (in)")) 
			{
				playMode = PlayMode.SplashIn;
			}

			if (GuiButton ("Splash (out)")) 
			{
				playMode = PlayMode.SplashOut;
			}

			if (GuiButton ("Frozen")) 
			{
				frozenValue = 0f;
				frozenRain.Play ();
				windAudio.Play ();
				playMode = PlayMode.Frozen;
			}
		}


		if (playMode == PlayMode.Blood)
		{
			if (GuiButton ("Hit Damage")) 
			{
				if (bloodRainController.HP <= 30) 
				{
					bloodRainController.Reset ();
					bloodRainController.HP = 100;
				} 
				else
				{
					damageAudio.Play ();
					bloodRainController.Attack (30);
				}
			}
			if (GuiButton ("Reset")) 
			{
				bloodRainController.Reset ();
			}
			GUILayout.Label ("Current HP = " + bloodRainController.HP.ToString ());
			return;
		}

		if (playMode == PlayMode.SplashIn) 
		{
			if (GuiButton ("Play Effect")) 
			{
				splashInAudio.Play ();
				splashInRain.Refresh ();
				splashInRain.Play ();
			}
		}

		if (playMode == PlayMode.SplashOut) 
		{
			if (GuiButton ("Play Effect")) 
			{
				splashOutAudio.Play ();
				splashOutRain.Refresh ();
				splashOutRain.Play ();
			}
		}

		if (playMode == PlayMode.Frozen) 
		{
			frozenRain.Alpha = frozenValue;
			GUILayout.Label ("Frozen Value (Sliding right to freeze)");
			frozenValue = GUILayout.HorizontalSlider (frozenValue, 0f, 1f, GUILayout.Height (40));
			windAudio.volume = frozenValue;
		}
	}

    

    private bool GuiButton (string buttonName) 
	{
		return GUILayout.Button (buttonName, GUILayout.Height (40), GUILayout.Width (150));
	}
    */
}
