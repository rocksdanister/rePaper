using System;
using UnityEngine;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using Debug = UnityEngine.Debug;
using System.Collections;
using Screen = System.Windows.Forms.Screen;
/*
References:
https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows-plus
*/

/// <summary>
/// Drawing behind desktop icons! >_<
/// </summary>
public class Headless : MonoBehaviour {

    public static Headless headless = null;
    void Awake()
    {

        if (headless == null)
        {
            headless = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (headless != this)
        {
            Destroy(this.gameObject);
            return;
        }
    }

    public bool pause_disable = false;
    public bool debuggin = false;
    #region thread_stuff
    Process currProcess;
    int processID;
    ManualResetEvent manualResetEvent = new ManualResetEvent(true);
    public Thread t_watcher;

    #endregion thread_stuff


    #region pause_stuff
    //Thread mainThread;
    volatile bool runningFullScreen = false; // volatile - cache refresh/flush
    StaticPinvoke.RECT appBounds;
    Rectangle screenBounds;
    IntPtr hWnd, unityWindow, workerw, desktopHandle, shellHandle, progman, result, p, shell_tray, old_unity_handle, new_unity_handle, folderView, workerWOrig;
    #endregion

    Process unity_process;
    // Use this for initialization
    void Start()
    {
        if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
            Application.targetFrameRate = 60;
        else
            Application.targetFrameRate = MenuController.menuController.userSettings.fps;

        if (Application.isEditor == true) //disable on Unity Editor.
            return;

        //{UNUSED} Hide application icon from taskbar(background process).
        #region borderless
        /*
        // Find the application's Window
        var handle = FindWindowByCaption(IntPtr.Zero, Application.productName);
        if (handle == IntPtr.Zero) return;

        // Move the Window Off Screen
        SetWindowPos(handle, 1, 0, 0, 640, 360, 0);
        //SetWindowPos(handle, 0, -720, 0, 720, 480, 0); //ORIGINAL

        // Remove the Window from the Taskbar
        ShowWindow(handle, (uint)0);
        SetWindowLong(handle, GWL_EXSTYLE, GetWindowLong(handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
        ShowWindow(handle, (uint)5);
        */
        #endregion

        // Renders behind desktop icons & windows.
        #region behind_icon

        // Find Progman windowhandle
        progman = StaticPinvoke.FindWindow("Progman", null);
        // Desktop        
        folderView = StaticPinvoke.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
        if (folderView == IntPtr.Zero)
        {
            do
            {
                workerWOrig = StaticPinvoke.FindWindowEx(StaticPinvoke.GetDesktopWindow(), workerWOrig, "WorkerW", null);
                folderView = StaticPinvoke.FindWindowEx(workerWOrig, IntPtr.Zero, "SHELLDLL_DefView", null);
            } while (folderView == IntPtr.Zero && workerWOrig != IntPtr.Zero);
        }
        folderView = StaticPinvoke.FindWindowEx(folderView, IntPtr.Zero, "SysListView32", "FolderView");
        
        result = IntPtr.Zero;

        //todo: Set Animate controls & elements inside windows settings using Systemparametersinfo()

        // Send 0x052C to Progman. Spawns WorkerW behind desktop icons. 
        // "Animate controls and elements inside windows" needs to be enabled in: This PC -> Properties -> Advanced System Settings -> Performance -> Settings.
        // This is used to show Fade effect on windows wallpaper change.
        // In windows 7 this setting is probably(?) under multiple settings.
        StaticPinvoke.SendMessageTimeout(progman,
                               0x052C,
                               new IntPtr(0),
                               IntPtr.Zero,
                               StaticPinvoke.SendMessageTimeoutFlags.SMTO_NORMAL,
                               1000,
                               out result);

        workerw = IntPtr.Zero;
        // Find newly created worker
        StaticPinvoke.EnumWindows(new StaticPinvoke.EnumWindowsProc((tophandle, topparamhandle) =>
        {
            p = StaticPinvoke.FindWindowEx(tophandle,
                                        IntPtr.Zero,
                                        "SHELLDLL_DefView",
                                        IntPtr.Zero);

            if (p != IntPtr.Zero)
            {
                // Gets the WorkerW Window after the current one.
                workerw = StaticPinvoke.FindWindowEx(IntPtr.Zero,
                                           tophandle,
                                           "WorkerW",
                                           IntPtr.Zero);
            }

            return true;
        }), IntPtr.Zero);

        if(IntPtr.Equals(workerw,IntPtr.Zero) || workerw == null)
        {
            //todo: set the settings through code using SystemParametersInfo() - complication: microsoft uses registry to update the radio button UI in the Performance dialog, 
            //which DOES not reflect actual applied settings! o_O..will have to edit registry too.
            if (MenuController.menuController.WarningMsg("It looks like some settings need to be set to make rePaper work:- \n" +
                            "1. Open This PC(My Computer)\n" +
                            "2. Right-click empty area\n" +
                            "3. Select Properties\n" +
                            "4. Select Advanced System Settings\n" +
                            "5. Under Performance tab select Settings\n" +
                            "6. Enable Animate controls and elements inside windows & Apply.\n" +
                            "If Windows 7 just set - Adjust for best appearance & Apply\n" +
                            "If still not working, close & start rePaper again or restart windows." +
                            "Press OK to Exit, Cancel to proceed anyway.",
                            "Hold on..") == 1)
            {
                main.instance.Close_Action(null, null);
            }
        }

        IntPtr handle = IntPtr.Zero;
        // Find the application's Window, should work in 90% case
        handle = StaticPinvoke.FindWindowByCaption(IntPtr.Zero, Application.productName);
        while ( handle != null && !IntPtr.Equals(handle,IntPtr.Zero) )
        {
            #region processid
            //Process.GetCurrentProcess().MainWindowHandle returns 0, searching with found handle instead.
            StaticPinvoke.GetWindowThreadProcessId(handle, out processID);
            unity_process = Process.GetProcessById(processID);

            // Verify weather the window handle is rePaper-Unity.exe before proceeding further!.
            if (unity_process.ProcessName.Equals(Application.productName, StringComparison.InvariantCultureIgnoreCase))
            {
                old_unity_handle = handle;
                break;
            }
            else // something else!!...probably explorer window with window title caption.
            {
                Debug.Log("Different window handle found instead of rePaper-Unity, searching again!");
                old_unity_handle = handle;

                //searching the next window handle...
                handle = StaticPinvoke.FindWindowEx(IntPtr.Zero, old_unity_handle, null, Application.productName);
            }
            #endregion processid
        }

        if(handle == null || IntPtr.Equals(handle,IntPtr.Zero))
        {
            Debug.Log("Could't find unity window! Handle:- ");
            return;
        }

        if (MenuController.menuController.isMultiMonitor == false)
            StaticPinvoke.SetWindowPos(handle, 1, 0, 0, UnityEngine.Screen.currentResolution.width, UnityEngine.Screen.currentResolution.height, 0);
        else //multiple monitors detected.  
        {
            /*
            //Attempted to make window appear on primary monitor, failure!.
            var monitor = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            StaticPinvoke.SetWindowPos(handle, 1, monitor.Left, monitor.Top, monitor.Width,monitor.Height, 0);
            */

            //Success, basically offset value of desktop handle needs to be added to x,y
            //todo:- update pause/sleep algorithm for multiple displays. ( currently based on primary display)
            MoveToDisplay(MenuController.menuController.userSettings.ivar2);
        }
        
        //os = SystemInfo.operatingSystem.ToLowerInvariant(); //unity operating system info.
        if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //windows 7(min: support)
        {
            if (debuggin == false)
            {
                workerWOrig = progman;
                StaticPinvoke.ShowWindow(workerw, (uint)0);
                StaticPinvoke.SetParent(handle, progman);
            }
        }
        else //assuming above win7, will fail in vista etc
        {
            if(debuggin == false)
                StaticPinvoke.SetParent(handle, workerw);
        }

        #endregion behind_icon

        #region pause_unpause_monitor

        t_watcher = new Thread(new ThreadStart(Update_T));
        t_watcher.IsBackground = true;  //exit with application
        StartCoroutine(Startup());

        desktopHandle = StaticPinvoke.GetDesktopWindow();
        shellHandle = StaticPinvoke.GetShellWindow();

        hWnd = workerWOrig; // bug fix: early launch, might detect application as hWndID.
        #endregion pause_unpause_monitor
    }

    const int SPIF_UPDATEINIFILE = 0x01;
    private const UInt32 SPI_SETDESKWALLPAPER = 0x14;
    /// <summary>
    /// Move rePaper to corresponding display.
    /// </summary>
    public void MoveToDisplay(int i )
    {
        StaticPinvoke.RECT appBounds_;
        Rectangle screenBounds_;

        Screen[] screens = Screen.AllScreens;

        if (i > (screens.Length - 1))
            i = 0; //primary monitor

        StaticPinvoke.GetWindowRect(workerw, out appBounds_);
        screenBounds_ = System.Windows.Forms.Screen.FromHandle(workerw).Bounds;
        //force refresh desktop
        StaticPinvoke.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, null,SPIF_UPDATEINIFILE);

        if (i == -1)
        {
            //crash, skipping for now.
            //StaticPinvoke.SetWindowPos(old_unity_handle, 1, appBounds_.Left, appBounds_.Top, screens[i].Bounds.Width, screens[i].Bounds.Height, 0);
        }
        else
        {
            // adding the workerw offsets to x & y. (basically the bounds change since rePaper is a child of workerw handle)
            StaticPinvoke.SetWindowPos(old_unity_handle, 1, screens[i].Bounds.X - appBounds_.Left, screens[i].Bounds.Y - appBounds_.Top, screens[i].Bounds.Width, screens[i].Bounds.Height, 0);
        }

    }

