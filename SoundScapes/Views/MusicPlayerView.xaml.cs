using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class MusicPlayerView : UserControl
{
    public MusicPlayerView()
    {
        var model = App.AppHost?.Services.GetService<MusicPlayerViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterMusicSlider(musicSlider);
    }
}