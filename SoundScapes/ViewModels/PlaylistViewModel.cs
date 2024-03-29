using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf.Controls;
using SoundScapes.Models;

namespace SoundScapes.ViewModels;

public partial class PlaylistViewModel : ObservableObject
{
    [ObservableProperty]
    private List<PlaylistModel> _playlists = [new PlaylistModel() { Duration = "1:00", Title = "Meow", SongsInside = "Meow" }];
    [ObservableProperty]
    private PlaylistModel? currentPlaylistSelected = null;
    [ObservableProperty]
    private RelayCommand _addPlaylistCommand;
    [ObservableProperty]
    private RelayCommand _editPlaylistCommand;
    [ObservableProperty]
    private RelayCommand _installPlaylistCommand;
    [ObservableProperty]
    private RelayCommand _removePlaylistCommand;

    public PlaylistViewModel()
    {
        AddPlaylistCommand = new RelayCommand(AddPlaylistCommand_Execute);
        EditPlaylistCommand = new RelayCommand(EditPlaylistCommand_Execute);
        InstallPlaylistCommand = new RelayCommand(InstallPlaylistCommand_Execute);
        RemovePlaylistCommand = new RelayCommand(RemovePlaylistCommand_Execute);
    }

    private void RemovePlaylistCommand_Execute()
    {
    }

    private void InstallPlaylistCommand_Execute()
    {
    }

    private void EditPlaylistCommand_Execute()
    {
    }

    private void AddPlaylistCommand_Execute()
    {
    }

    public void RegisterSearchPlaylistBox(AutoSuggestBox autoSuggestBox)
    {
        autoSuggestBox.QuerySubmitted += (o, e) =>
        {

        };
    }
}