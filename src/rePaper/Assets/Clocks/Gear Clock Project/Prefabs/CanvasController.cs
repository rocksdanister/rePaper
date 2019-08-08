using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CanvasController : MonoBehaviour {

    //public example painterScript;
    public Toggle rgb_toggle, color_toggle;
    public GameObject notice;
    //public AddClip addClipScript;
    //public AsciiArtFx asciiScript;
    public GameObject panel, updateNotice;
    public Button gear, fileButton;
    public Slider sliderBlend,sliderScale;
    public Dropdown image_size;

	// Use this for initialization
	void Start () {
        panel.SetActive(false);
        if(GameController.gameController.userSettings.rgbAnimation == 0)
        {
            rgb_toggle.isOn = true;
            color_toggle.isOn = false;
        }
        else
        {
            rgb_toggle.isOn = false;
            color_toggle.isOn = true;
        }


       // if (GameController.gameController.file_not_found == true)
       //     fileNotFound.SetActive(true);

      //  sliderScale.value = GameController.gameController.userSettings.sliderScale;
        //sliderBlend.value = GameController.gameController.userSettings.sliderBlend;


        if (GameController.gameController.userSettings.firstRun == true)
        {
            if (GameController.gameController.userSettings.updateCnt == 2)
            {
                updateNotice.SetActive(true);
            }
            ColorBlock tmp = gear.colors;
            Color tmp2 = tmp.normalColor;
            tmp2.a = 255f / 255f;
            tmp.normalColor = tmp2;
            gear.colors = tmp;
            fileButton.colors = tmp;
        }
        else
            notice.SetActive(false);


    }

    public void RGBToggle(bool val)
    {
       if(val == true)
        {
            GameController.gameController.userSettings.rgbAnimation = 0; //rgb
            rgb_toggle.isOn = true;
            color_toggle.isOn = false;
        }
       else
        {
            GameController.gameController.userSettings.rgbAnimation = 1;
            rgb_toggle.isOn = false;
            color_toggle.isOn = true;
        }

    }

    public void ColorToggle(bool val)
    {
        if (val == true)
        {
            GameController.gameController.userSettings.rgbAnimation = 1; //color
            rgb_toggle.isOn = false;
            color_toggle.isOn = true;
        }
        else
        {
            GameController.gameController.userSettings.rgbAnimation = 0;
            rgb_toggle.isOn = true;
            color_toggle.isOn = false;
        }
    }
    /*
    public void DropDown(int val)
    {
        GameController.gameController.userSettings.image_fit = val;

        if (val == 0)
            addClipScript.videoComponent.aspectRatio = VideoAspectRatio.FitOutside;
        else if (val == 1)
            addClipScript.videoComponent.aspectRatio = VideoAspectRatio.FitInside;
        else if (val == 2)
            addClipScript.videoComponent.aspectRatio = VideoAspectRatio.Stretch;
        else if (val == 3)
            addClipScript.videoComponent.aspectRatio = VideoAspectRatio.FitVertically;
        else if (val == 4)
            addClipScript.videoComponent.aspectRatio = VideoAspectRatio.FitHorizontally;
    }

    public void FileBrowser()
    {
        addClipScript.ShowFileDilCaller();
    }

    public void Set_Rate_Scale(float val)
    {
        GameController.gameController.userSettings.sliderScale = val;
        asciiScript.scaleFactor = val;
    }

    public void Set_Rate_Blend(float val)
    {
        GameController.gameController.userSettings.sliderBlend = val;
        asciiScript.blendRatio = val;
    }
    */

    public void SettingsButton()
    {
       // fileNotFound.SetActive(false);
        if (GameController.gameController.userSettings.firstRun == true)
        {
            Debug.Log("gear icon");
            ColorBlock tmp = gear.colors;
            Color tmp2 = tmp.normalColor;
            tmp2.a = 0f/255f;
            tmp.normalColor = tmp2;
            gear.colors = tmp;
            fileButton.colors = tmp;

            GameController.gameController.userSettings.firstRun = false;
            notice.SetActive(false);
        }

        if (panel.activeSelf == false)
            panel.SetActive(true);
        else
        {
            GameController.gameController.Save();
            panel.SetActive(false);
        }
    }

    public void ExitUpdatePanel()
    {
        GameController.gameController.userSettings.updateCnt = 2;
        GameController.gameController.Save();
        updateNotice.SetActive(false);
    }

    public void ExitPanelButton()
    {
      //  fileNotFound.SetActive(false);
        if (GameController.gameController.userSettings.firstRun == true)
        {
            ColorBlock tmp = gear.colors;
            Color tmp2 = tmp.normalColor;
            tmp2.a = 0f / 255f;
            tmp.normalColor = tmp2;
            gear.colors = tmp;
            fileButton.colors = tmp;

            GameController.gameController.userSettings.firstRun = false;
            notice.SetActive(false);
        }


        GameController.gameController.Save();
        panel.SetActive(false);
    }
	

}
