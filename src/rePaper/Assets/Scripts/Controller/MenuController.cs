using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.UI;
using System.Xml.Serialization;
using UnityDisplay = UnityEngine.Display;
using System.Collections.Generic;

public class MenuController : MonoBehaviour {

    //Bundled weather apikey, added in Unity Editor.. hidden from user.
    public string apiKeyDefault;

    //temporary variables.
    [HideInInspector] public bool isFirstTime = false; //first time application run (no config file)
    public static MenuController menuController;
    [HideInInspector] public string wallpaperPath; //temp current picture wallpaper path.
    [HideInInspector] public string tmpCityName; 
    [HideInInspector] public Color userUiColor; //temp selected UI color

    #region weather_parameters_class
    /// <summary>
    /// Weather parameters
    /// </summary>
    [Serializable]
    public class Weather
    {
        //rain shader & fog shader code version.
        public float rainVersion; 
        public float atmosphereVersion;
        public Rain[] rain = new Rain[9];
        public Atmosphere[] atmosphere = new Atmosphere[4];
       
        public Weather()
        {
            rainVersion = 1f;
            atmosphereVersion = 1f;
        }
        
    }
    //Weather weather_tmp = new Weather();
    public Weather WeatherParam = new Weather();
    /// <summary>
    /// Rain shader parameters
    /// </summary>
    [Serializable]
    public class Rain
    {
        public int code;// openweathermap weathercode
        public string name;
        public float rainAmt;
        public float drop1Amt;
        public float drop2Amt;
        public float staticDrpAmt;
        public float dropSpeed;
        public float zoomOut;
        public float rainTex;
        public float blurAmt;
        public int rainTrail;
        //future use, drizzle - intermittent etc.
        public float intervalMin; 
        public float intervalMax;
        public float rate1;

        public Rain()
        {

        }
        public Rain(int code, string name, float rainAmt, float drop1Amt, float drop2Amt, float staticDrpAmt, float dropSpeed, float zoomOut, 
                                            float rainTex, float blurAmt, int rainTrail, float intervalMin, float intervalMax, float rate1)
        {
            this.code = code;
            this.name = name;
            this.rainAmt = rainAmt;
            this.drop1Amt = drop1Amt;
            this.drop2Amt = drop2Amt;
            this.staticDrpAmt = staticDrpAmt;
            this.dropSpeed = dropSpeed;
            this.zoomOut = zoomOut;
            this.rainTex = rainTex;
            this.blurAmt = blurAmt;
            this.rainTrail = rainTrail;
            this.intervalMin = intervalMin;
            this.intervalMax = intervalMax;
            this.rate1 = rate1;
        }
    }

    /// <summary>
    /// PostProcess shader parameters: fog mis etc.
    /// </summary>
    [Serializable]
    public class Atmosphere
    {
        public int code;
        public string name;
        public float size;
        public float horiSpeed;
        public float vertSpeed;
        public float density;
        public string color;
        public Atmosphere()
        {

        }
        public Atmosphere(int code, string name, float size, float horiSpeed,float vertSpeed, float density, string color)
        {
            this.code = code;
            this.name = name;
            this.size = size;
            this.horiSpeed = horiSpeed;
            this.vertSpeed = vertSpeed;
            this.density = density;
            this.color = color;
        }
    }

    #endregion weather_parameters_class

    /// <summary>
    /// For Multimonitor use.
    /// </summary>
    [Serializable]
    public class Monitors
    {
        public int xres;
        public int yres;
        public int xoff;
        public int yoff;
        public string display_name;

        public Monitors()
        {
            xres = 0;
            yres = 0;
            xoff = 0;
            yoff = 0;
            display_name = null;
        }
    }
    //...debugging information
    [HideInInspector] public int weatherPollCnt = 0;

    /// <summary>
    /// openweathermap.org class
    /// </summary>
    [Serializable]
    public class WeatherOpenAPI
    {
        //public
        public string countryName;
        public string cityName;
        public int conditionID;
        public string conditionName;
        public float temp;
        public float windSpeed;
        public string weatherUnit;
        public long sunrise;
        public long sunset;
        public DateTime last_run_date;

        public WeatherOpenAPI()
        {
            countryName = "Country";
            cityName = "City";
            conditionID = 800; //clear sky
            temp = 31;
            windSpeed = 18;
            weatherUnit = "metric";
            sunrise = 946706400; //2000-1-1, 6am utx
            sunset =  946749600; // 2000-1-1, 6pm utx
            last_run_date = new System.DateTime(2000, 1, 1);
        }
    }
    [HideInInspector] public WeatherOpenAPI weather;

