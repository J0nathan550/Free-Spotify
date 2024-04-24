using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

public partial class PlaylistEditItemView : ContentDialog
{
    public PlaylistEditItemView()
    {
        var model = App.AppHost?.Services.GetService<PlaylistEditItemViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterTitleTextBox(TitleTextBox);
    }
}