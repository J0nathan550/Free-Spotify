using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoundScapes.Interfaces;
using SoundScapes.Models;

namespace SoundScapes.ViewModels;

public partial class MusicPlayerViewModel : ObservableObject
{
    private readonly IMusicPlayer _musicPlayer;
    [ObservableProperty]
    private SongModel _currentSong;
    [ObservableProperty]
    private RelayCommand _playSongCommand;
    [ObservableProperty]
    private RelayCommand _nextSongCommand;
    [ObservableProperty]
    private RelayCommand _previousSongCommand;
    [ObservableProperty]
    private RelayCommand _shuffleSongCommand;
    [ObservableProperty]
    private RelayCommand _repeatSongCommand;
    [ObservableProperty]
    private double _volumeValue = 100.0;

    public MusicPlayerViewModel(IMusicPlayer musicPlayer)
    {
        _musicPlayer = musicPlayer;
        _musicPlayer.ChangeVolume(VolumeValue);
        _currentSong = _musicPlayer.CurrentSong;
        _musicPlayer.SongChanged += (o, e) =>
        {
            CurrentSong = e;
        };
        _playSongCommand = new RelayCommand(PlaySongCommand_Execute);
        _nextSongCommand = new RelayCommand(NextSongCommand_Execute);
        _previousSongCommand = new RelayCommand(PreviousSongCommand_Execute);
        _shuffleSongCommand = new RelayCommand(ShuffleSongCommand_Execute);
        _repeatSongCommand = new RelayCommand(RepeatSongCommand_Execute);
    }

    private void PlaySongCommand_Execute()
    {
        _musicPlayer.Play();
    }
    
    private void NextSongCommand_Execute()
    {
        _musicPlayer.NextSong();
    }

    private void PreviousSongCommand_Execute()
    {
        _musicPlayer.PreviousSong();
    }

    private void RepeatSongCommand_Execute()
    {
        _musicPlayer.RepeatSong();
    }

    private void ShuffleSongCommand_Execute()
    {
        _musicPlayer.ShuffleSong();
    }

    partial void OnVolumeValueChanging(double value)
    {
        _musicPlayer.ChangeVolume(value);
    }

}