    /// <summary>
    /// Configuration file class.
    /// </summary>
    [Serializable]
    public class UserSettings
    {
        public int zipCode;
        public string cityName;
        public int layout;
        public bool isDone; // is configuration complete

        // layout
        public bool isClock;
        public bool isPerformance;
        public bool isWeather;

        //config
        public int clockType;
        public int pollingDelay;
        public int fps;
        public int imgScaling;
        public bool isMetric; //true = metric, false = imperial
        public bool isRestart; // restart wallpaper.exe
        public string apiKey;
        public bool isReload; // to reload scene , if settings changed?
        public bool autoUiHide; //auto hide UI when mouse moves away.
        public bool dayNightTint;
        public bool autoUiColor; //UI color based on wallpaper
        public bool runAtStartup;
        public bool sysColor; //system theme color
        public bool isDXVA; //hardware accleration video

        //video data
        public string vidPath; //file location, if null use picture wallpaper instead.

        //..debugging
        public bool isDemo; // control weather toggle

        public int userWeather;
        public string uiColor;
        public bool sun_overlay;
        public int blur_quality;
        public bool isLightTheme;
        public bool appFocusPause;

        //.. extra variables
        public bool bvar1; //bat_low_power_mode
        public bool bvar2; // debug_text
        public bool bvar3;
        public bool bvar4;
        public bool bvar5;

        public string svar1;
        public string svar2;
        public string svar3;
        public string svar4;
        public string svar5;

        public int ivar1; //blur intensity
        public int ivar2;
        public int ivar3;
        public int ivar4;
        public int ivar5;

        public UserSettings()
        {
            isDXVA = false;
            sysColor = false;
            apiKey = "default"; //openweathermap api key
            clockType = 0;
            isMetric = true;
            isDone = false;
            zipCode = 0;
            cityName = null;
            layout = 0;
            isClock = true;
            isPerformance = false;
            isWeather = true;
            pollingDelay = 2;
            fps = 24; // 100 - vsync
            imgScaling = 1; //0 -fill,1-stretch ,2 -none
            isRestart = false;
            isDemo = false;
            isReload = false;
            autoUiHide = true;
            dayNightTint = true;
            autoUiColor = true;
            vidPath = null;
            runAtStartup = false;
            userWeather = 800; // clear
            uiColor = "#F8F8FF";
            sun_overlay = false;
            blur_quality = 1; // 0- low, >=1 - good, 2 -very high, 3 - mipmap
            isLightTheme = false;
            appFocusPause = false;

            //extra variables
            bvar1 = false;
            bvar1 = false;
            bvar3 = false;
            bvar4 = false;
            bvar5 = false;

            svar1 = null;
            svar2 = null;
            svar3 = null;
            svar4 = null;
            svar5 = null;

            ivar1 = 0;
            ivar2 = 0;
            ivar3 = 0;
            ivar4 = 0;
            ivar5 = 0;

        }

    }
    [HideInInspector] public Monitors monitor;
    [HideInInspector] public UserSettings userSettings;
    [HideInInspector] public bool isMultiMonitor = false;

    void Awake()
    {
        CheckMonitors(); //multimonitor warning firstime
        //.. Singleton design, only one instance
        if (menuController == null)
        {
            DontDestroyOnLoad(gameObject);
            menuController = this;
        }
        else if (menuController != this)
        {
            Destroy(gameObject);
        }
    }


    #region multimonitor_check

    //https://gist.github.com/roydejong/130a91e1835154a3acaeda78c9dfbbd7

    public static System.IntPtr GetWindowHandle()
    {
        return StaticPinvoke.GetActiveWindow();
    }

