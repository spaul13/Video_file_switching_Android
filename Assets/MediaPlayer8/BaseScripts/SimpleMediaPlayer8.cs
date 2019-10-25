using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEngine.Serialization;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// The "Simple" module of the MediaPlayer8, that includes all of the streaming capabilities and local file playback
    /// Video/Audio
    /// </summary>
    public class SimpleMediaPlayer8 : MediaPlayer8
    {
        [SerializeField] RenderingSetup _Rendering;
        [SerializeField] SimpleParameters _Params;

        public new SimpleControllerProxy NativeController { get { return _NativeController; } }
        public override RenderingSetup Rendering { get { return _Rendering; } }

        protected override Parameters Params { get { return _Params; } }

        SimpleControllerProxy _NativeController;


        protected override ControllerProxy CreateControllerProxy() { return _NativeController = new SimpleControllerProxy(); }


        protected override bool LoadViaController(string mediaPath, Action<string> onDone, Action<AndroidJavaObject> onError)
        {
            Debug.Log("Sibu: Inside simpleMediaPlayer8, Load via controller");
            if (_NativeController == null)
                return false;

            if (_NativeController.Load(mediaPath))
            {
                if (onDone != null)
                    onDone(mediaPath);
                return true;
            }

            if (onError != null)
                onError(null);
            return false;
        }

    }
}
