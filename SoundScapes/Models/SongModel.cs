using CommunityToolkit.Mvvm.ComponentModel;
using SpotifyExplode.Tracks;

namespace SoundScapes.Models;

public partial class SongModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "...";
    [ObservableProperty]
    private string _artist = "...";
    [ObservableProperty]
    private string _icon = string.Empty;
    [ObservableProperty]
    private string _duration = "0:00";
    [ObservableProperty]
    private TrackId _songID;
}