using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Stores and fires common events from the native side
    /// </summary>
    public class PlaybackEventsProxy : AndroidJavaProxy, IDisposable
    {
        internal const string PACKAGE_QUALIFIED_JAVA_INTERFACE = C.PACKAGE_EXTESIONSFRAME8 + ".PlaybackListener";
        internal const string PACKAGE_QUALIFIED_CALL_WRAPPER_JAVA_CLASS = PACKAGE_QUALIFIED_JAVA_INTERFACE + "$CallWrapper";

        public Action<bool> timelineChanged;
        public Action<bool> loadingChanged;
		public Action<bool, PlayerState> playerStateChanged;
        public Action<string> playerError;
        public Action<int, int, int, float> videoSizeChanged;
        public Action renderedFirstFrame;
        //public Action videoTracksDisabled;
        /// <summary>
        /// Gives decoder-scoped errors. Usually, not that useful
        /// </summary>
        public Action<string> internalLoadError;


        public PlaybackEventsProxy() : base(PACKAGE_QUALIFIED_JAVA_INTERFACE)
        {
            loadingChanged = _ => { };
            playerStateChanged = (_, __) => { };
            playerError = _ => { };
            videoSizeChanged = (_, __, ___, ____) => { };
            renderedFirstFrame = () => { };
            //videoTracksDisabled = () => { };
            internalLoadError = _ => { };
        }

        #region ExoPlayer.EventListener
        void onTimelineChanged(AndroidJavaObject timeline, AndroidJavaObject manifest, int reason) { }
        void onTracksChanged(AndroidJavaObject trackGroups, AndroidJavaObject trackSelections) { }
		void onLoadingChanged(bool isLoading) { loadingChanged(isLoading); }
        void onPlayerStateChanged(bool playWhenReady, int playbackState) { playerStateChanged(playWhenReady, (PlayerState)playbackState); }
        void onRepeatModeChanged(int repeatMode) { }
        void onShuffleModeEnabledChanged(bool shuffleModeEnabled) { }
		/// <param name="exoException"> is of type com.google.android.exoplayer2.ExoPlaybackException</param>
		void onPlayerError(AndroidJavaObject exoException) { playerError(exoException == null ? "" : (exoException.Call<string>("getMessage") ?? "")); }
        void onPositionDiscontinuity(int reason) { }
        void onPlaybackParametersChanged(AndroidJavaObject playbackParameters) { }
        void onSeekProcessed() { }
		#endregion

		#region SimpleExoPlayer.VideoListener
		void onVideoSizeChanged(int width, int height, int unappliedRotationDegrees, float pixelWidthHeightRatio) { videoSizeChanged(width, height, unappliedRotationDegrees, pixelWidthHeightRatio); }
        void onRenderedFirstFrame() { renderedFirstFrame();  }
        //void onVideoTracksDisabled() { videoTracksDisabled(); }
        #endregion

        void onInternalLoadError(AndroidJavaObject throwable) { internalLoadError(throwable == null ? "" : (throwable.Call<string>("getMessage") ?? "")); }

        public void Dispose()
        {
            javaInterface.Dispose();
        }
    }
}