    /// <summary>
    /// Shows Error alert box with OK button.
    /// </summary>
    /// <param name="text">Main alert text / content.</param>
    /// <param name="caption">Message box title.</param>
    public void Error(string text, string caption)
    {
        try
        {
            int msgboxID = StaticPinvoke.MessageBox(GetWindowHandle(), text, caption, (uint)(0x00000006L | 0x00000010L));
            //Debug.Log(msgboxID);

            switch (msgboxID)
            {
                case 2: //cancel
                    Application.Quit();
                    break;
                case 10:  //tryagain
                    CheckMonitors();
                    break;
                case 11:  //continue

                    break;
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Checks if multiple monitors present, displays warning.
    /// </summary>
    void CheckMonitors()
    {
        if (UnityDisplay.displays.Length > 1)
        {
            isMultiMonitor = true; // to tell headless.cs to use offset setwindowpos() instead.
            if (!File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "monitor.xml"))
            {
                Error("Multiple monitors are in early stages of development.\nDepending on your setup wallpaper might appear on the wrongside, after launching the application select Multimonitor in Traymeny to adjust position & size.\nNote: Different DPI multimonitor not supported currently.\n'Try Again' to check again.\n'Continue' to Start,\n'Cancel' to Quit", "Warning: OPS.. Multiple monitors Detected");
            }
        }
        else
        {

        }

    }
    #endregion multimonitor_check

    #region first_run_warning

    /// <summary>
    /// Disclaimer Messagebox
    /// </summary>
    public int WarningMsg(string text, string caption)
    {
        try
        {
            int msgboxID = StaticPinvoke.MessageBox(GetWindowHandle(), text, caption, (uint)(0x00000001L | 0x00000030L));
            return msgboxID;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return 2;
        }
    }
    #endregion first_run_warning

    public class ApplicationRule
    {
        public string processName;
        public bool sleep;

        public ApplicationRule(string processName, bool sleep)
        {
            this.processName = processName;
            this.sleep = sleep;
        }
        public ApplicationRule()
        {

        }
    }
    public List<ApplicationRule> appList = new List<ApplicationRule>();
    // Use this for initialization
    void Start () {
               
        Deserialize(appList); //application pause behavior, user defined.

        userSettings = new UserSettings(); //load default values/settings      
        //...default value object for resetting. {unused}
        //defaultSettings = new UserSettings();

        weather = new WeatherOpenAPI();

        LoadWeatherParameters();

        Load();  // load configuration save data if it exists

        #region disclaimer
        //monitor disclaimer called in Awake
        if (isFirstTime == true)
        {
            if (WarningMsg("This software is provided 'as-is'," +
                                " without any express or implied warranty. " +
                                "In no event will the authors be held liable for any damages arising from the use of this software.", "Just a second") == 2) 
            {
                Application.Quit();
                return;
            }
        }

        monitor = new Monitors(); //load default
        LoadMonitor(); //creates new file if not found, with proper resolution of primary monitor
        #endregion disclaimer

        Color tmpColor;
        if(ColorUtility.TryParseHtmlString(MenuController.menuController.userSettings.uiColor, out tmpColor))
        {
            userUiColor = tmpColor;
        }
        else
        {
            Debug.Log("color parse error: start() menucontroller");
        }

        tmpCityName = userSettings.cityName; // if cityname changes, reload

        main.instance.Start_();
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Preload")
        {
            StartCoroutine(LoadingScreen());
        }
        
    }
  
    AsyncOperation async;
    /// <summary>
    /// Async "wallpaper" Level Loading.
    /// </summary>
    IEnumerator LoadingScreen()
    {
        Image loading = GameObject.FindGameObjectWithTag("loading").GetComponent<Image>();
        //loadingObj.SetActive(true);
        async = SceneManager.LoadSceneAsync("wallpaper");
        async.allowSceneActivation = false;

        while (async.isDone == false)
        {
            loading.fillAmount = async.progress;
            if (async.progress == 0.9f)
            {
                loading.fillAmount = 1f; // loading animation, see ui element.
                async.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    #region applicationrules_load_save
    /// <summary>
    /// xml application_rules file write.
    /// </summary>
    public void SerializeObject(List<ApplicationRule> list)
    {
        try
        {
            File.Delete(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "application_rules.xml");
            var serializer = new XmlSerializer(typeof(List<ApplicationRule>));
            using (var stream = File.OpenWrite(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "application_rules.xml"))
            {
                serializer.Serialize(stream, list);
            }
        }
        catch(Exception ex)
        {
            Debug.Log("error : " + ex.Message);
        }
    }

    /// <summary>
    /// xml application_rules file read.
    /// </summary>

    /// <summary>
    /// xml application_rules file read.
    /// </summary>
    public void Deserialize(List<ApplicationRule> list)
    {
        if (File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "application_rules.xml"))
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<ApplicationRule>));
                using (var stream = File.OpenRead(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "application_rules.xml"))
                {
                    var other = (List<ApplicationRule>)(serializer.Deserialize(stream));
                    list.Clear();
                    list.AddRange(other);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("error");
                //Debug.Log("error : " + ex.Message);
            }
        }
        else
        {
            ApplicationRule[] tmp = {
                                        new ApplicationRule("Discord",false),
                                        new ApplicationRule("Photoshop",true),
                                        new ApplicationRule("AfterFX",true),
                                        new ApplicationRule("blender",true)
                                    };
            appList.AddRange(tmp);
            SerializeObject(appList);
        }
    }
    #endregion applicationrules_load_save

    #region config_load_save
    /// <summary>
    /// XML configuration file load.
    /// </summary>
    /// <remarks>
    /// Location: %USERPROFILE%\\Saved Games\\rePaper\\config.xml
    /// </remarks>
    public void Load()
    {
        if (File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "config.xml"))
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(UserSettings));
                FileStream file = File.Open(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "config.xml", FileMode.Open);
                //loadData = (UserSettings)ser.Deserialize(file);
                userSettings = (UserSettings)ser.Deserialize(file);
                file.Close();

