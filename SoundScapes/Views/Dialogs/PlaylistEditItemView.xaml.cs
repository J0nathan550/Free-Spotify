using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

public partial class PlaylistEditItemView : ContentDialog
{
    public PlaylistEditItemViewModel? model;

    public PlaylistEditItemView()
    {
        model = App.AppHost?.Services.GetRequiredService<PlaylistEditItemViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterTitleTextBox(TitleTextBox);
    }
}