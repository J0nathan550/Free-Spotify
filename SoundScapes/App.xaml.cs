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

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

        Core.Initialize();
        Environment.SetEnvironmentVariable("SLAVA_UKRAINI", "1");
        
        AppHost = Host.CreateDefaultBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            // Views
            services.AddSingleton<MainWindow>();
            services.AddTransient<UnhandledExceptionWindow>();

            // View Models
            services.AddSingleton<SearchViewModel>();
            services.AddSingleton<PlaylistViewModel>();
            services.AddSingleton<MusicPlayerViewModel>();
            services.AddSingleton<UnhandledExceptionWindowViewModel>();

            // Services
            services.AddSingleton<IMusicPlayer, MusicPlayerService>();
            services.AddSingleton<ISettings, SettingsService>();
        })
        .Build();
    }

    private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        UnhandledExceptionWindow errorWindow = AppHost!.Services.GetRequiredService<UnhandledExceptionWindow>();
        errorWindow.viewModel!.ErrorMessage = e.Exception.Message + "\n\n\n" + e.Exception.StackTrace;
        errorWindow!.ShowDialog();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        MainWindow startupForm = AppHost.Services.GetRequiredService<MainWindow>();
        startupForm.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }

}