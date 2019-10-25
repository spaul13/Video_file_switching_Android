using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace frame8.MediaPlayer8Example
{
    public class NavToSimpleExampleButton : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(
                () => MySceneManager.Instance.ExecuteAfterPlayerFullyStopped(
                        MySceneManager.Instance.LoadExample
                    )
                );
        }
    }
}
