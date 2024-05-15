using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class PlaylistView : UserControl
{
    public readonly PlaylistViewModel playlistViewModel = App.AppHost!.Services.GetRequiredService<PlaylistViewModel>();
    public PlaylistView()
    {
        DataContext = playlistViewModel;
        InitializeComponent();
    }
}