using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

/// <summary>
/// Клас представлення для відображення списку плейлистів.
/// </summary>
public partial class PlaylistView : UserControl
{
    /// <summary>
    /// Конструктор класу PlaylistView.
    /// </summary>
    public PlaylistView()
    {
        // Отримуємо сервіс PlaylistViewModel з контейнера служб
        PlaylistViewModel? model = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();

        // Встановлюємо контекст даних для відображення
        DataContext = model;

        // Ініціалізуємо компоненти відображення
        InitializeComponent();

        // Реєструємо поле пошуку плейлисту в моделі представлення
        model?.RegisterSearchPlaylistBox(SearchPlaylistBox);
    }
}