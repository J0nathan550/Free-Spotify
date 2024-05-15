using CommunityToolkit.Mvvm.ComponentModel;

namespace SoundScapes.Models;

/// <summary>
/// Модель плейлисту.
/// </summary>
public partial class PlaylistModel : ObservableObject
{
    // Прапорець, що вказує, чи плейлист відмічений.
    [ObservableProperty]
    private bool _isChecked = false;

    // Назва плейлисту.
    [ObservableProperty]
    private string _title = "...";

    // Іконка плейлисту.
    [ObservableProperty]
    private string _icon = string.Empty;

    // Автори плейлисту.
    [ObservableProperty]
    private string _authors = "...";

    // Тривалість плейлисту у форматі хвилини:секунди.
    [ObservableProperty]
    private string _duration = "0:00";

    // Список пісень у плейлисті.
    [ObservableProperty]
    private List<SongModel> _songsInPlaylist = [];
}