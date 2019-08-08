using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Octokit;
using System.Threading.Tasks;
using System;

/// <summary>
/// Software update check.
/// </summary>
public class gitrequest : MonoBehaviour {

    int cnt = 0;
    bool failed = false;
    private async void Start()
    {
        //Only run outside of Unity Editor.
        if (UnityEngine.Application.isEditor == false)
        {
            Debug.Log("Calling update asycnthread");
            await AsyncThread();
        }
    }

    /// <summary>
    /// Compares software with github release tag.
    /// </summary>
    /// <remarks>
    /// Retries 6 times before stopping.
    /// </remarks>
    private async Task AsyncThread()
    {
        failed = false;
        await Task.Delay(30000); //30sec delay
        try
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("rePaper"));
            var releases = await client.Repository.Release.GetAll("rocksdanister", "rePaper");
            var latest = releases[0];

            string tmp = latest.TagName.Replace("v", string.Empty);
            var gitVersion = new Version(tmp);
            var unityVersion = new Version(UnityEngine.Application.version);
            var result = gitVersion.CompareTo(unityVersion);
            if (result > 0)
            {
                Debug.Log("git is greater");
                main.instance.update.Text = "Update Available!";
                main.instance.update.Enabled = true;
                main.instance.tray.ShowNotification(1000, "Update available!", "A new version of rePaper is available..");
            }
            else if (result < 0)
            {
                Debug.Log("unity is greater");    
                main.instance.update.Text = "Something Seems Off?";
                main.instance.update.Enabled = true;
            }
            else
            {
                Debug.Log("versions are equal");
                main.instance.update.Text = "Software is up-to-date";
                main.instance.update.Enabled = true;
            }
            
        }
        catch(Exception e) //can trigger if systray instance is null, but why would it?
        {
            cnt = cnt + 1;
            failed = true;
            Debug.Log("otokit fail cnt: " + cnt + " " + e + " " + e.Message);
        }

        if (failed == true && cnt < 5 )
        {
            await Task.Delay(60000); //1min, retry
            await AsyncThread();
        }
        
        if(cnt >= 5)
        {
            Debug.Log("ops no internet maybe? async update check failed");
            if (UnityEngine.Application.isEditor == false)
            {
                main.instance.update.Text = "Update Check Failed?";
                main.instance.update.Enabled = true;
            }
        }
         
    }
}
