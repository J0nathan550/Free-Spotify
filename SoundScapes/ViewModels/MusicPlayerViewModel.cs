﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundScapes.ViewModels;

public partial class MusicPlayerViewModel : ObservableObject
{
    private readonly IMusicPlayer _musicPlayer;
    [ObservableProperty]
    private SongModel _currentSong = new();
    [ObservableProperty]
    private List<SongModel> _songsList = [];
    private readonly List<SongModel> _originalSongsList = [];
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
    private readonly System.Timers.Timer _musicPositionUpdate = new() { Interval = 500 };

    public MusicPlayerViewModel(IMusicPlayer musicPlayer)
    {
        _musicPlayer = musicPlayer;
        _playSongCommand = new RelayCommand(PlaySongCommand_Execute);
        _nextSongCommand = new RelayCommand(NextSongCommand_Execute);
        _previousSongCommand = new RelayCommand(PreviousSongCommand_Execute);
        _shuffleSongCommand = new RelayCommand(ShuffleSongCommand_Execute);
        _repeatSongCommand = new RelayCommand(RepeatSongCommand_Execute);
        _musicPlayer.MediaPlayer.Volume = (int)VolumeValue;
        _musicPositionUpdate.Elapsed += _MusicPositionUpdate_Elapsed;
    }

    private void _MusicPositionUpdate_Elapsed(object? sender, ElapsedEventArgs e)
    {
        CheckMediaPlayerPosition();
    }

    public void CheckMediaPlayerPosition()
    {
        SongDuration = TimeConverter.ConvertMsToTime(_musicPlayer.MediaPlayer.Time);
        MusicPosition = _musicPlayer.MediaPlayer.Position;
        if (_musicPlayer.MediaPlayer.Position >= 0.99f)
        {
            if (_musicPlayer.IsRepeating)
            {
                RepeatSong();
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
            _musicPositionUpdate.Stop();
            return;
        }
        PlayMediaIcon = FontAwesomeIcon.Pause;
        _musicPositionUpdate.Start();
    }

    private void RepeatSong()
    {
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
    }

    public void PlaySong()
    {
        _musicPositionUpdate.Stop();
        _musicPlayer.CancellationTokenSourcePlay.Cancel();
        _musicPlayer.CancelPlayingMusic();
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
        _musicPlayer.OnPlaySong(CurrentSong);
        _musicPositionUpdate.Start();
    }

    private void NextSongCommand_Execute()
    {
        int index = SongsList.FindIndex(n => n == CurrentSong);
        index++;
        if (index > SongsList.Count - 1) index = 0;
        CurrentSong = SongsList[index];
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
        SongChanged?.Invoke(this, CurrentSong);
    }

    private void PreviousSongCommand_Execute()
    {
        int index = SongsList.FindIndex(n => n == CurrentSong);
        index--;
        if (index < 0) index = SongsList.Count - 1;
        CurrentSong = SongsList[index];
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
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

    private bool IsShuffling = false;
    private void ShuffleSongCommand_Execute()
    {
        IsShuffling = !IsShuffling;
        if (_originalSongsList.Count == 0)
        {
            _originalSongsList.AddRange(SongsList);
        }
        if (IsShuffling)
        {
            Random rand = new();
            for (int i = 0; i < SongsList.Count * 2; i++)
            {
                int a = rand.Next(0, SongsList.Count - 1);
                int b = rand.Next(0, SongsList.Count - 1);
                var temp = SongsList[a];
                SongsList[a] = SongsList[b];
                SongsList[b] = temp;
            }
            ShuffleMediaBrush = new SolidColorBrush(Colors.Lime);
        }
        else
        {
            SongsList.Clear();
            SongsList.AddRange(_originalSongsList);
            ShuffleMediaBrush = new SolidColorBrush(Colors.White);
        }
    }

    public void RegisterMusicSlider(Slider slider)
    {
        slider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler((o, e) =>
        {
            if (_musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.NothingSpecial || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Stopped || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Ended || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Error)
            {
                return;
            }
            _musicPlayer.MediaPlayer.SetPause(true);
            _musicPositionUpdate.Stop();
        }));
        slider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((o,e) =>
        {
            if (_musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.NothingSpecial || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Stopped || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Ended || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Error)
            {
                return;
            }
            long progress = (long)(MusicPosition * _musicPlayer.MediaPlayer.Length);
            SongDuration = TimeConverter.ConvertMsToTime(progress);
        }));
        slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((o,e) =>
        {
            if (_musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.NothingSpecial || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Stopped || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Ended || _musicPlayer.MediaPlayer.State == LibVLCSharp.Shared.VLCState.Error)
            {
                return;
            }
            _musicPlayer.MediaPlayer.Position = (float)MusicPosition;
            CheckMediaPlayerPosition();
            _musicPlayer.MediaPlayer.SetPause(false);
            _musicPositionUpdate.Start();
        }));
        slider.AddHandler(System.Windows.UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler((o, e) =>
        {
            if (!slider.IsMoveToPointEnabled || slider.Template.FindName("PART_Track", slider) is not Track track || track.Thumb == null || track.Thumb.IsMouseOver) return;
            track.Thumb.UpdateLayout();

            track.Thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = System.Windows.UIElement.MouseLeftButtonDownEvent,
                Source = track.Thumb
            });

            track.Thumb.RaiseEvent(new DragDeltaEventArgs(0, 0)
            {
                RoutedEvent = Thumb.DragDeltaEvent,
                Source = track.Thumb
            });
        }), true);
    }

    partial void OnVolumeValueChanging(double value)
    {
        _musicPlayer.MediaPlayer.Volume = (int)value;
    }

}