using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Describes how the rendering is done
    /// </summary>
    [Serializable]
    public class RenderingSetup
    {
        /// <summary>
        /// If not assigned, a new one will be created and will be set as the TargetRenderer's texture
        /// </summary>
        [SerializeField] [Tooltip("If not assigned, a new one will be created and will be set as the TargetRenderer's texture")]
        internal Texture2D targetTexture;

        /// <summary>
        /// If assigned, it'll be assigned to TargetRenderer's texture while loading
        /// </summary>
        [SerializeField] [Tooltip("If assigned, it'll be assigned to TargetRenderer's texture while loading")]
        internal Texture2D loadingTexture;

        /// <summary>
        /// If not assigned and TargetTexture also is not assigned, this GameObject's Renderer will be used. If not present, you can manually asign it to 
        /// <para>this.TargetRenderer property, although it's not required, since you can use the texture on multiple renderers (retrieve it by this.TargetTexture</para>
        /// <para>property if it'll be generated at runtime, i.e. if you don't manually assign TargetTexture in inspector)</para>
        /// </summary>
        [SerializeField] [Tooltip("If not assigned and TargetTexture also is not assigned, <this> GameObject's Renderer will be used. If not present, you can manually asign it to <this>.TargetRenderer property, although it's not required, since you can use the texture on multiple renderers (retrieve it by <this>.TargetTexture property if it'll be generated at runtime, i.e. if you don't manually assign TargetTexture in inspector)")]
        internal Renderer targetRenderer;

        /// <summary>
        /// If you have no idea what this is, let it as "_MainTex", which corresponds to Renderer.material.mainTexture
        /// </summary>
        [SerializeField] [Tooltip("If you have no idea what this is, let it as \"_MainTex\", which corresponds to Renderer.material.mainTexture")]
        internal string shaderTextureName = "_MainTex";

        /// <summary>
        /// Wether to use Renderer.sharedMaterial or Renderer.material
        /// </summary>
        [SerializeField] [Tooltip("Wether to use Renderer.sharedMaterial or Renderer.material")]
        internal bool rendererSharedMaterial;

        /// <summary>
        /// The texture onto which the video will render
        /// </summary>
        public Texture2D TargetTexture { get { return targetTexture; } }

        /// <summary> Can be assigned zero or more times, but please note that when switching from one renderer to another, the old one's texture won't be changed </summary>
        public Renderer TargetRenderer
        {
            get { return targetRenderer; }
            set { targetRenderer = value; MaybeChangeRendererTexture(); }
        }

        internal bool LoadingTextureIsActive { get; set; }

        /// <summary>
        /// if no parameter or null is provided, it'll be replaced with [LoadingTextureIsActive &amp;&amp; loadingTexture]
        /// </summary>
        /// <param name="loading">if true, the loading texture will show, if any; if false, the target texture will show, if any</param>
        internal virtual void MaybeChangeRendererTexture(bool? loading = null)
        {
            loading = loading ?? (LoadingTextureIsActive && loadingTexture);

            LoadingTextureIsActive = loading.Value;
            if (targetRenderer)
            {
                Texture tex = null;
                if (LoadingTextureIsActive)
                {
                    if (loadingTexture)
                        tex = loadingTexture;
                }
                else
                {
                    if (targetTexture)
                        tex = targetTexture;
                }

                if (tex)
                {
                    Material mat;
                    if (rendererSharedMaterial)
                        mat = targetRenderer.sharedMaterial;
                    else
                        mat = targetRenderer.material;
                    mat.SetTexture(shaderTextureName, tex);
                }
            }
        }
    }
}
