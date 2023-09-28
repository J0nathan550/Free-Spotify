using SpotifyExplode.Search;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Free_Spotify.Classes
{
    public class Playlist
    {
        public Border MainBorder { get; set; } = new Border();
        public string PlaylistName { get; set; } = string.Empty;
        public string PlaylistImage { get; set; } = string.Empty;
        public List<TrackSearchResult> SongsInsidePlaylist{ get; set; } = new List<TrackSearchResult>();
    }
}