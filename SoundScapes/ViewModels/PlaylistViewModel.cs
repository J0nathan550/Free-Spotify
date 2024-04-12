using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SoundScapes.Views.Dialogs;
using System.Windows;

namespace SoundScapes.ViewModels;

public partial class PlaylistViewModel : ObservableObject
{
    [ObservableProperty]
    private List<PlaylistModel>? _playlists = [];
    [ObservableProperty]
    private List<PlaylistModel>? _originalPlaylists = [];
    [ObservableProperty]
    private List<SongModel>? _songs = [];
    [ObservableProperty]
    private PlaylistModel? _currentPlaylistSelected = null;
    [ObservableProperty]
    private SongModel? _currentSong = null;
    private int CurrentSongIndex = -1;
    [ObservableProperty]
    private RelayCommand _openPlaylistCommand;
    [ObservableProperty]
    private RelayCommand _moveUpPlaylistCommand;
    [ObservableProperty]
    private RelayCommand _moveDownPlaylistCommand;
    [ObservableProperty]
    private IAsyncRelayCommand _addPlaylistCommand;
    [ObservableProperty]
    private IAsyncRelayCommand _editPlaylistCommand;
    [ObservableProperty]
    private IAsyncRelayCommand _installPlaylistCommand;
    [ObservableProperty]
    private IAsyncRelayCommand _removePlaylistCommand;
    [ObservableProperty]
    private Visibility _errorTextVisibility = Visibility.Visible;
    [ObservableProperty]
    private Visibility _playlistListBoxVisibility = Visibility.Visible;
    [ObservableProperty]
    private Visibility _musicListBoxVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private string _errorText = string.Empty;
    [ObservableProperty]
    private string _queryText = string.Empty;
    [ObservableProperty]
    private string _placeholderText = "Напишіть назву плейлиста...";
    [ObservableProperty]
    private bool _isInsidePlaylist = false;

    private readonly MusicPlayerViewModel _musicPlayerView;
    private readonly ISettings _settings;

    public PlaylistViewModel(MusicPlayerViewModel musicPlayer, ISettings settings)
    {
        _settings = settings;
        Playlists = _settings.SettingsModel.Playlists;
        OriginalPlaylists = _settings.SettingsModel.Playlists;

        AddPlaylistCommand = new AsyncRelayCommand(AddPlaylistCommand_ExecuteAsync);
        EditPlaylistCommand = new AsyncRelayCommand(EditPlaylistCommand_ExecuteAsync);
        InstallPlaylistCommand = new AsyncRelayCommand(InstallPlaylistCommand_ExecuteAsync);
        RemovePlaylistCommand = new AsyncRelayCommand(RemovePlaylistCommand_ExecuteAsync);
        MoveDownPlaylistCommand = new RelayCommand(MoveDownPlaylistCommand_Execute);
        MoveUpPlaylistCommand = new RelayCommand(MoveUpPlaylistCommand_Execute);
        OpenPlaylistCommand = new RelayCommand(OpenPlaylistCommand_Execute);
        CheckAmountOfItemsInPlaylist();
        RecalculatePlaylists();

        _musicPlayerView = musicPlayer;
        _musicPlayerView.SongPlaylistChanged += (o, e) => CurrentSong = e;
    }

    private void OpenPlaylistCommand_Execute()
    {
        IsInsidePlaylist = !IsInsidePlaylist;

        PlaceholderText = IsInsidePlaylist ? "Напишіть назву треку..." : "Напишіть назву плейлиста...";
        MusicListBoxVisibility = IsInsidePlaylist ? Visibility.Visible : Visibility.Collapsed;
        PlaylistListBoxVisibility = IsInsidePlaylist ? Visibility.Collapsed : Visibility.Visible;

        if (IsInsidePlaylist) Songs = CurrentPlaylistSelected?.SongsInPlaylist;

        RecalculatePlaylists();
        CheckAmountOfItemsInPlaylist();
    }

    private void MoveUpPlaylistCommand_Execute()
    {
        if (CurrentPlaylistSelected == null || OriginalPlaylists == null || Playlists == null || Playlists.Count == 0) return;
        if (!IsInsidePlaylist)
        {
            var temp = OriginalPlaylists;
            int currentIndex = temp.IndexOf(CurrentPlaylistSelected);
            if (currentIndex == 0)
            {
                // If trying to move up but already at the start, move to the end
                temp.Remove(CurrentPlaylistSelected);
                temp.Add(CurrentPlaylistSelected);
            }
            else
            {
                temp.RemoveAt(currentIndex);
                temp.Insert(currentIndex - 1, CurrentPlaylistSelected);
            }
            Playlists = null;
            OriginalPlaylists = temp;
            Playlists = OriginalPlaylists;
            RecalculatePlaylists();
        }
        else
        {
            List<SongModel> list = new(Songs!);
            if (CurrentSongIndex == 0)
            {
                return;
            }
            var song = list[CurrentSongIndex];
            list.RemoveAt(CurrentSongIndex);
            list.Insert(CurrentSongIndex - 1, song);
            Songs = null;
            CurrentPlaylistSelected.SongsInPlaylist = list;
            Songs = CurrentPlaylistSelected.SongsInPlaylist;
            RecalculatePlaylists();
        }
        _settings.Save();
    }

