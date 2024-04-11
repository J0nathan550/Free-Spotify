using CommunityToolkit.Mvvm.ComponentModel;

namespace SoundScapes.Models;

public partial class PlaylistModel : ObservableObject
{
    [ObservableProperty]
    private bool _isChecked = false;
    [ObservableProperty]
    private string _title = "...";
    [ObservableProperty]
    private string _icon = string.Empty;
    [ObservableProperty]
    private string _authors = "...";
    [ObservableProperty]
    private string _duration = "0:00";
    [ObservableProperty]
    private List<SongModel> _songsInPlaylist = [];

    public override string ToString()
    {
        return Title;
    }
}