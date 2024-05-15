using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SoundScapes.Views.Dialogs;
using System.IO;
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
        _musicPlayerView = musicPlayer;
        _musicPlayerView.SongPlaylistChanged += (o, e) => CurrentSong = e;
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
            List<PlaylistModel> temp = OriginalPlaylists;
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
            SongModel song = list[CurrentSongIndex];
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
            List<PlaylistModel> temp = OriginalPlaylists;
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
            SongModel song = list[CurrentSongIndex];
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
        string title = IsInsidePlaylist ? "Видалення трека з плейлиста" : "Видалення плейлиста";
        string content = IsInsidePlaylist ? "Ви впевнені у тому щоб видалити цей трек з плейлиста?" : "Ви впевнені у тому щоб видалити цей плейлист?";

        ContentDialog removeDialog = new()
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "Так",
            SecondaryButtonText = "Ні"
        };

        ContentDialogResult result = await removeDialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        _musicPlayerView.CancelPlayingMusic();
        if (!IsInsidePlaylist)
        {
            foreach (SongModel file in CurrentPlaylistSelected!.SongsInPlaylist)
            {
                string filePath = Path.Combine("SavedMusic", $"{file.Artist[0]} - {file.Title}.mp3");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            Songs?.Clear();
            OriginalPlaylists?.Remove(CurrentPlaylistSelected!);
            List<PlaylistModel>? temp = Playlists;
            Playlists = null;
            Playlists = temp;
        }
        else
        {
            if (CurrentSongIndex >= 0 && CurrentSongIndex < CurrentPlaylistSelected!.SongsInPlaylist.Count)
            {
                string filePath = Path.Combine("SavedMusic", $"{CurrentPlaylistSelected.SongsInPlaylist[CurrentSongIndex].Artist[0]} - {CurrentPlaylistSelected.SongsInPlaylist[CurrentSongIndex].Title}.mp3");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                CurrentPlaylistSelected.SongsInPlaylist.RemoveAt(CurrentSongIndex);
                Songs = CurrentPlaylistSelected.SongsInPlaylist;
                UpdateViewSongList();
            }
        }

        CheckAmountOfItemsInPlaylist();
        RecalculatePlaylists();
        _settings.Save();
    }

    private async Task InstallPlaylistCommand_ExecuteAsync()
    {
        PlaylistInstallSongView? installDialog = App.AppHost?.Services.GetRequiredService<PlaylistInstallSongView>();
        if (installDialog == null || installDialog.playlistInstallSongViewModel == null || CurrentPlaylistSelected == null) return;

        installDialog.playlistInstallSongViewModel.DownloadListQueue.Clear();
        installDialog.playlistInstallSongViewModel.DownloadedListQueue.Clear();

        List<SongModel> downloadQueueList = [];

        if (!IsInsidePlaylist)
        {
            downloadQueueList.AddRange(CurrentPlaylistSelected.SongsInPlaylist.Where(file => !File.Exists(Path.Combine("SavedMusic", $"{file.Artist[0]} - {file.Title}.mp3"))));
            if (downloadQueueList.Count == 0) 
            {
                ContentDialog contentDialog = new()
                {
                    Title = "Завантаження Треків",
                    Content = $"Усі трекі у цьому плейлисті вже завантаженні,\nабо іх нема у плейлисті взагалі!",
                    PrimaryButtonText = "OK"
                };
                await contentDialog.ShowAsync();
                return;
            }
        }
        else
        {
            if (CurrentSong == null || File.Exists(Path.Combine("SavedMusic", $"{CurrentSong.Artist[0]} - {CurrentSong.Title}.mp3")))
            {
                ContentDialog contentDialog = new()
                {
                    Title = "Завантаження Треків",
                    Content = $"Трек вже існує, і не потребує повторного завантаження.",
                    PrimaryButtonText = "OK"
                };
                await contentDialog.ShowAsync();
                return;
            }
            downloadQueueList.Add(CurrentSong);
        }

        installDialog.playlistInstallSongViewModel.DownloadListQueue = downloadQueueList;
        var result = await installDialog!.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            installDialog.playlistInstallSongViewModel.CancelDownloadCommand.Execute(null);
        }
    }

    private async Task EditPlaylistCommand_ExecuteAsync()
    {
        PlaylistEditItemView? contentEditDialog = App.AppHost?.Services.GetRequiredService<PlaylistEditItemView>();
        contentEditDialog!.model!.Title = CurrentPlaylistSelected!.Title;
        contentEditDialog.model.Icon = new Uri(CurrentPlaylistSelected.Icon).LocalPath;
        ContentDialogResult result = await contentEditDialog!.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (CurrentPlaylistSelected == null) return;
            List<PlaylistModel>? temp = OriginalPlaylists;
            if (temp != null)
            {
                int index = temp.FindIndex(model => model == CurrentPlaylistSelected);
                temp[index].Title = contentEditDialog.TitleTextBox.Text;
                temp[index].Icon = contentEditDialog.IconImage.Source.ToString();
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
        PlaylistAddItemView? contentAddDialog = App.AppHost?.Services.GetRequiredService<PlaylistAddItemView>();
        ContentDialogResult result = await contentAddDialog!.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            List<PlaylistModel>? temp = OriginalPlaylists;
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

        foreach (PlaylistModel playlist in OriginalPlaylists)
        {
            long totalTime = 0;
            HashSet<string> uniqueAuthors = [];

            foreach (SongModel song in playlist.SongsInPlaylist)
            {
                totalTime += song.DurationLong;
                foreach (string author in song.Artist.Split(','))
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
        List<SongModel>? temp = Songs;
        Songs = null;
        Songs = temp;
    }

    partial void OnCurrentSongChanged(SongModel? value)
    {
        if (value == null) return;
        _musicPlayerView.CurrentSong = value;
        _musicPlayerView.PlayMediaIcon = _musicPlayerView._musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
        _musicPlayerView.PlaySong();
        _musicPlayerView.IsPlayingFromPlaylist = true;
        CurrentSongIndex = Songs?.IndexOf(value) ?? -1;
    }

    partial void OnCurrentPlaylistSelectedChanged(PlaylistModel? value)
    {
        if (value == null) return;
        Songs = value.SongsInPlaylist;
        if (value.SongsInPlaylist.Count != 0)
        {
            _musicPlayerView.CurrentSong = value.SongsInPlaylist[0];
            _musicPlayerView.PlayMediaIcon = _musicPlayerView._musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
            _musicPlayerView.PlaySong();
            _musicPlayerView.IsPlayingFromPlaylist = true;
            CurrentSongIndex = Songs?.IndexOf(value.SongsInPlaylist[0]) ?? -1;
        }
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