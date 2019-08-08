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

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace MediaPlayer
{
    /// <summary>
    /// DXVA videoplayer setup.
    /// </summary>
    public class Playback : MonoBehaviour
    {
        
        // store the current position
        private Int64 currentPosition = 0;
        
        // state handling
        public Action<object, ChangedEventArgs<PlaybackState>> PlaybackStateChanged;

        public PlaybackState State
        {
            get { return this.currentState; }
            private set
            {
                if (this.currentState != value)
                {
                    this.previousState = this.currentState;
                    this.currentState = value;
                    if (this.PlaybackStateChanged != null)
                    {
                        var args = new ChangedEventArgs<PlaybackState>(this.previousState, this.currentState);
                        this.PlaybackStateChanged(this, args);
                    }
                }
            }
        }
        private PlaybackState currentState = PlaybackState.None;
        private PlaybackState previousState = PlaybackState.None;

        private Texture2D playbackTexture;
        private Plugin.StateChangedCallback stateCallback;

        /// <summary>
        /// Play - plays the currently loaded media
        /// </summary>
        /// <param name="selectedItem"></param>
        public void Play(string selectedItem)
        {
            CheckHR(Plugin.LoadContent(selectedItem));

            if(currentPosition != 0)
            {
               // Debug.Log("playing at " + currentPosition + ", current state: ?");

                CheckHR(Plugin.SetPosition(currentPosition));
            }
            CheckHR(Plugin.Play());
        }

        /// <summary>
        /// Pause - Gets the current playback position, then pauses media
        /// </summary>
        public void Pause()
        {
            CheckHR(Plugin.GetPosition(out currentPosition));
            CheckHR(Plugin.Pause());
           // Debug.Log("Paused at " + currentPosition);
        }

        /// <summary>
        /// Stop - Stops media playback
        /// </summary>
        public void Stop()
        {
            CheckHR(Plugin.Stop());
        }

        /// <summary>
        /// GetPosition - Get the current playtback position
        /// </summary>
        /// <returns></returns>
        public long GetPosition()
        {
            Int64 position = 0;
            CheckHR(Plugin.GetPosition(out position));
            return position;
        }

        /// <summary>
        /// GetDuration - Get the duration of the currnetly loaded media
        /// </summary>
        /// <returns></returns>
        public long GetDuration()
        {
            Int64 duration = 0;
            CheckHR(Plugin.GetDuration(out duration));
            return duration;
        }

        /// <summary>
        /// GetPlaybackRate - Gets media playback rate
        /// </summary>
        /// <returns></returns>
        public Double GetPlaybackRate()
        {
            Double rate = 0;
            CheckHR(Plugin.GetPlaybackRate(out rate));
            return rate;
        }

        /// <summary>
        /// SetPosition - Sets media playback 
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(long position)
        {
            CheckHR(Plugin.SetPosition(position));
        }

        IEnumerator Start()
        {
            yield return StartCoroutine("CallPluginAtEndOfFrames");
        }

        
        /// <summary>
        /// Called once this gameobject as become enabled
        /// </summary>
        private void OnEnable()
        {/*
            // create callback
            this.stateCallback = new Plugin.StateChangedCallback(MediaPlayback_Changed);

            // create media playback
            CheckHR(Plugin.CreateMediaPlayback(this.stateCallback));

            // create native texture for playback
            IntPtr nativeTexture = IntPtr.Zero;
            CheckHR(Plugin.CreatePlaybackTexture((uint)Screen.currentResolution.width, (uint)Screen.currentResolution.height, out nativeTexture));

            // create the unity texture2d 
            this.playbackTexture = Texture2D.CreateExternalTexture((int)Screen.currentResolution.width, (int)Screen.currentResolution.height, TextureFormat.BGRA32, false, false, nativeTexture);

            // set texture for the shader
            GetComponent<Renderer>().material.mainTexture = this.playbackTexture;
            */
        }
        

        // my OnEnable

        public void CustomOnEnable()
        {
            // create callback
            this.stateCallback = new Plugin.StateChangedCallback(MediaPlayback_Changed);

            // create media playback
            CheckHR(Plugin.CreateMediaPlayback(this.stateCallback));

            // create native texture for playback
            IntPtr nativeTexture = IntPtr.Zero;
            CheckHR(Plugin.CreatePlaybackTexture((uint)Screen.currentResolution.width, (uint)Screen.currentResolution.height, out nativeTexture));

            // create the unity texture2d 
            this.playbackTexture = Texture2D.CreateExternalTexture((int)Screen.currentResolution.width, (int)Screen.currentResolution.height, TextureFormat.BGRA32, false, false, nativeTexture);

            // set texture for the shader
            GetComponent<Renderer>().material.mainTexture = this.playbackTexture;

        }
        //my func
        public void DestroyTexture()
        {
            if(this.playbackTexture !=null)
                DestroyImmediate(this.playbackTexture);
        }

        /// <summary>
        /// Called once the gameobjet has een disabled
        /// </summary>
        private void OnDisable()
        {
            Plugin.ReleaseMediaPlayback();
        }

        /// <summary>
        /// Callback for media playback change event (media state change)
        /// </summary>
        /// <param name="args"></param>
        private void MediaPlayback_Changed(Plugin.PLAYBACK_STATE args)
        {
#if UNITY_UWP
    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
    {
        OnStateChanged(args);
    }, false);
#else
            OnStateChanged(args);
#endif
        }

        private void OnStateChanged(Plugin.PLAYBACK_STATE args)
        {
            var state = (PlaybackState)args.type;
            switch (state)
            {
                case PlaybackState.Buffering: 
                    break;
                case PlaybackState.Ended:
                    break;
                case PlaybackState.None:
                    break;
                case PlaybackState.Paused:
                    break;
                case PlaybackState.Playing:
                    break;
            }

            var stateType = (Plugin.StateType)Enum.ToObject(typeof(Plugin.StateType), args.type);
            switch (stateType)
            {
                case Plugin.StateType.StateType_StateChanged:
                    this.State = (PlaybackState)Enum.ToObject(typeof(PlaybackState), args.state);
                    //Debug.Log("State Type: " + stateType.ToString() + " - " + this.State.ToString());
                    break;
                case Plugin.StateType.StateType_Opened:
                    // Only output this information on initial play
                    if (currentPosition == 0)
                    {
                        Plugin.MEDIA_DESCRIPTION description = args.description;
                        //Debug.Log("Opening Media:");
                        //Debug.Log(" Width = " + description.width.ToString());
                        //Debug.Log(" Height = " + description.height.ToString());
                        //Debug.Log(" Duration = " + description.duration.ToString());
                        //Debug.Log(" Seekable = " + description.isSeekable.ToString());
                    }
                    break;
                case Plugin.StateType.StateType_Failed:
                    CheckHR(args.hresult);
                    break;
                default:
                    break;
            }
        }

        private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();

                // Set time for the plugin
                Plugin.SetTimeFromUnity(Time.timeSinceLevelLoad);

                // Issue a plugin event with arbitrary integer identifier.
                // The plugin can distinguish between different
                // things it needs to do based on this ID.
                // For our simple plugin, it does not matter which ID we pass here.
                GL.IssuePluginEvent(Plugin.GetRenderEventFunc(), 1);
            }
        }

        //..backup
        /*
         private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();

                // Set time for the plugin
                Plugin.SetTimeFromUnity(Time.timeSinceLevelLoad);

                // Issue a plugin event with arbitrary integer identifier.
                // The plugin can distinguish between different
                // things it needs to do based on this ID.
                // For our simple plugin, it does not matter which ID we pass here.
                GL.IssuePluginEvent(Plugin.GetRenderEventFunc(), 1);
            }
        }
        */

        public static void CheckHR(long hresult)
        {
            if (hresult != 0)
            {
               // Debug.LogError("Media Failed: HRESULT = 0x" + hresult.ToString("X", System.Globalization.NumberFormatInfo.InvariantInfo));
            }
        }

        private static class Plugin
        {
            public enum StateType
            {
                StateType_None = 0,
                StateType_Opened,
                StateType_StateChanged,
                StateType_Failed,
            };

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct MEDIA_DESCRIPTION
            {
                public UInt32 width;
                public UInt32 height;
                public Int64 duration;
                public byte isSeekable;

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("width: " + width);
                    sb.AppendLine("height: " + height);
                    sb.AppendLine("duration: " + duration);
                    sb.AppendLine("canSeek: " + isSeekable);

                    return sb.ToString();
                }
            };

            [StructLayout(LayoutKind.Explicit, Pack = 4)]
            public struct PLAYBACK_STATE
            {
                [FieldOffset(0)]
                public UInt16 type;

                [FieldOffset(4)]
                public UInt16 state;

                [FieldOffset(4)]
                public Int64 hresult;

                [FieldOffset(4)]
                public MEDIA_DESCRIPTION description;

                [FieldOffset(4)]
                public Int64 position;
            };

            public delegate void StateChangedCallback(PLAYBACK_STATE args);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateMediaPlayback")]
            internal static extern long CreateMediaPlayback(StateChangedCallback callback);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "ReleaseMediaPlayback")]
            internal static extern void ReleaseMediaPlayback();

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreatePlaybackTexture")]
            internal static extern long CreatePlaybackTexture(UInt32 width, UInt32 height, out System.IntPtr playbackTexture);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "LoadContent")]
            internal static extern long LoadContent([MarshalAs(UnmanagedType.BStr)] string sourceURL);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "Play")]
            internal static extern long Play();

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "Pause")]
            internal static extern long Pause();

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "Stop")]
            internal static extern long Stop();

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "GetPosition")] 
            internal static extern long GetPosition(out Int64 position);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDuration")] 
            internal static extern long GetDuration(out Int64 duration);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "GetPlaybackRate")]
            internal static extern long GetPlaybackRate(out Double duration);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "SetPosition")] 
            internal static extern long SetPosition(Int64 position);

            // Unity plugin
            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "SetTimeFromUnity")]
            internal static extern void SetTimeFromUnity(float t);

            [DllImport("MediaPlayback", CallingConvention = CallingConvention.StdCall, EntryPoint = "GetRenderEventFunc")]
            internal static extern IntPtr GetRenderEventFunc();
        }
    }
}