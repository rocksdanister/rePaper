using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UnityEngine;

/// <summary>
/// Systemtray menu & actions
/// </summary>
public class main : MonoBehaviour {

    public SystemTray tray;

    public static main instance = null;
    void Awake()
    {
        //singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
    }

    //..menuitems variables
    MenuItem[] weathers = new MenuItem[22];
    MenuItem displaySetup;
    public MenuItem startup;
    public MenuItem video;
    public MenuItem update;
    MenuItem gear_clock, circle_clock, simple_clock;
    MenuItem auto_ui_color, manual_ui_color;

    /// <summary>
    /// Switch trayicon between pause/unpause
    /// </summary>
    /// <param name="isFullscreen">is fullscreen application running</param>
    public void ChangeTrayIcon(bool isFullscreen)
    {
        if (tray != null)
        {
            if (isFullscreen)
            {
                if (tray.trayIcon.Icon != tray.ico_pause)
                    tray.trayIcon.Icon = tray.ico_pause;
            }
            else
            {
                if (tray.trayIcon.Icon != tray.ico_run)
                    tray.trayIcon.Icon = tray.ico_run;
            }
        }
    }

    /// <summary>
    /// initialize traymenu, called after menucontroller script initilization.  Running on Main Unity Thread x_x..
    /// </summary>
    public void Start_() {

        tray = CreateSystemTrayIcon();
        if (tray != null)
        {
            tray.SetTitle("rePaper");
            MenuItem weather = new MenuItem("Weather");

            weathers[0] = tray.trayMenu.MenuItems.Add("Clear", new EventHandler(Weather_Btn));
            weathers[1] = tray.trayMenu.MenuItems.Add("Thunder & Heavy Rain", new EventHandler(Weather_Btn));
            weathers[2] = tray.trayMenu.MenuItems.Add("Ragged Thunder", new EventHandler(Weather_Btn));
            weathers[3] = tray.trayMenu.MenuItems.Add("Drizzle Thunder", new EventHandler(Weather_Btn));
            weathers[4] = tray.trayMenu.MenuItems.Add("Thunderstorm", new EventHandler(Weather_Btn));
            weathers[5] = tray.trayMenu.MenuItems.Add("Light Drizzle", new EventHandler(Weather_Btn));
            weathers[6] = tray.trayMenu.MenuItems.Add("Drizzle", new EventHandler(Weather_Btn));
            weathers[7] = tray.trayMenu.MenuItems.Add("Light Rain", new EventHandler(Weather_Btn));
            weathers[8] = tray.trayMenu.MenuItems.Add("Rain", new EventHandler(Weather_Btn));
            weathers[9] = tray.trayMenu.MenuItems.Add("Heavy Rain", new EventHandler(Weather_Btn));
            weathers[10] = tray.trayMenu.MenuItems.Add("User Rain - 1", new EventHandler(Weather_Btn));
            weathers[11] = tray.trayMenu.MenuItems.Add("User Rain - 2", new EventHandler(Weather_Btn));
            weathers[12] = tray.trayMenu.MenuItems.Add("Light Snow", new EventHandler(Weather_Btn));
            weathers[13] = tray.trayMenu.MenuItems.Add("Snow", new EventHandler(Weather_Btn));
            weathers[14] = tray.trayMenu.MenuItems.Add("Heavy Snow", new EventHandler(Weather_Btn));
            weathers[15] = tray.trayMenu.MenuItems.Add("Sleet", new EventHandler(Weather_Btn));
            weathers[16] = tray.trayMenu.MenuItems.Add("Mist", new EventHandler(Weather_Btn));
            weathers[17] = tray.trayMenu.MenuItems.Add("Haze", new EventHandler(Weather_Btn));
            weathers[18] = tray.trayMenu.MenuItems.Add("Fog", new EventHandler(Weather_Btn));
            weathers[19] = tray.trayMenu.MenuItems.Add("Dust", new EventHandler(Weather_Btn));
            weathers[20] = tray.trayMenu.MenuItems.Add("User Atmosphere - 1", new EventHandler(Weather_Btn));
            weathers[21] = tray.trayMenu.MenuItems.Add("User Atmosphere - 2", new EventHandler(Weather_Btn));

            weather.MenuItems.AddRange(weathers);
            tray.trayMenu.MenuItems.Add(weather);
            tray.trayMenu.MenuItems.Add("-");

            MenuItem clockType = new MenuItem("Clock Style");
            gear_clock = tray.trayMenu.MenuItems.Add("Gear", new EventHandler(Clock_Btn));
            circle_clock = tray.trayMenu.MenuItems.Add("Circle", new EventHandler(Clock_Btn));
            simple_clock = tray.trayMenu.MenuItems.Add("Simple", new EventHandler(Clock_Btn));
            clockType.MenuItems.Add(gear_clock);
            clockType.MenuItems.Add(circle_clock);
            clockType.MenuItems.Add(simple_clock);
            tray.trayMenu.MenuItems.Add(clockType);

            MenuItem uiColor = new MenuItem("UI Color");
            auto_ui_color = tray.trayMenu.MenuItems.Add("Auto", new EventHandler(UI_Btn));
            manual_ui_color = tray.trayMenu.MenuItems.Add("Pick One", new EventHandler(UI_Btn));
            uiColor.MenuItems.Add(auto_ui_color);
            uiColor.MenuItems.Add(manual_ui_color);
            tray.trayMenu.MenuItems.Add(uiColor);

            tray.trayMenu.MenuItems.Add("-");

            video = new MenuItem("Choose Wallpaper", new EventHandler(Video_Event));
            tray.trayMenu.MenuItems.Add(video);
            tray.trayMenu.MenuItems.Add("-");

            //trying stuff
            displaySetup = new MenuItem("Select Display");
            int i = 0;
            //displaySetup.MenuItems.Add("Span", new EventHandler(UserDisplayMenu));
            foreach (var item in System.Windows.Forms.Screen.AllScreens)
            {
                displaySetup.MenuItems.Add("Display " + i.ToString(), new EventHandler(UserDisplayMenu));
                i++;
            }

            tray.trayMenu.MenuItems.Add(displaySetup);
            if (MenuController.menuController.isMultiMonitor == false)
                displaySetup.Enabled = false;

            /*
            MenuItem displaySetup = new MenuItem("Multimonitor", new EventHandler(DisplayBtn));
            tray.trayMenu.MenuItems.Add(displaySetup);
            if (MenuController.menuController.isMultiMonitor == false)
                displaySetup.Enabled = false;
            */

            startup = new MenuItem("Run at Startup", new EventHandler(System_Startup_Btn));
            tray.trayMenu.MenuItems.Add(startup);
            tray.trayMenu.MenuItems.Add("-");

            MenuItem website = new MenuItem("Project Website", new EventHandler(WebpageBtn));
            tray.trayMenu.MenuItems.Add(website);
            
            MenuItem kofi = new MenuItem("Buy the Dev Coffee!", new EventHandler(KofiBtn));
            tray.trayMenu.MenuItems.Add(kofi);

            tray.trayMenu.MenuItems.Add("-");
            update = new MenuItem("Checking for update", new EventHandler(Update_Check));
            tray.trayMenu.MenuItems.Add(update);
            update.Enabled = false;

            MenuItem settings = new MenuItem("Settings", new EventHandler(Settings_Launcher));
            tray.trayMenu.MenuItems.Add(settings);
            tray.trayMenu.MenuItems.Add("-");

            MenuItem close = new MenuItem("Exit", new EventHandler(Close_Action));
            tray.trayMenu.MenuItems.Add(close);

            tray.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(Settings_Launcher_Mouse);

            tray.ShowNotification(1000, "Hello..", "I'll just stay in systemtray, right click for more option...");

            if (MenuController.menuController.userSettings.isDemo == true) //weather control
            {
                WeatherMenuEnable(true);
            }
            else
            {
                WeatherMenuEnable(false);
            }

            SetStartup(MenuController.menuController.userSettings.runAtStartup); //set startup entry

            // startup entry, checkmark btn
            if (MenuController.menuController.userSettings.runAtStartup == true)
            {
                SetStartup(true); //re adding incase filepath changes etc
                startup.Checked = true;
            }
            else
            {
                SetStartup(false);
                startup.Checked = false;
            }

            WeatherBtnCheckMark();
            ClockCheckMark();
            ColorCheckMark();
            //CheckStartupRegistry();
            //Events not working... unity intercepting them!
            //SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnPowerChange);
            //SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

        }
    }

