using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

public partial class PlaylistAddSongItemView : ContentDialog
{
    public PlaylistAddSongItemViewModel? viewModel;

    public PlaylistAddSongItemView()
    {
        viewModel = App.AppHost?.Services.GetService<PlaylistAddSongItemViewModel>();
        DataContext = viewModel;
        InitializeComponent();
        viewModel?.RegisterSearchPlaylistBox(SearchPlaylistBox);
    }
}