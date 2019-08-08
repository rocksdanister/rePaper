//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MediaPlayer;

public class MediaItemsUI : MonoBehaviour
{
    public List<string> UrlItems = new List<string>();
    public List<string> LocalItems = new List<string>();

    private Playback mediaPlayback;
    private Dropdown mediaList;

    private void Awake()
    {
        this.mediaPlayback = FindObjectOfType<Playback>();
        if (this.mediaPlayback == null)
        {
            Debug.LogError("Cannot find MediaPlayer in the scene.\n");
            return;
        }

        this.mediaList = GetComponentInChildren<Dropdown>();
        if (this.mediaList == null)
        {
            Debug.LogError("There wasn't a DropDown in the GameObject hierarchy.\n");
            return;
        }

        // the LocalItems will need to be placed in streaming assets folder
        for(var index = 0; index < this.LocalItems.Count; ++index)
        {
            // ensure the path is absolute
            string item = this.LocalItems[index];
            if (!item.Contains("/StreamingAssets"))
                item = (Application.dataPath + "/StreamingAssets" + "/" + item).Replace("/", "\\");

            this.LocalItems[index] = item;
        }

        // populate the list
        this.mediaList.ClearOptions();
        this.mediaList.AddOptions(this.LocalItems);
        this.mediaList.AddOptions(this.UrlItems);

        var buttonList = GetComponentsInChildren<Button>();
        if (buttonList.Length == 0)
        {
            Debug.LogError("There are no buttons in the GameObject hierarchy.\n");
            Debug.LogError("Place this script on the root of the UI.");

            return;
        }

        foreach (var button in buttonList)
        {
            var label = button.GetComponentInChildren<Text>();
            button.onClick.AddListener(() => Button_OnClick(label));
        }
    }

    private void Start()
    {
       // this.mediaPlayback.Play("K:/360 VR Videos/Demo2/nyc.mp4");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            var selectedItem = mediaList.options[mediaList.value].text;
            this.mediaPlayback.Play(selectedItem);
        }
    }

    private void Button_OnClick(Text label)
    {
        if (label == null)
        {
            return;
        }

        switch(label.text)
        {
            case "Play":
                var selectedItem = mediaList.options[mediaList.value].text;
                this.mediaPlayback.Play("K:/360 VR Videos/Demo2/nyc.mp4");
                break;
            case "Pause":
                this.mediaPlayback.Pause();
                break;
            case "Stop":
                this.mediaPlayback.Stop();
                break;
            case "Current":
                long position = this.mediaPlayback.GetPosition();
                Debug.Log((position).ToString());

                double rate = this.mediaPlayback.GetPlaybackRate();
                Debug.Log("playback rate: " + rate);
                break;
            case "Duration":
                long duration = this.mediaPlayback.GetDuration();
                Debug.Log((duration).ToString());
                break;
            case "Forward":
                long dur = this.mediaPlayback.GetDuration();
                long cur = this.mediaPlayback.GetPosition();

                if (cur < dur)
                {
                    this.mediaPlayback.SetPosition(cur + 30000000);
                }
                break;
            case "Backward":
                long durr = this.mediaPlayback.GetDuration();
                long curr = this.mediaPlayback.GetPosition();

                if (curr < durr)
                {
                    this.mediaPlayback.SetPosition(curr - 30000000);
                }

                break;
            default:
                Debug.LogWarningFormat("Button '{0}' not handled.", label.text);
                break;
        }
    }
}
