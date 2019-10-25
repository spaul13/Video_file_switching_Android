using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEngine.Serialization;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// The "AdaptiveCache" module of the MediaPlayer8
    /// <para>The purpose of this component is to stream H.264 content without any interruptions due to network slow speed. </para>
    /// <para>The "adaptive" part takes care of disk+ram caching &amp; network speed predictions so after the initial waiting time, </para>
    /// <para>the video will play without any re-buffering (given there is enough disk and/or ram available). No disk or ram resources </para>
    /// <para>are used beyond those needed to achieve no interruptions. </para>
    /// <para>Another feature of the "AdaptiveCache" module is fragmented caching: nothing is wasted; if you re-wind, the content is played </para>
    /// <para>from disk(there is a limit for disk cache's size, of course)</para>
    /// </summary>
    public class AdaptiveCacheMediaPlayer8 : MediaPlayer8
    {
        [SerializeField] RenderingSetup _Rendering;
        [SerializeField] AdaptiveCacheParameters _Params;

        public new AdaptiveCacheControllerProxy NativeController { get { return _NativeController; } }
        public override RenderingSetup Rendering { get { return _Rendering; } }

        protected override Parameters Params { get { return _Params; } }

        AdaptiveCacheControllerProxy _NativeController;


        protected override ControllerProxy CreateControllerProxy() { return _NativeController = new AdaptiveCacheControllerProxy(); }

        protected override bool LoadViaController(string mediaPath, Action<string> onDone, Action<AndroidJavaObject> onError)
        {
            if (_NativeController == null)
                return false;

            return _NativeController.Load(mediaPath, onDone, onError);
        }
    }
}
