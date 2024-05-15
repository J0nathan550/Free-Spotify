using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class MusicPlayerView : UserControl
{
    public MusicPlayerView()
    {
        MusicPlayerViewModel? model = App.AppHost?.Services.GetRequiredService<MusicPlayerViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterMusicSlider(musicSlider);
    }
}