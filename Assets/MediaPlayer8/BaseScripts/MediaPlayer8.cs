using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Base class for the 2 main module: "Simple" and "AdaptiveCache"
    /// </summary>
    public abstract class MediaPlayer8 : MonoBehaviour
    {
        /// <summary>
        /// If the media player is ready to load a Media Source
        /// </summary>
        public bool Initialized { get { return _ControllerProxy != null && _ControllerProxy.Initialized; } }

        /// <summary>
        /// The controller clas through which common commands can be requested and common callbacks are received
        /// </summary>
        public ControllerProxy NativeController { get { return _ControllerProxy; } }

        /// <summary>
        /// Includes setup config. like which texture to use, which renderer, when, what loading texture to use (if any) etc.
        /// </summary>
        public abstract RenderingSetup Rendering { get; }

        /// <summary>
        /// Abstract params defined by each inheritor class
        /// </summary>
        protected abstract Parameters Params { get;  }

        IntPtr _NativeTextureID = IntPtr.Zero;
        int _TextureWidth = 128, _TextureHeight = 128; // just some initial, valid values
        ControllerProxy _ControllerProxy;
        bool _LoadingTextureIsActive = true;

        private enum MediaSurfaceEventType
        {
            Initialize = 0,
            Shutdown = 1,
            Update = 2,
            Count_
        };

        /// <summary> Need to be changed accordingly when more than 1 rendering native plugin is used </summary>
        public static int NativeEventsStartIndex
        {
            get { return _NativeEventsStartIndex; }
            set
            {
                _NativeEventsStartIndex = value;
#if (UNITY_ANDROID && !UNITY_EDITOR)
			    _MediaSurfaceSetEventBase(_NativeEventsStartIndex);
#endif
            }
        }

        static int _NativeEventsStartIndex = 0;


        static void IssuePluginEvent(MediaSurfaceEventType eventType) { GL.IssuePluginEvent((int)eventType + NativeEventsStartIndex); }


        protected virtual void Awake()
        {
            _ControllerProxy = CreateControllerProxy();
            _MediaSurfaceInit();

            if (!Rendering.targetTexture)
            {
                Rendering.targetTexture =
                    Texture2D.CreateExternalTexture(
                        _TextureWidth,
                        _TextureHeight,
                        TextureFormat.RGBA32,
                        true,
                        false,
                        IntPtr.Zero
                    );

                // Only use this gameObject's renderer if no target texture is specified via inspector
                if (!Rendering.targetRenderer)
                    Rendering.targetRenderer = GetComponent<Renderer>();
            }

            Rendering.MaybeChangeRendererTexture();

            IssuePluginEvent(MediaSurfaceEventType.Initialize);
        }

        /// <summary>
        /// Abstract ControllerProxy creator method defined by each inheritor class
        /// </summary>
        protected abstract ControllerProxy CreateControllerProxy();

        /// <summary>
        /// <para> Initializes the native texture, the native controller, sets a loading texture if specified.</para> 
        /// <para> If successful(onDone called), Load() can be called</para> 
        /// </summary>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        public virtual void Initialize(Action onDone, Action<string> onError)
        {
            Rendering.MaybeChangeRendererTexture(Rendering.loadingTexture != null);

            // Delay 1 frame to allow MediaSurfaceInit from the render thread
            ExecuteAfterOneFrame(
                () =>
                {
                    _ControllerProxy.Init(
                        _MediaSurfaceGetObject(),
                        Params,
                        onDone,
                        //() => { Debug.Log("ControllerProxy inited; calling onDone.."); if (onDone != null) onDone(); },
                        ajoThrowable => { if (onError != null) onError(GetAndroidThrowableError(ajoThrowable)); }
                    );
                }
            );
        }

        /// <summary>
        /// <para> Loads a media from an URL to your media file/stream or use "path/to/file/file.extension" to play the </para> 
        /// <para>file in "path/to/project/Assets/StreamingAssets/path/to/file/file.extension" </para> 
        /// </summary>
        /// <param name="mediaPath"></param>
        /// <param name="onDone"></param>
        /// <param name="onError"></param>
        public virtual void Load(string mediaPath, Action<string> onDone, Action<string> onError)
        {
            if (!_ControllerProxy.Initialized)
            {
                if (onError == null)
                    throw new UnityException("call Init first");
                onError("call Init first");
            }

            Rendering.MaybeChangeRendererTexture(Rendering.loadingTexture != null);

            // Remove previous OnVideoSizeChanged, if any, and re-subscribe
            NativeController.Events.videoSizeChanged -= OnVideoSizeChanged;
            NativeController.Events.videoSizeChanged += OnVideoSizeChanged;

            NativeController.Events.playerStateChanged -= OnPlayerStateChanged;
            NativeController.Events.playerStateChanged += OnPlayerStateChanged;

            StartCoroutine(
                RetrieveStreamingAssetIfNeeded(
                    mediaPath,
                    fullMediaPath =>
                    {
                        bool accepted = LoadViaController(
                            fullMediaPath,
                            ajo => { Rendering.MaybeChangeRendererTexture(false); if (onDone != null) onDone(ajo); },
                            ajoThrowable => { if (onError != null) onError(GetAndroidThrowableError(ajoThrowable)); }
                        );
                        Debug.Log("Load accepted = " + accepted);
                    }
                )
            );
        }

        protected abstract bool LoadViaController(string mediaPath, Action<string> onDone, Action<AndroidJavaObject> onError);

        /// <summary>
        /// Called whenever the player's state changes
        /// <para>See PlayerState</para>
        /// </summary>
        /// <param name="playWhenReady"></param>
        /// <param name="state"></param>
        protected virtual void OnPlayerStateChanged(bool playWhenReady, PlayerState state)
        {}

        /// <summary>
        /// Called when a new media is loaded AND it has a different frame size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="unappliedRotationDegrees"></param>
        /// <param name="pixelWidthHeightRatio"></param>
        protected virtual void OnVideoSizeChanged(int width, int height, int unappliedRotationDegrees, float pixelWidthHeightRatio)
        {
            _MediaSurfaceSetTextureSize(_TextureWidth=width, _TextureHeight=height);
            Debug.Log("OnVideoSizeChanged: " + width + "x" + height);
        }

        /// <summary>
        /// For mobile local files, we need to retrieve the file from the package and copy it to persistentDataPath in order to play it
        /// </summary>
        /// <param name="mediaPath"></param>
        /// <param name="onDone"></param>
        /// <returns></returns>
        protected virtual IEnumerator RetrieveStreamingAssetIfNeeded(string mediaPath, Action<string> onDone)
        {
            if (mediaPath.ToLower().StartsWith("http://") 
				|| mediaPath.ToLower().StartsWith("https://") 
				|| mediaPath.ToLower().StartsWith("rtmp://") 
				|| mediaPath.ToLower().StartsWith("file:///"))
            {

            }
            else
            {
                string streamingMediaPath = Application.streamingAssetsPath + "/" + mediaPath;
                string persistentPath = Application.persistentDataPath + "/" + mediaPath;
                if (!File.Exists(persistentPath))
                {
                    WWW www = new WWW(streamingMediaPath);
                    yield return www;

                    if (string.IsNullOrEmpty(www.error))
                        File.WriteAllBytes(persistentPath, www.bytes);
                    else
                        Debug.LogError("RetrieveStreamingAssetIfNeeded: Failed to copy file from apk to persistent data path ("+ persistentPath + "): " + www.error);
                }
                mediaPath = persistentPath;
            }

            Debug.Log("Movie FullPath: " + mediaPath);
            if (onDone != null)
                onDone(mediaPath);
        }

        void ExecuteAfterOneFrame(Action a) { StartCoroutine(ExecuteAfterOneFrameCoroutine(a)); }

        IEnumerator ExecuteAfterOneFrameCoroutine(Action action)
        {
            yield return null;

            if (action != null)
                action();
        }

        /// <summary>
        /// Updating the texture each frame
        /// </summary>
        protected virtual void Update()
        {
            if (NativeController.Initialized && NativeController.GetPlayWhenReady())
            {
                IntPtr currTexId = _MediaSurfaceGetNativeTexture();
                if (currTexId != _NativeTextureID)
                {
                    _NativeTextureID = currTexId;
                    Rendering.targetTexture.UpdateExternalTexture(currTexId);
                }

                IssuePluginEvent(MediaSurfaceEventType.Update);
            }
        }

        // TODO find if need to release more
        /// <summary>
        /// Releasing resources. Disposing NativeController
        /// </summary>
        protected virtual void OnDestroy()
        {
            Debug.Log("Shutting down video");
            // This will trigger the shutdown on the render thread
            IssuePluginEvent(MediaSurfaceEventType.Shutdown);

            try { NativeController.Events.videoSizeChanged -= OnVideoSizeChanged; } catch { }
            try { NativeController.Events.playerStateChanged -= OnPlayerStateChanged; } catch { }
            
            _ControllerProxy.Dispose();
            _ControllerProxy = null;
            //mediaPlayer.Call("stop");
            //mediaPlayer.Call("release");
            //mediaPlayer = null;
            //#endif
        }

        protected virtual string GetAndroidThrowableError(AndroidJavaObject throwable) { return throwable == null ? "" : (throwable.Call<string>("getMessage") ?? ""); }


        [DllImport("MediaPlayer8")]
        private static extern void _MediaSurfaceInit();

        [DllImport("MediaPlayer8")]
        private static extern void _MediaSurfaceSetEventBase(int eventBase);

        // This function returns an Android Surface object that is
        // bound to a SurfaceTexture object on an independent OpenGL texture id.
        // Each frame, before the TimeWarp processing, the SurfaceTexture is checked
        // for updates, and if one is present, the contents of the SurfaceTexture
        // will be copied over to the provided surfaceTexId and mipmaps will be 
        // generated so normal Unity rendering can use it.
        [DllImport("MediaPlayer8")]
        private static extern IntPtr _MediaSurfaceGetObject();

        [DllImport("MediaPlayer8")]
        private static extern IntPtr _MediaSurfaceGetNativeTexture();

        [DllImport("MediaPlayer8")]
        private static extern void _MediaSurfaceSetTextureSize(int texWidth, int texHeight);

    }
}
