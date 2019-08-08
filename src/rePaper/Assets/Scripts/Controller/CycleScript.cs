using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using DateTime = System.DateTime;
using System.Globalization;

/// <summary>
/// This class handles: day/night color shift, weather query, weather effect.
/// </summary>
public class CycleScript : MonoBehaviour {

    //.. getweather
    int startMinute1 = 0, startHr1 = 0;
    int randNo;
    string currDataDate, prevDataDate;
    bool call_weather_function = false;
    bool is_save_file_loaded = false;

    public ParticleSystem rainTrail;
    GameObject tmpSnow;
    public Renderer wallpaperQuad;
    Material wallpaperMat;
    public Sprite transaprent_Img;
    public Image weatherIcon;
    public Sprite[] weatherIcons;
    public StaticRainBehaviour freeze_effect_1, freeze_effecT_2;
    public StaticRainBehaviour static_rain_5, static_rain_4, static_rain_1;
    public FrictionFlowRainBehaviour flow_rain_5;
    public SimpleRainBehaviour flow_rain_4, flow_rain_1;
    //[HideInInspector] public bool enableWeather;
    //public ParticleSystem lightningParticle;
    public RainCameraController rain4, rain5, ice,rain1;
    Coroutine cr, cr2;
    bool transition_pending = false, turn_on_lightning;
   // WeatherObject weather;
    public UB.D2FogsPE fogScript;
    public SpriteRenderer wallpaper;
    public Color lightning;
    public Color[] cycle;
    //public Color[] fogTypes;
    Color ambientLight;
    [HideInInspector] public float diff;
    bool isLightning;

    //..yahoo weather get.
     Text weather_cityState;
     Text weather_temperature;
     Text weather_condition;
     Text weather_windSpeed;
     Text sunrise_time;
     Text sunset_time;
    //wind 
    float tmpWindSpeed;
    float windSpeed;

    //..openapi weather
    int currWeather = 0, prevWeather = 0;
    bool ignore_checks = false;
    float dim_percent = 1.0f; // dim based on weather


    [HideInInspector] public int startMinute, startHr;
    System.DateTime Noon = new System.DateTime(
                        2018, 1, 1, 12, 00, 00, 00);
    System.DateTime MidNight = new System.DateTime(
                        2018, 1, 1, 00, 00, 00, 00);

    void Start() {
        wallpaperMat = wallpaperQuad.GetComponent<Renderer>().sharedMaterial;
        int tmp = 10 - MenuController.menuController.userSettings.ivar1; // original ivar1 : 0 ->100%, 10 -> 0%
        float tmpf = 0.7f * tmp;
        wallpaperMat.SetFloat("_BlurAdjust", tmpf);

        if(MenuController.menuController.userSettings.blur_quality == 0) //cheap
            wallpaperMat.SetFloat("_BlurSample", 2);
        else if(MenuController.menuController.userSettings.blur_quality == 1)
            wallpaperMat.SetFloat("_BlurSample", 16);
        else
            wallpaperMat.SetFloat("_BlurSample", 64);

        //rainScript.RainIntensity = 0f;
        Noon.ToString("HH:mm:ss"); //24hr format
        startMinute = System.DateTime.Now.Minute;
        startHr = System.DateTime.Now.Hour;

        weather_cityState = GameObject.FindGameObjectWithTag("weather_1").GetComponent<Text>();
        weather_temperature = GameObject.FindGameObjectWithTag("weather_2").GetComponent<Text>();
        weather_condition = GameObject.FindGameObjectWithTag("weather_3").GetComponent<Text>();
        weather_windSpeed = GameObject.FindGameObjectWithTag("weather_4").GetComponent<Text>();
        sunrise_time = GameObject.FindGameObjectWithTag("weather_5").GetComponent<Text>();
        sunset_time = GameObject.FindGameObjectWithTag("weather_6").GetComponent<Text>();

        startMinute1 = System.DateTime.Now.Minute;
        startHr1 = System.DateTime.Now.Hour;

        if (MenuController.menuController.userSettings.isDemo == true) //location weather disabled, custom weather
        {
            WeatherSystemTrayBtn(MenuController.menuController.userSettings.userWeather); //will also call button checkmark

        }
        else
        {
            StartCoroutine(WeatherUpdate());
        }

        randNo = Random.Range(0, 16);
        // port end

        if(MenuController.menuController.userSettings.sysColor == true)
            StartCoroutine(SystemThemeEvent());

        if(MenuController.menuController.userSettings.sun_overlay == false || MenuController.menuController.userSettings.isWeather == false)
        {
            sunrise_time.gameObject.SetActive(false);
            sunset_time.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Converts hex to unity color.
    /// </summary>
    /// <returns>unity RGB color</returns>
    Color HtmltoUnityColor(string htmlColor)
    {
        Color userDefinedColor;
        if (ColorUtility.TryParseHtmlString(htmlColor, out userDefinedColor)) //convert html color to RGB unity color.
        {
            return userDefinedColor;
        }
        else
        {
            Debug.Log("failure: colorconvert html");
            return Color.white;
        }
    }

    bool _ran = false;
    /// <summary>
    /// Switches system theme based on time, async method.
    /// </summary>
    IEnumerator SystemThemeEvent()
    {
        WaitForSeconds countTime = new WaitForSeconds(60f);
        main.instance.SystemThemeChange(); 
        while (true)
        {
            if (DateTime.Now.Hour == 6 || DateTime.Now.Hour == 18 && _ran == false)
            {
                _ran = true;
                main.instance.SystemThemeChange();
            }

            if (DateTime.Now.Hour != 6 || DateTime.Now.Hour != 18 && _ran == true)
            {
                _ran = false;
            }

            yield return countTime;
        }
    }


    #region assetbundle
    GameObject assetBundlePrefab;
    AssetBundle myLoadedAssetBundle;
    GameObject instantiatedSnow;
    Animator snowAnim;
    /// <summary>
    /// Loads assetbundle: snow
    /// </summary>
    /// <param name="str">Prefab names: light_snow, Snow8,snow_00386</param>
    void AssetBundleLoad(string str)
    {
        myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "snow"));
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            main.instance.tray.ShowNotification(1000, "Snow Missing", "Snow file missing, try redownloading?");
            return;
        }
        assetBundlePrefab = myLoadedAssetBundle.LoadAsset< GameObject > (str);

        instantiatedSnow = Instantiate(assetBundlePrefab);
        snowAnim = instantiatedSnow.GetComponent<Animator>();
        snowAnim.speed = 0.75f;

        //...scaling, its pretty lazy attemp to be honest...planning on swapping out aftereffects one with particle system istead.
        Vector3 tmp = instantiatedSnow.transform.localScale;
        tmp.x = ((float)Screen.currentResolution.width / (float)Screen.currentResolution.height);
        tmp.x /= (1920.0f / 1080.0f);

        if(str == "light_snow")
            tmp.x *= 1.41f;