    #region multimoniotr_menu

    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
    {
        UpdateTrayMenuDisplay();
        MoveToDisplay(0);
    }

    //future use.
    void UpdateTrayMenuDisplay()
    {
        displaySetup.MenuItems.Clear();
        System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
        int i = 0;
        //displaySetup.MenuItems.Add("Span", new EventHandler(UserDisplayMenu));
        foreach (var item in screens)
        {
            displaySetup.MenuItems.Add("Display " + i.ToString(), new EventHandler(UserDisplayMenu));
            i++;
        }

        if (screens.Length > 1)
            displaySetup.Enabled = true;
        else
            displaySetup.Enabled = false;
    }

    private void UserDisplayMenu(object sender, EventArgs e)
    {
        int i = 0; 
        string s = (sender as MenuItem).Text;
        /*
        if (s == "Span")
        {
            MoveToDisplay(-1);
            return;
        }
        */

        foreach (var item in System.Windows.Forms.Screen.AllScreens)
        {
            if (s == "Display " + i.ToString())
            {
                MoveToDisplay(i);
                MenuController.menuController.userSettings.ivar2 = i;
                MenuController.menuController.Save();
                break;
            }
            i++;
        }
    }

    void MoveToDisplay(int i)
    {
        var obj = GameObject.Find("SetupDesktop").GetComponent<Headless>();
        if(obj != null)
            obj.MoveToDisplay(i);
    }

