using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Models;
using SoundScapes.Views.Dialogs;
using System.Windows;

namespace SoundScapes.ViewModels;

public partial class PlaylistViewModel : ObservableObject
{
    [ObservableProperty]
    private List<PlaylistModel>? _playlists = [];
    private List<PlaylistModel>? OriginalPlaylists = [];
    [ObservableProperty]
    private PlaylistModel? currentPlaylistSelected = null;
    [ObservableProperty]
    private IAsyncRelayCommand _addPlaylistCommand;
    [ObservableProperty]
    private IAsyncRelayCommand _editPlaylistCommand;
    [ObservableProperty]
    private IAsyncRelayCommand _installPlaylistCommand;
    [ObservableProperty]
    private IAsyncRelayCommand _removePlaylistCommand;
    [ObservableProperty]
    private RelayCommand _moveUpPlaylistCommand;
    [ObservableProperty]
    private RelayCommand _moveDownPlaylistCommand;
    [ObservableProperty]
    private Visibility _errorTextVisibility = Visibility.Visible;
    [ObservableProperty]
    private string _errorText = string.Empty;

    public PlaylistViewModel()
    {
        AddPlaylistCommand = new AsyncRelayCommand(AddPlaylistCommand_ExecuteAsync);
        EditPlaylistCommand = new AsyncRelayCommand(EditPlaylistCommand_ExecuteAsync);
        InstallPlaylistCommand = new AsyncRelayCommand(InstallPlaylistCommand_ExecuteAsync);
        RemovePlaylistCommand = new AsyncRelayCommand(RemovePlaylistCommand_ExecuteAsync);
        MoveDownPlaylistCommand = new RelayCommand(MoveDownPlaylistCommand_Execute);
        MoveUpPlaylistCommand = new RelayCommand(MoveUpPlaylistCommand_Execute);
        CheckAmountOfItemsInPlaylist();
    }

    private void MoveUpPlaylistCommand_Execute()
    {
        if (CurrentPlaylistSelected == null || OriginalPlaylists == null || Playlists == null || Playlists.Count == 0) return;
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
    }

    private void MoveDownPlaylistCommand_Execute()
    {
        if (CurrentPlaylistSelected == null || OriginalPlaylists == null || Playlists == null || Playlists.Count == 0) return;
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
    }

    private async Task RemovePlaylistCommand_ExecuteAsync()
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
        }
    }

    private void CheckAmountOfItemsInPlaylist()
    {
        if (Playlists?.Count == 0)
        {
            ErrorText = "У вас нема жодного створеного плейлиста!\nСтворіть його натиснувши праву клавишу миші, та обрати у меню 'Додати плейлист...'";
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
            Playlists = OriginalPlaylists;
            if (Playlists == null) return;
            string query = e.QueryText.ToLower();
            List<PlaylistModel> list = [];
            foreach (var item in Playlists)
            {
                if (item.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                {
                    list.Add(item);
                }
            }
            Playlists = null;
            Playlists = list;
        };
    }
}