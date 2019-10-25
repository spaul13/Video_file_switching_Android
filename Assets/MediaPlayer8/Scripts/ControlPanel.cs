using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using frame8.Logic.Media.MediaPlayer;
using UnityEngine.Serialization;

namespace frame8.MediaPlayer8Example
{
    /// <summary>
    /// <para>Used to ease the demonstration of the package in the example scenes</para>
    /// <para>Contains conveniently-formatted methods to be used as targets for Unity's Buttons' onClick events</para>
    /// <para>that can be assigned with no coding (using the inspector)</para>
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        /// <summary>
        /// If not assigned, will try to FindObjectOfType<MediaPlayer
        /// </summary>
        [SerializeField]
        [Tooltip("If not assigned, will try to FindObjectOfType<MediaPlayer>")]
        MediaPlayer8 _MediaPlayer;

        /// <summary>
        /// If not assigned, _MediaPlayer will be assigned to it, but only if "_MediaPlayer is AdaptiveCacheMediaPlayer8"
        /// </summary>
        [SerializeField]
        [Tooltip("If not assigned, _MediaPlayer will be assigned to it, but only if <_MediaPlayer is AdaptiveCacheMediaPlayer8>")]
        AdaptiveCacheMediaPlayer8 _AdaptivePlayer;

        /// <summary>
        /// Example media sources
        /// </summary>
        [SerializeField]
        MediaSourceBasicInfo[] _MediaSources;

        int _CurURL = -1;
        bool _Seeking;
        Slider _ProgressAndSeekSlider, _BufferSlider, _VolumeSlider;
		Text _TitleText, _ProgressText;
		InputField _CustomURLInputField;

        void Start()
        {
            var allMPs = FindObjectsOfType<MediaPlayer8>();
            foreach (var comp in allMPs)
                if (comp.enabled)
                {
                    _MediaPlayer = comp;
                    break;
                }

            if (_MediaPlayer == null)
                _MediaPlayer = FindObjectOfType<MediaPlayer8>();

            if (_MediaPlayer == null)
            {
                if (_AdaptivePlayer == null)
                    throw new UnityException("neither _MediaPlayer nor _AdaptivePlayer is assigned & enabled in inspector and none could be found with FindObjectOfType");

                _MediaPlayer = _AdaptivePlayer;
            }

            if (_MediaPlayer is AdaptiveCacheMediaPlayer8)
                _AdaptivePlayer = (AdaptiveCacheMediaPlayer8)_MediaPlayer;

            // Using try-catch in case no child with that name exists 
            Debug.Log("\n Inside start, current Mediaplayer = " + _MediaPlayer.name);
            _ProgressAndSeekSlider = transform.Find("ProgressAndSeekSlider").GetComponent<Slider>();
            _BufferSlider = transform.Find("BufferSlider").GetComponent<Slider>();
            _VolumeSlider = transform.Find("VolumeSlider").GetComponent<Slider>();
            _TitleText = transform.Find("TitleText").GetComponent<Text>();
            _TitleText.text = "";
            _ProgressText = transform.Find("ProgressTextPanel/ProgressText").GetComponent<Text>();
			_CustomURLInputField = transform.Find("EnterURLPanel/URLInputField").GetComponent<InputField>();

			InvokeRepeating("Update2", 3f, 1f);
        }

        /// <summary>
        /// Loads the next media source from the provided array
        /// </summary>
        public void LoadNext()
        {
			LoadURL(_MediaSources[_CurURL = (_CurURL + 1) % _MediaSources.Length]);
        }

		public void LoadURL(MediaSourceBasicInfo mediaSourceInfo)
		{
			_CustomURLInputField.text = mediaSourceInfo.url;
            Debug.Log("\n Sibu: Inside Load URL, current URL = " + mediaSourceInfo.url);
			Action load = () =>
			{
                //// Auto-play
                //if (_CurURL == -1)
                Debug.Log("Sibu: Before calling play \n");

                    Play();

				_MediaPlayer.Load(mediaSourceInfo.url, null, s => Debug.Log("Load error:" + s));
				if (_TitleText)
				{
					int index = Array.FindIndex(_MediaSources, m => m.url == mediaSourceInfo.url);
					var indexStr = index < 0 ? "" : ( (index+1) + "/" + _MediaSources.Length + ": ");
					var niceURL = mediaSourceInfo.url
							.Replace("http://www.", "www.").Replace("https://www.", "www.");//.Replace("http://", "").Replace("https://", "");
					if (mediaSourceInfo.url == "")
						niceURL = "No stream available for this source type. Please enter one manualy";
					_TitleText.text = indexStr + mediaSourceInfo.title + " - " + niceURL;
				}
			};
			if (_MediaPlayer.Initialized)
				load();
			else
				_MediaPlayer.Initialize(load, s => Debug.Log("Init error:" + s));
		}

