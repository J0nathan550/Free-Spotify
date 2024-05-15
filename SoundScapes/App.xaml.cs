using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoundScapes.Interfaces;
using SoundScapes.Services;
using SoundScapes.ViewModels;
using SoundScapes.Views;
using SoundScapes.Views.Dialogs;
using System.Windows;

namespace SoundScapes;

/// <summary>
/// Головний клас додатку.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Статичне властивість для збереження хоста додатку.
    /// </summary>
    public static IHost? AppHost { get; private set; }

    /// <summary>
    /// Конструктор додатку.
    /// </summary>
    public App()
    {
        // Обробник подій для необроблених винятків
        Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

        // Ініціалізує LibVLCSharp та встановлює змінну середовища
        Core.Initialize();
        Environment.SetEnvironmentVariable("SLAVA_UKRAINI", "1");

        // Створення та конфігурація хоста додатку
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // Відображення
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainView>();
                services.AddSingleton<MusicPlayerView>();
                services.AddSingleton<PlaylistView>();
                services.AddSingleton<SearchView>();
                services.AddSingleton<HelpView>();
                services.AddTransient<PlaylistAddItemView>();
                services.AddTransient<PlaylistEditItemView>();
                services.AddTransient<PlaylistAddSongItemView>();
                services.AddTransient<PlaylistInstallSongView>();
                services.AddTransient<UnhandledExceptionWindow>();

                // Відображення моделей
                services.AddSingleton<SearchViewModel>();
                services.AddSingleton<MusicPlayerViewModel>();
                services.AddSingleton<PlaylistViewModel>();
                services.AddTransient<PlaylistAddItemViewModel>();
                services.AddTransient<PlaylistEditItemViewModel>();
                services.AddTransient<PlaylistAddSongItemViewModel>();
                services.AddTransient<PlaylistInstallSongViewModel>();
                services.AddSingleton<UnhandledExceptionWindowViewModel>();

                // Служби
                services.AddSingleton<IMusicPlayer, MusicPlayerService>();
                services.AddSingleton<ISettings, SettingsService>();
            })
            .Build();
    }

    /// <summary>
    /// Обробник подій для необроблених винятків.
    /// </summary>
    private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        UnhandledExceptionWindow errorWindow = AppHost!.Services.GetRequiredService<UnhandledExceptionWindow>();
        errorWindow.viewModel!.ErrorMessage = e.Exception.Message + "\n\n\n" + e.Exception.StackTrace;
        errorWindow!.ShowDialog();
    }

    /// <summary>
    /// Обробник подій для запуску додатка.
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        // Асинхронний запуск додатка
        await AppHost!.StartAsync();

        // Відображення головного вікна додатка
        MainWindow startupForm = AppHost.Services.GetRequiredService<MainWindow>();
        startupForm.Show();

        base.OnStartup(e);
    }

    /// <summary>
    /// Обробник подій для завершення роботи додатка.
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        // Асинхронне завершення роботи додатка
        await AppHost!.StopAsync();
        base.OnExit(e);
    }
}
