using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Implementation of BaseDebugPanel for the Simple module
    /// </summary>
    public class SimpleDebugPanel : BaseDebugPanel
    {
        protected override MediaPlayer8 Player { get { return _SimpleMediaPlayer; } }
        SimpleMediaPlayer8 _SimpleMediaPlayer;

        internal override void RetrievePlayer()
        { _SimpleMediaPlayer = _MediaPlayerGO.GetComponent<SimpleMediaPlayer8>(); }
    }
}