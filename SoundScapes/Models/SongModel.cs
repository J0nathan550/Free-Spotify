using CommunityToolkit.Mvvm.ComponentModel;

namespace SoundScapes.Models;

public partial class SongModel : ObservableObject
{
    [ObservableProperty]
    private int _index;
    [ObservableProperty]
    private string _title = "...";
    [ObservableProperty]
    private string _artist = "...";
    [ObservableProperty]
    private string _icon = string.Empty;
    [ObservableProperty]
    private string _duration = "0:00";
    [ObservableProperty]
    private long _durationLong = 0;
    [ObservableProperty]
    private string _songID = string.Empty;

    public override string ToString()
    {
        return $"{Index} {Title}";
    }
}