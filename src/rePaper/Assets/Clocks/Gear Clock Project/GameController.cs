using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController gameController;
    // Use this for initialization

    void Awake()
    {
        Application.targetFrameRate = 24;
        //.. Singleton design, only one isntance.
        if (gameController == null)
        {
            DontDestroyOnLoad(gameObject);
            gameController = this;
        }
        else if (gameController != this)
        {
            Destroy(gameObject);
        }
    }

    [Serializable]
    public class UserSettings
    {
        
        public bool firstRun;
        /*
        public float sliderBlend;
        public float sliderScale;
        */
        public int updateCnt;
        

        //..clock
        public bool isCont;
        public int rgbAnimation;
        //public Color gearColor;

        //..extra variables
        public float var1;
        public float var2;
        public float var3;
        public float var4;

        public UserSettings()
        {
         //   isCont = false;
          //  rgbAnimation = 0; //0 =rbg , 1 =colorselct
            //gearColor = Color.white;
            updateCnt = 1;
         //   sliderBlend = 1f;
         //   sliderScale = 1f;
            firstRun = true;

            var1 = 1.0f;
            var2 = 1.0f;
            var3 = 1.0f;
            var4 = 1.0f;
        }
    }

    [HideInInspector] public UserSettings userSettings, loadData;

    void Start()
    {
        userSettings = new UserSettings(); //load default values/settings  
        LoadData();  // load save data if it exists

        SceneManager.LoadScene("SampleScene");

    }

    public void LoadData()
    {
        string str = Environment.CurrentDirectory;
        Debug.Log(str);
        if (File.Exists(str + "\\config.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(str + "\\config.dat", FileMode.Open);
            loadData = (UserSettings)bf.Deserialize(file);
            file.Close();

            userSettings.firstRun = loadData.firstRun;
            userSettings.var1 = loadData.var1;
            userSettings.var2 = loadData.var2;
            userSettings.var3 = loadData.var3;
            userSettings.var4 = loadData.var4;
            userSettings.updateCnt = loadData.updateCnt;
           // userSettings.sliderBlend = loadData.sliderBlend;
           // userSettings.sliderScale = loadData.sliderScale;
            //userSettings.gearColor = loadData.gearColor;
           // userSettings.rgbAnimation = loadData.rgbAnimation;
           // userSettings.isCont = loadData.isCont;
        }
        else
        {
            Debug.Log("FILE NOT FOUND");
            Save(); // creating an existing so that steam can update config.dat too when save class changed.
        }
    }

    public void Save()
    {
        string str = Environment.CurrentDirectory;
        //.. using binary file.
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(str + "\\config.dat");
        bf.Serialize(file, userSettings);
        file.Close();
    }
}