               // userSettings = loadData;
            }
            catch (Exception ex)
            {
                Debug.Log("something went wrong reading file: " + ex.Message);
            }
        }
        else
        {
            //Save();
            isFirstTime = true;
        }
    }

    /// <summary>
    /// XML configuration file save.
    /// </summary>
    /// <remarks>
    /// Location: %USERPROFILE%\\Saved Games\\rePaper\\config.xml
    /// </remarks>
    public void Save()
    {
        try
        {
            XmlSerializer ser = new XmlSerializer(typeof(UserSettings));

            var folder = Directory.CreateDirectory(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper"));
            FileStream file = File.Create(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "config.xml");
            ser.Serialize(file, userSettings);
            file.Close();
        }
        catch(Exception e)
        {
            Debug.Log("Save() " + e.Message);
        }
    }

    #endregion

    #region monitor_load_save
    /// <summary>
    /// XML monitor configuration file load (multimonitor setups).
    /// </summary>
    /// <remarks>
    /// Location: %USERPROFILE%\\Saved Games\\rePaper\\monitor.xml
    /// </remarks>
    public void LoadMonitor()
    {
        if (File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "monitor.xml"))
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Monitors));
                FileStream file = File.Open(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "monitor.xml", FileMode.Open);
                //loadMonitorData = (Monitors)ser.Deserialize(file);
                monitor = (Monitors)ser.Deserialize(file); 
                file.Close();
                /*
                monitor.xres = loadMonitorData.xres;
                monitor.yres = loadMonitorData.yres;
                monitor.xoff = loadMonitorData.xoff;
                monitor.yoff = loadMonitorData.yoff;
                monitor.display_name = loadMonitorData.display_name;
                */
            }
            catch (Exception ex)
            {
                Debug.Log("something went wrong reading file: " + ex.Message);
            }
        }
        else
        {
            monitor.xres = Screen.currentResolution.width;
            monitor.yres = Screen.currentResolution.height;
            Save_Monitor();
        }
    }

    /// <summary>
    /// XML monitor configuration file save (multimonitor setups).
    /// </summary>
    /// <remarks>
    /// Location: %USERPROFILE%\\Saved Games\\rePaper\\monitor.xml
    /// </remarks>
    public void Save_Monitor()
    {
        try
        {
            XmlSerializer ser = new XmlSerializer(typeof(Monitors));
            var folder = Directory.CreateDirectory(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper"));
            FileStream file = File.Create(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "monitor.xml");
            ser.Serialize(file, monitor);
            file.Close();
        }
        catch(Exception e)
        {
            Debug.Log("save_monitor(): " + e.Message);
        }
    }
    #endregion

    #region weather_load_save
    /// <summary>
    /// Load stored weather to reduce weatherquery to server, binary file.
    /// </summary>
    /// <returns>
    /// True if load successful, else false.
    /// </returns>
    public bool Load_Weather()
    {
        if (File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather.dat"))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather.dat", FileMode.Open);
                //tmpWeather = (WeatherOpenAPI)bf.Deserialize(file);
                weather = (WeatherOpenAPI)bf.Deserialize(file); 
                file.Close();                
                /*
                weather.cityName = tmpWeather.cityName;
                weather.countryName = tmpWeather.countryName;
                weather.conditionName = tmpWeather.conditionName;
                weather.conditionID = tmpWeather.conditionID;
                weather.temp = tmpWeather.temp;
                weather.windSpeed = tmpWeather.windSpeed;
                weather.weatherUnit = tmpWeather.weatherUnit;
                weather.sunrise = tmpWeather.sunrise;
                weather.sunset = tmpWeather.sunset;
                weather.last_run_date = tmpWeather.last_run_date;
                */

                //Debug.Log("weather loaded: " + weather.temp);
                return true;
            }
            catch(Exception e)
            {
                Debug.Log("load_weather: " + e.Message);
                return false;
            }
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// Save stored weather to reduce weatherquery to server, binary file.
    /// </summary>
    public void Save_Weather()
    {
        try
        {
            //.. using binary file.
            BinaryFormatter bf = new BinaryFormatter();

            var folder = Directory.CreateDirectory(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper"));
            FileStream file = File.Create(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather.dat");
            bf.Serialize(file, weather);
            file.Close();
        }
        catch(Exception e)
        {
            Debug.Log("save_weather(): " + e.Message);
        }
    }

    #endregion weather_load_save

    #region weather_parameters_save_load

    public void LoadWeatherParameters()
    {

        if (File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather_parameters.xml"))
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Weather));
                FileStream file = File.Open(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather_parameters.xml", FileMode.Open);
                //weather_tmp = (Weather)ser.Deserialize(file);
                WeatherParam = (Weather)ser.Deserialize(file);
                file.Close();

                //WeatherParam = weather_tmp;  
            }
            catch (Exception ex)
            {
                Debug.Log("something went wrong reading file weather_parameters: " + ex.Message);
            }
        }
        else
        {
            // no-file, save default parameters in xml.
            SaveDefaultWeatherParameters();
        }
    }

    /// <summary>
    /// Saves default weatherparameter file, editing is done in config utility only.
    /// </summary>
    public void SaveDefaultWeatherParameters()
    {
        WeatherParam.rain = new Rain[]
        {
            new Rain(202,"thunderstorm with heavy rain",0.35f,0,0,0,0.6f,0.27f,0.7f,0.2f,50,0,0,0),
            new Rain(221,"ragged thunderstorm",0.1f,0.1f,0.1f,0.1f,0.4f,0.1f,0.2f,0.06f,0,0,0,0),
            new Rain(230,"drizzle thunder",0.19f,0,0,0,0.0625f,0.125f,0.3f,0.06f,0,0,0,0),
            new Rain(250,"thunderstorm",0.263f,0,0,0,0.35f,0.27f,0.6f,0.08f,10,0,0,0),
            new Rain(300,"light intensity drizzle",0.1f,0.1f,0.1f,0.1f,0.04f,0.1f,0.25f,0.04f,0,0,0,0),
            new Rain(320,"drizzle",0.19f,0,0,0,0.0625f,0.125f,0.3f,0.06f,0,0,0,0),
            new Rain(502,"very heavy rain",0.35f,0,0,0,0.6f,0.27f,0.7f,0.2f,50,0,0,0),
            new Rain(550,"rain",0.263f,0,0,0,0.35f,0.27f,0.6f,0.08f,10,0,0,0),
            new Rain(500,"light rain",0.15f,0,0.11f,0,0.15f,0.27f,0.2f,0.05f,5,0,0,0),
            new Rain(901,"User Rain - 1",0.6f,0,0.11f,0,0.15f,0.1f,0.5f,0.2f,10,0,0,0),
            new Rain(902,"User Rain - 2",1.0f,0,0.11f,0,0.2f,0.15f,0.4f,0.25f,25,0,0,0)
        };

        WeatherParam.atmosphere = new Atmosphere[]
        {
            new Atmosphere(701,"mist",0.22f,0.08f,0.05f,0.13f, "#9C9C9C"),
            new Atmosphere(721,"haze",0.3f,0.08f,0.05f,0.09f, "#9C9C9C"),
            new Atmosphere(741,"fog",0.07f,0.06f,0f,0.18f, "#D8E8F8"),
            new Atmosphere(761,"dust",0.2f,0.05f,0.0f,0.09f, "#F1E0B7"),
            new Atmosphere(903,"User Atmosphere - 1",0.16f,0.20f,0.01f,0.34f, "#808000"),
            new Atmosphere(904,"User Atmosphere - 2",0.13f,0.06f,0.0f,0.29f, "#FF7F50")
        };

        try
        {
            XmlSerializer ser = new XmlSerializer(typeof(Weather));
            var folder = Directory.CreateDirectory(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper"));
            FileStream file = File.Create(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather_parameters.xml");
            ser.Serialize(file, WeatherParam);
            file.Close();

            //WeatherParam = weather_tmp;
        }
        catch (Exception e)
        {
            Debug.Log("error saving file weather_parameters: " + e.Message);
        }
    }

    #endregion weather_parameters_save_load

}
