using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using frame8.Logic.Media.MediaPlayer;
using UnityEngine.Serialization;

//namespace frame8.MediaPlayer8Example
//{
    public class call_control : MonoBehaviour
    {
        frame8.MediaPlayer8Example.ControlPanel cp;
        int count = 0;
        frame8.MediaPlayer8Example.ControlPanel.MediaSourceBasicInfo gg;
        String prefix = "file:///sdcard/viking_play/";

        // Use this for initialization
        void Start()
        {
            cp = GameObject.Find("Canvas/ControlPanel").GetComponent<frame8.MediaPlayer8Example.ControlPanel>();
            gg = new frame8.MediaPlayer8Example.ControlPanel.MediaSourceBasicInfo();
            gg.title = "DASH";
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log("\n sibu: Inside update");
            Debug.Log("\n the current time.deltatime = " + Time.deltaTime);
            if (GvrControllerInput.AppButtonDown)
            {
                Debug.Log("\n sibu: pushing app button down");
                gg.url = prefix + (count%10).ToString() +".mp4";
                cp.LoadURL(gg);
                count++;
                
            }
            /*
            else
            {
                gg.url = prefix + "100.mp4";
                cp.LoadURL(gg);
            }
            */

        

        }

        /*
        public class MediaSourceBasicInfo
        {
            public string title, url;
        }
        */
    }
//}
