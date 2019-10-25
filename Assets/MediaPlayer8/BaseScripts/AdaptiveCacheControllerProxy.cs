using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Contains functionality specific to the AdaptiveCache native part
    /// </summary>
    public class AdaptiveCacheControllerProxy : ControllerProxy
    {
        internal const string PACKAGE_QUALIFIED_FRAGMENT_JAVA_CLASS = C.PACKAGE_EXTENSIONSFRAME8_ADAPTIVECACHE + ".AdaptiveCacheMediaPlayer8Fragment";

        protected Action<string> _CurrentLoadSuccessCallback;
        protected Action<AndroidJavaObject> _CurrentLoadErrorCallback;

        protected Action<double> _CurrentSeekRequestSuccessCallback;
        protected Action<string, AndroidJavaObject> _CurrentSeekRequestErrorCallback;

        protected Action _CurrentStopSuccessCallback;
        protected Action<string, AndroidJavaObject> _CurrentStopErrorCallback;


        #region commands
        /// <summary>
        /// Calls base's implementation, but will thrown an UnityException before if the AdaptiveCacheMediaPlayer8 is missing from the native side.
        /// </summary>
        /// <param name="androidSurfaceIntPtr"></param>
        /// <param name="mediaPlayerParams"></param>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        internal override void Init(IntPtr androidSurfaceIntPtr, Parameters mediaPlayerParams, Action onDone, Action<AndroidJavaObject> onError)
        {
            // Don't bother with further initialization if the adaptive caching package does not exist in the first place
            try { AndroidJavaClass adaptiveCachingFragment = new AndroidJavaClass(PACKAGE_QUALIFIED_FRAGMENT_JAVA_CLASS); }
            catch (Exception e) { throw new UnityException("AdaptiveCacheMediaPlayer8 is not available in this package! \n" + e); }

            base.Init(androidSurfaceIntPtr, mediaPlayerParams, onDone, onError);
        }

        /// <summary> Use the other overload to also receive callbacks </summary>
        internal override bool Load(string url) { return Load(url, null, null); }
        /// <summary> Use the other overload to also receive callbacks </summary>
        public override bool RequestSeek01(double position01) { return RequestSeek01(position01, null, null); }
        /// <summary> Use the other overload to also receive callbacks </summary>
        public override bool StopPlaybackAndLoading() { return StopPlaybackAndLoading(null, null); }

        /// <summary>
        /// In the adaptive cache plugin, callbacks need to be provided, because it takes significantly more time to load a media source and there are several possible errors to be handled
        /// </summary>
        /// <param name="url"></param>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        /// <returns>if accepted</returns>
        public bool Load(string url, Action<string> onDone, Action<AndroidJavaObject> onError)
        {
            if (_MediaPlayerFragment == null)
                return false;

            _CurrentLoadSuccessCallback = onDone;
            _CurrentLoadErrorCallback = onError;
            if (!Load_SkipChecks(url))
            {
                _CurrentLoadSuccessCallback = null;
                _CurrentLoadErrorCallback = null;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Same as with Load(_,_,_), this one is a request, not a command. Callbacks are your friend
        /// </summary>
        /// <param name="position01"></param>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        /// <returns>if accepted</returns>
        public bool RequestSeek01(double position01, Action<double> onDone, Action<string, AndroidJavaObject> onError)
        {
            if (_MediaPlayerFragment == null)
                return false;

            _CurrentSeekRequestSuccessCallback = onDone;
            _CurrentSeekRequestErrorCallback = onError;
            if (!base.RequestSeek01_SkipChecks(position01))
            {
                _CurrentSeekRequestSuccessCallback = null;
                _CurrentSeekRequestErrorCallback = null;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Stops everything. Useful when shutting down the player. Note that this AdaptiveCache version is used with callbacks, as opposed to base version
        /// <para>A new Load() call should be done in order to re-use the player, in case disposal was not intended</para>
        /// </summary>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        /// <returns>if accepted</returns>
        public bool StopPlaybackAndLoading(Action onDone, Action<string, AndroidJavaObject> onError)
        {
            if (_MediaPlayerFragment == null)
                return false;

            _CurrentStopSuccessCallback = onDone;
            _CurrentStopErrorCallback = onError;
            if (!base.StopPlaybackAndLoading_SkipChecks())
            {
                _CurrentStopSuccessCallback = null;
                _CurrentStopErrorCallback = null;

                return false;
            }

            return true;
        }

        /// <summary>
        /// If the speed test is running.
        /// This happens for a (heuristically) determined amount of time, when a new video is loaded
        /// </summary>
        public bool IsSpeedTestPhase()
        {
            if (_MediaPlayerFragment != null)
                return _MediaPlayerFragment.Call<bool>("isSpeedTestPhase");
            return false;
        }

        /// <summary> See AdaptiveCacheDebugPanel</summary>
        /// <returns>if accepted</returns>
        public bool GetBufDebugLine1(ref char[] line) { return GetBufDebugLine(ref line, "getBufDebugLine1"); }
        /// <summary> See AdaptiveCacheDebugPanel</summary>
        /// <returns>if accepted</returns>
        public bool GetBufDebugLine2(ref char[] line) { return GetBufDebugLine(ref line, "getBufDebugLine2"); }
        /// <summary> See AdaptiveCacheDebugPanel</summary>
        /// <returns>if accepted</returns>
        public string GetBufDebugLine1() { char[] line = new char[0]; GetBufDebugLine(ref line, "getBufDebugLine1"); return new string(line ?? new char[0]); }
        /// <summary> See AdaptiveCacheDebugPanel</summary>
        /// <returns>if accepted</returns>
        public string GetBufDebugLine2() { char[] line = new char[0]; GetBufDebugLine(ref line, "getBufDebugLine2"); return new string(line ?? new char[0]); }

        bool GetBufDebugLine(ref char[] line, string methodName)
        {
            if (_MediaPlayerFragment != null)
            {
                var ajo = _MediaPlayerFragment.Call<AndroidJavaObject>(methodName);
                if (ajo == null)
                    return false;

                var rawArray = ajo.GetRawObject();
                if (rawArray == null)
                    return false;

                if (rawArray.ToInt32() == 0)
                    return false;

                // TODO find if this impacts performance; if so, iterate through java elements and populate line1 instead
                line = AndroidJNI.FromCharArray((IntPtr)rawArray.ToInt32());

                return true;
            }
            return false;
        }
        #endregion

        #region callbacks
        protected override void onInitialized(AndroidJavaObject exoFragment)
        {
            var castedVar = CastAJO(exoFragment, PACKAGE_QUALIFIED_FRAGMENT_JAVA_CLASS);
            //Debug.Log("onInitialized: exoFragment=" + (exoFragment==null ? "null" : exoFragment.ToString()) + "; castedVar=" + (castedVar == null ? "null" : castedVar.ToString()));
            base.onInitialized(castedVar);

            exoFragment.Dispose();
        }
        void onStartedLoading(string url)
        {
            if (_CurrentLoadSuccessCallback != null)
                _CurrentLoadSuccessCallback(url);
            _CurrentLoadSuccessCallback = null;
            _CurrentLoadErrorCallback = null;
        }
        void onLoadError(AndroidJavaObject throwable)
        {
            if (_CurrentLoadErrorCallback != null)
                _CurrentLoadErrorCallback(throwable);
            _CurrentLoadSuccessCallback = null;
            _CurrentLoadErrorCallback = null;
        }
        void onSeekRequestAccepted(double v01)
        {
            if (_CurrentSeekRequestSuccessCallback != null)
                _CurrentSeekRequestSuccessCallback(v01);
            _CurrentSeekRequestSuccessCallback = null;
            _CurrentSeekRequestErrorCallback = null;
        }
        void onSeekRequestRejected(string reason, AndroidJavaObject throwable)
        {
            if (_CurrentSeekRequestErrorCallback != null)
                _CurrentSeekRequestErrorCallback(reason, throwable);
            _CurrentSeekRequestSuccessCallback = null;
            _CurrentSeekRequestErrorCallback = null;
        }
        void onStopLoadingSuccess()
        {
            if (_CurrentStopSuccessCallback != null)
                _CurrentStopSuccessCallback();
            _CurrentStopSuccessCallback = null;
            _CurrentStopErrorCallback = null;
        }
        void onStopLoadingError(AndroidJavaObject throwable)
        {
            if (_CurrentStopErrorCallback != null)
                _CurrentStopErrorCallback(throwable == null ? "" : (throwable.Call<string>("getMessage") ?? ""), throwable);
            _CurrentStopSuccessCallback = null;
            _CurrentStopErrorCallback = null;
        }
        #endregion

        public override void Dispose()
        {

            base.Dispose();
        }
    }
}