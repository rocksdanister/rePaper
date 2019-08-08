using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI clock (circle) & UI notification class.
/// </summary>
public class CanvasScript : MonoBehaviour {

    int tmp_time_hr;
    //public CycleScript cycleScript;
    public GameObject notification;
    public GameObject Clock, Weather_Text;//, System_Status;
    int pollingDelay = 2;

    //public Text cpuText;
    //public Image cpuImage;
    //public Text ramText;
    //public Image ramImage;

    public Text clockText;
    public Image clockImage;
    public Image clockMinuteImg;

    System.DateTime time;
    float hrPer;
    float hrPerCur;
    float mrPer;
    float mrPerCur;
    Color tmpClr;

    int startSecond, startMinute, startHr;
    bool firstRun = false, firstRun2 = false, firstRun3 = false;
    bool transition_pending = false, transition_pending_2 = false;
    Coroutine cr, cr2;
    //public GameObject settingsButton, demoButton;
    //Color settingsButtonColor;

    private void Start()
    {
        //settingsButtonColor = settingsButton.GetComponent<Image>().color;
        notification.SetActive(false); //firstrun notification

        clockMinuteImg.fillAmount = 0f;
        clockImage.fillAmount = 0f;

        //..ui elements on/off
        if(MenuController.menuController.userSettings.isDemo == false)
        {
            //demoButton.SetActive(false);
        }
        if(MenuController.menuController.userSettings.isClock == false)
        {
            Clock.SetActive(false);
        }
        if(MenuController.menuController.userSettings.isPerformance == false)
        {
            //System_Status.SetActive(false);
        }
        if (MenuController.menuController.userSettings.isWeather == false)
        {
            Weather_Text.SetActive(false);
        }

        startSecond = System.DateTime.Now.Second;
        startMinute = System.DateTime.Now.Minute;
        startHr = System.DateTime.Now.Hour;

        pollingDelay = MenuController.menuController.userSettings.pollingDelay;
    }

    //..Hr hand animation
    IEnumerator Clock_Animation_Hr(float a =1f, float b =1f)
    {
        WaitForSeconds countTime = new WaitForSeconds(0.1f);
        float t = 0;
        while (t <= 1)
        {
            yield return countTime;
            t += 0.1f;
            clockImage.fillAmount = Mathf.SmoothStep(a, b, t);
        }
        yield return null;
    }

    //..minute hand animation
    IEnumerator Clock_Animation(float a = 1f, float b = 1f)
    {
        WaitForSeconds countTime = new WaitForSeconds(0.1f);
        float t = 0;
        while (t <= 1)
        {
            yield return countTime;
            t += 0.1f;
            clockMinuteImg.fillAmount = Mathf.SmoothStep(a, b, t);
        }
        yield return null;
    }

    bool tmp_flag = false;
    void Update () {

        //skip rest.
        if (Time.timeScale == 0)
            return;

        if (MenuController.menuController.isFirstTime == true && tmp_flag == false)
        {
            //Color tmp = settingsButtonColor;
            //tmp.a = 1.0f;
            //settingsButton.GetComponent<Image>().color = tmp;
            tmp_flag = true;
            notification.SetActive(true);
        }

        if(transition_pending == true)
        {
            if (cr != null)
                StopCoroutine(cr);
            cr = StartCoroutine(Clock_Animation(mrPerCur, mrPer));
            transition_pending = false;
        }
        if(transition_pending_2 == true)
        {
            if (cr2 != null)
                StopCoroutine(cr2);

            cr2 = StartCoroutine(Clock_Animation_Hr(hrPerCur, hrPer));
            transition_pending_2 = false;
        }

        if (System.Math.Abs(startSecond - System.DateTime.Now.Second) >= pollingDelay || firstRun == false)
        {
            if (firstRun == true)
                startSecond = System.DateTime.Now.Second;

            if (MenuController.menuController.userSettings.isPerformance == true)
            {
                //to-do: cpu & ram usage.
            }
            firstRun = true;
        }
        //minute circle positon calc.
        if (System.Math.Abs(startMinute - System.DateTime.Now.Minute) >= 1 || firstRun2 == false )
        {
            time = System.DateTime.Now;
            clockText.text = time.ToString("hh:mm");
            if (firstRun2 == true)
                startMinute = System.DateTime.Now.Minute;

            if (MenuController.menuController.userSettings.isClock == true)
            {

                if (firstRun2 == false)
                {
                    mrPerCur = 0f;
                    mrPer = System.DateTime.Now.Minute / 60f;
                    transition_pending = true;
                }
                else
                {
                    mrPerCur = mrPer; // previous minute
                    mrPer = System.DateTime.Now.Minute / 60f;
                    transition_pending = true;
                }
                firstRun2 = true;
            }
        }
        //hour circle positon calc.
        if(System.Math.Abs(startHr- System.DateTime.Now.Hour) >= 1 || firstRun3 == false )
        {
            time = System.DateTime.Now;
            tmp_time_hr = time.Hour;

            clockText.text = time.ToString("hh:mm");
            if (firstRun3 == true)
                startHr = System.DateTime.Now.Hour;
  
            if (MenuController.menuController.userSettings.isClock == true)
            {
                if (firstRun3 == true)
                    hrPerCur = hrPer; //previous hr
                else
                    hrPerCur = 0f;

                if (tmp_time_hr < 12 && tmp_time_hr != 0)
                {

                    hrPer = (tmp_time_hr / 12f);

                    if ((tmp_time_hr) < 6)
                        hrPer += 0.5f; // fill starts from bottom
                    else
                    {
                        hrPer = ((tmp_time_hr - 6) / 12f);
                    }

                }
                else if (tmp_time_hr == 12 || tmp_time_hr == 0)
                {
                    hrPer = 0.5f;
                }
                else // >12
                {
                    hrPer = ((tmp_time_hr - 12) / 12f);

                    if ((tmp_time_hr - 12) < 6)
                        hrPer += 0.5f; // fill starts from bottom
                    else
                    {
                        hrPer = ((tmp_time_hr - 18) / 12f);
                    }
                }
                transition_pending_2 = true;
                firstRun3 = true;
            }
        }

    }

}

