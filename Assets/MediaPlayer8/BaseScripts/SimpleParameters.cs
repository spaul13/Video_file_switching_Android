using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Parameters used with SimpleMediaPlayer8
    /// The default values are taken from the native side
    /// </summary>
    [Serializable]
    public class SimpleParameters : Parameters
    {
        internal const string PACKAGE_QUALIFIED_JAVA_CLASS = C.PACKAGE_EXTESIONSFRAME8 + ".SimpleMediaPlayer8Params";

        // The default values are taken from the native side
        /// <summary>
        /// The minimum duration of media that the player will attempt to ensure is buffered at all times
        /// </summary>
        [SerializeField]
        float minBufferSeconds = 15f;

        /// <summary>
        /// The maximum duration of media that the player will attempt buffer
        /// </summary>
        [SerializeField]
        float maxBufferSeconds = 30f;

        /// <summary>
        /// The duration of media that must be buffered for playback to start or resume following a user action such as a seek
        /// </summary>
        [SerializeField]
        float bufferForPlaybackSeconds = 2.5f;

        /// <summary>
        /// The default duration of media that must be buffered for playback to resume after a rebuffer. 
        /// A rebuffer is defined to be caused by buffer depletion rather than a user action
        /// </summary>
        [SerializeField]
        float bufferForPlaybackAfterRebufferSeconds = 5f;


        internal override AndroidJavaObject CreateAJO(AndroidJavaObject controlListenerProxy, IntPtr surfacePTR)
        {
            var ajo = base.CreateAJO(PACKAGE_QUALIFIED_JAVA_CLASS, controlListenerProxy, surfacePTR);
            ajo.Call("init", 
                (int)(minBufferSeconds * 1000), 
                (int)(maxBufferSeconds * 1000), 
                (long)(bufferForPlaybackSeconds * 1000), 
                (long)(bufferForPlaybackAfterRebufferSeconds * 1000)
            );

            return ajo;
        }
    }
}
