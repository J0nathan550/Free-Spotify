using SpotifyExplode.Artists;

namespace SoundScapes.Classes;

public class ArtistConverter
{
    public static string FormatArtists(List<Artist> artists)
    {
        if (artists == null || artists.Count == 0) return string.Empty;
        if (artists.Count == 1) return artists[0].Name;
        string formattedNames = string.Join(", ", artists.Select(artist => artist.Name));
        return $"{formattedNames}";
    }
}