using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
//using SimpleFileBrowser;
using MediaPlayer;

/// <summary>
/// Unity & DXVA videoplayer playback.
/// </summary>
public class VideoScript : MonoBehaviour {

    //..intel HW DXVA variables
    Material wallpaperMat;
    private Playback mediaPlayback;
    bool isVideoPlaying = false;
    bool isDXVALoaded = false;

    //..Unity videoplayer variables
    public Controller controllerScript;
    Material mat;
    VideoPlayer videoComponent;

    void Start () {
        wallpaperMat = this.GetComponent<Renderer>().material;
        this.mediaPlayback = FindObjectOfType<Playback>();
        mat = gameObject.GetComponent<MeshRenderer>().material;

        //..Unity videoplayer settings
        videoComponent = this.gameObject.GetComponent<VideoPlayer>();
        videoComponent.playOnAwake = false;
        videoComponent.isLooping = true;
        videoComponent.skipOnDrop = false; //stuttering fix when pause/unpause?
        videoComponent.EnableAudioTrack(0, false); //AudioSampleProvider buffer overflow. 1216 sample frames discarded error fix (disable audio).

        if (MenuController.menuController.userSettings.isDXVA == true)
        {
            SetupHardwareAcceleration();
        }
        else
        {
            SetupUnityVideoPlayer();
        }
        
    }

    #region setup unity_videoplayer
    /// <summary>
    /// Initialize & play unity videoplayer.
    /// </summary>
    void SetupUnityVideoPlayer()
    {
        if (MenuController.menuController.userSettings.vidPath == null) //no videofile selected.
        {
            //nothing, just don't do anything & retain current wallpaper.
        }
        else
        {
            if (System.IO.File.Exists(MenuController.menuController.userSettings.vidPath) == true)
            {
                videoComponent.url = MenuController.menuController.userSettings.vidPath;
                videoComponent.audioOutputMode = VideoAudioOutputMode.Direct;
                videoComponent.controlledAudioTrackCount = 1;
                videoComponent.Play();
                controllerScript.TestScaling(gameObject);
                controllerScript.UIColorWhite();
            }
            else //videofile not found, display message dialogue.
            {
                main.instance.tray.ShowNotification(1000, "Error", "Video File Missing...");
            }

        }
    }

    /// <summary>
    /// Stop unity videoplayer playback, reverts to picture wallpaper.
    /// </summary>
    public void StopPlayBack() //unity videoplayer
    {
        videoComponent.Stop();
        videoComponent.clip = null;

        MenuController.menuController.userSettings.vidPath = null;
        MenuController.menuController.Save();

        controllerScript.GetWallpaperImage();
    }

    /// <summary>
    /// Pause/Unpause unity videoplayer based on timescale = 0/!=0
    /// </summary>
    public void PauseVideoUnity()
    {
        if (Time.timeScale == 0)
        {
            if (videoComponent.isPlaying) //unity videoplayer
                videoComponent.Pause();
        }
        else
        {
            if (MenuController.menuController.userSettings.isDXVA == false && MenuController.menuController.userSettings.vidPath != null)
                videoComponent.Play();
        }
    }

    #endregion setup unity_videoplayer  

    #region dxva_videoplayer
    /// <summary>
    /// Initialize & play DXVA videoplayer.
    /// </summary>
    void SetupHardwareAcceleration()
    {
        //Debug.Log(MenuController.menuController.userSettings.vidPath);
        if (MenuController.menuController.userSettings.vidPath == null)
        {
            //nothing, just do picture wallpaper instead.
            //this.mediaPlayback.DestroyTexture();
        }
        else
        {
            this.mediaPlayback.CustomOnEnable(); //create texture, replacement for OnEnable()

            if (System.IO.File.Exists(MenuController.menuController.userSettings.vidPath) == true)
            {
                wallpaperMat.EnableKeyword("_DXVA_COLOR");
                isDXVALoaded = true;
                isVideoPlaying = true;
                this.mediaPlayback.Play(MenuController.menuController.userSettings.vidPath);
                gameObject.transform.rotation = Quaternion.Euler(180, 0, 0);
                gameObject.GetComponent<ReverseNormals>().enabled = true;
                controllerScript.TestScaling(gameObject);
                controllerScript.UIColorWhite();
            }
            else //videofile not found, display message dialogue.
            {
                main.instance.tray.ShowNotification(1000, "Error", "Video File Missing...");
            }

        }
    }

