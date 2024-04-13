using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

public partial class PlaylistInstallSongView : ContentDialog
{
    public readonly PlaylistInstallSongViewModel? playlistInstallSongViewModel;
    public PlaylistInstallSongView()
    {
        playlistInstallSongViewModel = App.AppHost?.Services.GetService<PlaylistInstallSongViewModel>();
        DataContext = playlistInstallSongViewModel;
        InitializeComponent();
    }
}