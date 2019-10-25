using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// ControllerProxy for the "Simple" module
    /// </summary>
    public class SimpleControllerProxy : ControllerProxy
    {
        internal const string PACKAGE_QUALIFIED_FRAGMENT_JAVA_CLASS = C.PACKAGE_EXTESIONSFRAME8 + ".SimpleMediaPlayer8Fragment";


        #region commands

        #endregion

        #region callbacks
        protected override void onInitialized(AndroidJavaObject exoFragment)
        {
            base.onInitialized(CastAJO(exoFragment, PACKAGE_QUALIFIED_FRAGMENT_JAVA_CLASS));

            exoFragment.Dispose();
        }
        #endregion

        public override void Dispose()
        {

            base.Dispose();
        }
    }
}