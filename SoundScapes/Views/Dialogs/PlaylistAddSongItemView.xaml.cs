using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

/// <summary>
/// Вікно для додавання пісні до плейлисту.
/// </summary>
public partial class PlaylistAddSongItemView : ContentDialog
{
    /// <summary>
    /// ViewModel для додавання пісні до плейлисту.
    /// </summary>
    public PlaylistAddSongItemViewModel? viewModel = App.AppHost?.Services.GetRequiredService<PlaylistAddSongItemViewModel>();

    /// <summary>
    /// Ініціалізує новий екземпляр вікна PlaylistAddSongItemView.
    /// </summary>
    public PlaylistAddSongItemView()
    {
        // Встановлення контексту даних та ініціалізація компонентів
        DataContext = viewModel;
        InitializeComponent();

        // Реєстрація текстового поля пошуку плейлисту
        viewModel?.RegisterSearchPlaylistBox(SearchPlaylistBox);
    }
}