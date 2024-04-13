using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;

namespace SoundScapes.Views;

public partial class MusicPlayerView : System.Windows.Controls.UserControl
{
    public MusicPlayerView()
    {
        var model = App.AppHost?.Services.GetService<MusicPlayerViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.NextSongCommand.Execute(this);
        model?.PlaySongCommand.Execute(this);
        model?.RegisterMusicSlider(musicSlider);
    }
}