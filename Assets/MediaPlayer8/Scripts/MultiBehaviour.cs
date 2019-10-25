using UnityEngine;
using System.Collections;

namespace frame8.MediaPlayer8Example
{
    public class MultiBehaviour : MonoBehaviour
    {
        public float degreesPerSec;

        public bool rotationDirectionChange = true;
        public float rotationDirectionChangeAngleVar;
        float startY, angleValMin, angleValMax;
        public bool disableAtRuntime = true;

        // Use this for initialization
        void Start()
        {
            if (disableAtRuntime)
                gameObject.SetActive(false);
            startY = transform.rotation.eulerAngles.y;
            angleValMin = startY - rotationDirectionChangeAngleVar;
            angleValMax = startY + rotationDirectionChangeAngleVar;
        }

        // Update is called once per frame
        void Update()
        {
            var e = transform.rotation.eulerAngles;
            if (rotationDirectionChange)
                e.y = startY + rotationDirectionChangeAngleVar * Mathf.Sin(Mathf.Deg2Rad * degreesPerSec * Time.time);
            else
                e.y = startY + degreesPerSec * Time.time;
            transform.rotation = Quaternion.Euler(e);
        }
    }
}
