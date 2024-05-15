using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

/// <summary>
/// Представлення для відображення діалогового вікна додавання елемента до плейлиста.
/// </summary>
public partial class PlaylistAddItemView : ContentDialog
{
    /// <summary>
    /// Конструктор класу PlaylistAddItemView.
    /// </summary>
    public PlaylistAddItemView()
    {
        // Отримуємо доступ до ViewModel для додавання елемента до плейлиста через сервіси додатка
        PlaylistAddItemViewModel? model = App.AppHost?.Services.GetRequiredService<PlaylistAddItemViewModel>();
        DataContext = model; // Встановлюємо контекст даних для цієї View
        InitializeComponent(); // Ініціалізуємо компоненти
        model?.RegisterTitleTextBox(TitleTextBox); // Реєструємо текстове поле заголовку у ViewModel
    }
}