    #endregion
    //unity might be intercepting the messages or windows fked up, todo:- have to find a solution
    #region power_suspend_resume_UNUSED
    void OnPowerChange(System.Object sender, PowerModeChangedEventArgs e)
        {
        Debug.Log("POWER CHANGE");
            switch (e.Mode)
            {
                case PowerModes.Resume:
                Debug.Log("Resume");
                MenuController.menuController.weatherPollCnt += 10;
                break;
                case PowerModes.Suspend:
                Debug.Log("suspend");
                MenuController.menuController.weatherPollCnt += 5;
                break;
                
        }
    }
    #endregion power_suspend_resume

    /// <summary>
    /// Update traymenu color submenu checkmark
    /// </summary>
    public void ColorCheckMark()
    {
        if (UnityEngine.Application.isEditor == false)
        {
            //if not video playing
            if (MenuController.menuController.userSettings.vidPath == null) 
            {
                auto_ui_color.Checked = false;
                manual_ui_color.Checked = false;
                if (auto_ui_color.Enabled == false)
                    auto_ui_color.Enabled = true;

                if (MenuController.menuController.userSettings.autoUiColor == true)
                    auto_ui_color.Checked = true;
                else
                    manual_ui_color.Checked = true;
            }
            else //video playback, only manual color mode.
            {
                if (auto_ui_color.Enabled == true)
                    auto_ui_color.Enabled = false;

                auto_ui_color.Checked = false;
                manual_ui_color.Checked = true;
            }
        }

    }

    /// <summary>
    /// traymenu color picker submenu action
    /// </summary>
    private void UI_Btn(System.Object sender, System.EventArgs e)
    {
        string s = (sender as MenuItem).Text;
        var obj = GameObject.Find("Main Camera").GetComponent<Controller>();    
        if (s == "Auto")
        {
            if (MenuController.menuController.userSettings.vidPath == null)
            {
                if (obj != null)
                {
                    MenuController.menuController.userSettings.autoUiColor = true;
                    MenuController.menuController.Save();
                    obj.UIColorAuto(); 
                }
                else
                    Debug.Log("Controller script not found in main.cs");
            }
        }
        else //custom
        {
            if (obj != null)
                obj.RunColorDialog(); //closes filedialog and configwindow if open
            else
                Debug.Log("Controller script not found in main.cs");
        }
        ColorCheckMark();
    }

