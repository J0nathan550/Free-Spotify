using CommunityToolkit.Mvvm.ComponentModel;

namespace SoundScapes.Models;

public partial class SongModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;
    [ObservableProperty]
    private string? _artist;
    [ObservableProperty]
    private string? _icon;
    [ObservableProperty]
    private string? _duration;
    [ObservableProperty]
    private string? _songLink;
}