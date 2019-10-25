using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Base class with common parameters for the MediaPlayer8
    /// </summary>
    [Serializable]
    public abstract class Parameters
    {
        [SerializeField]
        internal bool pauseOnAppPause = true;
        [SerializeField]
        internal bool resumeOnAppResume = true;
        //internal AndroidJavaObject controlListenerAJO;
        //internal AndroidJavaObject surfaceAJO;

        internal abstract AndroidJavaObject CreateAJO(AndroidJavaObject controlListenerProxy, IntPtr surfacePTR);

        protected virtual AndroidJavaObject CreateAJO(string className, AndroidJavaObject controlListenerProxy, IntPtr surfacePTR) 
        {
            var ajo = new AndroidJavaObject(className, controlListenerProxy, null); // passing the controlListener only, as the surface must be set via JNI below

            AndroidJNI.SetObjectField(ajo.GetRawObject(), AndroidJNIHelper.GetFieldID(ajo.GetRawClass(), "surface"), surfacePTR);
            ajo.Set("pauseOnAppPause", pauseOnAppPause);
            ajo.Set("resumeOnAppResume", resumeOnAppResume);

            return ajo;
        }
    }
}
