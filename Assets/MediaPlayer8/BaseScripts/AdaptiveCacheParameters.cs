using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Parameters used with AdaptiveCacheMediaPlayer8
    /// The default values are taken from the native side
    /// </summary>
    [Serializable]
    public class AdaptiveCacheParameters : Parameters
    {
        internal const string PACKAGE_QUALIFIED_JAVA_CLASS = C.PACKAGE_EXTENSIONSFRAME8_ADAPTIVECACHE + ".AdaptiveCacheMediaPlayer8Params";

        /// <summary>
        /// The max disk cache size allowed represented as a percentage of the available disk for the current application (not the globally available space)
        /// A number in interval [0, 1]
        /// </summary>
        [SerializeField]
        float maxDiskCacheSizeFactorOfFree = .5f;
        /// <summary>
        /// The max ram cache size allowed represented as a percentage of the available RAM for the current application (not the globally available RAM)
        /// A number in interval [0, 1]
        /// </summary>
        [SerializeField]
        float maxRAMCacheSizeFactorOfAllowed = .9f;


        internal override AndroidJavaObject CreateAJO(AndroidJavaObject controlListenerProxy, IntPtr surfacePTR)
        {
            var ajo = base.CreateAJO(PACKAGE_QUALIFIED_JAVA_CLASS, controlListenerProxy, surfacePTR);
            ajo.Call("init", (double)maxDiskCacheSizeFactorOfFree, (double)maxRAMCacheSizeFactorOfAllowed);

            return ajo;
        }
    }
}
