using System;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System.Security.Cryptography;
using System.Windows.Forms;
using Screen = UnityEngine.Screen;

/// <summary>
/// this class does imageloading, runs settings panel, level restart, ui color.
/// </summary>
public class Controller : MonoBehaviour {

    public string hash_config, hash_monitor;
    public Color defaultUiColor;
    public CycleScript cycleScript;
    public VideoScript vidScript;
    public Renderer quadWallpaper;
    Material quad_mat;
    public GameObject[] Clocks;
    public SpriteRenderer[] brighterSprite;
    public Image[] brighterImg;
    public Text[] uiText;
    public Image[] img;
    public SpriteRenderer[] gearClock;
    public Color avgColor;
    //[HideInInspector] public static bool isDone;
    public GameObject wallpaper;
    // Use this for initialization

    /// <summary>
    /// Retrieves windows desktop wallpaper & loads it.
    /// </summary>
    void Start () {

        quad_mat = quadWallpaper.GetComponent<Renderer>().material;  //wallpaper gameobject

        if (MenuController.menuController.userSettings.isClock == true)
        {
            if(MenuController.menuController.userSettings.clockType < Clocks.Length && MenuController.menuController.userSettings.clockType >=0)
                Clocks[MenuController.menuController.userSettings.clockType].SetActive(true);
        }

        if (MenuController.menuController.userSettings.vidPath == null) //no video selected.
        {
            GetWallpaperImage();  //set current picture wallpaper.
        } 
        else
        {
            MenuController.menuController.wallpaperPath = GetWallpaperImagePath(); // to make video play on applicaiton start
        }

        if(MenuController.menuController.userSettings.autoUiHide == true && MenuController.menuController.isFirstTime != true) //fadeout ui elements when mouse goes away.
            StartCoroutine(FadeOutUI());

    }

    #region Wallpaper_Retreive_from_Windows
  
    Texture2D tex = null;
    private const UInt32 SPI_GETDESKWALLPAPER = 0x73;
    private const int MAX_PATH = 260;
    /// <summary>
    /// Retrieves windows desktop wallpaper & loads it.
    /// </summary>
    public void GetWallpaperImage()
    {
        RegistryKey currentMachine = Registry.CurrentUser;
        RegistryKey controlPanel = currentMachine.OpenSubKey("Control Panel");
        RegistryKey desktop = controlPanel.OpenSubKey("Desktop");

        string filePath = Convert.ToString(desktop.GetValue("WallPaper"));

        controlPanel.Close();

        if (!System.IO.File.Exists(filePath))
        {
            //Try to retrieve wallpaper image with cached file..
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Themes\\CachedFiles";
            if (Directory.Exists(filePath))
            {
                string[] filePaths = Directory.GetFiles(filePath);
                if (filePaths.Length > 0)
                {
                    filePath = filePaths[0];
                }
            }
            else
            {
                //Try to retrieve wallpaper image by original source..
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
                filePath = regKey.GetValue("WallpaperSource").ToString() + "h";
                if (!System.IO.File.Exists(filePath))
                {
                    //Failed to retrieve wallpaper image by original source..
                    filePath = new String('\0', MAX_PATH);
                    StaticPinvoke.SystemParametersInfo(SPI_GETDESKWALLPAPER, (UInt32)filePath.Length, filePath, 0);
                    filePath = filePath.Substring(0, filePath.IndexOf('\0'));
                }
            }
        }

        if (System.IO.File.Exists(filePath))
        {
           
            MenuController.menuController.wallpaperPath = filePath;
            tex = new Texture2D(2, 2);
            StartCoroutine(WaitForImageDownload(filePath)); //async image loading. 
        }
        else
        {
            UnityEngine.Debug.Log("Failed to retrieve wallpaper image using all methods!");
        }

        desktop.Close();
        currentMachine.Close();
    }

