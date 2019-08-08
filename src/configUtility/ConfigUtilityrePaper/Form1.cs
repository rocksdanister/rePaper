using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;


namespace ConfigUtilityrePaper
{
    public partial class Form1 : Form
    {
        public bool isFirstTime = false;

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
            public bool isDXVA;

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
            public bool bvar1;
            public bool bvar2;
            public bool bvar3;
            public bool bvar4;
            public bool bvar5;

            public string svar1;
            public string svar2;
            public string svar3;
            public string svar4;
            public string svar5;

            public int ivar1;
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
                blur_quality = 1; // 0- low, >=1 - high
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

        #region applicationrules_load_save
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
            catch (Exception ex)
            {
                MessageBox.Show("Error writing file " + ex.Message+ " "+ ex.StackTrace);
            }
        }

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
                    MessageBox.Show("Error reading file " + ex.Message + " " + ex.StackTrace);
                }
            }
            else
            {
                //default value, added discord because of weird behavior.
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
        Weather weather_tmp = new Weather();
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
            public Atmosphere(int code, string name, float size, float horiSpeed, float vertSpeed, float density, string color)
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

        #region weather_parameters_save_load

        public void LoadWeatherParameters()
        {

            if (File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather_parameters.xml"))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Weather));
                    FileStream file = File.Open(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather_parameters.xml", FileMode.Open);
                    weather_tmp = (Weather)ser.Deserialize(file);

                    file.Close();

                    WeatherParam = weather_tmp;
                }
                catch (Exception ex)
                {
                    //Debug.Log("something went wrong reading file weather_parameters: " + ex.Message);
                }
            }
            else
            {
                // no-file, save default parameters in xml.
                SaveWeatherParameters(true);
            }
        }

        public void SaveWeatherParameters(bool isDefault = false)
        {
            if (isDefault == true)
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
            }

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Weather));
                var folder = Directory.CreateDirectory(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper"));
                FileStream file = File.Create(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "weather_parameters.xml");
                ser.Serialize(file, WeatherParam);
                file.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #endregion weather_parameters_save_load

        public UserSettings userSettings, loadData, defaultSettings;

        public void Save_Data()
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
                MessageBox.Show(e.Message);
            }
        }

        public void Load_Data()
        {
            if (File.Exists(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "config.xml"))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(UserSettings));
                    FileStream file = File.Open(System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Saved Games\\rePaper\\") + "config.xml", FileMode.Open);

                    loadData = (UserSettings)ser.Deserialize(file);
                    file.Close();

                    userSettings = loadData;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    //throw e;
                }
            }
            else
            {
                //Save_Data();
                isFirstTime = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) //clock toggle
        {
            userSettings.isClock = clockBox.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e) //weather toggle, ui text
        {
            userSettings.isWeather = weatherBox.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e) //auto ui hide
        {
            userSettings.autoUiHide = uiHideBox.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e) //weather control
        {
            userSettings.isDemo = demoBox.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e) //daynight
        {
            userSettings.dayNightTint = nightshiftBox.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e) //ui color
        {
            userSettings.autoUiColor = autoUIBox.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)  //clock style
        {
            userSettings.clockType = clockDrop.SelectedIndex;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) //wallpaperscale
        {
            userSettings.imgScaling = scalingDrop.SelectedIndex;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)  //weather unit
        {
            try
            {
                if (weatherDrop.SelectedIndex == 0)
                {
                    userSettings.isMetric = true;
                }
                else
                {
                    userSettings.isMetric = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

     

        private void label12_Click(object sender, EventArgs e) //slider label
        {
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label12.Text = trackBar1.Value.ToString();
            userSettings.fps = trackBar1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            userSettings.isReload = true;
            Save_Data();
            SaveWeatherParameters();
            Application.Exit();
        }

        private void weather_textbox_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(weather_textbox.Text, out userSettings.zipCode);
            userSettings.cityName = weather_textbox.Text;
        }
        private void weather_textbox_KeyDown(object sender, KeyEventArgs e) //limiting inputs
        {

        }
        private void weather_textbox_KeyPress(object sender, KeyPressEventArgs e) //limiting inputs
        {
            e.Handled = e.KeyChar != (char)Keys.Back && !char.IsSeparator(e.KeyChar) && !char.IsLetter(e.KeyChar) && !(e.KeyChar==',') && !char.IsNumber(e.KeyChar);
        }

        private void openweathermap_link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://openweathermap.org/");
        }


        private void DXVABox_CheckedChanged(object sender, EventArgs e)
        {
            userSettings.isDXVA = DXVABox.Checked;
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://openweathermap.org/");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/EdoFrank/RainDropEffect");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://twitter.com/The_ArtOfCode");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.fontsquirrel.com/fonts/merriweather");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/erikflowers/weather-icons");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/octokit/octokit.net");
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Bunny83/SimpleJSON");
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.pexels.com/@stywo");
        }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:awoo.git@gmail.com");

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();
        }


        private void tabPage2_Click(object sender, EventArgs e)
        {
           
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void label34_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://software.intel.com/en-us/articles/hardware-accelerated-video-decode-in-unity");
        }

        private void linkLabel12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://openweather.co.uk/privacy-policy");
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void checkBoxSunOverlay_CheckedChanged(object sender, EventArgs e)
        {
            userSettings.sun_overlay = checkBoxSunOverlay.Checked;
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void qualityDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            userSettings.blur_quality = qualityDrop.SelectedIndex;
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void darkThemeBox_CheckedChanged_1(object sender, EventArgs e)
        {
            userSettings.sysColor = darkThemeBox.Checked;

            if(userSettings.sysColor == true)
            {
                radioDarkTheme.Enabled = true;
                radioLightTheme.Enabled = true;
            }
            else
            {
                radioDarkTheme.Enabled = false;
                radioLightTheme.Enabled = false;
            }

            if (userSettings.isLightTheme == false)
            {
                radioLightTheme.Checked = false;
                radioDarkTheme.Checked = true;
            }
            else
            {
                radioLightTheme.Checked = true;
                radioDarkTheme.Checked = false;
            }

        }

        private void radioLightTheme_CheckedChanged(object sender, EventArgs e)
        {
            if (radioLightTheme.Checked == true)
                userSettings.isLightTheme = true;
            else
                userSettings.isLightTheme = false;
        }

        private void radioDarkTheme_CheckedChanged(object sender, EventArgs e)
        {
            if (radioDarkTheme.Checked == true)
                userSettings.isLightTheme = false;
            else
                userSettings.isLightTheme = true;
        }

        private void startupBox_CheckedChanged(object sender, EventArgs e)
        {
            userSettings.runAtStartup = startupBox.Checked;
        }

        private void pauseButton_CheckedChanged(object sender, EventArgs e)
        {
            userSettings.appFocusPause = pauseButton.Checked;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox2_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            
            System.Diagnostics.Process.Start(e.LinkText);
        }

        int tmpbarval;
        private void trackBar2_Scroll(object sender, EventArgs e) //blur level slider
        {
            tmpbarval = 10 - trackBar2.Value;
            blur_lvl_label.Text = tmpbarval.ToString();
            userSettings.ivar1 = trackBar2.Value; //trackbar  0->10 right to left
        }

        private void blur_lvl_label_Click(object sender, EventArgs e)
        {

        }

        private void apiKey_richTextBos_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void apiKeyBox_TextChanged(object sender, EventArgs e)
        {
            userSettings.apiKey = apiKeyBox.Text;
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void batPowBox_CheckedChanged(object sender, EventArgs e)
        {
            userSettings.bvar1 = batPowBox.Checked;
        }

        private void checkBox1_CheckedChanged_2(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            rainAmtPercent.Text = (ranAmtTrackbar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].rainAmt = (ranAmtTrackbar.Value / 100f);
        }

        private void customise_Click(object sender, EventArgs e)
        {

        }

        private void label30_Click(object sender, EventArgs e)
        {

        }

        private void drop1Trackbar_Scroll(object sender, EventArgs e)
        {
            drop1Percent.Text = (drop1Trackbar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].drop1Amt = (drop1Trackbar.Value / 100f);
        }

        private void drop2Trackbar_Scroll(object sender, EventArgs e)
        {
            drop2Percent.Text = (drop2Trackbar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].drop2Amt = (drop2Trackbar.Value / 100f);
        }

        private void staticDropTrackBar_Scroll(object sender, EventArgs e)
        {
            staticDropPercent.Text = (staticDropTrackBar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].staticDrpAmt = (staticDropTrackBar.Value / 100f);
        }

        private void rainTrailTrackbar_Scroll(object sender, EventArgs e)
        {
            rainTrailPercent.Text = rainTrailTrackbar.Value.ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].rainTrail = rainTrailTrackbar.Value;
        }

        private void dropSpeedTrackbar_Scroll(object sender, EventArgs e)
        {
            dropSpeedPercent.Text = (dropSpeedTrackbar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].dropSpeed = (dropSpeedTrackbar.Value / 100f);
        }

        private void dropSizeTrackbar_Scroll(object sender, EventArgs e)
        {
            dropSizePercent.Text = (dropSizeTrackbar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].zoomOut = (dropSizeTrackbar.Value / 100f);
        }

        private void textureSizeTrackbar_Scroll(object sender, EventArgs e)
        {
            textureSizePercent.Text = (textureSizeTrackbar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].rainTex = (textureSizeTrackbar.Value / 100f);
        }

        private void blurTrackbar_Scroll(object sender, EventArgs e)
        {
            blurPercent.Text = (blurTrackbar.Value / 100f).ToString();
            WeatherParam.rain[WeatherSelectBox.SelectedIndex].blurAmt = (blurTrackbar.Value / 100f);
        }

        private void WeatherSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWeatherParameterTab();
        }

        void UpdateWeatherParameterTab()
        {
            if (WeatherSelectBox.SelectedIndex < WeatherParam.rain.Length)
            {
                rainPanel.Enabled = true;
                rainPanel.Visible = true;
                atmospherePanel.Enabled = false;
                atmospherePanel.Visible = false;

                ranAmtTrackbar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].rainAmt * 100f);
                rainAmtPercent.Text = (ranAmtTrackbar.Value / 100f).ToString();

                drop1Trackbar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].drop1Amt * 100f);
                drop1Percent.Text = (drop1Trackbar.Value / 100f).ToString();

                drop2Trackbar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].drop2Amt * 100f);
                drop2Percent.Text = (drop2Trackbar.Value / 100f).ToString();

                staticDropTrackBar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].staticDrpAmt * 100f);
                staticDropPercent.Text = (staticDropTrackBar.Value / 100f).ToString();

                rainTrailTrackbar.Value = WeatherParam.rain[WeatherSelectBox.SelectedIndex].rainTrail;
                rainTrailPercent.Text = rainTrailTrackbar.Value.ToString();

                dropSpeedTrackbar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].dropSpeed * 100f);
                dropSpeedPercent.Text = (dropSpeedTrackbar.Value / 100f).ToString();

                dropSizeTrackbar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].zoomOut * 100f);
                dropSizePercent.Text = (dropSizeTrackbar.Value / 100f).ToString();

                textureSizeTrackbar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].rainTex * 100f);
                textureSizePercent.Text = (textureSizeTrackbar.Value / 100f).ToString();

                blurTrackbar.Value = Convert.ToInt32(WeatherParam.rain[WeatherSelectBox.SelectedIndex].blurAmt * 100f);
                blurPercent.Text = (blurTrackbar.Value / 100f).ToString();
            }
            else
            {
                rainPanel.Enabled = false;
                rainPanel.Visible = false;
                atmospherePanel.Enabled = true;
                atmospherePanel.Visible = true;

                atmoSizeTrackbar.Value = Convert.ToInt32(WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].size * 100f);
                atmoSizePer.Text = (atmoSizeTrackbar.Value / 100f).ToString();

                atmoHorizontalTrackbar.Value = Convert.ToInt32(WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].horiSpeed * 100f);
                atmoHorizontalPer.Text = (atmoHorizontalTrackbar.Value / 100f).ToString();

                atmoVerticalTrackbar.Value = Convert.ToInt32(WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].vertSpeed * 100f);
                atmoVerticalPer.Text = (atmoVerticalTrackbar.Value / 100f).ToString();

                atmoDensityTrackbar.Value = Convert.ToInt32(WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].density * 100f);
                atmoDensityPer.Text = (atmoDensityTrackbar.Value / 100f).ToString();

                atmoColorBtn.BackColor = System.Drawing.ColorTranslator.FromHtml(WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].color);
            }

        }

        private void rainTrailPercent_Click(object sender, EventArgs e)
        {

        }

        private void resetRainbtn_Click(object sender, EventArgs e)
        {
            SaveWeatherParameters(true);
            UpdateWeatherParameterTab();
        }

        private void atmoDensityTrackbar_Scroll(object sender, EventArgs e)
        {
            atmoDensityPer.Text = (atmoDensityTrackbar.Value / 100f).ToString();
            WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].density = (atmoDensityTrackbar.Value / 100f);
        }

        private void atmoColorBtn_Click(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            // allow the user from selecting a custom color.
            MyDialog.AllowFullOpen = true;
            // diable the user to get help. (The default is false.)
            MyDialog.ShowHelp = false;
            if (MyDialog.ShowDialog() == DialogResult.OK)
            {
                atmoColorBtn.BackColor = MyDialog.Color;
                WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].color = System.Drawing.ColorTranslator.ToHtml(MyDialog.Color); //convert color to html.
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveWeatherParameters(true);
            UpdateWeatherParameterTab();
        }

        private void atmoSizeTrackbar_Scroll(object sender, EventArgs e)
        {
            atmoSizePer.Text = (atmoSizeTrackbar.Value / 100f).ToString();
            WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].size = (atmoSizeTrackbar.Value / 100f);
        }

        private void atmoHorizontalTrackbar_Scroll(object sender, EventArgs e)
        {
            atmoHorizontalPer.Text = (atmoHorizontalTrackbar.Value / 100f).ToString();
            WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].horiSpeed = (atmoHorizontalTrackbar.Value / 100f);
        }

        private void atmoVerticalTrackbar_Scroll(object sender, EventArgs e)
        {
            atmoVerticalPer.Text = (atmoVerticalTrackbar.Value / 100f).ToString();
            WeatherParam.atmosphere[WeatherSelectBox.SelectedIndex - WeatherParam.rain.Length].vertSpeed = (atmoVerticalTrackbar.Value / 100f);
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {

        }

        string UserWeatherCodeToText(int code)
        {
            foreach (var item in WeatherParam.rain)
            {
                if (item.code == code)
                    return item.name;
            }
            foreach (var item in WeatherParam.atmosphere)
            {
                if (item.code == code)
                    return item.name;
            }

            return null;
        }

        private void atmoDensityPer_Click(object sender, EventArgs e)
        {

        }

        void WeatherParameterDropDownInitialSelection()
        {
            string weatherName = UserWeatherCodeToText(userSettings.userWeather);
            try
            {
                foreach (var boxElement in WeatherSelectBox.Items)
                {
                    if (weatherName == boxElement.ToString())
                    {
                        WeatherSelectBox.SelectedItem = boxElement;
                        return;
                    }
                }
                WeatherSelectBox.SelectedIndex = 0;
            }
            catch(Exception)
            {
                WeatherSelectBox.SelectedIndex = 0;
            }
        }

        private void processAddBtn_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "application (*.exe) |*.exe";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //var filePath = openFileDialog1.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    //MessageBox.Show(fileName);
                    ApplicationRule applicationObj = new ApplicationRule
                    {
                        processName = fileName,
                        sleep = true
                    };
                    appList.Add(applicationObj);
                    SerializeObject(appList);

                    RefreshProcessListBox();
                    processListBox.SelectedIndex = appList.Count-1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        int tmpIndex;
        private void processRemoveBtn_Click(object sender, EventArgs e)
        {
            if (processListBox.SelectedIndex >= 0)
            {
                try
                {
                    appList.RemoveAt(processListBox.SelectedIndex);
                    tmpIndex = processListBox.SelectedIndex;
                    SerializeObject(appList);
                    RefreshProcessListBox();
                    if(tmpIndex >=0)
                        processListBox.SelectedIndex = tmpIndex - 1;
                }
                catch (Exception)
                {
                    //MessageBox.Show("index error");
                }
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        //unused.
        void ProcessListBoxAllProcesses()
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process item in processlist)
            {
                processListBox.Items.Add(item.ProcessName);
            }
        }

     
        void AddItemProcessListBox()
        {
            processListBox.Items.Add(appList[appList.Count-1].processName);
        }

        private void processListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProcessRadioBtn();
        }

        void RefreshProcessListBox()
        {
            //not efficient, but cheap code >_>.
            processListBox.Items.Clear();
            foreach (var item in appList)
            {
                processListBox.Items.Add(item.processName);
            }
            UpdateProcessRadioBtn();
        }

        private void processAlwaysPauseBtn_CheckedChanged(object sender, EventArgs e)
        {
            if(processListBox.SelectedIndex >=0)
            {
                RadioButton rb = sender as RadioButton;
                if (rb.Checked) //workaround: event being fired twice.
                {
                    try
                    {
                        appList[processListBox.SelectedIndex].sleep = true;
                        SerializeObject(appList);
                        //RefreshProcessListBox();

                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("index error");
                    }
                }
            }
        }

        private void processAlwaysRunBtn_CheckedChanged(object sender, EventArgs e)
        {

            if (processListBox.SelectedIndex >= 0)
            {
                RadioButton rb = sender as RadioButton;
                if (rb.Checked)
                {
                    try
                    {
                        appList[processListBox.SelectedIndex].sleep = false;
                        SerializeObject(appList);
                        //RefreshProcessListBox();
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("index error");
                    }
                }
            }
        }

        private void debugBox_CheckedChanged(object sender, EventArgs e)
        {
            userSettings.bvar2 = debugBox.Checked;
        }

        void UpdateProcessRadioBtn()
        {
            if (processListBox.SelectedIndex >= 0)
            {
                try
                {
                    if (appList[processListBox.SelectedIndex].sleep == true)
                        processAlwaysPauseBtn.Select();// = true;
                    else
                        processAlwaysRunBtn.Select();// = false
                }
                catch (Exception)
                {
                    //MessageBox.Show("index error");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Deserialize(appList); // load application rule list for pause/unpause listboxview.
            RefreshProcessListBox();

            try
            {

                // richTextBox1.LoadFile(System.AppDomain.CurrentDomain.BaseDirectory +"\\help.rtf");
                richTextBox1.Rtf = Properties.Resources.readme;
            }
            catch(Exception exx)
            {
                richTextBox1.Text = exx.Message + "\nCorrupted download?";
            }

  
            userSettings = new UserSettings(); //load default values/settings  
            defaultSettings = new UserSettings();             //...default value object for resetting.

            Load_Data();  // load save data if it exists

            //loading initial values
            apiKeyBox.Text = userSettings.apiKey;
            clockBox.Checked = userSettings.isClock;
            weatherBox.Checked = userSettings.isWeather;
            uiHideBox.Checked = userSettings.autoUiHide;
            demoBox.Checked = userSettings.isDemo; //weather control
            nightshiftBox.Checked = userSettings.dayNightTint;
            autoUIBox.Checked = userSettings.autoUiColor;
            clockDrop.SelectedIndex = userSettings.clockType;
            scalingDrop.SelectedIndex = userSettings.imgScaling;
            startupBox.Checked = userSettings.runAtStartup;
            darkThemeBox.Checked = userSettings.sysColor;
            DXVABox.Checked = userSettings.isDXVA;
            checkBoxSunOverlay.Checked = userSettings.sun_overlay;
            qualityDrop.SelectedIndex = userSettings.blur_quality;
            pauseButton.Checked = userSettings.appFocusPause;
            batPowBox.Checked = userSettings.bvar1;
            debugBox.Checked = userSettings.bvar2;

            trackBar1.Value = userSettings.fps;
            label12.Text = trackBar1.Value.ToString();

            tmpbarval = 10 - userSettings.ivar1;
            blur_lvl_label.Text = tmpbarval.ToString();
            trackBar2.Value = userSettings.ivar1;

            if (userSettings.sysColor == true)
            {
                radioDarkTheme.Enabled = true;
                radioLightTheme.Enabled = true;

                if (userSettings.isLightTheme == false)
                {
                    radioLightTheme.Checked = false;
                    radioDarkTheme.Checked = true;
                }
                else
                {
                    radioLightTheme.Checked = true;
                    radioDarkTheme.Checked = false;
                }
            }
            else
            {
                radioDarkTheme.Enabled = false;
                radioLightTheme.Enabled = false;
            }

            try
            {
                if (userSettings.isMetric == true)
                {
                    weatherDrop.SelectedIndex = 0;
                }
                else
                {
                    weatherDrop.SelectedIndex = 1;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            weather_textbox.Text = userSettings.cityName;

            #region customise_weather
            LoadWeatherParameters();

            foreach (var item in WeatherParam.rain)
            {
                WeatherSelectBox.Items.Add(item.name);
            }
            foreach (var item in WeatherParam.atmosphere)
            {
                WeatherSelectBox.Items.Add(item.name);
            }
            WeatherParameterDropDownInitialSelection();         

            //MessageBox.Show(WeatherSelectBox.Items[0].ToString());
            UpdateWeatherParameterTab();

            #endregion
        }

    }
}
