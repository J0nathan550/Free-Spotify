﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using System.Windows.Media;

namespace SoundScapes.ViewModels;

public partial class MusicPlayerViewModel : ObservableObject
{
    private readonly IMusicPlayer _musicPlayer;
    [ObservableProperty]
    private SongModel _currentSong = new();
    [ObservableProperty]
    private List<SongModel> _songsList = [];
    [ObservableProperty]
    private FontAwesomeIcon _playMediaIcon = FontAwesomeIcon.Play;
    [ObservableProperty]
    private SolidColorBrush _shuffleMediaBrush = new(Colors.White);
    [ObservableProperty]
    private SolidColorBrush _repeatMediaBrush = new(Colors.White);
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
    [ObservableProperty]
    private double _musicPosition = 0.0;
    [ObservableProperty]
    private string _songDuration = "0:00";
    public event EventHandler<SongModel>? SongChanged;


    public MusicPlayerViewModel(IMusicPlayer musicPlayer)
    {
        _musicPlayer = musicPlayer;
        _playSongCommand = new RelayCommand(PlaySongCommand_Execute);
        _nextSongCommand = new RelayCommand(NextSongCommand_Execute);
        _previousSongCommand = new RelayCommand(PreviousSongCommand_Execute);
        _shuffleSongCommand = new RelayCommand(ShuffleSongCommand_Execute);
        _repeatSongCommand = new RelayCommand(RepeatSongCommand_Execute);
        _musicPlayer.MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
        _musicPlayer.MediaPlayer.Volume = (int)VolumeValue;
    }

    private void MediaPlayer_PositionChanged(object? sender, LibVLCSharp.Shared.MediaPlayerPositionChangedEventArgs e)
    {
        SongDuration = TimeConverter.ConvertMsToTime(_musicPlayer.MediaPlayer.Time);
        MusicPosition = e.Position;
        if (e.Position > 0.99f)
        {
            if (_musicPlayer.IsRepeating)
            {
                RepeatSong();
                return;
            }
            if (_musicPlayer.IsShuffling)
            {
                ShuffleSong();
                return;
            }
            NextSongCommand.Execute(null);
        }
    }

    private void PlaySongCommand_Execute()
    {
        _musicPlayer.OnPauseSong();
        if (_musicPlayer.IsPaused)
        {
            PlayMediaIcon = FontAwesomeIcon.Play;
            return;
        }
        PlayMediaIcon = FontAwesomeIcon.Pause;
    }

    private void RepeatSong()
    {
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
    }

    private void ShuffleSong()
    {
        Task.Run(() =>
        {
            Random rng = new();
            int index = rng.Next(0, SongsList.Count);
            CurrentSong = SongsList[index];
            SongChanged?.Invoke(this, CurrentSong);
        });
    }

    public void PlaySong()
    {
        _musicPlayer.CancellationTokenSourcePlay.Cancel();
        _musicPlayer.CancelPlayingMusic();
        _musicPlayer.OnPlaySong(CurrentSong);
    }

    private void NextSongCommand_Execute()
    {
        int index = SongsList.FindIndex(n => n == CurrentSong);
        index++;
        if (index > SongsList.Count - 1) index = 0;
        CurrentSong = SongsList[index];
        SongChanged?.Invoke(this, CurrentSong);
    }

    private void PreviousSongCommand_Execute()
    {
        int index = SongsList.FindIndex(n => n == CurrentSong);
        index--;
        if (index < 0) index = SongsList.Count - 1;
        CurrentSong = SongsList[index];
        SongChanged?.Invoke(this, CurrentSong);
    }

    private void RepeatSongCommand_Execute()
    {
        _musicPlayer.OnRepeatingSong();
        if (_musicPlayer.IsRepeating)
        {
            RepeatMediaBrush = new SolidColorBrush(Colors.Lime);
            return;
        }
        RepeatMediaBrush = new SolidColorBrush(Colors.White);
    }

    private void ShuffleSongCommand_Execute()
    {
        _musicPlayer.OnShufflingSong();
        if (_musicPlayer.IsShuffling)
        {
            ShuffleMediaBrush = new SolidColorBrush(Colors.Lime);
            return;
        }
        ShuffleMediaBrush = new SolidColorBrush(Colors.White);
    }

    partial void OnVolumeValueChanging(double value)
    {
        _musicPlayer.MediaPlayer.Volume = (int)value;
    }

}