    /// <summary>
    /// Async external wallpaper file loading.
    /// </summary>
    /// <param name="path">imagefile path.</param>
    IEnumerator WaitForImageDownload(string path)
    {
        WWW www = new WWW(path);
        yield return www;
        www.LoadImageIntoTexture(tex);
        www.Dispose();
        tex.Compress(true);

        main.instance.ColorCheckMark();
        SetWallpaper(); 
    }

    /// <summary>
    /// Retrieves current wallpaper path, including name.
    /// </summary>
    /// <returns>
    /// Wallpaper path string.
    /// </returns>
    public static string GetWallpaperImagePath()
    {
        RegistryKey currentMachine = Registry.CurrentUser;
        RegistryKey controlPanel = currentMachine.OpenSubKey("Control Panel");
        RegistryKey desktop = controlPanel.OpenSubKey("Desktop");

        string filePath = Convert.ToString(desktop.GetValue("WallPaper"));

        controlPanel.Close();

        if (!System.IO.File.Exists(filePath))
        {
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Themes\\CachedFiles";
            if (Directory.Exists(filePath))
            {
                string[] filePaths = Directory.GetFiles(filePath);
                if (filePaths.Length > 0)
                {
                    filePath = filePaths[0];
                }
            }
            else
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
                filePath = regKey.GetValue("WallpaperSource").ToString() + "h";
                if (!System.IO.File.Exists(filePath))
                {
                    filePath = new String('\0', MAX_PATH);
                    StaticPinvoke.SystemParametersInfo(SPI_GETDESKWALLPAPER, (UInt32)filePath.Length, filePath, 0);
                    filePath = filePath.Substring(0, filePath.IndexOf('\0'));
                }
            }
        }

        desktop.Close();
        currentMachine.Close();

