using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SoundScapes.Views;
using SoundScapes.Views.Dialogs;
using System.Text;
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
    private SongModel? _currentSongSelected = null;
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

    private readonly MusicPlayerViewModel _musicPlayer;

    public PlaylistViewModel(MusicPlayerViewModel musicPlayer)
    {
        AddPlaylistCommand = new AsyncRelayCommand(AddPlaylistCommand_ExecuteAsync);
        EditPlaylistCommand = new AsyncRelayCommand(EditPlaylistCommand_ExecuteAsync);
        InstallPlaylistCommand = new AsyncRelayCommand(InstallPlaylistCommand_ExecuteAsync);
        RemovePlaylistCommand = new AsyncRelayCommand(RemovePlaylistCommand_ExecuteAsync);
        MoveDownPlaylistCommand = new RelayCommand(MoveDownPlaylistCommand_Execute);
        MoveUpPlaylistCommand = new RelayCommand(MoveUpPlaylistCommand_Execute);
        OpenPlaylistCommand = new RelayCommand(OpenPlaylistCommand_Execute);
        CheckAmountOfItemsInPlaylist();
        RecalculatePlaylists();
        _musicPlayer = musicPlayer;
    }

    private void OpenPlaylistCommand_Execute()
    {
        IsInsidePlaylist = !IsInsidePlaylist;
        if (IsInsidePlaylist)
        {
            PlaceholderText = "Напишіть назву треку...";
            Songs = null;
            Songs = CurrentPlaylistSelected?.SongsInPlaylist;
            MusicListBoxVisibility = Visibility.Visible;
            PlaylistListBoxVisibility = Visibility.Collapsed;
            RecalculatePlaylists();
            CheckAmountOfItemsInPlaylist();
            return;
        }
        PlaceholderText = "Напишіть назву плейлиста...";
        MusicListBoxVisibility = Visibility.Collapsed;
        PlaylistListBoxVisibility = Visibility.Visible;
        RecalculatePlaylists();
        CheckAmountOfItemsInPlaylist();
    }

    private void MoveUpPlaylistCommand_Execute()
    {
        if (CurrentPlaylistSelected == null || CurrentSongSelected == null || OriginalPlaylists == null || Playlists == null || Playlists.Count == 0) return;
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
            var temp = CurrentPlaylistSelected.SongsInPlaylist;
            int currentIndex = temp.IndexOf(CurrentSongSelected);
            if (currentIndex == 0)
            {
                // If trying to move up but already at the start, move to the end
                temp.Remove(CurrentSongSelected);
                temp.Add(CurrentSongSelected);
            }
            else
            {
                temp.RemoveAt(currentIndex);
                temp.Insert(currentIndex - 1, CurrentSongSelected);
            }
            Songs = null;
            CurrentPlaylistSelected.SongsInPlaylist = temp;
            Songs = CurrentPlaylistSelected.SongsInPlaylist;
            RecalculatePlaylists();
        }
    }

    private void MoveDownPlaylistCommand_Execute()
    {
        if (CurrentPlaylistSelected == null || CurrentSongSelected == null || OriginalPlaylists == null || Playlists == null || Playlists.Count == 0) return;
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
            var temp = CurrentPlaylistSelected.SongsInPlaylist;
            int currentIndex = temp.IndexOf(CurrentSongSelected);
            if (currentIndex == temp.Count - 1)
            {
                temp.Remove(CurrentSongSelected);
                temp.Insert(0, CurrentSongSelected);
            }
            else
            {
                temp.RemoveAt(currentIndex);
                temp.Insert(currentIndex + 1, CurrentSongSelected);
            }
            Songs = null;
            CurrentPlaylistSelected.SongsInPlaylist = temp;
            Songs = CurrentPlaylistSelected.SongsInPlaylist;
            RecalculatePlaylists();
        }
    }

    private async Task RemovePlaylistCommand_ExecuteAsync()
    {
        if (!IsInsidePlaylist)
        {
            ContentDialog removeDialog = new() { Title = "Видалення плейлиста", Content = "Ви впевнені у тому щоб видалити цей плейлист?", PrimaryButtonText = "Так", SecondaryButtonText = "Ні" };
            var result = await removeDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var temp = OriginalPlaylists;
                temp?.Remove(CurrentPlaylistSelected!);
                Playlists = null;
                OriginalPlaylists = temp;
                Playlists = OriginalPlaylists;
                CheckAmountOfItemsInPlaylist();
                RecalculatePlaylists();
            }
        }
        else
        {
            ContentDialog removeDialog = new() { Title = "Видалення трека з плейлиста", Content = "Ви впевнені у тому щоб видалити цей трек з плейлиста?", PrimaryButtonText = "Так", SecondaryButtonText = "Ні" };
            var result = await removeDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (CurrentPlaylistSelected == null) return;
                var temp = CurrentPlaylistSelected.SongsInPlaylist;
                temp.Remove(CurrentSongSelected!);
                Songs = null;
                CurrentPlaylistSelected.SongsInPlaylist = temp;
                Songs = CurrentPlaylistSelected.SongsInPlaylist;
                CheckAmountOfItemsInPlaylist();
                RecalculatePlaylists();
            }
        }
    }

    private async Task InstallPlaylistCommand_ExecuteAsync()
    {
        await Task.Delay(1);
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
    }

    public void CheckAmountOfItemsInPlaylist()
    {
        if (!IsInsidePlaylist)
        {
            if (OriginalPlaylists?.Count == 0)
            {
                ErrorText = "У вас нема жодного створеного плейлиста!\nСтворіть його натиснувши праву клавишу миші, та обрати у меню 'Додати плейлист...'";
                ErrorTextVisibility = Visibility.Visible;
                return;
            }
            if (OriginalPlaylists?.Count > 0 && Playlists?.Count == 0)
            {
                ErrorText = $"По запиту '{QueryText}' нічого не було знайдено!";
                ErrorTextVisibility = Visibility.Visible;
                return;
            }
            ErrorText = string.Empty;
            ErrorTextVisibility = Visibility.Collapsed;
            return;
        }
        if (Songs?.Count == 0)
        {
            ErrorText = "У вас нема жодного треку!\nДодайте пісню у пошуку натиснувши на сердце!";
            ErrorTextVisibility = Visibility.Visible;
            return;
        }
        if (CurrentPlaylistSelected?.SongsInPlaylist.Count > 0 && Songs?.Count == 0)
        {
            ErrorText = $"По запиту '{QueryText}' нічого не було знайдено!";
            ErrorTextVisibility = Visibility.Visible;
            return;
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
                Playlists = OriginalPlaylists;
                if (Playlists == null) return;
                List<PlaylistModel> listPlaylist = [];
                foreach (var item in Playlists)
                {
                    if (item.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                    {
                        listPlaylist.Add(item);
                    }
                }
                Playlists = null;
                Playlists = listPlaylist;
                return;
            }
            Songs = CurrentPlaylistSelected?.SongsInPlaylist;
            if (Songs == null) return;
            List<SongModel> listSongs = [];
            foreach (var item in Songs)
            {
                if (item.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                {
                    listSongs.Add(item);
                }
            }
            RecalculatePlaylists();
            Songs = null;
            Songs = listSongs;
        };
    }

    public void RecalculatePlaylists()
    {
        if (OriginalPlaylists == null) return;

        foreach (var playlist in OriginalPlaylists)
        {
            long totalTime = 0;
            StringBuilder authorsBuilder = new();
            HashSet<string> uniqueAuthors = [];

            foreach (var song in playlist.SongsInPlaylist)
            {
                totalTime += song.DurationLong;
                string[] songAuthors = song.Artist.Split(',');
                foreach (var author in songAuthors)
                {
                    uniqueAuthors.Add(author.Trim());
                }
            }

            foreach (var author in uniqueAuthors)
            {
                if (authorsBuilder.Length > 0) authorsBuilder.Append(", ");
                authorsBuilder.Append(author);
            }

            if (authorsBuilder.Length > 0) authorsBuilder.Append('.');
            else authorsBuilder.Append("...");

            playlist.Duration = TimeConverter.ConvertMsToTime(totalTime);
            playlist.Authors = authorsBuilder.ToString();
        }
    }

    partial void OnCurrentSongSelectedChanged(SongModel? value)
    {
        if (value != null) // when the list in search is getting changed it can send null
        {
            _musicPlayer.CurrentSong = value;
            _musicPlayer.PlayMediaIcon = FontAwesomeIcon.Pause;
            _musicPlayer.PlaySong();
        }
    }

}