        /// <summary>
        /// Notifies the player to start/resume playback when ready and sets Screen.sleepTimeOut to SleepTimeout.NeverSleep
        /// </summary>
        public void Play()
        {
            Debug.Log("Sibu: Inside Play()");
            _MediaPlayer.NativeController.SetPlayWhenReady(true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        /// <summary>
        /// Pauses playback and sets Screen.sleepTimeOut to SleepTimeout.SystemSetting
        /// </summary>
        public void Pause()
        {
            Debug.Log("Sibu: Inside Pause");
            _MediaPlayer.NativeController.SetPlayWhenReady(false);
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }

        /// <summary>
        /// Notifies the player to stop any loading (downloading, buffering, disk writing etc.)
        /// </summary>
        public void StopLoad() {
            Debug.Log("Sibu: StopLoad() \n");
            StopLoadWithReturnValue(() => Debug.Log("StopLoad done"), (s, err) => Debug.Log("StopLoad error: " + s)); } // this is just because we can't use non-void-returning methods with the button event manager in inspector 

        /// <summary>
        /// Same as StopLoad(), but tells you if the command is accepted or not
        /// </summary>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        /// <returns>true if it's accepted (and thus one of the provided callbacks will be called)</returns>
        public bool StopLoadWithReturnValue(Action onDone, Action<string, AndroidJavaObject> onError)
        {
            bool accepted;
            Debug.Log("Sibu: StopLoadWithReturnValue() \n");
            if (_AdaptivePlayer)
                accepted = _AdaptivePlayer.NativeController.StopPlaybackAndLoading(onDone, onError);
            else
            {
                accepted = _MediaPlayer.NativeController.StopPlaybackAndLoading();
                if (onDone != null)
                    onDone();
            }
            Debug.Log("StopLoad accepted=" + accepted);
            return accepted;
        }
        /// <summary>
        /// Callback from a Slider or a similar component to change the volume
        /// Notifies the player to change the volume
        /// </summary>
        /// <param name="value"></param>
        public void OnVolumeValueChanged(float value)
        {
            _MediaPlayer.NativeController.RequestSetVolume(value);
        }
        /// <summary>
        /// Callback from a Slider or a similar component when a seeking event may occur
        /// </summary>
        public void OnBeginPotentialSeek()
        {
            Debug.Log("Sibu: OnBeginPotentialSeek() \n");
            _Seeking = true;
        }
        /// <summary>
        /// Currently, used only as a callback from a Slider or a similar component when a seeking event that might have occured, was cancelled
        /// </summary>
        public void OnCancelAnyUIEvent()
        {
            Debug.Log("Sibu: OnCancelAnyUIEvent() \n");
            _Seeking = false;
        }
        /// <summary>
        /// Callback from a Slider or a similar component when a seeking event that might have occured, ended
        /// </summary>
        public void OnEndPotentialSeek()
        {
            Debug.Log("Sibu: OnEndPotentialSeek() \n");
            if (!_ProgressAndSeekSlider)
                return;

            if (_Seeking)
            {
                if (!RequestSeekTo(_ProgressAndSeekSlider.value)) //reset the visual position if the request is not honored
                {
                    var prog01 = (float)_MediaPlayer.NativeController.GetProgress01();
                    if (prog01 >= 0f) // only if it's possible to know the current pos
                        _ProgressAndSeekSlider.value = prog01;
                }
                _Seeking = false;
            }
        }

		public void OnCustomURLSubmitted()
		{
			LoadURL(new MediaSourceBasicInfo() { title = "Custom URL", url = _CustomURLInputField.text });
		}

        void Update2()
        {
            //Debug.Log("Sibu: Calling Update2\n");
            if (!_MediaPlayer.Initialized)
                return;

            if (!_Seeking)
                UpdateProgressSlider();
            UpdateBufferSlider();
            UpdateProgressText();
        }

        void UpdateProgressSlider()
        {
            if (!_ProgressAndSeekSlider)
                return;

            var progress = _MediaPlayer.NativeController.GetProgress01();
            if (progress >= 0d)
                _ProgressAndSeekSlider.value = (float)progress;
        }
        void UpdateBufferSlider()
        {
            if (!_BufferSlider)
                return;

            double buf = _MediaPlayer.NativeController.GetBufferLevel01RelativeToDuration();
            if (buf >= 0d)
                _BufferSlider.value = (float)buf;
        }
        void UpdateProgressText()
        {
            if (!_ProgressText)
                return;

            var t = TimeSpan.FromSeconds(_MediaPlayer.NativeController.GetProgressSeconds());
            string progressStr = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    t.Hours,
                    t.Minutes,
                    t.Seconds);
            t = TimeSpan.FromSeconds(_MediaPlayer.NativeController.GetDurationSeconds());
            string durStr = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    t.Hours,
                    t.Minutes,
                    t.Seconds);

            _ProgressText.text = progressStr + "/" + durStr;
        }
        bool RequestSeekTo(float value)
        {
            Debug.Log("Sibu: Request SeekTo \n");
            bool accepted;
            if (_AdaptivePlayer)
                accepted = _AdaptivePlayer.NativeController.RequestSeek01(value, d => Debug.Log("Seek executed and rebuffering started (if needed)"), (s, err) => Debug.Log("Seek executed, but error: " + s));
            else
                accepted = _MediaPlayer.NativeController.RequestSeek01(value);

            Debug.Log("Seek will execute=" + accepted);

            return accepted;
        }

        void OnDisable()
        {
            Debug.Log("Sibu: On Disable \n");
            _Seeking = false;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        void OnDestroy()
        {
            Debug.Log("Sibu: On Destory \n");
            _Seeking = false;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }

        /// <summary>
        /// Class holding a title and an URL, representing a Media Source
        /// </summary>
        [Serializable]
        public class MediaSourceBasicInfo
        {
            public string title, url;
        }

    }
}
