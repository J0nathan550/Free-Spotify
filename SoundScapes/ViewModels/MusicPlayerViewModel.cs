using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using Microsoft.Extensions.DependencyInjection;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundScapes.ViewModels;

public partial class MusicPlayerViewModel : ObservableObject
{
    public readonly IMusicPlayer _musicPlayer;
    private readonly ISettings _settings;
    [ObservableProperty]
    private SongModel _currentSong = new();
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
    private IAsyncRelayCommand _addFavoriteSongCommand;
    [ObservableProperty]
    private double _volumeValue = 100.0;
    [ObservableProperty]
    private double _musicPosition = 0.0;
    [ObservableProperty]
    private string _songDuration = "0:00";
    
    public readonly System.Timers.Timer _musicPositionUpdate = new() { Interval = 500 };
    
    public MusicPlayerViewModel(IMusicPlayer musicPlayer, ISettings settings)
    {
        _settings = settings;
        _settings.Load();
        _musicPlayer = musicPlayer;
        _musicPlayer.MediaPlayer.Volume = _settings.SettingsModel.Volume;
        VolumeValue = _settings.SettingsModel.Volume;

        _playSongCommand = new RelayCommand(PlaySongCommand_Execute);
        _nextSongCommand = new RelayCommand(NextSongCommand_Execute);
        _previousSongCommand = new RelayCommand(PreviousSongCommand_Execute);
        _shuffleSongCommand = new RelayCommand(ShuffleSongCommand_Execute);
        _repeatSongCommand = new RelayCommand(RepeatSongCommand_Execute);
        _addFavoriteSongCommand = new AsyncRelayCommand(AddFavoriteSongCommand_Execute);

        _musicPlayer.MediaPlayer.Volume = (int)VolumeValue;
        _musicPositionUpdate.Elapsed += (o,e) => CheckMediaPlayerPosition();
        _musicPlayer.ExceptionThrown += (o,e) => NextSongCommand_Execute();
    }

    private async Task AddFavoriteSongCommand_Execute()
    {
        await Task.Delay(1);
    }

    public void CheckMediaPlayerPosition()
    {
        SongDuration = TimeConverter.ConvertMsToTime(_musicPlayer.MediaPlayer.Time);
        MusicPosition = _musicPlayer.MediaPlayer.Position;
        _musicPlayer.MediaPlayer.Volume = (int)VolumeValue;
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
        PlayMediaIcon = _musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
        if (_musicPlayer.IsPaused) _musicPositionUpdate.Stop();
        else _musicPositionUpdate.Start();
    }

    private void RepeatSong()
    {
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
    }

    public void PlaySong()
    {
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPositionUpdate.Stop();
        _musicPlayer.CancellationTokenSourcePlay.Cancel();
        _musicPlayer.CancelPlayingMusic();
        if (_musicPlayer.IsPaused)
        {
            _musicPlayer.OnPauseSong();
            PlayMediaIcon = FontAwesomeIcon.Pause;
        }
        _musicPlayer.MediaPlayer.Position = 0;
        _musicPlayer.OnPlaySong(CurrentSong);
        _musicPositionUpdate.Start();
    }

    private void NextSongCommand_Execute()
    {
        SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();
        ObservableCollection<SongModel>? songs = searchViewModel!.SongsList;

        if (songs == null || songs.Count == 0)
        {
            return;
        }

        int currentIndex = songs.IndexOf(CurrentSong);

        // If the current song is not found, currentIndex will be -1
        if (currentIndex == -1)
        {
            return;
        }

        // Start searching for the next song from the current index
        for (int i = currentIndex + 1; i < songs.Count; i++)
        {
            // Wrap around to the start of the list if necessary
            if (i == songs.Count)
            {
                i = 0;
            }

            // If a different song is found, set it as the current song
            if (!songs[i].Equals(CurrentSong))
            {
                CurrentSong = songs[i];
                searchViewModel.CurrentSong = CurrentSong;
                SongDuration = "0:00";
                MusicPosition = 0;
                _musicPlayer.MediaPlayer.Position = 0;
                return;
            }
        }

        // If no different song is found, set the first song in the list as the current song
        CurrentSong = songs[0];
        searchViewModel.CurrentSong = CurrentSong;
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
    }

