using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

public partial class PlaylistAddItemView : ContentDialog
{
    public PlaylistAddItemView()
    {
        PlaylistAddItemViewModel? model = App.AppHost?.Services.GetRequiredService<PlaylistAddItemViewModel>();
        DataContext = model;
        InitializeComponent();
        model?.RegisterTitleTextBox(TitleTextBox);
    }
}