using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;

namespace SoundScapes.Views;

public partial class PlaylistView : System.Windows.Controls.UserControl
{
    public PlaylistView()
    {
        PlaylistViewModel? model = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterSearchPlaylistBox(SearchPlaylistBox);
    }
}