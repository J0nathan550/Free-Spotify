using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows;

namespace SoundScapes.Views.Dialogs;

/// <summary>
/// Вікно, яке відображається при неперехопленій винятковій ситуації.
/// </summary>
public partial class UnhandledExceptionWindow : Window
{
    /// <summary>
    /// ViewModel для вікна неперехопленої виняткової ситуації.
    /// </summary>
    public readonly UnhandledExceptionWindowViewModel? viewModel;

    /// <summary>
    /// Конструктор вікна неперехопленої виняткової ситуації.
    /// </summary>
    public UnhandledExceptionWindow()
    {
        // Отримання ViewModel з реєстра служб та призначення її контекстом даних вікна.
        viewModel = App.AppHost?.Services.GetRequiredService<UnhandledExceptionWindowViewModel>();
        DataContext = viewModel;
        InitializeComponent();
    }
}