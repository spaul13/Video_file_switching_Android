using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace frame8.Logic.Media.MediaPlayer
{
    /// <summary>
    /// Represents the main states a MediaPlayer can be in
    /// </summary>
    public enum PlayerState
    {
        /// <summary> The player does not have a source to play, so it is neither buffering nor ready to play </summary>
        IDLE = 1, // See ExoPlayer.java

        /// <summary> The player not able to immediately play from the current position. The cause is
        /// Renderer specific, but this state typically occurs when more data needs to be
        /// loaded to be ready to play, or more data needs to be buffered for playback to resume </summary>
        BUFFERING,

        /// <summary>
        /// The player is able to immediately play from the current position. The player will be playing if getPlayWhenReady
        /// returns true, and paused otherwise
        /// </summary>
        READY,

        /// <summary> The player has finished playing the media </summary>
        ENDED
    }
}
