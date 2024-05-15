using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;

namespace SoundScapes.Views;

public partial class MusicPlayerView : System.Windows.Controls.UserControl
{
    public MusicPlayerView()
    {
        MusicPlayerViewModel? model = App.AppHost?.Services.GetRequiredService<MusicPlayerViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterMusicSlider(musicSlider);
    }
}