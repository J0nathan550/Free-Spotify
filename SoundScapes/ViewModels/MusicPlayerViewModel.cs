using CommunityToolkit.Mvvm.ComponentModel;
using SoundScapes.Interfaces;
using SoundScapes.Models;

namespace SoundScapes.ViewModels;

public partial class MusicPlayerViewModel : ObservableObject
{
    private readonly IMusicPlayer _musicPlayer;
    [ObservableProperty]
    private SongModel _currentSong;
    public MusicPlayerViewModel(IMusicPlayer musicPlayer)
    {
        _musicPlayer = musicPlayer;
        _currentSong = _musicPlayer.CurrentSong;
        _musicPlayer.SongChanged += (o, e) =>
        {
            CurrentSong = e;
        };
    }
}