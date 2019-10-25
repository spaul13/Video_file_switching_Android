using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Base class for debugging common info about a MediaPlayer
    /// <para>If this component is enabled and it finds a MediaPlayer8 component in the provided _MediaPlayerGO</para>
    /// <para>It'll set MediaPlayer8.NativeController.SetCollectDebugInfo(true)</para>
    /// </summary>
    public abstract class BaseDebugPanel : MonoBehaviour
    {
        /// <summary>
        /// If not assigned, will try to find it as "DebugInfoText" among children
        /// </summary>
        [SerializeField] [Tooltip("If not assigned, will try to find it as \"DebugInfoText\" among children")]
        protected Text _DebugInfoText;

        /// <summary>
        /// The first active component of type MediaPlayer8 on this game object will be debugged. If not assigned, this.gameObject will be used
        /// </summary>
        [Tooltip("The first active component of type MediaPlayer8 on this game object will be debugged. If not assigned, this.gameObject will be used")]
        [SerializeField]
        protected GameObject _MediaPlayerGO;

        /// <summary>
        /// The debugged MediaPlayer8
        /// </summary>
        protected abstract MediaPlayer8 Player { get; }

        bool tmp;


        /// <summary>
        /// Declared for inheritance purposes
        /// </summary>
        protected virtual void Awake() {}

        /// <summary>
        /// Tries to set Player.NativeController.SetCollectDebugInfo(true) once every 3 seconds unti it succeeds
        /// It assumes that, because the component is enabled, the debug info should show
        /// </summary>
        protected virtual IEnumerator Start()
        {
            if (!_DebugInfoText)
                _DebugInfoText = transform.Find("DebugInfoText").GetComponent<Text>();

            if (!_MediaPlayerGO)
                _MediaPlayerGO = gameObject;

            RetrievePlayer();

            if (!Player || !Player.enabled)
                throw new UnityException("Assign MediaPlayerGO in inspector or make sure an instance is already present on \"" + name + "\" and it's enabled");

            yield return new WaitForSeconds(3f);
            while (!Player.NativeController.SetCollectDebugInfo(true))
            {
                //Debug.Log("SetCollectDebugInfo(true) not succeeded; retrying after 3sec...");
                yield return new WaitForSeconds(3f);
            }
            tmp = true;
        }

        internal abstract void RetrievePlayer();

        /// <summary>
        /// Collects and updates debug info inside the provided UnityEngine.UI.Text
        /// </summary>
        protected virtual void Update()
        {
            if (Input.touchCount == 2 && Input.touches[1].phase == TouchPhase.Began)
            {
                Player.NativeController.SetCollectDebugInfo((tmp = !tmp));
            }

            string str = "";
            if (Player.NativeController.GetDebugInfo(ref str))
                _DebugInfoText.text = str;
        }
    }
}