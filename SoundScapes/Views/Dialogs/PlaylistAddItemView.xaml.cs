using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;

namespace SoundScapes.Views.Dialogs;

public partial class PlaylistAddItemView : ContentDialog
{
    public PlaylistAddItemView()
    {
        var model = App.AppHost?.Services.GetService<PlaylistAddItemViewModel>();
        DataContext = model;
        InitializeComponent();
    }
}