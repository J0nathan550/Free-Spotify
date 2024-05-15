using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

/// <summary>
/// Вікно для відображення діалогу завантаження треків у плейлист.
/// </summary>
public partial class PlaylistInstallSongView : ContentDialog
{
    /// <summary>
    /// ViewModel для діалогу завантаження треків у плейлист.
    /// </summary>
    public readonly PlaylistInstallSongViewModel? playlistInstallSongViewModel;

    /// <summary>
    /// Ініціалізує новий екземпляр вікна PlaylistInstallSongView.
    /// </summary>
    public PlaylistInstallSongView()
    {
        // Отримання ViewModel з сервісного контейнера
        playlistInstallSongViewModel = App.AppHost?.Services.GetRequiredService<PlaylistInstallSongViewModel>();

        // Встановлення контексту даних та ініціалізація компонентів
        DataContext = playlistInstallSongViewModel;
        InitializeComponent();
    }
}