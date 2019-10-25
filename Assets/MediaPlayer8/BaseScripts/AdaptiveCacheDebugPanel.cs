using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Implementation of BaseDebugPanel for the AdaptiveCache module
    /// </summary>
    public class AdaptiveCacheDebugPanel : BaseDebugPanel
    {
        /// <summary>
        /// <para>"Displays" the cached fragments on disk</para>
        /// <para>If not assigned, will try to find it at path 'DebugLines/Line1/Text'</para>
        /// </summary>
        [SerializeField] [Tooltip("If not assigned, will try to find it at path 'DebugLines/Line1/Text'")]
        Text _DebugBufLine1Text;

        /// <summary>
        /// <para>Displays indicators for: </para>
        /// <para>-current position in the time space(asterisk)</para>
        /// <para>-current position in the byte array space(apostrophe)</para>
        /// <para>-current buffer position in the byte array space(comma)</para>
        /// <para>other symbols are shown when the indicators overlap</para>
        /// <para>If not assigned, will try to find it at path 'DebugLines/Line2/Text'</para>
        /// </summary>
        [SerializeField] [Tooltip("If not assigned, will try to find it at path 'DebugLines/Line2/Text'")]
        Text _DebugBufLine2Text;

        protected override MediaPlayer8 Player { get { return _AdaptiveCacheMediaPlayer; } }

        AdaptiveCacheMediaPlayer8 _AdaptiveCacheMediaPlayer;


        internal override void RetrievePlayer()
        { _AdaptiveCacheMediaPlayer = _MediaPlayerGO.GetComponent<AdaptiveCacheMediaPlayer8>(); }

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();

            GameObject debugLinesParent = transform.Find("DebugLines").gameObject;
            if (debugLinesParent)
            {
                if (!_DebugBufLine1Text)
                    _DebugBufLine1Text = debugLinesParent.transform.Find("Line1/Text").GetComponent<Text>();
                if (!_DebugBufLine2Text)
                    _DebugBufLine2Text = debugLinesParent.transform.Find("Line2/Text").GetComponent<Text>();
                debugLinesParent.SetActive(true);
            }
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            char[] c = new char[0];
            if (_AdaptiveCacheMediaPlayer.NativeController.GetBufDebugLine1(ref c))
                _DebugBufLine1Text.text = new string(c);
            if (_AdaptiveCacheMediaPlayer.NativeController.GetBufDebugLine2(ref c))
                _DebugBufLine2Text.text = new string(c);
        }
    }
}