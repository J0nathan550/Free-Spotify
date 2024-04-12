using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

public partial class PlaylistInstallSongView : ContentDialog
{
    public PlaylistInstallSongView()
    {
        var model = App.AppHost?.Services.GetService<PlaylistInstallSongViewModel>();
        DataContext = model;
        InitializeComponent();
    }
}