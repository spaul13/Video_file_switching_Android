using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.MediaPlayer8.Scripts.Editor
{
    class CodeReferenceMenuItem
    {
        [MenuItem("frame8/MediaPlayer8/Code reference")]
        public static void OpenDoc()
        { Application.OpenURL("http://thefallengames.com/unityassetstore/mediaplayer8/doc"); }
    }
}
