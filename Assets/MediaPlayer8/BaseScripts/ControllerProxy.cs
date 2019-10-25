using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// The base controller functionality for the 2 core modules: Simple and AdaptiveCache
    /// </summary>
    public abstract class ControllerProxy : AndroidJavaProxy, IDisposable
    {
        internal const string PACKAGE_QUALIFIED_JAVA_INTERFACE = C.PACKAGE_EXTESIONSFRAME8 + ".ControlListener";
        internal const string PACKAGE_QUALIFIED_CALL_WRAPPER_JAVA_CLASS = PACKAGE_QUALIFIED_JAVA_INTERFACE + "$CallWrapper";

        /// <summary> The currently played media's location</summary>
        public string MediaPath { get; private set; }

        /// <summary> If ready to load media a source </summary>
        public bool Initialized { get { return _MediaPlayerFragment != null; } }

        /// <summary>
        /// Used to subscribe to common events from the native side
        /// </summary>
        public PlaybackEventsProxy Events { get; private set; }

        protected AndroidJavaObject _MediaPlayerFragment;
        protected UnityBridgeAJC _UnityBridgeAJC;

        protected Action _CurrentInitSuccessCallback;
        protected Action<AndroidJavaObject> _CurrentInitErrorCallback;

        public ControllerProxy() : base(PACKAGE_QUALIFIED_JAVA_INTERFACE)
        {
            _UnityBridgeAJC = new UnityBridgeAJC();
            Events = new PlaybackEventsProxy();
        }

        #region commands
        /// <summary>
        /// Initializes native components
        /// </summary>
        /// <param name="androidSurfaceIntPtr"></param>
        /// <param name="mediaPlayerParams"></param>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        internal virtual void Init(IntPtr androidSurfaceIntPtr, Parameters mediaPlayerParams, Action onDone, Action<AndroidJavaObject> onError)
        {
            _CurrentInitSuccessCallback = onDone;
            _CurrentInitErrorCallback = onError;

            // Setting a wrapper for the interface, so we'll receive the events on unity's thread
            var wrapperToThis =
                new AndroidJavaObject(
                    PACKAGE_QUALIFIED_CALL_WRAPPER_JAVA_CLASS,
                    _UnityBridgeAJC.UnityHandler,
                    this
                );

            var mediaPlayerParamsAJO =  mediaPlayerParams.CreateAJO(wrapperToThis, androidSurfaceIntPtr);
            _UnityBridgeAJC.initOrReinit(mediaPlayerParamsAJO);
        }

        /// <summary>
        /// Loads a media from URL. SetPlayWhenReady(true) can be called at anytime to make it auto-play
        /// </summary>
        /// <param name="url"></param>
        /// <returns>if accepted</returns>
        internal virtual bool Load(string url)
        {
            if (_MediaPlayerFragment == null)
                return false;

            return Load_SkipChecks(url);
        }

        /// <summary>
        /// Sets if to play the media after it's loaded (in case it's loading or will load). OR sets if to play/pause the media in case it's ready for playback or already playing
        /// </summary>
        /// <param name="playWhenReady"></param>
        /// <returns>if accepted</returns>
        public bool SetPlayWhenReady(bool playWhenReady)
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<bool>("setPlayWhenReady", new object[] { playWhenReady });
            return false;
        }

        public bool GetPlayWhenReady()
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<bool>("getPlayWhenReady");
            return false;
        }

        /// <summary>
        /// If to collect debug info. May slow down the framerate a little bit
        /// </summary>
        /// <param name="value"></param>
        /// <returns>if accepted</returns>
        public bool SetCollectDebugInfo(bool value)
        {
            if (_MediaPlayerFragment != null)
            {
                _MediaPlayerFragment.Call("setCollectDebugInfo", new object[] { value });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the debug info into the specified string
        /// </summary>
        /// <param name="str"></param>
        /// <returns>if accepted</returns>
        public bool GetDebugInfo(ref string str)
        {
            if (_MediaPlayerFragment != null)
            {
                str = _MediaPlayerFragment.Call<string>("getDebugInfo");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get's the normalized progress
        /// </summary>
        /// <returns></returns>
        public double GetProgress01()
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<double>("getProgress01");
            return -1d;
        }

        /// <summary>
        /// Get's the normalized buffer level as "the last contiguously cached nanosecond from the current position / total duration in nanoseconds".
        /// <para>TL;DR: use it as the normalizedValue for a Slider to display the currently buffered amount in seconds</para>
        /// </summary>
        /// <returns></returns>
        public double GetBufferLevel01RelativeToDuration()
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<double>("getBufferLevel01RelativeToDuration");
            return -1d;
        }

        /// <summary>
        /// values >=1 mean a low probability of interrruptions. the closer to 0, the bigger the chance of interruptions
        /// </summary>
        /// <returns></returns>
        public double GetNormalizedBufferHealth()
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<double>("getNormalizedBufferHealth");
            return -1d;
        }

        /// <summary>
        /// The currenttly played position
        /// </summary>
        /// <returns></returns>
        public double GetProgressSeconds()
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<double>("getProgressSeconds");
            return -1d;
        }

        public double GetDurationSeconds()
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<double>("getDurationSeconds");
            return -1d;
        }

        public float GetVolume()
        {
            if (_MediaPlayerFragment == null)
                return -1f;

            return _MediaPlayerFragment.Call<float>("getVolume"); ;
        }

        /// <summary>
        /// Requesting to set the volume
        /// </summary>
        /// <param name="v01"></param>
        /// <returns>if accepted</returns>
        public bool RequestSetVolume(float v01)
        {
            if (_MediaPlayerFragment == null)
                return false;

            return _MediaPlayerFragment.Call<bool>("setVolume", v01 ); ;
        }

        /// <summary>
        /// Requests a seek
        /// </summary>
        /// <param name="position01"></param>
        /// <returns>if accepted</returns>
        public virtual bool RequestSeek01(double position01)
        {
            if (_MediaPlayerFragment == null)
                return false;

            return RequestSeek01_SkipChecks(position01);
        }

        /// <summary>
        /// Stops everything. Useful when shutting down the player.
        /// <para>A new Load() call should be done in order to re-use the player, in case disposal was not intended</para>
        /// </summary>
        /// <returns>if accepted</returns>
        public virtual bool StopPlaybackAndLoading()
        {
            if (_MediaPlayerFragment == null)
                return false;

            return StopPlaybackAndLoading_SkipChecks();
        }
        protected bool Load_SkipChecks(string url) { MediaPath = url; return _MediaPlayerFragment.Call<bool>("load", url); }
        protected bool RequestSeek01_SkipChecks(double position01) { return _MediaPlayerFragment.Call<bool>("requestSeek01", position01); }
        protected bool StopPlaybackAndLoading_SkipChecks() { return _MediaPlayerFragment.Call<bool>("stopPlaybackAndLoading"); }
        #endregion

        #region callbacks
        /// <summary>
        /// Called from the native side when the native controller is initialized
        /// </summary>
        /// <param name="exoFragment"></param>
        protected virtual void onInitialized(AndroidJavaObject exoFragment)
        {
            _MediaPlayerFragment = exoFragment;
            Debug.Log("onInitialized:" + exoFragment);

            // Setting a wrapper for the interface, so we'll receive the events on unity's thread
            var wrapperObject = 
                new AndroidJavaObject(
                    PlaybackEventsProxy.PACKAGE_QUALIFIED_CALL_WRAPPER_JAVA_CLASS, 
                    _UnityBridgeAJC.UnityHandler, 
                    Events
                );
            if (_MediaPlayerFragment.Call<bool>("setPlaybackListener", wrapperObject))
            {
                if (_CurrentInitSuccessCallback != null)
                    _CurrentInitSuccessCallback();
            }
            else
            {
                if (_CurrentInitErrorCallback != null)
                    _CurrentInitErrorCallback(new AndroidJavaObject("java.lang.Exception", "Failed to set playback listener"));
            }
            _CurrentInitErrorCallback = null;
            _CurrentInitSuccessCallback = null;
        }
        //protected virtual void onStartedLoading(string url) { }
        //protected virtual void onLoadError(AndroidJavaObject throwable) { }
        protected virtual void onInitializationError(AndroidJavaObject throwable)
        {
            Debug.Log("onInitializationError:" + throwable);
            if (_CurrentInitErrorCallback != null)
                _CurrentInitErrorCallback(throwable);
            _CurrentInitErrorCallback = null;
            _CurrentInitSuccessCallback = null;
        }
        //    void onLoaded(String url)
        //	{
        //	}
        protected virtual void onDestroyView() { }
        protected virtual void onRelease() { }
        #endregion

        protected AndroidJavaObject CastAJO(AndroidJavaObject ajo, string targetClass)
        {
            using (var objClass = new AndroidJavaClass("java.lang.Class"))
            using (var targetClassObj = objClass.CallStatic<AndroidJavaObject>("forName", targetClass))
                return targetClassObj.Call<AndroidJavaObject>("cast", ajo);
        }

        /// <summary>
        /// Releases native components and hooks
        /// </summary>
        public virtual void Dispose()
        {
            if (_MediaPlayerFragment != null)
            {
                _MediaPlayerFragment.Call("release");
                _MediaPlayerFragment.Dispose();
                _MediaPlayerFragment = null;
            }
            if (_UnityBridgeAJC != null)
            {
                _UnityBridgeAJC.Dispose();
                _UnityBridgeAJC = null;
            }
            javaInterface.Dispose();
        }
    }
}