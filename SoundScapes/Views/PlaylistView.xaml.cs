using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;

namespace SoundScapes.Views;

public partial class PlaylistView : System.Windows.Controls.UserControl
{
    public PlaylistView()
    {
        var model = App.AppHost?.Services.GetService<PlaylistViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterSearchPlaylistBox(SearchPlaylistBox);
    }
}