    /// <summary>
    /// Stop DXVA playback, destroys media instance.
    /// </summary>
    public void Stop_DXVA()
    {
        if (isDXVALoaded == true)
        {
            this.mediaPlayback.Stop();
            isDXVALoaded = false;
        }
        else
        {

        }
    }

    /// <summary>
    /// dxva videopause. 
    /// </summary>
    /// <param name="pause">true: pause, false: unpause</param>
    public void manual_pause_dxva(bool pause)
    {
        if (pause == true)
        {
            this.mediaPlayback.Pause();
            isVideoPlaying = false;
        }
        else
        {
            this.mediaPlayback.Play(MenuController.menuController.userSettings.vidPath);
            isVideoPlaying = true;
        }
    }

    /// <summary>
    /// Pause if timescale =0, unpause !=0
    /// </summary>
    public void PauseVideo()
    {
    if (Time.timeScale == 0)
    {
        //Debug.Log("PAUSED video dxva");
        this.mediaPlayback.Pause();
        isVideoPlaying = false;
    }
    else
    {
        //Debug.Log("RESUMED video dxva");
        if (isVideoPlaying == false)
        {
            this.mediaPlayback.Play(MenuController.menuController.userSettings.vidPath);
            isVideoPlaying = true;
        }
    }
    }
    
    long tmpFrame;
    long duration;
    //float tmpTime = -1;
    /// <summary>
    /// DXVA videoplayer loop playback.
    /// </summary>
    private void DXVA_Loop()
    {
        if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null) //check if dxva on
        {
            tmpFrame = this.mediaPlayback.GetPosition();
            duration = this.mediaPlayback.GetDuration();
            if (duration > 0)
            {
                if (tmpFrame >= duration)
                {
                    tmpFrame = 0;
                    this.mediaPlayback.SetPosition(tmpFrame);
                    /*
                    if (MenuController.menuController.userSettings.vidPath == null)
                    {

                    }
                    else
                    {
                        if (System.IO.File.Exists(MenuController.menuController.userSettings.vidPath) == true)
                        {
                            this.mediaPlayback.Play(MenuController.menuController.userSettings.vidPath);
                            //   videoComponent.url = GameController.gameController.userSettings.filePath;
                            //    videoComponent.Play();
                        }

                    }
                    */
                }
            }
        }
    }

    #endregion dxva_videoplayer

    #region winform_filebrowser
    /// <summary>
    /// Call filedialog.
    /// </summary>
    public void ShowFileDilCaller()
    {
        var obj = GameObject.Find("Main Camera").GetComponent<Controller>();
        if (obj != null)
            obj.RunFileDialog();
        else
            Debug.Log("Controller script not found");
    }
    
    /// <summary>
    /// Load & play unity videoplayer videoclip.
    /// </summary>
    public void LoadFile()
    {
        //mat.SetTexture("_MainTex", vidTexture); // memory leak? DONT FORGET TO CHECK!
        videoComponent.url = MenuController.menuController.userSettings.vidPath;
            if (mat.mainTexture != null)
                Destroy(mat.mainTexture); //clear existing wallpaper texture?, make sure to trigger change wallpaper if video playing!
            videoComponent.Play();
            controllerScript.UIColorWhite();

          //  main.instance.video.Checked = true; //videosystray checkmark
        
    }

    #endregion winform_filebrowser


    private void Update()
    {
        DXVA_Loop();
    }
}