        if (System.IO.File.Exists(filePath))
        {
            //UnityEngine.Debug.Log(filePath);
            return filePath;
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to retrieve wallpaper image using all methods!");
            return null;
        }


    }
    #endregion

    #region Average_Wallpaper_Color
    /// <summary>
    /// Calculates the average color of wallpaper using cpu.
    /// </summary>
    public Color32 GetAverageColorOfTexture()
    {
        if (MenuController.menuController.userSettings.vidPath == null && tex != null)
        {
            try
            {
                Color32[] texColors = tex.GetPixels32(6); //mipmap(low res texture) level reduces memory usage, less processing time. I'm making the assumption GCC will collect this.

                int total = 0;

                float r = 0;
                float g = 0;
                float b = 0;

                for (int i = 0; i < texColors.Length; i++)
                {
                    if (texColors[i].a > 0)
                    {
                        total++;
                        r += texColors[i].r;
                        g += texColors[i].g;
                        b += texColors[i].b;
                    }
                }
                return new Color32((byte)(r / total), (byte)(g / total), (byte)(b / total), 0);
            }
            catch(Exception e)
            {
                Debug.Log("getavgcolor_error: " + e.Message);
                //default white
                Color32 color = new Color(1f, 1f, 1f, 1f);
                return color;
            }
        }
        else
        {
            //default white
            Color32 color = new Color(1f, 1f, 1f, 1f);
            return color;
        }
    }

    #endregion

    #region UI_Color
    /// <summary>
    /// Change current clock style.
    /// </summary>
    /// <param name="str">clock name.</param>
    public void ChangeClockType(string str = null)
    {
        if (str == null)
        {
            Clocks[MenuController.menuController.userSettings.clockType].SetActive(false);

            MenuController.menuController.userSettings.clockType++;
            if (MenuController.menuController.userSettings.clockType > 2)
                MenuController.menuController.userSettings.clockType = 0;

            MenuController.menuController.Save();

            if (MenuController.menuController.userSettings.isClock == true)
                Clocks[MenuController.menuController.userSettings.clockType].SetActive(true);
        }
        else
        {
            Clocks[MenuController.menuController.userSettings.clockType].SetActive(false);

            if (str.ToLowerInvariant() == "gear")
                MenuController.menuController.userSettings.clockType = 0;
            else if(str.ToLowerInvariant() == "circle")
                MenuController.menuController.userSettings.clockType = 1;
            else if(str.ToLowerInvariant() == "simple")
                MenuController.menuController.userSettings.clockType = 2;
            else
            {

            }

            MenuController.menuController.Save();

            if (MenuController.menuController.userSettings.isClock == true)
                Clocks[MenuController.menuController.userSettings.clockType].SetActive(true);
        }
    }

    // different color changing functions for UI depending on the gameobject type.
    void ChangeGearColor()
    {
        foreach (SpriteRenderer item in gearClock)
        {
            item.color = avgColor;
        }
    }

    void ChangeUIColor()
    {
        foreach (Text item in uiText)
        {
            item.gameObject.GetComponent<Outline>().enabled = false;
            item.color = avgColor;
        }
    }

    void ChangeImgColor()
    {
        foreach (Image item in img)
        {
            item.gameObject.GetComponent<Outline>().enabled = false;
            item.color = avgColor;
        }
    }

    void BrighterImgColor()
    {
        foreach (Image item in brighterImg)
        {
            item.color = avgColor;
            item.color = item.color;// * 1.2f;
        }
    }

    void BrighterSpriteColor()
    {
        foreach (SpriteRenderer item in brighterSprite)
        {
            //item.gameObject.GetComponent<Outline>().enabled = false;

            item.color = avgColor;
            item.color = item.color;// * 1.2f;
        }
    }

    /// <summary>
    /// Converts RGB to HSV to adjust brightness of RGB color, intensity of avgColor is reduced/increased.
    /// </summary>
    /// <param name="color">RGB color.</param>
    void ColorConvert(Color color)
    {
        float H, S, V; //  A value of 0 is black, with increasing lightness moving away from black.
        Color.RGBToHSV(color, out H, out S, out V);

        if (V < 0.5f)
        {
            V += V * 0.60f;

        }
        else
        {
            V += V * 0.40f;

        }
        avgColor = Color.HSVToRGB(H, S, V);
    }

    /// <summary>
    /// Sets UI color to default/user selected color.
    /// </summary>
    public void UIColorWhite() //for video only, no getpixel avg color called. FN called from videoscript.
    {
        if (MenuController.menuController.userUiColor != null)
            avgColor = MenuController.menuController.userUiColor;
        else
            avgColor = defaultUiColor; //defined in editor controller script

        if (MenuController.menuController.userSettings.autoUiHide == true)
        {
            avgColor.a = 0f; // for auto-hide only!
        }
        else
        {
            avgColor.a = 1.0f;
        }

        ChangeGearColor();
        ChangeUIColor();
        ChangeImgColor();
        BrighterImgColor();
        BrighterSpriteColor();
    }

    /// <summary>
    /// Sets UI color based on wallpaper.
    /// </summary>
    public void UIColorAuto()
    {
        if (MenuController.menuController.userSettings.vidPath == null) // avoid for video, MEMORY NOM NOM...
        {
            avgColor = defaultUiColor; //default
            if (MenuController.menuController.userSettings.autoUiColor == true)
            {
                avgColor = GetAverageColorOfTexture(); 
                ColorConvert(avgColor); //RGB ->HSL for adjusting brightness
            }

            if (MenuController.menuController.userSettings.autoUiHide == true && MenuController.menuController.isFirstTime != true)
            {
                avgColor.a = 0f; // for auto-hide only!
            }
            else
            {
                avgColor.a = 1.0f;
            }

            ChangeGearColor();
            ChangeUIColor();
            ChangeImgColor();
            BrighterImgColor();
            BrighterSpriteColor();
        }
    }

    /// <summary>
    /// UI fadein when mouseover, Unity is able to retrieve mouse position without any change... it has to be a hook of sort? unsure.
    /// </summary>
    IEnumerator FadeOutUI()
    {
        WaitForSeconds countTime = new WaitForSeconds(0.05f); //delay
        while(true)
        {
            if (Input.mousePosition.x < Screen.width * 0.60f && 
                    Input.mousePosition.x > Screen.width * 0.40f && 
                    Input.mousePosition.y > Screen.height*0.40f && 
                    Input.mousePosition.y < Screen.height * 0.60f  && Headless.isDesktop == true ) //middle, clock area
            {
                avgColor.a += 0.1f;
                if (avgColor.a > 1.0f)
                    avgColor.a = 1.0f;

                ChangeGearColor();
                ChangeUIColor();
                ChangeImgColor();
                BrighterImgColor();
                BrighterSpriteColor();
            }
            /*
            else if (Input.mousePosition.x >= Screen.width * 0.60f &&
                        Input.mousePosition.x <= Screen.width * 0.40f &&
                        Input.mousePosition.y <= Screen.height * 0.40f &&
                        Input.mousePosition.y >= Screen.height * 0.60f && avgColor.a > 0f) //middle
            {
                avgColor.a -= 0.1f;

                ChangeGearColor();
                ChangeUIColor();
                ChangeImgColor();
                BrighterImgColor();
                BrighterSpriteColor();
            }
            */
            else if (Input.mousePosition.x >= Screen.width * 0.90f && Headless.isDesktop == true) //right edge, weather text
            {
                avgColor.a += 0.1f;
                if (avgColor.a > 1.0f)
                    avgColor.a = 1.0f;

                ChangeGearColor();
                ChangeUIColor();
                ChangeImgColor();
                BrighterImgColor();
                BrighterSpriteColor();
            }
            else //fadeout
            {
                avgColor.a -= 0.1f;
                if (avgColor.a < 0f)
                    avgColor.a = 0f;

                ChangeGearColor();
                ChangeUIColor();
                ChangeImgColor();
                BrighterImgColor();
                BrighterSpriteColor();
            }

            yield return countTime;
        }
    }

    #endregion UI_Color

    /// <summary>
    /// Called after loading wallpaper, sets scaling, ui color.
    /// </summary>
    void SetWallpaper()
    {
       
        if (MenuController.menuController.userUiColor != null)
            avgColor = MenuController.menuController.userUiColor;
        else
            avgColor = defaultUiColor; //defined in editor controller script

        if (MenuController.menuController.userSettings.autoUiColor == true)
        {
            avgColor = GetAverageColorOfTexture(); // check for memory leak?
            ColorConvert(avgColor); //RGB ->HSL for adjusting brightness
        }

        if (MenuController.menuController.userSettings.autoUiHide == true && MenuController.menuController.isFirstTime != true)
        {
            avgColor.a = 0f; // for auto-hide only!
        }
        else
        {
            avgColor.a = 1.0f;
        }

        ChangeGearColor();
        ChangeUIColor();
        ChangeImgColor();
        BrighterImgColor();
        BrighterSpriteColor();

        quad_mat.SetTexture("_MainTex", tex);

        //WallpaperScaler(quadWallpaper.gameObject);
        TestScaling(quadWallpaper.gameObject);
    }

    /// <summary>
    /// Checks if two floats are approx equal.
    /// </summary>
    /// <returns> true if ~ equal</returns>
    bool EqualityCheckFloat(float a, float b)
    {
        if (Mathf.Abs(a - b) <= 0.2)
        {
            return true;
        }
        return false;
    }

    float sizeX, sizeY, aspectRatio;
    /// <summary>
    /// Shitty \(*_*)/ Scaling algorithm. Just doing 2xheight when height>width.
    /// </summary>
    /// <param name="wallpaper">Quad gameobject.</param>
    public void TestScaling(GameObject wallpaper)
    {
        Vector2 cameraSize =
            Camera.main.ViewportToWorldPoint(Vector3.one) -
            Camera.main.ViewportToWorldPoint(Vector3.zero);

        sizeX = Mathf.Abs(cameraSize.x) / (wallpaper.GetComponent<MeshRenderer>().bounds.size.x / wallpaper.transform.localScale.x);
        sizeY = Mathf.Abs(cameraSize.y) / (wallpaper.GetComponent<MeshRenderer>().bounds.size.y / wallpaper.transform.localScale.y);

        if(MenuController.menuController.userSettings.vidPath != null) //video, just stretching by default. videoplayer has its own scale settings.
        {
            wallpaper.transform.localScale = new Vector3(sizeX, sizeY, 1);
            return;
        }

        if (MenuController.menuController.userSettings.imgScaling == 0)  //fill
        {
            aspectRatio = ((float)tex.width / (float)tex.height);            
            if (tex.width > tex.height)
            {
                if (EqualityCheckFloat(aspectRatio, ((float)Screen.currentResolution.width / (float)Screen.currentResolution.height)))
                    wallpaper.transform.localScale = new Vector3(sizeX, sizeY, 1);
                else
                {
                    wallpaper.transform.localScale = new Vector3(sizeX*1.05f, sizeY*1.25f, 1);
                }
            }
            else //height > width
            {
                wallpaper.transform.localScale = new Vector3(sizeX, sizeY*2, 1);
            }
            
        }
        else if (MenuController.menuController.userSettings.imgScaling == 1) //stretch
            wallpaper.transform.localScale = new Vector3(sizeX, sizeY, 1);
    }
  

    #region wallpaper_scaling_OLD_UNUSED
    void WallpaperScaler(GameObject wallpaper)
    {
        if (MenuController.menuController.userSettings.imgScaling != 2) //no scaling
        {
            Vector2 cameraSize =
            Camera.main.ViewportToWorldPoint(Vector3.one) -
            Camera.main.ViewportToWorldPoint(Vector3.zero);

           // Debug.Log("X= "+ wallpaper.GetComponent<MeshRenderer>().bounds.size.x + "Y= " + wallpaper.GetComponent<MeshRenderer>().bounds.size.y);
            float sizeX = Mathf.Abs(cameraSize.x) / (wallpaper.GetComponent<MeshRenderer>().bounds.size.x / wallpaper.transform.localScale.x);
            float sizeY = Mathf.Abs(cameraSize.y) / (wallpaper.GetComponent<MeshRenderer>().bounds.size.y / wallpaper.transform.localScale.y);

           // Debug.Log("tex dimension" + tex.width + " " + tex.height);
            if (MenuController.menuController.userSettings.imgScaling == 1) //stretch
            {
              //  Debug.Log("Stretching Wallpaper");
                wallpaper.transform.localScale = new Vector3(sizeX, sizeY, 1);
            }
            else if (MenuController.menuController.userSettings.imgScaling == 0) //fill
            {
               // Debug.Log("Filling Wallpaper");
                /*
                Debug.Log("sizeX: " + sizeX);
                Debug.Log("sizeY: " + sizeY);
                Debug.Log("Delta: " + Mathf.Abs(sizeX - sizeY));
                */
                if (sizeY > sizeX)
                {
                    if (Mathf.Abs(sizeX - sizeY) >= 0.01f)
                        wallpaper.transform.localScale = new Vector3(sizeY, sizeY, 1);
                    else
                        wallpaper.transform.localScale = new Vector3(sizeX, sizeX, 1); //vertical(9:16 etc) or 16:9 images?
                }
                else
                    wallpaper.transform.localScale = new Vector3(sizeX, sizeX, 1); // old   wallpaper.transform.localScale = new Vector3(sizeX, sizeX, 1);
            }
            //Controller.isDone = false;
        }
    }

    #endregion

    /// <summary>
    /// Calculates checksum of file.
    /// </summary>
    /// <param name="filepath">path to the file.</param>
    string CalculateCheckSum(string filepath)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            using (var stream = File.OpenRead(filepath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    System.Diagnostics.Process configUtility;
    /// <summary>
    /// Opens config.rePaper.exe application.
    /// </summary>
    /// <remarks>
    /// Wanted to avoid input hooks, at the time this was the fastest way of doing it than writing ipc code. 
    /// todo:- Probably easier to just load the form as a dll or load it entirely?.
    /// </remarks>
    public void RunConfigUtility()
    {
        if (configUtility == null || configUtility.HasExited == true)//Application.isEditor != true)
        {
            runOnceConfig = false;
            string str = System.AppDomain.CurrentDomain.BaseDirectory + "\\bin\\config-rePaper.exe";   //make sure to use this format for file, appdomain system gives the accurate path always.
            if (File.Exists(str) == true)
            {
                //just my weird code.. nothing to see here xD
                if (true)//CalculateCheckSum(str) == hash_config)
                    configUtility = System.Diagnostics.Process.Start(str);
                else
                    main.instance.tray.ShowNotification(1000, "Error", "Files corrupted, try redownloading.");
            }
            else
            {
                main.instance.tray.ShowNotification(1000, "Error", "Files missing, try redownloading.");
            }


        }
    }

    System.Diagnostics.Process fileDialog;
    /// <summary>
    /// Opens filedialogue.
    /// </summary>
    public void RunFileDialog()
    {
        OpenFileDialog ofDialog = new System.Windows.Forms.OpenFileDialog();
        ofDialog.Title = " Select video or picture for wallpaper";
        ofDialog.RestoreDirectory = true;
        //ofDialog.Filter = "Exe (.exe)|*.exe|MSI (.msi)|*.msi| All (*.*)|*.*";
        //ofDialog.DefaultExt = "gb";
        ofDialog.Filter = "Videos & Pictures (*.mp4;*.jpg;*.png;*.bmp;*.tiff;*.jpeg)|*.mp4;*.jpg;*.png;*.bmp;*.tiff;*.jpeg"+
                            "|Video (*.mp4)|*.mp4"+
                            "|Pictures (*.jpg;*.png;*.bmp;*.tiff;*.jpeg)|*.jpg;*.png;*.bmp;*.tiff;*.jpeg";
        ofDialog.FilterIndex = 0;
        ofDialog.Multiselect = false;

        if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
            vidScript.manual_pause_dxva(true); //pause dxva video, diff thread

        if (ofDialog.ShowDialog() == DialogResult.OK)
        {
            if (CheckIfImage(ofDialog.FileName) != true) //if video
            {
                //MenuController.menuController.wallpaperPath = null; // image 
                MenuController.menuController.userSettings.vidPath = ofDialog.FileName;
                MenuController.menuController.Save();
                main.instance.ColorCheckMark(); //disable auto color menu

                if (MenuController.menuController.userSettings.isDXVA == false)
                {                  
                    vidScript.LoadFile();
                }
                else
                {
                   if(MenuController.menuController.userSettings.vidPath !=null) //stop if dxva video already playing
                       vidScript.Stop_DXVA();

                    cycleScript.UnloadAssets();
                    SceneManager.LoadScene("wallpaper");
                    //restart the scene if hardware accleariton istead
                }
            }
            else       //it is image
            {
                
                if(GetWallpaperImagePath() == ofDialog.FileName 
                    && MenuController.menuController.userSettings.vidPath !=null) //force change if user clicks same wallpaper ! ( to exit videoplayback etc)
                {
                    WallpaperChangeDetect(true);
                }
                else                
                    main.instance.SetCustomWallpaper(ofDialog.FileName); //sents a windows signal to change, applicaiton will detect that in Controller.WallpaperChangeDetect

            }
        }
        else //filedialog cancel
        {
            if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
            {
                vidScript.manual_pause_dxva(false); // unpause
            }
        }

    }

    /// <summary>
    /// Closes all open dialogues/utility of rePaper.
    /// </summary>
    public void CloseAllOpenWindows()
    {
        if (fileDialog != null)
        {
            fileDialog.Kill(); //processTerminate() c++ call
            fileDialog.Close(); //cleanup
        }

        if(configUtility !=null)
        {
            configUtility.Kill();
            configUtility.Close();
        }

        if(displaySettings !=null)
        {
            displaySettings.Kill();
            displaySettings.Close();
        }
    }

    /// <summary>
    /// Checks if the file extension is imagefile.
    /// </summary>
    /// <param name="fileName">filename with extension.</param>
    /// <returns>true if image.</returns>
    bool CheckIfImage(string fileName)
    {
        string targetExtension = System.IO.Path.GetExtension(fileName);
        if (String.IsNullOrEmpty(targetExtension))
            return false;
        else
            targetExtension = "*" + targetExtension.ToLowerInvariant();

        List<string> recognisedImageExtensions = new List<string>(); 

        foreach (System.Drawing.Imaging.ImageCodecInfo imageCodec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()) //list of all image extensions.
            recognisedImageExtensions.AddRange(imageCodec.FilenameExtension.ToLowerInvariant().Split(";".ToCharArray()));

        foreach (string extension in recognisedImageExtensions)
        {
            if (extension.Equals(targetExtension))
            {
                return true;
            }
        }
        return false;

    }

    public void RestartScene() //unused
    {

        SceneManager.LoadScene("wallpaper");
    }

    System.Diagnostics.Process displaySettings;
    /// <summary>
    /// Opens multimonitor.exe application, for wallpaper position/size offsetting.
    /// </summary>
    public void DisplaySettings()
    {
        //...write the try catch block, dont forget
        if (displaySettings == null || displaySettings.HasExited == true)//Application.isEditor != true)
        {
            string str = System.AppDomain.CurrentDomain.BaseDirectory + "\\bin\\Display\\multimonitor.exe";

            if (File.Exists(str) == true)
            {
                if (true)//CalculateCheckSum(str) == hash_monitor)
                    fileDialog = System.Diagnostics.Process.Start(str);
                else
                {
                    Debug.Log("Error: multimonitor.exe corrupt");
                    main.instance.tray.ShowNotification(1000, "Error", "Files corrupted, try redownloading.");
                }
            }
            else
            {
                Debug.Log("Error: multimonitor.exe missing");
                main.instance.tray.ShowNotification(1000, "Error", "Files missing, try redownloading.");
            }

            //fileDialog.WaitForInputIdle();
        }
    }

    public Color userDefinedColor;
    /// <summary>
    /// Opens winform colordialoge & apply color to UI.
    /// </summary>
    public void RunColorDialog()
    {
        ColorDialog MyDialog = new ColorDialog();   
        // allow the user from selecting a custom color.
        MyDialog.AllowFullOpen = true;
        // diable the user to get help. (The default is false.)
        MyDialog.ShowHelp = false;
        // Sets the initial color to previously saved color.
        MyDialog.Color = System.Drawing.ColorTranslator.FromHtml(MenuController.menuController.userSettings.uiColor);

        if (MyDialog.ShowDialog() == DialogResult.OK)
        {
            string htmlColor = System.Drawing.ColorTranslator.ToHtml(MyDialog.Color); //convert color to html.

            if (ColorUtility.TryParseHtmlString(htmlColor, out userDefinedColor)) //convert html color to RGB unity color.
            {
                MenuController.menuController.userSettings.uiColor = htmlColor;
                MenuController.menuController.userSettings.autoUiColor = false;
                MenuController.menuController.Save();

                MenuController.menuController.userUiColor = userDefinedColor;
                UIColorWhite();
            }
            else
            {
                Debug.Log("failure: colorconvert html");
            }
        }        
    }

    /// <summary>
    /// Monitors currently set wallpaper, need to rewrite this using events in the future ( Unity seems to be blocking some event messages currently)
    /// </summary>
    public void WallpaperChangeDetect(bool exception = false) //exception when filedialog used, so when same image selected it reloads eitherways.
    {
        if (MenuController.menuController.wallpaperPath != GetWallpaperImagePath() || exception)//&& MenuController.menuController.userSettings.vidPath == null)
        {
            Debug.Log("wallpaper changed");

            if (MenuController.menuController.userSettings.vidPath != null) //if user changes picture wallpaper through windows properties or when i trigger the change.
            {
                vidScript.StopPlayBack(); //unity videoplayer , also makes vidpath in userSettings false
                if (MenuController.menuController.userSettings.isDXVA == true)
                {

                    vidScript.Stop_DXVA();
                    main.instance.ColorCheckMark(); //update ui systray menu

                    cycleScript.UnloadAssets();
                    SceneManager.LoadScene("wallpaper");
                }
            }


            //...stop playback
            //vidScript.StopPlayBack();

            //Scene loadedLevel = SceneManager.GetActiveScene();
            //SceneManager.LoadScene(loadedLevel.buildIndex);

            Texture2D.DestroyImmediate(tex, true); //destroy old texture,memory save.
                                                   // SetWallpaper();

            //SceneManager.LoadScene("wallpaper"); //aggressive memory management
            GetWallpaperImage();
        }
    }

    bool runOnceConfig = false;
    // Update is called once per frame
    void Update() {

        if (displaySettings != null)
        {
            displaySettings.Refresh();

            if (displaySettings.HasExited == true)
            {
                displaySettings.Close();
                displaySettings = null;
            }
        }

        //configuration utility, waiting for exit
        if (configUtility != null)
        {

            configUtility.Refresh(); //not sure if needed, check again.
            if (configUtility.HasExited == true)
            {
                Debug.Log("Configwindow Exited");
                configUtility.Close(); //free up resources
                configUtility = null;

                //Debug.Log(MenuController.menuController.userSettings.isReload);
                MenuController.menuController.Load(); // loaded onto single menucontroller
                if (MenuController.menuController.userSettings.isReload == true)
                {
                  //  FileBrowser.HideDialog(true); //force close open file dialog
                    //..apply new settings
                    MenuController.menuController.userSettings.isReload = false;
                    MenuController.menuController.isFirstTime = false; // since setting is saved, no need for notification dialogue.
                    MenuController.menuController.Save(); // save isReload = false with the new data

                    //...system tray update
                    if (MenuController.menuController.userSettings.isDemo == true) //weather control
                    {
                        main.instance.WeatherMenuEnable(true);
                    }
                    else
                    {
                        main.instance.WeatherMenuEnable(false);
                    }

                    main.instance.SetStartup(MenuController.menuController.userSettings.runAtStartup);
                    //checkmark btn
                    if (MenuController.menuController.userSettings.runAtStartup == true)
                        main.instance.startup.Checked = true;
                    else
                        main.instance.startup.Checked = false;

                    if(MenuController.menuController.userSettings.vidPath !=null)
                        vidScript.Stop_DXVA();

                    main.instance.ClockCheckMark(); // update clock systray checkmark
                    main.instance.ColorCheckMark(); //update ui button checkmark

                    cycleScript.UnloadAssets();
                    //......restart scene, cleary memory.

                    MenuController.menuController.LoadWeatherParameters(); //updated weather parameters.

                    MenuController.menuController.Deserialize(MenuController.menuController.appList); //update application exclusion list
                    SceneManager.LoadScene("wallpaper");
                }
            }
            else
            {
              //  Debug.Log("Running Process?");
            }
        }
        else
        {
            //Debug.Log("PGM NULL");
        }

        WallpaperChangeDetect(); 
    }
}
