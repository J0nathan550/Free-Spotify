using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class MusicPlayerView : UserControl
{
    public MusicPlayerView()
    {
        // Отримання MusicPlayerViewModel через впровадження сервісів
        MusicPlayerViewModel? model = App.AppHost?.Services.GetRequiredService<MusicPlayerViewModel>();

        // Встановлення контексту даних для зв'язку з MusicPlayerViewModel
        DataContext = model;

        // Ініціалізація компонентів
        InitializeComponent();

        // Реєстрація музичного слайдера з MusicPlayerViewModel
        model?.RegisterMusicSlider(musicSlider);
    }
}