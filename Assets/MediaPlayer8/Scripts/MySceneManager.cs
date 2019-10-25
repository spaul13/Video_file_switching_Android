using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace frame8.MediaPlayer8Example
{
    /// <summary>
    /// Handles the scene management. It automatically loads the first scene and then loads any scene using the dedicated methods
    /// </summary>
    public class MySceneManager : MonoBehaviour
    {
        public SceneEnum _FirstScene;

        public static MySceneManager Instance { get { return _Instance; } }
        static MySceneManager _Instance;

        void Awake()
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            switch (_FirstScene)
            {
                case SceneEnum.EXAMPLE:
                    LoadExample();
                    break;
                case SceneEnum.ADAPTIVE_CACHE_EXAMPLE:
                    LoadAdaptiveCacheExample();
                    break;
            }
        }

        public void LoadExample()
        {
            SceneManager.LoadScene("Example", LoadSceneMode.Single);
        }

        public void LoadAdaptiveCacheExample()
        {
            SceneManager.LoadScene("AdaptiveCacheExample", LoadSceneMode.Single);
        }

        public void ExecuteAfterPlayerFullyStopped(Action action)
        {
            if (FindObjectOfType<ControlPanel>().StopLoadWithReturnValue(action, (_, __) => action()))
                return;

            // If the call is not accepted/no callbacks will be executed, do the action alone
            action();
        }

        public enum SceneEnum
        {
            EXAMPLE,
            ADAPTIVE_CACHE_EXAMPLE
        }
    }
}