    /// <summary>
    /// Update traymenu clocks submenu checkmark
    /// </summary>
    public void ClockCheckMark()
    {
        gear_clock.Checked = false;
        circle_clock.Checked = false;
        simple_clock.Checked = false;

        if (MenuController.menuController.userSettings.clockType == 0)
            gear_clock.Checked = true;
        else if (MenuController.menuController.userSettings.clockType == 1)
            circle_clock.Checked = true;
        else if (MenuController.menuController.userSettings.clockType == 2)
            simple_clock.Checked = true;
    }

    /// <summary>
    /// Update traymenu weather submenu checkmark
    /// </summary>
    public void WeatherBtnCheckMark()
    {
        try
        {
            foreach (var item in weathers) //button text
            {
                item.Checked = false;

                if (MenuController.menuController.userSettings.userWeather == WeatherNumberToNameConvert(item))
                {
                    item.Checked = true;
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log("Error weathrbtn checkmark" + e.Message);
        }

    }

    /// <summary>
    /// openweathermap weathercode to traymenu text mapping.
    /// </summary>
    /// <param name="var">weather submenu.</param>
    /// <returns>
    /// openweathermap weathercode, returns -100 when none exist.
    /// </returns>
    private int WeatherNumberToNameConvert(MenuItem var)
    {
        if (var.Text == "Clear")
            return 800;
        else if (var.Text == "Thunder & Heavy Rain")
            return 202;
        else if (var.Text == "Ragged Thunder")
            return 221;
        else if (var.Text == "Drizzle Thunder")
            return 230;
        else if (var.Text == "Thunderstorm")
            return 250;
        else if (var.Text == "Light Drizzle")
            return 300;
        else if (var.Text == "Drizzle")
            return 320;
        else if (var.Text == "Light Rain")
            return 500;
        else if (var.Text == "Rain")
            return 550;
        else if (var.Text == "Heavy Rain")
            return 502;
        else if (var.Text == "Light Snow")
            return 600;
        else if (var.Text == "Snow")
            return 650;
        else if (var.Text == "Heavy Snow")
            return 602;
        else if (var.Text == "Sleet")
            return 611;
        else if (var.Text == "Mist")
            return 701;
        else if (var.Text == "Haze")
            return 721;
        else if (var.Text == "Fog")
            return 741;
        else if (var.Text == "Dust")
            return 761;
        else if (var.Text == "User Rain - 1")
            return 901;
        else if (var.Text == "User Rain - 2")
            return 902;
        else if (var.Text == "User Atmosphere - 1")
            return 903;
        else if (var.Text == "User Atmosphere - 2")
            return 904;
        else
            return -100;
    }

    private void WebpageBtn(System.Object sender, System.EventArgs e)
    {
        System.Diagnostics.Process.Start("https://rocksdanister.github.io/rePaper");
    }

    private void KofiBtn(System.Object sender, System.EventArgs e)
    {
        System.Diagnostics.Process.Start("https://ko-fi.com/rocksdanister");
    }

    /// <summary>
    /// Multimonitor display utility launch.
    /// </summary>
    private void DisplayBtn(System.Object sender, System.EventArgs e)
    {

        var obj = GameObject.Find("Main Camera").GetComponent<Controller>();
        if (obj != null)
        {
            obj.DisplaySettings();
        }
        else
            Debug.Log("Controller script not found");
    }

    #region registry_monitor_unused
    /*
    RegistryMonitor monitor;
    void RegistryMonitor()
    {
        Debug.Log("monitoring");
        //monitor = new RegistryMonitor(RegistryHive.CurrentUser, "Environment");
        monitor = new RegistryMonitor(Registry.CurrentUser + "\\Control Panel\\Desktop"); //new RegistryMonitor(RegistryHive.CurrentUser, "Control Panel\\Desktop");
        monitor.RegChanged += new EventHandler(OnRegChanged);
        monitor.Start();

    }
    private void OnRegChanged(object sender, EventArgs e)
    {
        Debug.Log("REGISTRY CHANGED");
    }
    */
    #endregion


    /// <summary>
    /// Check if current system is at night.
    /// </summary>
    /// <returns>
    /// true if time b/w 6pm - 6am, false otherwise.
    /// </returns>
    bool NightTimeCheck()
    {
        var time = DateTime.Now.TimeOfDay;
        var start = new TimeSpan(18, 0, 0);
        var end = new TimeSpan(6, 0, 0);
        // If the start time and the end time is in the same day.
        if (start <= end)
            return time >= start && time <= end;
        // The start time and end time is on different days.
        return time >= start || time <= end;
    }

    /// <summary>
    /// Clock type change traymenu
    /// </summary>
    private void Clock_Btn(System.Object sender, System.EventArgs e)
    {
        var obj = GameObject.Find("Main Camera").GetComponent<Controller>();
        if (obj != null)
        {
            string s = (sender as MenuItem).Text;
            obj.ChangeClockType(s);
        }
        else
            Debug.Log("Controller script not found");

        ClockCheckMark();
    }

    /// <summary>
    /// Update traymenu, launches github page in browser.
    /// </summary>
    private void Update_Check(System.Object sender, System.EventArgs e)
    {
        System.Diagnostics.Process.Start("https://github.com/rocksdanister/rePaper");
    }

    /// <summary>
    /// Calls Filebrowser for wallpape
    /// </summary>
    private void Video_Event(System.Object sender, System.EventArgs e)
    {
        var obj = GameObject.Find("WallpaperQuad").GetComponent<VideoScript>();
        obj.ShowFileDilCaller();
    }

    /// <summary>
    /// Calls Filebrowser on trayicon mouse doubleclick.
    /// </summary>
    private void Video_Event_Mouse(System.Object sender, System.Windows.Forms.MouseEventArgs e)
    {
        var obj = GameObject.Find("WallpaperQuad").GetComponent<VideoScript>();
        obj.ShowFileDilCaller();
    }

    /// <summary>
    /// Creates application shortcut to link to windows startup in registry.
    /// </summary>
    private void CreateShortcut()
    {
        try
        {
            WshShell shell = new WshShell();
            var shortCutLinkFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "\\rePaperStartup.lnk";
            var windowsApplicationShortcut = (IWshShortcut)shell.CreateShortcut(shortCutLinkFilePath);
            windowsApplicationShortcut.Description = "shortcut of rePaper Live Wallpaper";
            windowsApplicationShortcut.WorkingDirectory = System.IO.Directory.GetParent(System.AppDomain.CurrentDomain.BaseDirectory).ToString();
            windowsApplicationShortcut.TargetPath = System.IO.Directory.GetParent(System.AppDomain.CurrentDomain.BaseDirectory).ToString() + "\\Start.exe";
            windowsApplicationShortcut.Save();
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Enable/Disable weather selection traymenu.
    /// </summary>
    /// <param name="val">Enable/Disable traymenu.</param>
    public void WeatherMenuEnable(bool val)
    {
        if(val == false)
        {
            foreach (var item in weathers)
            {
                item.Enabled = false;
            }
        }
        else
        {
            foreach (var item in weathers)
            {
                item.Enabled = true;
            }
        }
    }

    /// <summary>
    /// traymenu run at startup button
    /// </summary>
    private void System_Startup_Btn(System.Object sender, System.EventArgs e)
    {
        // temporary variable
        MenuController.menuController.userSettings.runAtStartup = !MenuController.menuController.userSettings.runAtStartup;
        MenuController.menuController.Save();

        if (MenuController.menuController.userSettings.runAtStartup == true) //btn checkmark
            startup.Checked = true;
        else
            startup.Checked = false;

        SetStartup(MenuController.menuController.userSettings.runAtStartup);
    }

    /// <summary>
    /// traymenu, launch configuration utility.
    /// </summary>
    private void Settings_Launcher(System.Object sender, System.EventArgs e)
    {
        var obj = GameObject.Find("Main Camera").GetComponent<Controller>();
        if (obj != null)
            obj.RunConfigUtility();
        else
            Debug.Log("Controller script not found");
    }

    /// <summary>
    /// traymenu, launch configuration utility.
    /// </summary>
    private void Settings_Launcher_Mouse(System.Object sender, System.Windows.Forms.MouseEventArgs e)
    {
        var obj = GameObject.Find("Main Camera").GetComponent<Controller>();
        if (obj != null)
            obj.RunConfigUtility();
        else
            Debug.Log("Controller script not found");
    }

    /// <summary>
    /// stops dxva playback instance on Unity editor stop, else persists on windows soundmenu
    /// </summary>
    private void OnDisable()
    {
        /*
        if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
        {
            var videoScript = GameObject.Find("WallpaperQuad").GetComponent<VideoScript>();
            if (videoScript != null)
            {
                videoScript.Stop_DXVA(); // should fix the applicaiton name persisting in audio dialog
            }
            else
                Debug.Log("Videoscript not found in main.cs");
        }
        */
    }

    /// <summary>
    /// traymenu - Exit Application.
    /// </summary>
    /// <remarks>
    /// Disposes traymenu, stops dxva playback instance, refreshes desktop by calling setwallpaper, closes all open windows.
    /// </remarks>
    public void Close_Action(System.Object sender, System.EventArgs e)
    {
        //SystemEvents.PowerModeChanged -= OnPowerChange;
        //SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;

        tray.Dispose();
        if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
        {
            var videoScript = GameObject.Find("WallpaperQuad").GetComponent<VideoScript>();
            if (videoScript != null)
            {
                videoScript.Stop_DXVA(); // should fix the applicaiton name persisting in audio dialog
            }
            else
                Debug.Log("Videoscript not found in main.cs");
        }

        SetWallpaper(); //reset to old wallpaper, forcing screen redraw.
        //SystemThemeChange(true); //sets apptheme to light, systheme to dark

        var obj = GameObject.Find("Main Camera").GetComponent<Controller>();
        if (obj != null)
            obj.CloseAllOpenWindows(); //closes multimonitor & configwindow if open
        else
            Debug.Log("Controller script not found in main.cs");

        UnityEngine.Application.Quit(); //quits unity.
    }

    /// <summary>
    /// traymenu - toggle b/w various weather presets.
    /// </summary>
    #region weatherbtn
    private void Weather_Btn(System.Object sender, System.EventArgs e)
    {
        var obj = GameObject.Find("Ambient light & Rain").GetComponent<CycleScript>();

        string s = (sender as MenuItem).Text;
        if (s == "Clear")
        {
            obj.WeatherSystemTrayBtn(800);
        }
        else if (s == "Thunder & Heavy Rain")
        {
            obj.WeatherSystemTrayBtn(202);
        }
        else if (s == "Ragged Thunder")
        {
            obj.WeatherSystemTrayBtn(221);
        }
        else if (s == "Drizzle Thunder")
        {
            obj.WeatherSystemTrayBtn(230);
        }
        else if (s == "Thunderstorm")
        {
            obj.WeatherSystemTrayBtn(250);
        }
        else if (s == "Light Drizzle")
        {
            obj.WeatherSystemTrayBtn(300);
        }
        else if (s == "Drizzle")
        {
            obj.WeatherSystemTrayBtn(320);
        }
        else if(s == "Light Rain")
        {
            obj.WeatherSystemTrayBtn(500);
        }
        else if(s == "Rain")
        {
            obj.WeatherSystemTrayBtn(550);
        }
        else if (s == "Heavy Rain")
        {
            obj.WeatherSystemTrayBtn(502);
        }
        else if (s == "Light Snow")
        {
            obj.WeatherSystemTrayBtn(600);
        }
        else if (s == "Snow")
        {
            obj.WeatherSystemTrayBtn(650);
        }
        else if (s == "Heavy Snow")
        {
            obj.WeatherSystemTrayBtn(602);
        }
        else if (s == "Sleet")
        {
            obj.WeatherSystemTrayBtn(611);
        }
        else if (s == "Mist")
        {
            obj.WeatherSystemTrayBtn(701);
        }
        else if (s == "Haze")
        {
            obj.WeatherSystemTrayBtn(721);
        }
        else if (s == "Fog")
        {
            obj.WeatherSystemTrayBtn(741);
        }
        else if (s == "Dust")
        {
            obj.WeatherSystemTrayBtn(761);
        }
        else if(s == "User Rain - 1")
        {
            obj.WeatherSystemTrayBtn(901);
        }
        else if (s == "User Rain - 2")
        {
            obj.WeatherSystemTrayBtn(902);
        }
        else if (s == "User Atmosphere - 1")
        {
            obj.WeatherSystemTrayBtn(903);
        }
        else if (s == "User Atmosphere - 2")
        {
            obj.WeatherSystemTrayBtn(904);
        }

    }

    #endregion

    /// <summary>
    /// traymenu - adds startup entry in registry under "rePaper-Unity" name.
    /// </summary>
    /// <remarks>
    /// If disabled in taskmanager, entry gets added but key value will remain disabled.
    /// </remarks>
    /// <param name="val">true: set entry, false: delete entry.</param>
    public void SetStartup(bool val)
    {
        //create shortcut first, overwrite if exist with new path.
        CreateShortcut(); 

        RegistryKey rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        if (val)
            rk.SetValue(UnityEngine.Application.productName, System.AppDomain.CurrentDomain.BaseDirectory + "\\rePaperStartup.lnk");
        else
        {
            try
            {
                rk.DeleteValue(UnityEngine.Application.productName, false);
            }
            catch (Exception)
            {
                Debug.Log("Regkey Does not exist to delete");
            }
        }
        rk.Close();
    }

    /// <summary>
    /// Switches system theme
    /// </summary>
    /// <param name="isExit">true: revert to dark sys & white app theme.</param>
    public void SystemThemeChange(bool isExit = false) 
    {
         if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //windows 7
        {
            Debug.Log("win7, themechange cancelled");
            return;
        }

        if (MenuController.menuController.userSettings.sysColor == true)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                                                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true);
                if(isExit == true) 
                {
                    rk.SetValue("AppsUseLightTheme", 1);
                    rk.SetValue("SystemUsesLightTheme", 0);
                }
                else if (NightTimeCheck() == true) //dark theme
                {
                    rk.SetValue("AppsUseLightTheme", 0);
                    rk.SetValue("SystemUsesLightTheme", 0);  //In previous testing builds of Windows 10, the DWORD was actually SystemUsesLightTheme; may 1903 update system theme.
                }
                else //day
                {
                    rk.SetValue("AppsUseLightTheme", 1);
                    if (MenuController.menuController.userSettings.isLightTheme == true)
                        rk.SetValue("SystemUsesLightTheme", 1);  //In previous testing builds of Windows 10, the DWORD was actually SystemUsesLightTheme.
                    else
                        rk.SetValue("SystemUsesLightTheme", 0);
                }
                rk.Close();
            }
            catch(Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    const int SPIF_UPDATEINIFILE = 0x01;
    const int SPIF_SENDWININICHANGE = 0x02;
    private const UInt32 SPI_SETDESKWALLPAPER = 0x14;
    private const UInt32 SPIF_SENDCHANGE = 0x02;
    /// <summary>
    /// Force refresh desktop.
    /// </summary>
    void SetWallpaper()
    {
        StaticPinvoke.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, null, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
    }

    /// <summary>
    /// Sets desktop wallpaper to user selected image in filebrowser.
    /// </summary>
    /// <param name="filePath">wallpaper path.</param>
    public void SetCustomWallpaper(string filePath)
    {
        if (filePath != null)
        {
            StaticPinvoke.SystemParametersInfo(SPI_SETDESKWALLPAPER, (UInt32)filePath.Length, filePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

        }
    }

    /// <summary>
    /// Add entry to traymenu.
    /// </summary>
    private static List<SystemTray> trays = new List<SystemTray>();
    public static SystemTray CreateSystemTrayIcon()
    {
        if (!UnityEngine.Application.isEditor)
        {
            trays.Add(new SystemTray());
            return trays[trays.Count - 1];
        }
        return null;
    }

}
