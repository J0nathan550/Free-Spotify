using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class PlaylistView : UserControl
{
    public PlaylistView()
    {
        var model = App.AppHost?.Services.GetService<PlaylistViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterSearchPlaylistBox(SearchPlaylistBox);
    }
}