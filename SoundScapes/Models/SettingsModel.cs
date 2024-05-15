namespace SoundScapes.Models;

/// <summary>
/// Модель налаштувань.
/// </summary>
public class SettingsModel
{
    // Гучність.
    public int Volume { get; set; } = 100;

    // Список плейлистів.
    public List<PlaylistModel> Playlists { get; set; } = [];
}