    private void PreviousSongCommand_Execute()
    {
        SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();
        ObservableCollection<SongModel>? songs = searchViewModel!.SongsList;

        if (songs == null || songs.Count == 0)
        {
            return;
        }

        int currentIndex = songs.IndexOf(CurrentSong);

        // If the current song is not found, currentIndex will be -1
        if (currentIndex == -1)
        {
            return;
        }

        // Start searching for the previous song from the current index
        for (int i = currentIndex - 1; i >= 0; i--)
        {
            // Wrap around to the end of the list if necessary
            if (i < 0)
            {
                i = songs.Count - 1;
            }

            // If a different song is found, set it as the current song
            if (!songs[i].Equals(CurrentSong))
            {
                CurrentSong = songs[i];
                searchViewModel.CurrentSong = CurrentSong;
                SongDuration = "0:00";
                MusicPosition = 0;
                _musicPlayer.MediaPlayer.Position = 0;
                return;
            }
        }

        // If no different song is found, set the last song in the list as the current song
        CurrentSong = songs[^1];
        searchViewModel.CurrentSong = CurrentSong;
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
    }

    private void RepeatSongCommand_Execute()
    {
        _musicPlayer.OnRepeatingSong();
        RepeatMediaBrush = _musicPlayer.IsRepeating ? new SolidColorBrush(Colors.Lime) : new SolidColorBrush(Colors.White);
    }

    private bool IsShuffling = false;

    private void ShuffleSongCommand_Execute()
    {
        SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();
        if (searchViewModel == null || searchViewModel.SongsList == null) return;
        ToggleShuffle(searchViewModel.SongsList);
    }

    private void ToggleShuffle(ObservableCollection<SongModel> songs)
    {
        IsShuffling = !IsShuffling;

        List<SongModel> temp = new(songs); // Create a copy of the ObservableCollection

        if (IsShuffling)
        {
            ShuffleList(temp);
            ShuffleMediaBrush = new SolidColorBrush(Colors.Lime);
        }
        else
        {
            temp.Sort((a, b) => a.Index.CompareTo(b.Index));
            ShuffleMediaBrush = new SolidColorBrush(Colors.White);
        }

        // Clear the original ObservableCollection and add items from the temp list
        songs.Clear();
        foreach (SongModel song in temp)
        {
            songs.Add(song);
        }
    }

    private static void ShuffleList(List<SongModel> list)
    {
        Random rand = new();
        for (int i = 0; i < list.Count; i++)
        {
            int j = rand.Next(i, list.Count);
            if (i != j)
            {
                (list[j], list[i]) = (list[i], list[j]);
            }
        }
    }

    public void RegisterMusicSlider(Slider slider)
    {
        // Event handlers for drag start, delta, and completion
        slider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState()) return;
            _musicPlayer.MediaPlayer.SetPause(true);
            _musicPositionUpdate.Stop();
        }));

        slider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState()) return;
            long progress = (long)(MusicPosition * _musicPlayer.MediaPlayer.Length);
            SongDuration = TimeConverter.ConvertMsToTime(progress);
        }));

        slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState()) return;
            _musicPlayer.MediaPlayer.Position = (float)MusicPosition;
            CheckMediaPlayerPosition();
            _musicPlayer.MediaPlayer.SetPause(false);
            _musicPositionUpdate.Start();
        }));

        // Handle mouse left button down event for smoother dragging
        slider.AddHandler(System.Windows.UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState() || !slider.IsMoveToPointEnabled) return;
            if (slider.Template.FindName("PART_Track", slider) is not Track track || track.Thumb == null || track.Thumb.IsMouseOver) return;

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

    public void CancelPlayingMusic()
    {
        _musicPlayer.MediaPlayer.Stop();
        _musicPositionUpdate.Stop();
        CurrentSong = new();
        MusicPosition = 0;
        SongDuration = "0:00";
    }

    private bool IsValidMediaPlayerState()
    {
        return _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.NothingSpecial &&
               _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.Stopped &&
               _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.Ended &&
               _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.Error;
    }

    partial void OnVolumeValueChanging(double value)
    {
        _musicPlayer.MediaPlayer.Volume = (int)value;
        _settings.SettingsModel.Volume = (int)value;
        _settings.Save();
    }
}