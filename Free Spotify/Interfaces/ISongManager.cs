/// <summary>
/// An interface that defines the operations for managing a playlist of songs.
/// </summary>
namespace Free_Spotify.Interfaces
{
    public interface ISongManager
    {
        /// <summary>
        /// Plays the previous song in the playlist.
        /// </summary>
        public void PreviousSong();

        /// <summary>
        /// Plays the next song in the playlist.
        /// </summary>
        public void NextSong();

        /// <summary>
        /// Shuffles the order of songs in the playlist.
        /// </summary>
        public void ShuffleSongs();
    }
}