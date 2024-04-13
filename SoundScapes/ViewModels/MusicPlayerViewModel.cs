using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SoundScapes.Views.Dialogs;
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
    [ObservableProperty]
    private bool _isPlayingFromPlaylist = false;
    
    public event EventHandler<SongModel>? SongChanged;
    public event EventHandler<SongModel>? SongPlaylistChanged;
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

        IsPlayingFromPlaylist = true;
    }

    private async Task AddFavoriteSongCommand_Execute()
    {
        var contentAddDialog = App.AppHost?.Services.GetService<PlaylistAddSongItemView>();
        var playlistViewModel = App.AppHost?.Services.GetService<PlaylistViewModel>();
        var musicPlayerViewModel = App.AppHost?.Services.GetService<MusicPlayerViewModel>();

        if (musicPlayerViewModel!.IsPlayingFromPlaylist) return;

        // Check if any necessary service or data is missing
        if (CurrentSong == null || contentAddDialog?.viewModel == null || playlistViewModel?.OriginalPlaylists == null) return;

        if (string.IsNullOrEmpty(CurrentSong.SongID))
        {
            return;
        }

        // Show the dialog and wait for the result
        var result = await contentAddDialog!.ShowAsync();

        // If the user didn't confirm the dialog, return
        if (result != ContentDialogResult.Primary) return;

        var playlistsSelected = contentAddDialog.viewModel.PlaylistsSelected;
        var originalPlaylists = playlistViewModel.OriginalPlaylists;

        // Iterate over selected playlists and add the current song to each playlist
        foreach (var playlist in playlistsSelected)
        {
            // Find the index of the playlist in the original playlists list
            var index = originalPlaylists.FindIndex(p => p.Title == playlist.Title && p.Duration == playlist.Duration);
            if (index != -1)
            {
                // Add the current song to the playlist if it doesn't exist already
                if (!originalPlaylists[index].SongsInPlaylist.Contains(CurrentSong))
                {
                    originalPlaylists[index].SongsInPlaylist.Add(CurrentSong);
                }
            }
        }

        // If inside a specific playlist, update the songs list
        if (playlistViewModel.IsInsidePlaylist)
        {
            playlistViewModel.Songs = null;
            playlistViewModel.Songs = playlistViewModel.CurrentPlaylistSelected?.SongsInPlaylist;
        }

        _settings.Save();

        // Check and update the amount of items in the playlist
        playlistViewModel.CheckAmountOfItemsInPlaylist();

        // Recalculate the playlists
        playlistViewModel.RecalculatePlaylists();
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
        _musicPlayer.MediaPlayer.Position = 0;
        _musicPlayer.OnPlaySong(CurrentSong);
        _musicPositionUpdate.Start();
    }

    private void NextSongCommand_Execute()
    {
        var model = App.AppHost?.Services.GetService<PlaylistViewModel>();
        List<SongModel>? songs = IsPlayingFromPlaylist ? model?.Songs : App.AppHost?.Services.GetService<SearchViewModel>()?.SongsList;

        if (songs == null || songs.Count == 0)
        {
            GoToNextPlaylistIfEmpty(model);
            return;
        }

        int currentIndex = songs.FindIndex(n => n == CurrentSong);
        int index = (currentIndex + 1) % songs.Count;

        if (IsPlayingFromPlaylist && index == 0)
        {
            GoToNextPlaylist(model);
            return;
        }

        int loopCount = 0;
        while (loopCount < songs.Count)
        {
            if (songs[index] != CurrentSong)
            {
                CurrentSong = songs[index];
                SongDuration = "0:00";
                MusicPosition = 0;
                _musicPlayer.MediaPlayer.Position = 0;

                if (IsPlayingFromPlaylist)
                    SongPlaylistChanged?.Invoke(this, CurrentSong);
                else
                    SongChanged?.Invoke(this, CurrentSong);

                return;
            }

            index = (index + 1) % songs.Count;
            loopCount++;
        }
    }

    private void GoToNextPlaylistIfEmpty(PlaylistViewModel? model)
    {
        if (model != null && model.OriginalPlaylists != null)
        {
            int currentIndexPlaylist = model.Playlists!.IndexOf(model.CurrentPlaylistSelected!);
            if (currentIndexPlaylist == -1) return;
            int nextIndex = (currentIndexPlaylist + 1) % model.Playlists.Count; // there was a bug
            int loopCount = 0;

            while (loopCount < model.Playlists.Count)
            {
                if (model.Playlists[nextIndex].SongsInPlaylist.Count > 0)
                {
                    model.CurrentPlaylistSelected = null;
                    model.CurrentPlaylistSelected = model.Playlists[nextIndex];
                    
                    if (IsPlayingFromPlaylist)
                        SongPlaylistChanged?.Invoke(this, CurrentSong);
                    else
                        SongChanged?.Invoke(this, CurrentSong);

                    return;
                }

                nextIndex = (nextIndex + 1) % model.Playlists.Count;
                loopCount++;
            }
        }
    }

    private void GoToNextPlaylist(PlaylistViewModel? model)
    {
        if (model != null)
        {
            int currentIndexPlaylist = model.Playlists!.IndexOf(model.CurrentPlaylistSelected!);
            int nextIndex = (currentIndexPlaylist + 1) % model.Playlists.Count;

            int loopCount = 0;
            while (loopCount < model.Playlists.Count)
            {
                if (model.Playlists[nextIndex].SongsInPlaylist.Count > 0)
                {
                    model.CurrentPlaylistSelected = null;
                    model.CurrentPlaylistSelected = model.Playlists[nextIndex];
                    
                    if (IsPlayingFromPlaylist)
                        SongPlaylistChanged?.Invoke(this, CurrentSong);
                    else
                        SongChanged?.Invoke(this, CurrentSong);

                    return;
                }

                nextIndex = (nextIndex + 1) % model.Playlists.Count;
                loopCount++;
            }
        }
    }

    private void PreviousSongCommand_Execute()
    {
        var model = App.AppHost?.Services.GetService<PlaylistViewModel>();
        List<SongModel>? songs = IsPlayingFromPlaylist ? model?.Songs : App.AppHost?.Services.GetService<SearchViewModel>()?.SongsList;

        if (songs == null || songs.Count == 0)
        {
            GoToPreviousPlaylistIfEmpty(model);
            return;
        }

        int currentIndex = songs.FindIndex(n => n == CurrentSong);
        int index = (currentIndex - 1 + songs.Count) % songs.Count;

        if (IsPlayingFromPlaylist && index == songs.Count - 1)
        {
            GoToPreviousPlaylist(model);
            return;
        }

        int loopCount = 0;
        while (loopCount < songs.Count)
        {
            if (songs[index] != CurrentSong)
            {
                CurrentSong = songs[index];
                SongDuration = "0:00";
                MusicPosition = 0;
                _musicPlayer.MediaPlayer.Position = 0;

                if (IsPlayingFromPlaylist)
                    SongPlaylistChanged?.Invoke(this, CurrentSong);
                else
                    SongChanged?.Invoke(this, CurrentSong);

                return;
            }

            index = (index - 1 + songs.Count) % songs.Count;
            loopCount++;
        }
    }

    private void GoToPreviousPlaylistIfEmpty(PlaylistViewModel? model)
    {
        if (model != null && model.OriginalPlaylists != null)
        {
            int currentIndexPlaylist = model.Playlists!.IndexOf(model.CurrentPlaylistSelected!);
            if (currentIndexPlaylist == -1) return;
            int previousIndex = (currentIndexPlaylist - 1 + model.Playlists.Count) % model.Playlists.Count;
            int loopCount = 0;

            while (loopCount < model.Playlists.Count)
            {
                if (model.Playlists[previousIndex].SongsInPlaylist.Count > 0)
                {
                    model.CurrentPlaylistSelected = null;
                    model.CurrentPlaylistSelected = model.Playlists[previousIndex];

                    if (IsPlayingFromPlaylist)
                        SongPlaylistChanged?.Invoke(this, CurrentSong);
                    else
                        SongChanged?.Invoke(this, CurrentSong);

                    return;
                }

                previousIndex = (previousIndex - 1 + model.Playlists.Count) % model.Playlists.Count;
                loopCount++;
            }
        }
    }

    private void GoToPreviousPlaylist(PlaylistViewModel? model)
    {
        if (model != null)
        {
            int currentIndexPlaylist = model.Playlists!.IndexOf(model.CurrentPlaylistSelected!);
            int previousIndex = (currentIndexPlaylist - 1 + model.Playlists.Count) % model.Playlists.Count;

            int loopCount = 0;
            while (loopCount < model.Playlists.Count)
            {
                if (model.Playlists[previousIndex].SongsInPlaylist.Count > 0)
                {
                    model.CurrentPlaylistSelected = null;
                    model.CurrentPlaylistSelected = model.Playlists[previousIndex];

                    if (IsPlayingFromPlaylist)
                        SongPlaylistChanged?.Invoke(this, CurrentSong);
                    else
                        SongChanged?.Invoke(this, CurrentSong);

                    return;
                }

                previousIndex = (previousIndex - 1 + model.Playlists.Count) % model.Playlists.Count;
                loopCount++;
            }
        }
    }

    private void RepeatSongCommand_Execute()
    {
        _musicPlayer.OnRepeatingSong();
        RepeatMediaBrush = _musicPlayer.IsRepeating ? new SolidColorBrush(Colors.Lime) : new SolidColorBrush(Colors.White);
    }

    private bool IsShuffling = false;

    private void ShuffleSongCommand_Execute()
    {
        // Check if not playing from playlist
        if (!IsPlayingFromPlaylist)
        {
            var searchViewModel = App.AppHost?.Services.GetService<SearchViewModel>();
            if (searchViewModel == null || searchViewModel.SongsList == null) return;
            ToggleShuffle(searchViewModel.SongsList);
        }
        else
        {
            var playlistViewModel = App.AppHost?.Services.GetService<PlaylistViewModel>();
            if (playlistViewModel == null || playlistViewModel.Songs == null) return;
            ToggleShuffle(playlistViewModel.Songs);
        }
    }

    private void ToggleShuffle(List<SongModel> songs)
    {
        IsShuffling = !IsShuffling;
        List<SongModel> temp = new(songs); // Create a copy of the list

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

        songs.Clear(); // Clear the original list
        songs.AddRange(temp); // Update the original list with the shuffled or unshuffled temp list
        UpdateViewList();
    }

    private static void ShuffleList(List<SongModel> list)
    {
        Random rand = new();
        for (int i = 0; i < list.Count * 2; i++)
        {
            int a = rand.Next(0, list.Count - 1);
            int b = rand.Next(0, list.Count - 1);
            (list[b], list[a]) = (list[a], list[b]);
        }
    }

    private void UpdateViewList()
    {
        if (!IsPlayingFromPlaylist)
        {
            var searchViewModel = App.AppHost?.Services.GetService<SearchViewModel>();
            searchViewModel?.UpdateViewList();
        }
        else
        {
            var playlistViewModel = App.AppHost?.Services.GetService<PlaylistViewModel>();
            playlistViewModel?.UpdateViewSongList();
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