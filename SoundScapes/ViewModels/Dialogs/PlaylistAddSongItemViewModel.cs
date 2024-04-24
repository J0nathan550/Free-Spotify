using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Models;
using System.Windows;

namespace SoundScapes.ViewModels;

public partial class PlaylistAddSongItemViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isPrimaryButtonEnabled = false;
    [ObservableProperty]
    private List<PlaylistModel> _playlistsOriginal = [];
    [ObservableProperty]
    private List<PlaylistModel>? _playlists = [];
    [ObservableProperty]
    private List<PlaylistModel> _playlistsSelected = [];
    [ObservableProperty]
    private Visibility _errorTextVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private Visibility _searchTextVisibility = Visibility.Visible;
    [ObservableProperty]
    private RelayCommand<object>? _checkBoxSelectedCommand;
    public PlaylistAddSongItemViewModel()
    {
        var model = App.AppHost?.Services.GetService<PlaylistViewModel>();
        if (model == null || model.OriginalPlaylists == null) return;
        Playlists = null;
        foreach (var item in model.OriginalPlaylists)
        {
            item.IsChecked = false;
        }
        PlaylistsOriginal = model.OriginalPlaylists;
        if (PlaylistsOriginal.Count == 0)
        {
            ErrorTextVisibility = Visibility.Visible;
            SearchTextVisibility = Visibility.Collapsed;
        }
        Playlists = PlaylistsOriginal;
        CheckBoxSelectedCommand = new RelayCommand<object>(CheckBoxSelectedCommand_Execute);
    }

    private void CheckBoxSelectedCommand_Execute(object? obj)
    {
        if (obj is System.Windows.Controls.CheckBox checkbox)
        {
            if (checkbox.IsChecked == true)
            {
                if (checkbox.Tag is not PlaylistModel model) return;
                PlaylistModel modelConstructor = new()
                {
                    Title = model.Title,
                    Authors = model.Authors,
                    IsChecked = (bool)checkbox.IsChecked,
                    Duration = model.Duration,
                    Icon = model.Icon,
                    SongsInPlaylist = model.SongsInPlaylist,
                };
                PlaylistsSelected.Add(modelConstructor);
            }
            else
            {
                if (checkbox.Tag is not PlaylistModel model) return;
                var item = PlaylistsSelected.Where(m => m.Title == model.Title && m.Duration == model.Duration).FirstOrDefault();
                if (item != null)
                {
                    PlaylistsSelected.Remove(item);
                }
            }
            IsPrimaryButtonEnabled = PlaylistsSelected.Count > 0;
        }
    }

    public void RegisterSearchPlaylistBox(AutoSuggestBox autoSuggestBox)
    {
        autoSuggestBox.QuerySubmitted += (o, e) =>
        {
            string query = e.QueryText.ToLower();
            Playlists = PlaylistsOriginal;
            if (Playlists == null) return;
            List<PlaylistModel> filteredPlaylists = [];

            foreach (var item in Playlists)
            {
                foreach (var selectedPlaylist in PlaylistsSelected)
                {
                    // Check if the titles match to determine equality
                    
                    if (item.Title.Equals(selectedPlaylist.Title, StringComparison.InvariantCultureIgnoreCase))
                    {
                        item.IsChecked = selectedPlaylist.IsChecked;
                    }
                }

                if (item.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredPlaylists.Add(item);
                }
            }
            Playlists = null;
            Playlists = filteredPlaylists;
        };
    }

}