using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class MusicPlayerView : UserControl
{
    public MusicPlayerView()
    {
        DataContext = App.AppHost?.Services.GetService<MusicPlayerViewModel>();
        InitializeComponent();
    }
}