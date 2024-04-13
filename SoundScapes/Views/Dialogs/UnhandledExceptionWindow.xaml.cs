using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows;

namespace SoundScapes.Views.Dialogs
{
    public partial class UnhandledExceptionWindow : Window
    {
        public readonly UnhandledExceptionWindowViewModel? viewModel;

        public UnhandledExceptionWindow()
        {
            viewModel = App.AppHost?.Services.GetService<UnhandledExceptionWindowViewModel>();
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}