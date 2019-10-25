using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using frame8.Logic.Media.MediaPlayer;

namespace frame8.MediaPlayer8Example
{
    public class NavToAdaptiveCacheExampleButton : MonoBehaviour
    {
        void Awake()
        {
            // Don't fill the UI unnecessary if the AdaptiveCache upgrade is not included
            try { AndroidJavaClass adaptiveCachingFragment = new AndroidJavaClass(AdaptiveCacheControllerProxy.PACKAGE_QUALIFIED_FRAGMENT_JAVA_CLASS); }
            catch { gameObject.SetActive(false); }
        }

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(
                () => MySceneManager.Instance.ExecuteAfterPlayerFullyStopped(
                        MySceneManager.Instance.LoadAdaptiveCacheExample
                    )
                );
        }
    }
}