    private void MoveDownPlaylistCommand_Execute()
    {
        if (CurrentPlaylistSelected == null || OriginalPlaylists == null || Playlists == null || Playlists.Count == 0) return;
        if (!IsInsidePlaylist)
        {
            var temp = OriginalPlaylists;
            int currentIndex = temp.IndexOf(CurrentPlaylistSelected);
            if (currentIndex == temp.Count - 1)
            {
                temp.Remove(CurrentPlaylistSelected);
                temp.Insert(0, CurrentPlaylistSelected);
            }
            else
            {
                temp.RemoveAt(currentIndex);
                temp.Insert(currentIndex + 1, CurrentPlaylistSelected);
            }
            Playlists = null;
            OriginalPlaylists = temp;
            Playlists = OriginalPlaylists;
            RecalculatePlaylists();
        }
        else
        {
            List<SongModel> list = new(Songs!);
            if (CurrentSongIndex == Songs?.Count - 1)
            {
                return;
            }
            var song = list[CurrentSongIndex];
            list.RemoveAt(CurrentSongIndex);
            list.Insert(CurrentSongIndex + 1, song);
            Songs = null;
            CurrentPlaylistSelected.SongsInPlaylist = list;
            Songs = CurrentPlaylistSelected.SongsInPlaylist;
            RecalculatePlaylists();
        }
        _settings.Save();
    }

    private async Task RemovePlaylistCommand_ExecuteAsync()
    {
        ContentDialog removeDialog;
        if (!IsInsidePlaylist)
        {
            removeDialog = new ContentDialog
            {
                Title = "Видалення плейлиста",
                Content = "Ви впевнені у тому щоб видалити цей плейлист?",
                PrimaryButtonText = "Так",
                SecondaryButtonText = "Ні"
            };
        }
        else
        {
            removeDialog = new ContentDialog
            {
                Title = "Видалення трека з плейлиста",
                Content = "Ви впевнені у тому щоб видалити цей трек з плейлиста?",
                PrimaryButtonText = "Так",
                SecondaryButtonText = "Ні"
            };
        }

        var result = await removeDialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        if (!IsInsidePlaylist)
        {
            Songs?.Clear();
            OriginalPlaylists?.Remove(CurrentPlaylistSelected!);
            Playlists = null;
            Playlists = OriginalPlaylists;
        }
        else if (CurrentPlaylistSelected != null)
        {
            CurrentPlaylistSelected.SongsInPlaylist.RemoveAt(CurrentSongIndex);
            Songs = CurrentPlaylistSelected.SongsInPlaylist;
            UpdateViewSongList();
        }
        CheckAmountOfItemsInPlaylist();
        RecalculatePlaylists();
        _settings.Save();
    }


