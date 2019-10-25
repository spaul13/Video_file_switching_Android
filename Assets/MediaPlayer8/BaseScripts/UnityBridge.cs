using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Currently, used for constructing the native side of the player
    /// </summary>
    public class UnityBridgeAJC : AndroidJavaClass
    {
        internal const string PACKAGE_QUALIFIED_JAVA_CLASS = C.PACKAGE_EXOPLAYERFORUNITY + ".UnityBridge";

        internal AndroidJavaObject UnityHandler { get { return CallStatic<AndroidJavaObject>("getUnityHandler"); } }


        internal UnityBridgeAJC() : base(PACKAGE_QUALIFIED_JAVA_CLASS) {}


        internal void initOrReinit(AndroidJavaObject mediaPlayerParamsAJO) { CallStatic("initOrReinit", mediaPlayerParamsAJO); }

        internal new void Dispose()
        { try { base.Dispose(); } catch { } }
    }
}
