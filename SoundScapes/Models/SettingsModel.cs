namespace SoundScapes.Models;

public class SettingsModel
{
    public int Volume { get; set; } = 100;
    public List<PlaylistModel>? Playlists { get; set; }
}