        instantiatedSnow.transform.localScale = tmp;
               
    }

    /// <summary>
    /// Unloads assetbundle: snow (free memory).
    /// </summary>
    public void UnloadAssets()
    {
        if (myLoadedAssetBundle != null)
        {
            myLoadedAssetBundle.Unload(true); //can be used to clear decompression operation memeory when false?

            if (instantiatedSnow != null)
                Destroy(instantiatedSnow);
        }
    }
    #endregion assetbundle

    #region weather_icon_change
    /// <summary>
    /// custom weather icon mapping, because openweathermap icons looks horrible.
    /// </summary>
    /// <remarks> ref: https://erikflowers.github.io/weather-icons/ </remarks>
    /// <param name="grp">openweathermap weather-code</param>
    void WeatherIconChanger(int grp)
    {
        if(System.DateTime.Now.Hour >= 6 && System.DateTime.Now.Hour <= 17) //day
        {
            if(grp == 2) //thunder
                weatherIcon.sprite = weatherIcons[12];
            else if(grp == 3) //drizzle
                weatherIcon.sprite = weatherIcons[10]; //drizzle just rain?
            else if(grp == 5) //rain
                weatherIcon.sprite = weatherIcons[10];
            else if( grp == 6) //snow
                weatherIcon.sprite = weatherIcons[14];
            else if( grp == 800) //clear sky
                weatherIcon.sprite = weatherIcons[0];
            else if (grp ==801) //few clouds
                weatherIcon.sprite = weatherIcons[2];
            else if (grp ==802) //scattered clouds
                weatherIcon.sprite = weatherIcons[4];
            else if (grp ==803) //broken clouds
                weatherIcon.sprite = weatherIcons[6];
        }
        else //night
        {
            if (grp == 2) //thunder
                weatherIcon.sprite = weatherIcons[13];
            else if (grp == 3) //drizzle
                weatherIcon.sprite = weatherIcons[11]; //drizzle just rain?
            else if (grp == 5) //rain
                weatherIcon.sprite = weatherIcons[11];
            else if (grp == 6) //snow
                weatherIcon.sprite = weatherIcons[15];
            else if (grp == 800)
                weatherIcon.sprite = weatherIcons[1];
            else if (grp == 801) //few clouds
                weatherIcon.sprite = weatherIcons[3];
            else if (grp == 802) //scattered clouds
                weatherIcon.sprite = weatherIcons[5];
            else if (grp == 803) //broken clouds
                weatherIcon.sprite = weatherIcons[7];
        }

        //..independent of day/night
        if (grp == 701) //mist
            weatherIcon.sprite = weatherIcons[18]; //haze & mist same img?
        else if (grp == 721) //haze
            weatherIcon.sprite = weatherIcons[18];
        else if (grp == 741) //fog
            weatherIcon.sprite = weatherIcons[17];
        else if (grp == 761) //dust
            weatherIcon.sprite = weatherIcons[16];

        if (grp == 9000)
            weatherIcon.sprite = transaprent_Img;
    }

    #endregion

    #region Wind_Unity_Conversion
    /// <summary>
    /// Converts wind unity based on user selection.
    /// </summary>
    void WindUnitConvert()
    {
        if (MenuController.menuController.userSettings.isMetric == true)
        {
            windSpeed = 3.6f * MenuController.menuController.weather.windSpeed; // m/s -> km/hr
        }
        else
        {
            windSpeed = 1.61f * MenuController.menuController.weather.windSpeed; // m/h -> km/hr
        }
    }

    #endregion

    void Rain(float intensity, float mist)
    {
      //  wallpaperMat.SetFloat("_RainIntensity", intensity);
       // rainScript.RainIntensity = intensity;
      //  rainScript.RainMistThreshold = 1;// mist;      
    }

    /// <summary>
    /// Sets heartfelt raindrop shader properties.
    /// </summary>
    /// <param name="rainAmt">Rain intensity, 0-1 range.</param>
    /// <param name="zoom">Rain drop size, 0-1 range.</param>
    /// <param name="dropSpeed">Rain drop velocity, 0-1 range.</param>
    /// <param name="blurAmount">Blur intensity, 0-1 range.</param>
    void RainWindowEffect(float rainAmt, float zoom, float dropSpeed, float blurAmount)
    {
        wallpaperMat.SetFloat("_RainIntensity", rainAmt);
        wallpaperMat.SetFloat("_ZoomOut", zoom); 
        wallpaperMat.SetFloat("_Speed", dropSpeed);
        wallpaperMat.SetFloat("_Blur", blurAmount);
    }

    
    void RainWindowEffect(int code)
    {
        foreach (var item in MenuController.menuController.WeatherParam.rain)
        {
            if(code == item.code)
            {
                wallpaperMat.SetFloat("_RainIntensity", item.rainAmt);
                wallpaperMat.SetFloat("_ZoomOut", item.zoomOut);
                wallpaperMat.SetFloat("_Speed", item.dropSpeed);
                wallpaperMat.SetFloat("_Blur", item.blurAmt);
                wallpaperMat.SetFloat("_RainNormal", item.rainTex);
                wallpaperMat.SetFloat("_RainDrop2", item.drop2Amt);
                wallpaperMat.SetFloat("_RainDrop1", item.drop1Amt);
                wallpaperMat.SetFloat("_RainStatic", item.staticDrpAmt);
                RainT_Trail_Particle(item.rainTrail, (item.rainAmt + Mathf.Abs(1-item.zoomOut)/2f)/2f );
            }
        }
  
    }

    IEnumerator Rain_Custom(float timeSlow, float rangeMin, float rangeMax, float breakMin, float breakMax)
    {
        while (true)
        {
            //wallpaperMat.SetFloat("_RainDrop2", intensity);
            //wallpaperMat.SetFloat("_TimeDivide", timeSlow);
            yield return new WaitForSeconds(Random.Range(rangeMin, rangeMax));
            //wallpaperMat.SetFloat("_TimeDivide", timeSlow/2f);
            yield return new WaitForSeconds(Random.Range(breakMin, breakMax));
        }
    }

    void Rain_Camera_Effect(int rainID, float alpha)
    {
        
        if (rainID == 4)
        { 
            //rain4.Alpha = alpha;
            //rain4.Play();
            //static_rain_4.StartRain();
            //flow_rain_4.StartRain();
            
           // rain5.Alpha = alpha;
            // rain5.Play();
            //static_rain_5.StartRain();
           // flow_rain_5.StartRain();
        }
        else if (rainID == 5)
        {
           // rain5.Alpha = alpha;
            // rain5.Play();
            //static_rain_5.StartRain();
           // flow_rain_5.StartRain();
        }
        else if (rainID == 10) //ice 
        {
            if (MenuController.menuController.userSettings.blur_quality == 0)
            {
              //  freeze_effect_1.ShaderType = RainDropTools.RainDropShaderType.Cheap;
               // freeze_effecT_2.ShaderType = RainDropTools.RainDropShaderType.Cheap;
            }
            else
            {
               // freeze_effect_1.ShaderType = RainDropTools.RainDropShaderType.Expensive;
               // freeze_effecT_2.ShaderType = RainDropTools.RainDropShaderType.Expensive;
            }
            ice.Alpha = alpha;
            //ice.Play();
            freeze_effect_1.StartRain();
            freeze_effecT_2.StartRain();
        }
        else if( rainID == 11)
        {
            if (MenuController.menuController.userSettings.blur_quality == 0)
            {
              //  static_rain_1.ShaderType = RainDropTools.RainDropShaderType.Cheap;
              //  flow_rain_1.ShaderType = RainDropTools.RainDropShaderType.Cheap;
            }
            else
            {
                //freeze_effect_1.ShaderType = RainDropTools.RainDropShaderType.Expensive;
                //flow_rain_1.ShaderType = RainDropTools.RainDropShaderType.Expensive;
            }
            static_rain_1.StartRain();
            flow_rain_1.StartRain();
        }
    
    }

    /*
    void Snow_Particle_Caller(int maxParticle, int emissionRate)
    {
        
        var main = snowParticle1.main;
        var emission = snowParticle1.emission;
        main.maxParticles = maxParticle;
        emission.rateOverTime = emissionRate;
        //snowParticle1.Play();
    }
    */

    /// <summary>
    /// Converts wind unity based on user selection.
    /// </summary>
    void EnableBlur(bool isSnow = false)
    {
        if (MenuController.menuController.userSettings.blur_quality == 0)
            wallpaperMat.EnableKeyword("_CHEAP_BLUR");
        else if (MenuController.menuController.userSettings.blur_quality == 1)
            wallpaperMat.EnableKeyword("_GOOD_BLUR");
        else 
            wallpaperMat.EnableKeyword("_HIGH_BLUR");
           
    }

    /// <summary>
    /// Raintrail particle system effect.
    /// </summary>
    /// <param name="maxParticle"> max raintrail count</param>
    /// <param name="rainAmt"> multiply 0.5f by this value</param>
    void RainT_Trail_Particle(int maxParticle, float sizeMultiple)
    {
        var rainP = rainTrail.main;
        rainP.maxParticles = maxParticle;
        rainP.startSizeY = 0.5f * sizeMultiple;
        rainTrail.Play();
    }

    /// <summary>
    /// Weather Effect Play.
    /// </summary>
    /// <param name="exception">if true don't compare previous & current weather, switch effect anyway.</param>
    void WeatherSystem(bool exception = false)
    {
        #region windspeed_unused
        /*
        //..wind: https://www.windows2universe.org/earth/Atmosphere/wind_speeds.html
        Vector3 tmpSpeed;
        //rainScript.EnableWind = true;
        if (windSpeed > 6 && windSpeed <= 11)
        {
            tmpSpeed.x = 40; //min
            tmpSpeed.y = 51; //max
            tmpSpeed.z = 0;  // soundmultiplier
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 19)
        {
            tmpSpeed.x = 60;
            tmpSpeed.y = 71;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 28)
        {
            tmpSpeed.x = 90;
            tmpSpeed.y = 101;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 38)
        {
            tmpSpeed.x = 110;
            tmpSpeed.y = 121;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 49)
        {
            tmpSpeed.x = 140;
            tmpSpeed.y = 151;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 61)
        {
            tmpSpeed.x = 152;
            tmpSpeed.y = 161;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 74)
        {
            tmpSpeed.x = 170;
            tmpSpeed.y = 181;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 88)
        {
            tmpSpeed.x = 190;
            tmpSpeed.y = 201;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 102)
        {
            tmpSpeed.x = 240;
            tmpSpeed.y = 251;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed <= 117)
        {
            tmpSpeed.x = 290;
            tmpSpeed.y = 301;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        else if (windSpeed > 117)//hurricane
        {
            tmpSpeed.x = 900;
            tmpSpeed.y = 1001;
            tmpSpeed.z = 0;
            rainScript.WindSpeedRange = tmpSpeed;
        }
        */
        #endregion windspeed

        if (currWeather != prevWeather || exception == true)
        {
            // previous weather resetting
            //raindrop shader
            wallpaperMat.SetFloat("_RainNormal", 0.6f);
            //wallpaperMat.SetFloat("_TimeDivide", 1);
            wallpaperMat.SetFloat("_RainDrop2", 0);
            wallpaperMat.SetFloat("_RainDrop1", 0);
            wallpaperMat.SetFloat("_RainStatic", 0);

            wallpaperMat.DisableKeyword("_ISRAIN_YES_LIGHTNING");
            wallpaperMat.DisableKeyword("_ISRAIN_YES");
            wallpaperMat.DisableKeyword("_ISRAIN_NO");
            wallpaperMat.DisableKeyword("_CHEAP_BLUR");
            wallpaperMat.DisableKeyword("_GOOD_BLUR");
            wallpaperMat.DisableKeyword("_HIGH_BLUR");
            wallpaperMat.DisableKeyword("_SNOW_BLUR");
            //wallpaperMat.DisableKeyword("_HEAT");
            wallpaperMat.DisableKeyword("_REV_RAIN");
            //..asset bundle snow
            if (myLoadedAssetBundle != null)
            {
                myLoadedAssetBundle.Unload(true); //can be used to clear decompression operation memeory when false?

                if(instantiatedSnow !=null)
                    Destroy(instantiatedSnow);
            }

            rainTrail.Stop();
            //..snow prefab
            if (tmpSnow != null)
                Destroy(tmpSnow);
            dim_percent = 1.0f;
            WeatherIconChanger(9000);
            fogScript.enabled = false;
            turn_on_lightning = false;
            //lightningParticle.Stop(); //slight delay in lightning coroutine
            //rainScript.RainIntensity = 0f;
            //rainScript.RainMistThreshold = 1f;
            if (cr != null)
                StopCoroutine(cr);
            /*
            if(cr2 != null)
                StopCoroutine(cr2);
            */
            //snowParticle1.Stop();
            //snowFog.Stop();
            //rain camera effect
            //rain5.StopImmidiate();
            //rain4.StopImmidiate();
            rain1.StopImmidiate();
            ice.StopImmidiate();

            //weather-codes: https://openweathermap.org/weather-conditions
            // to-do: store weather parameters in class & write to external file so that its end user editable.
            if (MenuController.menuController.weather.conditionID >= 200 && MenuController.menuController.weather.conditionID < 300) // grp-2xx thunderstorm
            {
                EnableBlur();             
                wallpaperMat.EnableKeyword("_ISRAIN_YES_LIGHTNING");

                if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null )
                    wallpaperMat.EnableKeyword("_REV_RAIN");

                dim_percent = 0.5f;
                WeatherIconChanger(2);
                if (MenuController.menuController.weather.conditionID == 212 || MenuController.menuController.weather.conditionID == 202) // heavy thunderstorm, thunderstorm with heavy rain
                {
                    RainWindowEffect(202);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.7f);
                    RainT_Trail_Particle(50);
                    RainWindowEffect(0.35f, .27f, 0.6f, 0.2f);
                    */

                }
                else if (MenuController.menuController.weather.conditionID == 221) // ragged thunderstorm
                {
                    RainWindowEffect(221);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.2f);
                    RainWindowEffect(.1f, .1f, 0.04f, 0.06f);
                    wallpaperMat.SetFloat("_RainDrop2", 1f * (0.5f + 0.5f)/10);
                    wallpaperMat.SetFloat("_RainDrop1", 1f/10f);
                    wallpaperMat.SetFloat("_RainStatic", 1f/10f);
                    */
                }
                else if (MenuController.menuController.weather.conditionID == 230 || MenuController.menuController.weather.conditionID == 231 ||
                    MenuController.menuController.weather.conditionID == 232 || MenuController.menuController.weather.conditionID == 201) //low,mid,high drizzle thunder, 201 -light rain
                {
                    RainWindowEffect(230);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.3f);
                    RainWindowEffect(.19f, .125f, 0.0625f, 0.06f);
                    wallpaperMat.SetFloat("_RainDrop2", 0f);
                    */
                }
                else //default thunderstorm
                {
                    RainWindowEffect(250);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.6f);
                    RainT_Trail_Particle(10);
                    RainWindowEffect(0.263f, .27f, 0.35f, 0.08f);
                    */
                }
            }
            else if (MenuController.menuController.weather.conditionID >= 300 && MenuController.menuController.weather.conditionID < 400) // grp-3xx drizzle
            {
                EnableBlur();
                wallpaperMat.EnableKeyword("_ISRAIN_YES");

                if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
                    wallpaperMat.EnableKeyword("_REV_RAIN");

                dim_percent = 0.8f;
                WeatherIconChanger(3);
                if (MenuController.menuController.weather.conditionID == 300 || MenuController.menuController.weather.conditionID == 310 
                        || MenuController.menuController.weather.conditionID == 321) // light intensity drizzle, light etc
                {
                    RainWindowEffect(300);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.25f);
                    RainWindowEffect(.1f, .1f, 0.04f, 0.06f);
                    wallpaperMat.SetFloat("_RainDrop2", 1f*(0.5f+ 0.5f)/10f);
                    wallpaperMat.SetFloat("_RainDrop1", 1f/10f);
                    wallpaperMat.SetFloat("_RainStatic", 1f/10f);
                    */
                }
                else  //.. stock drizzle
                {
                    RainWindowEffect(320);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.3f);
                    RainWindowEffect(.19f, .125f, 0.0625f, 0.06f);
                    */
                }
            }
            else if (MenuController.menuController.weather.conditionID >= 500 && MenuController.menuController.weather.conditionID < 600) // grp-5xx rain
            {
                EnableBlur();
                wallpaperMat.EnableKeyword("_ISRAIN_YES");

                if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
                    wallpaperMat.EnableKeyword("_REV_RAIN");

                dim_percent = 0.7f;
                WeatherIconChanger(5);
                if (MenuController.menuController.weather.conditionID == 502 || MenuController.menuController.weather.conditionID == 503 
                    || MenuController.menuController.weather.conditionID == 504) // heavy intensity rain, very heavy rain, extreme rain
                {
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.7f);
                    RainT_Trail_Particle(50);
                    RainWindowEffect(0.35f, .27f, 0.6f, 0.2f);
                    */
                    RainWindowEffect(502);
                    dim_percent = 0.5f;
                }
                else if (MenuController.menuController.weather.conditionID == 500 || MenuController.menuController.weather.conditionID == 501) // light rain, moderate rain	
                {
                    RainWindowEffect(500);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.2f);
                    RainT_Trail_Particle(5);
                    RainWindowEffect(0.15f, .27f, 0.15f, 0.05f);
                    wallpaperMat.SetFloat("_RainDrop2", 1.1f/10f);
                    */

                }
                else //stock rain
                {
                    RainWindowEffect(550);
                    /*
                    wallpaperMat.SetFloat("_RainNormal", 0.6f);
                    RainT_Trail_Particle(10);
                    RainWindowEffect(0.263f, .27f, 0.35f, 0.08f);
                    */
                }
            }
            else if (MenuController.menuController.weather.conditionID >= 600 && MenuController.menuController.weather.conditionID < 700) //grp-6xx, snow
            {
                wallpaperMat.EnableKeyword("_SNOW_BLUR");
                dim_percent = 0.9f;
                WeatherIconChanger(6);
                if (MenuController.menuController.weather.conditionID == 600) // light snow
                {
                    AssetBundleLoad("light_snow");
                   // StartCoroutine(AssetBundleLoadAsync("light_snow"));
                 //   tmpSnow = Instantiate(snowPrefabs[0]);
                    Rain_Camera_Effect(10, 0.045f);
                   // Snow_Particle_Caller(250, 15);
                }
                else if (MenuController.menuController.weather.conditionID == 602) // heavy snow
                {
                    AssetBundleLoad("Snow8");
                    dim_percent = 0.7f;
                    //Rain_Camera_Effect(10, 0.09f);
                    Rain_Camera_Effect(10, 0.045f);
                    //Snow_Particle_Caller(1000, 50);
                }
                else if (MenuController.menuController.weather.conditionID == 611) //sleet
                {
                    /*
                    static_rain_1.StartRain();
                    flow_rain_1.StartRain();
                    */
                    Rain_Camera_Effect(11, 0.045f);

                    //snowParticle1.Play();
                    AssetBundleLoad("snow_00386");
                    snowAnim = instantiatedSnow.GetComponent<Animator>();
                    snowAnim.speed = 1.25f;

                    dim_percent = 0.8f;
                    //stock
                    //Rain_Camera_Effect(10, 0.045f);
                    //Snow_Particle_Caller(500, 25);
                }
                else //default snow
                {
                    AssetBundleLoad("snow_00386");
                    //StartCoroutine(AssetBundleLoadAsync("snow_00386"));
                    // tmpSnow = Instantiate(snowPrefabs[1]);
                    dim_percent = 0.8f;
                    //stock
                    //Rain_Camera_Effect(10, 0.065f);
                    Rain_Camera_Effect(10, 0.045f);
                   // Snow_Particle_Caller(500, 25);
                }
            }
            else if (MenuController.menuController.weather.conditionID >= 700 && MenuController.menuController.weather.conditionID < 800) // grp-7xx, atmosphere. No stock effect
            {
                dim_percent = 0.8f;
                if (MenuController.menuController.weather.conditionID == 701) // mist
                {
                    WeatherIconChanger(701);
                    fogScript.enabled = true;
                    fogScript.Color = HtmltoUnityColor(MenuController.menuController.WeatherParam.atmosphere[0].color);//fogTypes[1];
                    fogScript.Size = 10*MenuController.menuController.WeatherParam.atmosphere[0].size;
                    fogScript.HorizontalSpeed = MenuController.menuController.WeatherParam.atmosphere[0].horiSpeed;
                    fogScript.VerticalSpeed = MenuController.menuController.WeatherParam.atmosphere[0].vertSpeed;
                    fogScript.Density = 5*MenuController.menuController.WeatherParam.atmosphere[0].density;
                    /*
                    fogScript.enabled = true;
                    fogScript.Color = fogTypes[1];
                    fogScript.Size = 2.2f;
                    fogScript.HorizontalSpeed = 0.08f;
                    fogScript.VerticalSpeed = 0.05f;
                    fogScript.Density = 0.66f;
                    */
                }
                else if (MenuController.menuController.weather.conditionID == 721) // haze
                {
                    WeatherIconChanger(721);
                    fogScript.enabled = true;
                    fogScript.Color = HtmltoUnityColor(MenuController.menuController.WeatherParam.atmosphere[1].color);
                    fogScript.Size = 10*MenuController.menuController.WeatherParam.atmosphere[1].size;
                    fogScript.HorizontalSpeed = MenuController.menuController.WeatherParam.atmosphere[1].horiSpeed;
                    fogScript.VerticalSpeed = MenuController.menuController.WeatherParam.atmosphere[1].vertSpeed;
                    fogScript.Density = 5*MenuController.menuController.WeatherParam.atmosphere[1].density;
                    /*
                    fogScript.enabled = true;
                    fogScript.Color = fogTypes[1];
                    fogScript.Size = 3.0f;
                    fogScript.HorizontalSpeed = 0.08f;
                    fogScript.VerticalSpeed = 0.05f;
                    fogScript.Density = 0.45f;
                    */
                }
                else if (MenuController.menuController.weather.conditionID == 741) //fog
                {
                    dim_percent = 0.5f;
                    WeatherIconChanger(741);
                    fogScript.enabled = true;
                    fogScript.Color = HtmltoUnityColor(MenuController.menuController.WeatherParam.atmosphere[2].color);
                    fogScript.Size = 10*MenuController.menuController.WeatherParam.atmosphere[2].size;
                    fogScript.HorizontalSpeed = MenuController.menuController.WeatherParam.atmosphere[2].horiSpeed;
                    fogScript.VerticalSpeed = MenuController.menuController.WeatherParam.atmosphere[2].vertSpeed;
                    fogScript.Density = 5*MenuController.menuController.WeatherParam.atmosphere[2].density;
                    /*
                    fogScript.enabled = true;
                    fogScript.Color = fogTypes[0];
                    fogScript.Size = 0.7f;
                    fogScript.HorizontalSpeed = 0.06f;
                    fogScript.VerticalSpeed = 0f;
                    fogScript.Density = 0.9f;
                    */
                }
                else if (MenuController.menuController.weather.conditionID == 761) // dust
                {
                    WeatherIconChanger(761);
                    fogScript.enabled = true;
                    fogScript.Color = HtmltoUnityColor(MenuController.menuController.WeatherParam.atmosphere[3].color);
                    fogScript.Size = 10*MenuController.menuController.WeatherParam.atmosphere[3].size;
                    fogScript.HorizontalSpeed = MenuController.menuController.WeatherParam.atmosphere[3].horiSpeed;
                    fogScript.VerticalSpeed = MenuController.menuController.WeatherParam.atmosphere[3].vertSpeed;
                    fogScript.Density = 5*MenuController.menuController.WeatherParam.atmosphere[3].density;
                    /*
                    fogScript.enabled = true;
                    fogScript.Color = fogTypes[2];
                    fogScript.Size = 2.0f;
                    fogScript.HorizontalSpeed = 0.05f;
                    fogScript.VerticalSpeed = 0.0f;
                    fogScript.Density = 0.7f;
                    */
                }
            }
            else if (MenuController.menuController.weather.conditionID == 800) // clear
            {
                //wallpaperMat.EnableKeyword("_HEAT");
                dim_percent = 1.0f;
                WeatherIconChanger(800);
            }
            else if(MenuController.menuController.weather.conditionID > 800 && MenuController.menuController.weather.conditionID <= 900)// grp 80x, clouds
            {
                dim_percent = 0.9f;
                // just to be sure, instead of just passing random number to weathericonchanger
                if (MenuController.menuController.weather.conditionID == 801)
                    WeatherIconChanger(801);
                else if (MenuController.menuController.weather.conditionID == 802)
                    WeatherIconChanger(802);
                else if (MenuController.menuController.weather.conditionID == 803)
                    WeatherIconChanger(803);
                else if (MenuController.menuController.weather.conditionID == 804)
                    dim_percent = 0.7f;
            }
            else //custom presets
            {
                if(MenuController.menuController.weather.conditionID == 901) //custom preset rain
                {
                    EnableBlur();
                    wallpaperMat.EnableKeyword("_ISRAIN_YES");

                    if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
                        wallpaperMat.EnableKeyword("_REV_RAIN");

                    RainWindowEffect(901);
                    WeatherIconChanger(5);
                }
                else if(MenuController.menuController.weather.conditionID == 902) //thunder rain custom
                {
                    EnableBlur();
                    wallpaperMat.EnableKeyword("_ISRAIN_YES_LIGHTNING");

                    if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
                        wallpaperMat.EnableKeyword("_REV_RAIN");

                    //dim_percent = 0.5f;
                    WeatherIconChanger(2);
                    RainWindowEffect(902);

                }
                else if (MenuController.menuController.weather.conditionID == 903) //atmosphere effect custom #1
                {
                    fogScript.enabled = true;
                    fogScript.Color = HtmltoUnityColor(MenuController.menuController.WeatherParam.atmosphere[4].color);
                    fogScript.Size = 10 * MenuController.menuController.WeatherParam.atmosphere[4].size;
                    fogScript.HorizontalSpeed = MenuController.menuController.WeatherParam.atmosphere[4].horiSpeed;
                    fogScript.VerticalSpeed = MenuController.menuController.WeatherParam.atmosphere[4].vertSpeed;
                    fogScript.Density = 5 * MenuController.menuController.WeatherParam.atmosphere[4].density;
                    WeatherIconChanger(761);
                }
                else if (MenuController.menuController.weather.conditionID == 904) //atmosphere effect custom #2
                {
                    fogScript.enabled = true;
                    fogScript.Color = HtmltoUnityColor(MenuController.menuController.WeatherParam.atmosphere[5].color);
                    fogScript.Size = 10 * MenuController.menuController.WeatherParam.atmosphere[5].size;
                    fogScript.HorizontalSpeed = MenuController.menuController.WeatherParam.atmosphere[5].horiSpeed;
                    fogScript.VerticalSpeed = MenuController.menuController.WeatherParam.atmosphere[5].vertSpeed;
                    fogScript.Density = 5 * MenuController.menuController.WeatherParam.atmosphere[5].density;
                    WeatherIconChanger(761);
                }
            }
        }
       

    }

    #region weatherbtns
    /// <summary>
    /// Change weather - weather traymenu.
    /// </summary>
    /// <param name="i">openweathermap weather-code</param>
    public void WeatherSystemTrayBtn(int i)
    {
        currWeather = 404;
        prevWeather = 405;
        MenuController.menuController.userSettings.isMetric = true;
        MenuController.menuController.weather.windSpeed = 2f;

        if (MenuController.menuController.userSettings.isDemo == true)
        {
            MenuController.menuController.userSettings.userWeather = i;
            MenuController.menuController.Save();

            if(main.instance != null)
                main.instance.WeatherBtnCheckMark();
        }

        if (i == 800)
        {
            MenuController.menuController.weather.conditionID = 800;
            weather_condition.text = "clear";
        }
        if (i == 202)
        {
            MenuController.menuController.weather.conditionID = 202;
            weather_condition.text = "thunderstorm with heavy rain";
        }
        else if (i == 221)
        {
            MenuController.menuController.weather.conditionID = 221;
            weather_condition.text = "ragged thunderstorm";
        }
        else if (i == 230)
        {
            MenuController.menuController.weather.conditionID = 230;
            weather_condition.text = "drizzle thunder";
        }
        else if (i == 250)
        {
            MenuController.menuController.weather.conditionID = 250;
            weather_condition.text = "thunderstorm";
        }
        else if (i == 300)
        {
            MenuController.menuController.weather.conditionID = 300;
            weather_condition.text = "light intensity drizzle";
        }
        else if (i == 320)
        {
            MenuController.menuController.weather.conditionID = 320;
            weather_condition.text = "drizzle";
        }
        else if (i == 502)
        {
            MenuController.menuController.weather.conditionID = 502;
            weather_condition.text = "very heavy rain";
        }
        else if (i == 550)
        {
            MenuController.menuController.weather.conditionID = 550;
            weather_condition.text = "rain";
        }
        else if (i == 600)
        {
            MenuController.menuController.weather.conditionID = 600;
            weather_condition.text = "light snow";
        }
        else if (i == 602)
        {
            MenuController.menuController.weather.conditionID = 602;
            weather_condition.text = "heavy snow";
        }
        else if (i == 650)
        {
            MenuController.menuController.weather.conditionID = 650;
            weather_condition.text = "snow";
        }
        else if (i == 611)
        {
            //611
            MenuController.menuController.weather.conditionID = 611;
            weather_condition.text = "sleet";
        }
        else if (i == 701)
        {
            MenuController.menuController.weather.conditionID = 701;
            weather_condition.text = "mist";
        }
        else if (i == 721)
        {
            MenuController.menuController.weather.conditionID = 721;
            weather_condition.text = "haze";
        }
        else if (i == 741)
        {
            MenuController.menuController.weather.conditionID = 741;
            weather_condition.text = "fog";
        }
        else if (i == 761)
        {
            MenuController.menuController.weather.conditionID = 761;
            weather_condition.text = "dust";
        }
        else if( i == 500)
        {
            MenuController.menuController.weather.conditionID = 500;
            weather_condition.text = "light rain";
        }
        else if (i == 901)
        {
            MenuController.menuController.weather.conditionID = 901;
            weather_condition.text = "User Rain - 1";
        }
        else if (i == 902)
        {
            MenuController.menuController.weather.conditionID = 902;
            weather_condition.text = "User Rain - 2";
        }
        else if (i == 903)
        {
            MenuController.menuController.weather.conditionID = 903;
            weather_condition.text = "User Atmosphere - 1";
        }
        else if (i == 904)
        {
            MenuController.menuController.weather.conditionID = 904;
            weather_condition.text = "User Atmospher - 2";
        }

        WindUnitConvert();
        weather_windSpeed.text = "Wind: " + windSpeed.ToString("F2");
        WeatherSystem();
    }

    #region weather_toggle_unused
    int cnt = 0;
    public void ToggleWeatherbtn()
    {
        currWeather = 404;
        prevWeather = 405;
        MenuController.menuController.userSettings.isMetric = true;
        MenuController.menuController.weather.windSpeed = 2f;

        if (true) //condiiton, to be used later
        {
            if (cnt == 0)
            {
                MenuController.menuController.weather.conditionID = 800;
                weather_condition.text = "clear";
            }
            if (cnt == 1)
            {
                MenuController.menuController.weather.conditionID = 202;
                weather_condition.text = "thunderstorm with heavy rain";
            }
            else if (cnt == 2)
            {
                MenuController.menuController.weather.conditionID = 221;
                weather_condition.text = "ragged thunderstorm";
            }
            else if (cnt == 3)
            {
                MenuController.menuController.weather.conditionID = 230;
                weather_condition.text = "drizzle thunder";
            }
            else if (cnt == 4)
            {
                MenuController.menuController.weather.conditionID = 250;
                weather_condition.text = "thunderstorm";
            }
            else if (cnt == 5)
            {
                MenuController.menuController.weather.conditionID = 300;
                weather_condition.text = "light intensity drizzle";
            }
            else if (cnt == 6)
            {
                MenuController.menuController.weather.conditionID = 320;
                weather_condition.text = "drizzle";
            }
            else if (cnt == 7)
            {
                MenuController.menuController.weather.conditionID = 502;
                weather_condition.text = "very heavy rain";
            }
            else if (cnt == 8)
            {
                MenuController.menuController.weather.conditionID = 550;
                weather_condition.text = "rain";
            }
            else if (cnt == 9)
            {
                MenuController.menuController.weather.conditionID = 600;
                weather_condition.text = "light snow";
            }
            else if (cnt == 10)
            {
                MenuController.menuController.weather.conditionID = 602;
                weather_condition.text = "heavy snow";
            }
            else if (cnt == 11)
            {
                MenuController.menuController.weather.conditionID = 650;
                weather_condition.text = "snow";
            }
            else if ( cnt == 12)
            {
                //611
                MenuController.menuController.weather.conditionID = 611;
                weather_condition.text = "sleet";
            }
            else if (cnt == 13)
            {
                MenuController.menuController.weather.conditionID = 701;
                weather_condition.text = "mist";
            }
            else if (cnt == 14)
            {
                MenuController.menuController.weather.conditionID = 721;
                weather_condition.text = "haze";
            }
            else if (cnt == 15)
            {
                MenuController.menuController.weather.conditionID = 741;
                weather_condition.text = "fog";
            }
            else if (cnt == 16)
            {
                MenuController.menuController.weather.conditionID = 761;
                weather_condition.text = "dust";
            }

            cnt++;
        
            if (cnt > 16)
                cnt = 0;
            WindUnitConvert();
            weather_windSpeed.text = "Wind: "+ windSpeed.ToString("F2") ;
            WeatherSystem();
            }

    }
    #endregion weather_toggle_unused

    #endregion weatherbtns

    /// <summary>
    /// Updates UI, Calls WeatherSystem()
    /// </summary>
    /// <param name="dont_run_weathersystem">if true only update UI</param>
    public void ReceiveWeatherData(bool dont_run_weathersystem = false)
    {
        //Debug.Log("receviedWeather");
        prevWeather = currWeather;
        currWeather = MenuController.menuController.weather.conditionID;
    
        weather_cityState.text = MenuController.menuController.weather.cityName + ", " + MenuController.menuController.weather.countryName;
        sunrise_time.text = UnixTimeStampToDateTime(MenuController.menuController.weather.sunrise).ToString("hh:mm tt");
        sunset_time.text = UnixTimeStampToDateTime(MenuController.menuController.weather.sunset).ToString("hh:mm tt");
        if (MenuController.menuController.userSettings.isMetric == true) //userwants
        {
            if (MenuController.menuController.weather.weatherUnit == "metric") // retrived & stored data
            {
                weather_temperature.text = MenuController.menuController.weather.temp.ToString("N0") + "°" + "C";
                weather_windSpeed.text = "Wind: " + MenuController.menuController.weather.windSpeed.ToString("N0") + "m/s"; 
            }
            else if(MenuController.menuController.weather.weatherUnit == "imperial")
            {
                weather_temperature.text = (((MenuController.menuController.weather.temp - 32f) * 5f) / 9f).ToString("N0") +"°" + "C";  // F->C
                weather_windSpeed.text = "Wind: " + (MenuController.menuController.weather.windSpeed/2.237f).ToString("N0") + "m/s"; // mi/h ->m/s
            }
        }
        else
        {
            if (MenuController.menuController.weather.weatherUnit == "imperial")
            {
                weather_temperature.text = MenuController.menuController.weather.temp.ToString("N0") + "°" + "F";
                weather_windSpeed.text = "Wind: " + MenuController.menuController.weather.windSpeed.ToString("N0") + "mi/h";
            }
            else if( MenuController.menuController.weather.weatherUnit =="metric")
            {
                weather_temperature.text = ( ((MenuController.menuController.weather.temp * 9f) / 5f) + 32f ).ToString("N0") + "°" + "F"; // C->F
                weather_windSpeed.text = "Wind: " + (MenuController.menuController.weather.windSpeed* 2.237f).ToString("N0") + "mi/h";
            }
        }

        weather_condition.text = MenuController.menuController.weather.conditionName;

        if (dont_run_weathersystem == false)
        {
            WindUnitConvert(); // *->km/hr,internal use
            WeatherSystem();   //update weather effect
        }
    }

    void SnowStart()
    {
     //   snowParticle1.Play();
      //  snowFog.Play();
    }
    void SnowStop()
    {
        //snowParticle1.Stop();
        //snowFog.Stop();
    }

    #region unused_lightning
    Color tmpColor;
    IEnumerator Lightning()
    {
        //Debug.Log("lightning_a on");
        while (true)
        {
            if (true)
            {
                yield return new WaitForSeconds(2f); // delay before starting 
                tmpColor = wallpaper.color;

                yield return StartCoroutine(ColorTransition(wallpaper.color, lightning,0.2f));
                isLightning = true;
                //yield return new WaitForSeconds(0.01f);

                //wallpaper.color = tmpColor;
                yield return StartCoroutine(ColorTransition(wallpaper.color,tmpColor, 0.1f));
                isLightning = false;

                yield return new WaitForSeconds(Random.Range(3f, 7f));
            }
            yield return null;
        }

    }

    IEnumerator Lightning_b(int freq = 5, float range_min = 4f, float range_max = 8f)
    {
        isLightning = true;
        //lightningParticle.Play();
        //Debug.Log("lightning_b on");
        int i = freq;
        yield return new WaitForSeconds(2f); // delay before starting 
        while (i > 0)
        {
            //skip immediately
            if (transition_pending == true || turn_on_lightning == false)
            {
                //Debug.Log("break1");
                break;
            }

            tmpColor = wallpaper.color;

            yield return StartCoroutine(ColorTransition(wallpaper.color, lightning, 0.2f));

            yield return StartCoroutine(ColorTransition(wallpaper.color, tmpColor, 0.1f));
            
            //skip if color change pending, before waiting.
            if (transition_pending == true || turn_on_lightning == false)
            {
                //Debug.Log("break2");
                break;
            }

            yield return new WaitForSeconds(Random.Range(range_min, range_max));

            //skip if color change, before lightning playing again.
            if (transition_pending == true || turn_on_lightning == false)
            {
                //Debug.Log("break3");
                break;
            }
            i--;

        }
        //Debug.Log("lightningparticlestopped");
        //lightningParticle.Stop();
        isLightning = false;
        //yield return null;
    }
    #endregion

    bool transition_progressing;
    IEnumerator ColorTransition(Color a, Color b, float incr = 0.1f, bool is_this_pending_transition = false)  // a->b
    {
        WaitForSeconds countTime = new WaitForSeconds(0.05f);
        //Debug.Log("color transition");
        transition_progressing = true;
        float t = 0;
        while (t <= 1)
        {
            //yield return new WaitForSeconds(0.05f);
            yield return countTime;
            t += incr;
            wallpaper.color = Color.Lerp(a, b, t);

            //testing the waters
            wallpaperMat.SetVector("_OvColor", Color.Lerp(a, b, t));
        }
        if (is_this_pending_transition == true)
            transition_pending = false;

        //Debug.Log("color transition end");
        transition_progressing = false;
        yield return null;
    }

    IEnumerator incrByDelta(float delta)
    {
        WaitForSeconds countTime = new WaitForSeconds(delta);
        while (true)
        {
            yield return countTime;
            startHr++;
            if (startHr == 24)
                startHr = 0;
        }

    }

    /// <summary>
    /// UTS -> Local System Time.
    /// </summary>
    /// <param name="unixTimeStamp">UTS time.</param>
    /// <returns>Local System DateTime.</returns>
    public DateTime UnixTimeStampToDateTime(long unixTimeStamp) 
    {
        // Unix timestamp is seconds past epoch
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    DateTime tmp1;
    /// <summary>
    /// Checks if UTS & DateTime are equal (year, month & day only).
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <param name="uts">UTS Time</param>
    /// <returns>true if equal</returns>
    bool AreDatesEqual(long uts, DateTime d)
    {
       tmp1 = UnixTimeStampToDateTime(uts);

        if (d.Year == tmp1.Year)
        {
            if (d.Month == tmp1.Month)
            {
                if (d.Day == tmp1.Day)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Timespan from lastrun weatherquery time(system local) to current time.
    /// </summary>
    double TimeSpanCalculate()
    {
        System.TimeSpan diff = System.DateTime.Now.Subtract(MenuController.menuController.weather.last_run_date); //- giving minus value if time changed backwards or initial run
        return diff.TotalMinutes;
    }

    /// <summary>
    /// Weatherquery logic. Run every approx =>10 minutes.
    /// </summary>
    IEnumerator WeatherUpdate()
    {
        int i = 0, tmp = 0;
        is_save_file_loaded = MenuController.menuController.Load_Weather(); 
        if (MenuController.menuController.tmpCityName != MenuController.menuController.userSettings.cityName) //if userinput cityname changed, query weather.
        {
            MenuController.menuController.tmpCityName = MenuController.menuController.userSettings.cityName;
            ignore_checks = true;
        }
        randNo = Random.Range(0, 10);
        while (true)
        {  
            if (string.IsNullOrEmpty(MenuController.menuController.userSettings.cityName) != true)
            {
                // if last checked weather query time greater than 40minutes, recheck. Else use saved weather.dat data.
                if (ignore_checks == true || TimeSpanCalculate() >= 40f || TimeSpanCalculate() < 0 || is_save_file_loaded == false) 
                {

                    if (ignore_checks == false)
                    {
                        // Debug.Log("Error: Weather Data too old, reloading.");
                    }
                    i = 5;
                    while (i > 0)
                    {
                        i--;
                        yield return StartCoroutine(SendRequest());

                        if (sendRequestError == null || sendRequestError == "") //no http error
                        {
                            break;
                        }
                        else //http error occured
                        {
                            tmp = httpErrorCheck_retryTime(sendRequestError);
                            if (tmp > 0) //returns delay before retry, -ve if no retry
                            {
                                if (tmp == 30)
                                {
                                    //no internet error, keep retrying forever.
                                    i = 1;
                                    yield return new WaitForSeconds(tmp);
                                }
                                else //5xx server error, stop after 'i' <0, then retry after 40
                                {
                                    yield return new WaitForSeconds(tmp);
                                }
                            }
                            else //4xx error, incorrect userinput..exit loop
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //Debug.Log("Warning: Recent Weather data, not reloading");
                    call_weather_function = true;
                }
                if(call_weather_function == true)
                    ReceiveWeatherData();

                call_weather_function = false;
            }
            else
            {
                Debug.Log("Error: cityname null or empty");
            }            
            //10min, system time, will not always get called 10min since application is being paused.
            yield return new WaitForSecondsRealtime(600); 
        }   
    }

    //..weather query errorcode.
    string sendRequestError;
    /// <summary>
    /// openweathermap.org weatherquery.
    /// </summary>
    IEnumerator SendRequest()
    {
        sendRequestError = null;

        if (Is_Cityname_Input_Safe() == false) //unsafe user inputs in cityname such as !,; etc
        {
            main.instance.tray.ShowNotification(1000, "Error", " Save file seems corrupted, try deleted the folder in Saved Games");
            ignore_checks = false; //user needs to change the text at this point, scene restart doest matter.
            yield break; //exit coroutine.
        }

        if (Is_apikey_Input_Safe() == false)
        {
            main.instance.tray.ShowNotification(1000, "Error", " Save file seems corrupted, try deleted the folder in Saved Games");
            ignore_checks = false;
            yield break;
        }

        //retrieved from weather API
        string retrievedCountry;
        string retrievedCity;
        int conditionID;
        string conditionName;
        string conditionImage;
        float tempTemp;
        float windSpeed;
        long sunrise, sunset;

        //get the current weather
        WWW request;

        if (MenuController.menuController.userSettings.isMetric == true)
        {
            if (MenuController.menuController.userSettings.apiKey == "default" || MenuController.menuController.userSettings.apiKey == "Default")
                request = new WWW("https://api.openweathermap.org/data/2.5/weather?q=" + MenuController.menuController.userSettings.cityName + "&units=metric&appid=" + MenuController.menuController.apiKeyDefault); //32f9f5c44ab2cc69215f08625cde78cb"); 
            else
            {
                //verify sanitize input first
                request = new WWW("https://api.openweathermap.org/data/2.5/weather?q=" + MenuController.menuController.userSettings.cityName + "&units=metric&appid=" + MenuController.menuController.userSettings.apiKey);
            }
        }
        else
        {
            if (MenuController.menuController.userSettings.apiKey == "default" || MenuController.menuController.userSettings.apiKey == "Default")
            {
                request = new WWW("https://api.openweathermap.org/data/2.5/weather?q=" + MenuController.menuController.userSettings.cityName + "&units=imperial&appid=" + MenuController.menuController.apiKeyDefault); //32f9f5c44ab2cc69215f08625cde78cb");
            }
            else
            {
                request = new WWW("https://api.openweathermap.org/data/2.5/weather?q=" + MenuController.menuController.userSettings.cityName + "&units=imperial&appid=" + MenuController.menuController.userSettings.apiKey);
            }
        }

        yield return request;
        MenuController.menuController.weatherPollCnt++; // debugging
        sendRequestError = request.error;
        //Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        
        if (request.error == null || request.error == "")
        {
            var N = JSON.Parse(request.text);

            retrievedCountry = N["sys"]["country"].Value; //get the country
            retrievedCity = N["name"].Value; //get the city

            string temp = N["main"]["temp"].Value; //get the temperature 

            float.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture , out tempTemp); //parse the temperature, culture invariant to avoid errors in non english systems.
        
            float.TryParse(N["wind"]["speed"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out windSpeed); //parse the windspeed, culture invariant to avoid errors in non english systems.
            int.TryParse(N["weather"][0]["id"].Value, out conditionID); //get the current condition ID
            //conditionName = N["weather"][0]["main"].Value; //get the current condition Name
            conditionName = N["weather"][0]["description"].Value; //get the current condition Description
            conditionImage = N["weather"][0]["icon"].Value; //get the current condition Image

            string cod = N["cod"].Value;
            //Debug.Log("Cod: " +cod); // 404 not found, request error happens prob not needed

            long.TryParse(N["sys"]["sunrise"].Value, out sunrise);
            long.TryParse(N["sys"]["sunset"].Value, out sunset);

            //taking weather update-date backup, for comparison
            prevDataDate = currDataDate;
            currDataDate = N["dt"].Value;

            if (currDataDate != prevDataDate || ignore_checks == true) // dont change anything if weather data date is same.
            {
                Debug.Log("saving weather data, " + "currDate: " + currDataDate + "prevDate: " + prevDataDate);
                //.. saving weater data to file
                MenuController.menuController.weather.windSpeed = windSpeed;
                MenuController.menuController.weather.countryName = retrievedCountry;
                MenuController.menuController.weather.cityName = retrievedCity;
                MenuController.menuController.weather.temp = tempTemp;
                MenuController.menuController.weather.conditionID = conditionID;
                MenuController.menuController.weather.conditionName = conditionName;
                MenuController.menuController.weather.last_run_date = System.DateTime.Now;
                MenuController.menuController.weather.sunrise = sunrise;
                MenuController.menuController.weather.sunset = sunset;

                if (MenuController.menuController.userSettings.isMetric == true) // !user changes, but retrived data not changed problems?
                {
                    MenuController.menuController.weather.weatherUnit = "metric";
                }
                else
                {
                    MenuController.menuController.weather.weatherUnit = "imperial";
                }

                MenuController.menuController.Save_Weather();
                //..call receivefucntion in cyclescript
                call_weather_function = true;
                //is_save_file_loaded = true;
            }
            else
            {
                Debug.Log("Warning: Same Weather Date, Skipping File Save");
            }
        }
        else
        {
            Debug.Log("WWW Error: " + request.error);

        }
        ignore_checks = false;

        #region weatherImg_unused
        /*
        //get our weather image
        WWW conditionRequest = new WWW("http://openweathermap.org/img/w/" + conditionImage + ".png");
        yield return conditionRequest;

        if (conditionRequest.error == null || conditionRequest.error == "")
        {
            //create the material, put in the downloaded texture and make it visible
            var texture = conditionRequest.texture;
            Shader shader = Shader.Find("Unlit/Transparent Colored");
            if (shader != null)
            {
                var material = new Material(shader);
                material.mainTexture = texture;
                myWeatherCondition.material = material;
                myWeatherCondition.color = Color.white;
                myWeatherCondition.MakePixelPerfect();
            }
        }
        else
        {
            Debug.Log("WWW error: " + conditionRequest.error);
        }
        */
        #endregion

        request.Dispose();
    }

    /// <summary>
    /// Error handling, retry time for different http errors.
    /// </summary>
    /// <returns>wait time (in seconds) before retry, -1 if stop retry</returns>
    /// <param name="error">http request error code</param>
    int httpErrorCheck_retryTime(string error)
    {
        //...Unity WWW class has no way to get the int value of error code :(
        if (error.Contains("404")) //location not found error
        {
            MenuController.menuController.weather.last_run_date = new System.DateTime(2000, 1, 1); //put an old date, to force retry for new city..otherwise loads old data without verify new city
            MenuController.menuController.Save_Weather();
            weather_condition.text = "Location Not Found";
            return -1;
        }
        else if (error.Contains("400") || error.Contains("401") || error.Contains("402")
                || error.Contains("403") || error.Contains("405") || error.Contains("406") || error.Contains("407") || error.Contains("408")
                || error.Contains("409") || error.Contains("410") || error.Contains("411") || error.Contains("412") || error.Contains("413")
                || error.Contains("414") || error.Contains("415") || error.Contains("416") || error.Contains("417")
                ) //bad request to server
        {
            MenuController.menuController.weather.last_run_date = new System.DateTime(2000, 1, 1); //put an old date, to force retry for new city..otherwise loads old data without verify new city
            MenuController.menuController.Save_Weather();

            weather_condition.text = "Error 4xx";
            return -1; //quit
        }
        else if (error.Contains("500") || error.Contains("501") || error.Contains("502") || error.Contains("503")
            || error.Contains("504") || error.Contains("505"))
        {
            MenuController.menuController.weather.last_run_date = new System.DateTime(2000, 1, 1); //put an old date, to force retry for new city..otherwise loads old data without verify new city
            MenuController.menuController.Save_Weather();

            weather_condition.text = "Server Error, retrying..";
            return 240; //seconds
        }
        else
        {
            MenuController.menuController.weather.last_run_date = new System.DateTime(2000, 1, 1); //put an old date, to force retry for new city..otherwise loads old data without verify new city
            MenuController.menuController.Save_Weather();

            weather_condition.text = "No Internet, retrying..";
            return 30;
        }
    }

    /// <summary>
    /// Checking for unsafe characters in cityname input.
    /// </summary>
    /// <returns>true if safe.</returns>
    bool Is_Cityname_Input_Safe()
    {
        if(MenuController.menuController.userSettings.cityName != null)
        {
            foreach (var item in MenuController.menuController.userSettings.cityName)
            {
                if( char.IsSeparator(item) || char.IsLetter(item) || (item == ',') || char.IsNumber(item)) //safe characters
                {

                }
                else
                {
                    return false; //unsafe
                }
            }
        }
        return true; //safe
    }

    /// <summary>
    /// Checking for unsafe characters in apikey input.
    /// </summary>
    /// <returns>true if safe.</returns>
    bool Is_apikey_Input_Safe()
    {
        if (MenuController.menuController.userSettings.apiKey != null)
        {
            foreach (var item in MenuController.menuController.userSettings.apiKey)
            {
                if (char.IsSeparator(item) || char.IsLetter(item) || (item == ',') || char.IsNumber(item) || (item == '_') || (item == '-')) //safe characters
                {

                }
                else
                {
                    return false; //unsafe
                }
            }
        }
        return true; //safe
    }

    #region overlay_color_functions
    System.TimeSpan diff_sunset, diff_sunrise, diff_sunset_sunrise;
    float lerpDelta;
    /// <summary>
    /// Overlay color based on sunrise-sunset time. Fixed brightness at night.
    /// </summary>
    void OverlaySunriseSunset() 
    {
        diff_sunset = UnixTimeStampToDateTime(MenuController.menuController.weather.sunset).Subtract(DateTime.Now); // -ve if reverse
        //Debug.Log("sunsut-now diff: " + diff_sunset.TotalMinutes);
        diff_sunrise = DateTime.Now.Subtract(UnixTimeStampToDateTime(MenuController.menuController.weather.sunrise)); // -ve if reverse
        //Debug.Log("sunrise-now diff: " + diff_sunrise.TotalMinutes);
        diff_sunset_sunrise = UnixTimeStampToDateTime(MenuController.menuController.weather.sunset).Subtract(UnixTimeStampToDateTime(MenuController.menuController.weather.sunrise)); // -ve if reverse
        //Debug.Log("sunset-sunrise-now diff: " + diff_sunset_sunrise.TotalMinutes);

        if (diff_sunset_sunrise.TotalMinutes != 0)
            lerpDelta = (float)(1d - (diff_sunset.TotalMinutes / diff_sunset_sunrise.TotalMinutes));
        else
            lerpDelta = 1f;

        //  current - sunrise             && sunset - current
        if (diff_sunrise.TotalMinutes > 0 && diff_sunset.TotalMinutes > 0) // sunrise -> sunset
        {
            if (lerpDelta <= .2d ) //20% - close to sunrise?
            {
                ambientLight = Color.Lerp(cycle[3], cycle[0], lerpDelta / 0.2f);
            }
            else if (lerpDelta > .2d && lerpDelta <= 0.4d ) 
            {
                ambientLight = Color.Lerp(cycle[0], cycle[7], (lerpDelta - 0.2f) / 0.2f);
            }
            else if (lerpDelta > .4d && lerpDelta <= 0.7d) 
            {
                ambientLight = Color.Lerp(cycle[7], cycle[4], (lerpDelta - 0.5f) / 0.3f);
            }
            else if(lerpDelta > .7d && lerpDelta <= 0.9d)
            {
                ambientLight = Color.Lerp(cycle[4], cycle[1], (lerpDelta - 0.7f) / 0.2f);
            }
            else
            {
                ambientLight = Color.Lerp(cycle[1], cycle[6], (lerpDelta - 0.9f) / 0.1f);
            }
        }
        else //sunset -> sunrise
        {
            if (Mathf.Abs((float)diff_sunrise.TotalMinutes) <= 60) //60 minute till sunrise?
            {
                ambientLight = Color.Lerp(cycle[6], cycle[3], Mathf.Abs((float)diff_sunrise.TotalMinutes)/60f); //night ->3
            }
            else
                ambientLight = cycle[6];
        }
    }

    /// <summary>
    /// Overlay color based on 6am-6pm (default). Brightness linearly varies at night.
    /// </summary>
    void OverlayDefault() //color overlay fixed
    {
        if (firstRun == true)
            startMinute = System.DateTime.Now.Minute;
        startHr = System.DateTime.Now.Hour;

        //...time scaling calculations, in 24hr format
        if (startHr > 12)//System.DateTime.Now.Hour > 12)
        {
            diff = Mathf.Abs(startHr - Noon.Hour);
        }
        else
        {
            diff = Mathf.Abs(startHr - MidNight.Hour);
        }
        diff += startMinute / 60.0f;
        
        if (startHr > Noon.Hour) //afternoon, 0 - 12 
        {
            if(diff <= 5) // 12 pm -5pm
            {
                diff = diff / 5f;
                ambientLight = Color.Lerp(cycle[7], cycle[4], diff);  //day to evnin,
               // ambientLight = cycle[4];
            }
            else if (diff > 5 && diff <= 7) // 5pm - 7pm
            {
                diff = diff - 5f; //subtracing the previous session
                diff = diff / 2f; //duration
                ambientLight = Color.Lerp(cycle[4], cycle[1], diff);  //sunset
                //ambientLight = cycle[1];
            }
            else if(diff > 7 && diff <= 8) // 7pm - 8pm
            {
                diff = diff - 7f;
                diff = diff / 1f;
                ambientLight = Color.Lerp(cycle[1], cycle[2], diff); //sunset to night
                //ambientLight = cycle[2];
            }
            else // 8pm - 12am
            {
                diff = diff - 8f;
                diff = diff / 4f;
                ambientLight = Color.Lerp(cycle[2], cycle[6], diff); //night to midnight
                //ambientLight = cycle[6];
            }
        }
        else   //midnight
        {
            if(diff <= 5) //12am - 5am
            {
                diff = diff / 5f;
                ambientLight = Color.Lerp(cycle[6], cycle[5], diff); //orig 2->5
                //ambientLight = cycle[5];
            }
            else if (diff > 5 && diff <= 7) //5-7am
            {
                diff = diff - 5f;
                diff = diff / 2f;
                ambientLight = Color.Lerp(cycle[5], cycle[3], diff);
                //ambientLight = cycle[3];
            }
            else if (diff > 7 && diff <= 8) // 7-8am
            {
                diff = diff - 7f;
                diff = diff / 1f;
                ambientLight = Color.Lerp(cycle[3], cycle[0], diff);
                //ambientLight = cycle[0];
            }
            else
            {
                diff = diff - 8f;
                diff = diff / 4f;
                ambientLight = Color.Lerp(cycle[0], cycle[7], diff);
                //ambientLight = cycle[7];
            }
        }      
    }

    #endregion overlay_color_functions

    bool firstRun2 = true;
    float dim_percent_b = 1;
    bool firstRun = false;
    //..run every frame.
	void Update () {

        #region coroutines
        if ( transition_pending == true && transition_progressing == false && isLightning == false )
        {
           // Debug.Log(MenuController.menuController.userSettings.dayNightTint);
            if(MenuController.menuController.userSettings.dayNightTint == true)
                StartCoroutine( (ColorTransition(wallpaper.color, ambientLight, 0.1f, true) ) );
        }
        else if( turn_on_lightning == true && isLightning == false)
        {
            cr2 = StartCoroutine(Lightning_b());
        }
        #endregion coroutines

        #region color-cycle_call
        if ( System.Math.Abs(startMinute - System.DateTime.Now.Minute) >= 5 || firstRun == false || System.Math.Abs(startHr - System.DateTime.Now.Hour) >= 1  
                                                         || dim_percent_b != dim_percent) //hour is checked to verify if user changes the time.
        {
            /*
            #region memory_test
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            #endregion memory_test
            */
            if (MenuController.menuController.userSettings.sun_overlay == true && MenuController.menuController.userSettings.isDemo == false
                                                        && AreDatesEqual(MenuController.menuController.weather.sunrise, DateTime.Now) == true) //if control weather(isDemo) is on also!
                OverlaySunriseSunset(); //based on sunset-sunrise, with fixed night brightness
            else
                OverlayDefault();  // 6am-6pm, with varying night brightness

            dim_percent_b = dim_percent;
            ambientLight.a = ambientLight.a * dim_percent;
            transition_pending = true;
            firstRun = true;
        }
        #endregion color-cycle_call

    }

}