    /// <summary>
    /// Delay before monitoring paunse/unpause.
    /// </summary>
    IEnumerator Startup()
    {
        //why I did this instead of just sleeping the thread before entering loop, I will never know... x_x
        yield return new WaitForSeconds(5f);
        hWnd = StaticPinvoke.GetForegroundWindow();
        t_watcher.Start();
    }

    /// <summary>
    /// Sets timeScale = 0 which should stop some unity calculations (coroutines, physics etc).
    /// Its not actual pause! (gpu&cpu usage remains).
    /// </summary>
    /// <param name="val">true: timescale = 0, false: timescale = 1</param>
    void ReduceResourceUsage(bool val)
    {
        if (val == true)
        {
            Time.timeScale = 0; //stops fixedupdate (physics), coroutines etc only
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    #region UNUSED_CODE_ALLWINDOWS
    /*
    //private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
    //IntPtr HWND;
    /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
    /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
    /// <remarks> Does not distinguish background & foreground services well enough, idea scrapped for now. </remarks>
    public void GetOpenWindows()
    {
        
        IntPtr shellWindow = StaticPinvoke.GetShellWindow();

        StaticPinvoke.EnumWindows(new StaticPinvoke.EnumWindowsProc((hWnd, lParam) =>
        {
            if (hWnd == shellWindow) return true;
            if (!StaticPinvoke.IsWindowVisible(hWnd)) return true;

            #region memoryleak_only_test
            int length = StaticPinvoke.GetWindowTextLength(hWnd);
            if (length == 0) return true;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(length);
            StaticPinvoke.GetWindowText(hWnd, builder, length + 1);
            #endregion memoryleak_only_test

            Debug.Log(builder);
            return true;
        }), IntPtr.Zero);
        
    }
    */
    #endregion UNUSED_CODE_ALLWINDOWS

    /// <summary>
    /// Debugging.
    /// </summary>
    void OnGUI()
    {
        if (MenuController.menuController.userSettings.bvar2 == true) //debugging mode on
        {
            try
            {
                GUI.Label(new Rect(100, 100, 1000, 300), "Foreground-Process: " + currProcess.ProcessName);
            }
            catch (Exception )
            {
               // ignore
            }
        }
    }
    

    GameObject tmp;
    // Called every frame update.
    private void Update()
    {
        //since scene restart, have to find the current instance of gameobject.
        tmp = GameObject.Find("WallpaperQuad");
        //ReduceResourceUsage(runningFullScreen);
        if (runningFullScreen == true)
        {
            //Stops coroutine & physics functions in Unity. 
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        if (MenuController.menuController.userSettings.bvar1 == true) //battery_low_power_mode
        {
            //to-do: rewrite this using system events (unity seems to be intercepting the messages unfortunately).
            if (System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Offline)
            {
                Application.targetFrameRate = 5;
                Thread.Sleep(100); //too much?
            }
            else
            {
                if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
                {
                    //..dxva stutter unless 60fps
                    if (Application.targetFrameRate != 60)
                        Application.targetFrameRate = 60;
                }
                else
                {
                    if (Application.targetFrameRate != MenuController.menuController.userSettings.fps)
                        Application.targetFrameRate = MenuController.menuController.userSettings.fps;
                }
            }
        }
        else if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
        {
            //..dxva stuttering unless 60fps
              if (Application.targetFrameRate != 60)
               Application.targetFrameRate = 60;
        }
        else //non-dxva video/picture.
        {
            //..user selected framerate.
            if (Application.targetFrameRate != MenuController.menuController.userSettings.fps)
                Application.targetFrameRate = MenuController.menuController.userSettings.fps;
        }

        //..pause videoplayback, when timescale = 0
        if (MenuController.menuController.userSettings.isDXVA == true && MenuController.menuController.userSettings.vidPath != null)
        {
            if(tmp!=null)
                tmp.GetComponent<VideoScript>().PauseVideo();
        }
        if (tmp != null)
            tmp.GetComponent<VideoScript>().PauseVideoUnity(); 
    
    }

    // Called just before scene rendering, after all Update() calls: https://docs.unity3d.com/Manual/ExecutionOrder.html
    private void LateUpdate() 
    {
        //todo: Change scene to a lightweight scene after a while to reduce memory usage when paused. 
        //or better yet ditch unity, use external exe to manage unity..terminate when & relaunch when needed. - problem: unity free version splashscreen :(
        manualResetEvent.WaitOne(); //pause unity main-thread (0% cpu,gpu usage)
    }


    bool skipAhead = false;
    public static volatile bool isDesktop = true; //volatile - cache flush/refresh, thread synchronisation not essential in this application..?
    //string currProcessName = null; //foreground process-name
    /// <summary>
    /// Background thread that monitor foreground windows type, size etc to decide pause/unpause unity main-thread.
    /// </summary>
    private void Update_T()
    {
        while (true) 
        {
            // delay, can be higher but longer time to unpause/pause...50milliseconds seems fine on my i5 4core system.
            Thread.Sleep(50);
            // current foreground window. Only used to get foreground process-name, nothing is being stored. If you don't like rePaper monitoring then replace hWnd = StaticPinvoke.GetForegroundWindow() with hWnd = workerWOrig;
            hWnd = StaticPinvoke.GetForegroundWindow();
                        
            try
            {
                // retrieve foreground window process-name. Since repaper is started as non-admin process, name retrieval might fail for admin/certain processes.
                StaticPinvoke.GetWindowThreadProcessId(hWnd, out processID);
                currProcess = Process.GetProcessById(processID);
            }
            catch(Exception )
            {
              // todo: unity functions are thread unsafe, cant use unity debug.
            }

            if (true) //old-code/future use.
            {
                #region Exceptions_&_Fixes               
                // failed to find in some cases.
                if (IntPtr.Equals(workerWOrig, IntPtr.Zero) == true)
                {
                    folderView = StaticPinvoke.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (folderView == IntPtr.Zero)
                    {
                        do
                        {
                            workerWOrig = StaticPinvoke.FindWindowEx(StaticPinvoke.GetDesktopWindow(), workerWOrig, "WorkerW", null);
                            folderView = StaticPinvoke.FindWindowEx(workerWOrig, IntPtr.Zero, "SHELLDLL_DefView", null);
                        } while (folderView == IntPtr.Zero && workerWOrig != IntPtr.Zero);
                    }
                }
                

                try
                {
                    //currProcessName = currProcess.ProcessName.toLowerInvarient();
                    if ( pause_disable == true || MenuController.menuController.userSettings.bvar2 == true || String.IsNullOrEmpty(currProcess.ProcessName) ) //debugging, skip rePaper pause/sleep.
                    {
                        continue;
                    }
                    
                    skipAhead = false;
                    if (MenuController.menuController.userSettings.appFocusPause == false)
                    {
                        //user defined application-pause/sleep-rules
                        foreach (var item in MenuController.menuController.appList) 
                        {
                            //if (item.processName.ToLowerInvariant() == currProcessName)
                            if(currProcess.ProcessName.Equals(item.processName,StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (item.sleep == false) //always run when this process is foreground window, even when maximized
                                {
                                    hWnd = workerWOrig; //assume desktop, to force unpause/wakeup of rePaper.
                                }
                                else //sleep
                                {
                                    isDesktop = false;
                                    runningFullScreen = true;
                                    main.instance.ChangeTrayIcon(runningFullScreen);
                                    manualResetEvent.Reset();
                                    skipAhead = true;
                                }
                                break;
                            }
                        }
                        if (skipAhead == true)
                            continue;
                    }

                    if (currProcess.ProcessName.Equals("repaper-unity",StringComparison.InvariantCultureIgnoreCase) // bug fix: early launch, might detect application as hWnd ID
                            || currProcess.ProcessName.Equals("config-repaper", StringComparison.InvariantCultureIgnoreCase) //config utility for rePaper
                            || currProcess.ProcessName.Equals("shellexperiencehost", StringComparison.InvariantCultureIgnoreCase) //notification tray etc
                            || currProcess.ProcessName.Equals("searchui", StringComparison.InvariantCultureIgnoreCase) //startmenu search etc
                            //|| currProcessName.Equals("applicationframehost", StringComparison.InvariantCultureIgnoreCase) //windows10 dialogues like adjust time, all uwp processes(store apps) comes under this.
                            //|| currProcessName.Equals("discord", StringComparison.InvariantCultureIgnoreCase) //fix, when trying to minimize maximised discord window, it does not minimize properly to systemtray, foreground windowhandle still at discord.
                       ) 
                    {
                        hWnd = workerWOrig; //assume desktop, to force unpause/wakeup of rePaper. For win7 workerWOrig is set as progman in Start()
                    }

                }
                catch (Exception)
                {
                    //ignore
                }
                
                if(IntPtr.Equals(old_unity_handle, hWnd))// bug fix: early launch, might detect application as hWnd ID. what a pain..
                {
                    hWnd = workerWOrig;
                }

                if (IntPtr.Equals(shell_tray, IntPtr.Zero) == true) //startmenu, sometimes become zero?
                    shell_tray = StaticPinvoke.FindWindow("Shell_TrayWnd", null);

                #endregion Exceptions_&_Fixes
             
                if (hWnd != null && !hWnd.Equals(IntPtr.Zero))
                {
                    if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle)))
                    {
                        StaticPinvoke.GetWindowRect(hWnd, out appBounds);
                        //open applications window size, could use IsZoomed() if such fine control is not required.
                        screenBounds = System.Windows.Forms.Screen.FromHandle(hWnd).Bounds;
                        if ((appBounds.Bottom - appBounds.Top) >= screenBounds.Height * .9f && (appBounds.Right - appBounds.Left) >= screenBounds.Width * .9f) //foreground window > 90% work_area, ie: work_area = display_resolution - taskbar
                        {
                            //todo: could avoid this check everytime by making two versions of this fn, win7 & other.
                            if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //windows 7
                            {
                                if (IntPtr.Equals(hWnd, progman) != true)  //not equal to desktop & fullscreen apprunning
                                {
                                    isDesktop = false;
                                    runningFullScreen = true;
                                    main.instance.ChangeTrayIcon(runningFullScreen);
                                    manualResetEvent.Reset();

                                }
                                else // currently at desktop(if user clicks or minimize all apps)
                                {
                                    isDesktop = true;
                                    manualResetEvent.Set();
                                    runningFullScreen = false;
                                    main.instance.ChangeTrayIcon(runningFullScreen);
                                }
                            }
                            else // windows 8, 10 etc..
                            {
                                if (IntPtr.Equals(hWnd, workerWOrig) != true) //not equal to desktop & fullscreen apprunning
                                {
                                    isDesktop = false;
                                    runningFullScreen = true;
                                    main.instance.ChangeTrayIcon(runningFullScreen);
                                    manualResetEvent.Reset();
                                }
                                else // currently at desktop(if user clicks)
                                {
                                    isDesktop = true;
                                    manualResetEvent.Set();
                                    runningFullScreen = false;
                                    main.instance.ChangeTrayIcon(runningFullScreen);
                                }
                            }

                        }
                        else if (IntPtr.Equals(shell_tray, IntPtr.Zero) != true && IntPtr.Equals(hWnd, shell_tray) == true) //systrayhandle
                        {
                            isDesktop = true;
                            main.instance.ChangeTrayIcon(runningFullScreen);
                            runningFullScreen = false;
                            manualResetEvent.Set();
                        }
                        else //application window is not greater >90%
                        {                            
                            isDesktop = false;

                            if (MenuController.menuController.userSettings.appFocusPause == false)
                            {
                                runningFullScreen = false;
                                main.instance.ChangeTrayIcon(runningFullScreen);
                                manualResetEvent.Set();
                            }
                            else
                            {
                                runningFullScreen = true;
                                main.instance.ChangeTrayIcon(runningFullScreen);
                                manualResetEvent.Reset();
                            }
                        }
                    }
                }
            }
            
        } //while loop

    } 

}
