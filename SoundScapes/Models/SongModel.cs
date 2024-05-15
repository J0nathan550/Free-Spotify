using CommunityToolkit.Mvvm.ComponentModel;

namespace SoundScapes.Models;

/// <summary>
/// Модель пісні.
/// </summary>
public partial class SongModel : ObservableObject
{
    // Індекс пісні.
    [ObservableProperty]
    private int _index;

    // Назва пісні.
    [ObservableProperty]
    private string _title = "...";

    // Виконавець пісні.
    [ObservableProperty]
    private string _artist = "...";

    // Іконка пісні.
    [ObservableProperty]
    private string _icon = string.Empty;

    // Тривалість пісні у форматі хвилини:секунди.
    [ObservableProperty]
    private string _duration = "0:00";

    // Тривалість пісні у секундах.
    [ObservableProperty]
    private long _durationLong = 0;

    // Ідентифікатор пісні.
    [ObservableProperty]
    private string _songID = string.Empty;
}