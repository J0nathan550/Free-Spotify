using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Models;
using System.Windows;
using System.Windows.Controls;

namespace SoundScapes.ViewModels;

/// <summary>
/// ViewModel для елемента додавання пісні до плейлисту.
/// </summary>
public partial class PlaylistAddSongItemViewModel : ObservableObject
{
    // Властивість, що вказує, чи є вмикана основна кнопка.
    [ObservableProperty]
    private bool _isPrimaryButtonEnabled = false;

    // Початковий список плейлистів.
    [ObservableProperty]
    private List<PlaylistModel> _playlistsOriginal = [];

    // Список плейлистів.
    [ObservableProperty]
    private List<PlaylistModel>? _playlists = [];

    // Вибрані плейлисти.
    [ObservableProperty]
    private List<PlaylistModel> _playlistsSelected = [];

    // Видимість тексту помилки.
    [ObservableProperty]
    private Visibility _errorTextVisibility = Visibility.Collapsed;

    // Видимість тексту пошуку.
    [ObservableProperty]
    private Visibility _searchTextVisibility = Visibility.Visible;

    // Команда вибору прапорця.
    [ObservableProperty]
    private RelayCommand<object>? _checkBoxSelectedCommand;

    /// <summary>
    /// Конструктор класу PlaylistAddSongItemViewModel.
    /// </summary>
    public PlaylistAddSongItemViewModel()
    {
        PlaylistViewModel? model = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();
        if (model == null || model.OriginalPlaylists == null) return;
        Playlists = null;
        foreach (PlaylistModel item in model.OriginalPlaylists)
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

    /// <summary>
    /// Метод виконання команди вибору прапорця.
    /// </summary>
    private void CheckBoxSelectedCommand_Execute(object? obj)
    {
        if (obj is CheckBox checkbox)
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
                PlaylistModel? item = PlaylistsSelected.Where(m => m.Title == model.Title && m.Duration == model.Duration).FirstOrDefault();
                if (item != null)
                {
                    PlaylistsSelected.Remove(item);
                }
            }
            IsPrimaryButtonEnabled = PlaylistsSelected.Count > 0;
        }
    }

    /// <summary>
    /// Реєстрація AutoSuggestBox для пошуку плейлистів.
    /// </summary>
    /// <param name="autoSuggestBox">AutoSuggestBox для реєстрації.</param>
    public void RegisterSearchPlaylistBox(AutoSuggestBox autoSuggestBox)
    {
        autoSuggestBox.QuerySubmitted += (o, e) =>
        {
            string query = e.QueryText.ToLower();
            Playlists = PlaylistsOriginal;
            if (Playlists == null) return;
            List<PlaylistModel> filteredPlaylists = [];

            foreach (PlaylistModel item in Playlists)
            {
                foreach (PlaylistModel selectedPlaylist in PlaylistsSelected)
                {
                    // Перевірка, чи співпадають заголовки для визначення рівності.
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