    private async Task InstallPlaylistCommand_ExecuteAsync()
    {
        var contentAddDialog = App.AppHost?.Services.GetService<PlaylistInstallSongView>();
        var result = await contentAddDialog!.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return;
        }
        //delete
    }

    private async Task EditPlaylistCommand_ExecuteAsync()
    {
        var contentAddDialog = App.AppHost?.Services.GetService<PlaylistEditItemView>();
        var result = await contentAddDialog!.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (CurrentPlaylistSelected == null) return;
            var temp = OriginalPlaylists;
            if (temp != null)
            {
                int index = temp.FindIndex(model => model == CurrentPlaylistSelected);
                temp[index].Title = contentAddDialog.TitleTextBox.Text;
                temp[index].Icon = contentAddDialog.IconImage.Source.ToString();
                Playlists = null;
                OriginalPlaylists = temp;
                Playlists = OriginalPlaylists;
                CheckAmountOfItemsInPlaylist();
                RecalculatePlaylists();
                _settings.Save();
            }
        }
    }

    private async Task AddPlaylistCommand_ExecuteAsync()
    {
        var contentAddDialog = App.AppHost?.Services.GetService<PlaylistAddItemView>();
        var result = await contentAddDialog!.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var temp = OriginalPlaylists;
            temp?.Add(new PlaylistModel() { Title = contentAddDialog.TitleTextBox.Text, Icon = contentAddDialog.IconImage.Source.ToString() });
            Playlists = null;
            OriginalPlaylists = temp;
            Playlists = OriginalPlaylists;
            CheckAmountOfItemsInPlaylist();
            RecalculatePlaylists();
        }
        _settings.Save();
    }

    public void CheckAmountOfItemsInPlaylist()
    {
        if (!IsInsidePlaylist)
        {
            if (OriginalPlaylists == null || OriginalPlaylists.Count == 0)
            {
                ErrorText = "У вас нема жодного створеного плейлиста!\nСтворіть його натиснувши праву клавишу миші, та обрати у меню 'Додати плейлист...'";
                ErrorTextVisibility = Visibility.Visible;
                return;
            }
            if (OriginalPlaylists.Count > 0 && (Playlists == null || Playlists.Count == 0))
            {
                ErrorText = $"По запиту '{QueryText}' нічого не було знайдено!";
                ErrorTextVisibility = Visibility.Visible;
                return;
            }
        }
        else
        {
            if (Songs == null || Songs.Count == 0)
            {
                ErrorText = "У вас нема жодного треку!\nДодайте пісню у пошуку натиснувши на сердце!";
                ErrorTextVisibility = Visibility.Visible;
                return;
            }
            if (CurrentPlaylistSelected?.SongsInPlaylist.Count > 0 && (Songs == null || Songs.Count == 0))
            {
                ErrorText = $"По запиту '{QueryText}' нічого не було знайдено!";
                ErrorTextVisibility = Visibility.Visible;
                return;
            }
        }

        ErrorText = string.Empty;
        ErrorTextVisibility = Visibility.Collapsed;
        
    }

    public void RegisterSearchPlaylistBox(AutoSuggestBox autoSuggestBox)
    {
        autoSuggestBox.QuerySubmitted += (o, e) =>
        {
            string query = e.QueryText.ToLower();
            if (!IsInsidePlaylist)
            {
                Playlists = OriginalPlaylists?.Where(p => p.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase)).ToList();
                return;
            }
            Songs = CurrentPlaylistSelected?.SongsInPlaylist?.Where(s => s.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase)).ToList();
            RecalculatePlaylists();
        };
    }

    public void RecalculatePlaylists()
    {
        if (OriginalPlaylists == null) return;

        foreach (var playlist in OriginalPlaylists)
        {
            long totalTime = 0;
            HashSet<string> uniqueAuthors = [];

            foreach (var song in playlist.SongsInPlaylist)
            {
                totalTime += song.DurationLong;
                foreach (var author in song.Artist.Split(','))
                {
                    uniqueAuthors.Add(author.Trim());
                }
            }

            playlist.Duration = TimeConverter.ConvertMsToTime(totalTime);
            playlist.Authors = uniqueAuthors.Count != 0 ? string.Join(", ", uniqueAuthors) + "." : "...";
        }
    }

    public void UpdateViewSongList()
    {
        var temp = Songs;
        Songs = null;
        Songs = temp;
    }

    partial void OnCurrentSongChanged(SongModel? value)
    {
        if (value == null) return;
        _musicPlayerView.CurrentSong = value;
        _musicPlayerView.PlayMediaIcon = _musicPlayerView._musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
        _musicPlayerView.PlaySong();
        CurrentSongIndex = Songs?.IndexOf(value) ?? -1;
    }

    partial void OnCurrentPlaylistSelectedChanged(PlaylistModel? value)
    {
        if (value == null) return;
        Songs = value.SongsInPlaylist;
        _musicPlayerView._musicPlayer.CancelPlayingMusic();
        _musicPlayerView._musicPlayer.MediaPlayer.Stop();
        _musicPlayerView.MusicPosition = 0;
        _musicPlayerView.SongDuration = "0:00";
        _musicPlayerView._musicPositionUpdate.Stop();
        if (value.SongsInPlaylist.Count == 0)
        {
            // Select next playlist if available
            SelectNextPlaylist();
            return;
        }
        _musicPlayerView.CurrentSong = value.SongsInPlaylist[0];
        _musicPlayerView.PlayMediaIcon = _musicPlayerView._musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
        _musicPlayerView.PlaySong();
        CurrentSongIndex = Songs?.IndexOf(value.SongsInPlaylist[0]) ?? -1;
    }

    private void SelectNextPlaylist()
    {
        if (Playlists == null || CurrentPlaylistSelected == null)
        {
            _musicPlayerView.CurrentSong = new();
            return;
        }
        int currentIndex = Playlists.IndexOf(CurrentPlaylistSelected);

        // Start from the next playlist index and loop through the playlists
        for (int i = currentIndex + 1; i < Playlists.Count; i++)
        {
            var nextPlaylist = Playlists[i];
            if (nextPlaylist != null && nextPlaylist.SongsInPlaylist.Count > 0)
            {
                // Found a valid playlist, set it as the current playlist and exit the loop
                CurrentPlaylistSelected = nextPlaylist;
                return;
            }
        }

        _musicPlayerView.CurrentSong = new();
    }

    partial void OnPlaylistsChanged(List<PlaylistModel>? value)
    {
        if (value != null)
        {
            _settings.SettingsModel.Playlists = value;
            _settings.Save();
        }
    }
}