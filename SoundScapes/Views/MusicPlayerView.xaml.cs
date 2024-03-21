using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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