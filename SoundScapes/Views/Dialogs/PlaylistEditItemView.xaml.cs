using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

/// <summary>
/// Вікно для редагування елементів плейлисту.
/// </summary>
public partial class PlaylistEditItemView : ContentDialog
{
    /// <summary>
    /// ViewModel для редагування елементів плейлисту.
    /// </summary>
    public PlaylistEditItemViewModel? model;

    /// <summary>
    /// Ініціалізує новий екземпляр вікна PlaylistEditItemView.
    /// </summary>
    public PlaylistEditItemView()
    {
        // Отримання ViewModel з сервісного контейнера
        model = App.AppHost?.Services.GetRequiredService<PlaylistEditItemViewModel>();

        // Встановлення контексту даних та ініціалізація компонентів
        DataContext = model;
        InitializeComponent();

        // Реєстрація TextBox для заголовка
        model?.RegisterTitleTextBox(TitleTextBox);
    }
}
