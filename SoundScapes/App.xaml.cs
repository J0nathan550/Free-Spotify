using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoundScapes.ViewModels;
using SoundScapes.Views;
using System.Windows;

namespace SoundScapes;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            // Views
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainView>();
            services.AddSingleton<MusicPlayerView>();
            services.AddSingleton<PlaylistView>();
            services.AddSingleton<SearchView>();
            services.AddSingleton<SettingsView>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MusicPlayerViewModel>();
            services.AddTransient<PlaylistViewModel>();
            services.AddTransient<SearchViewModel>();
            services.AddTransient<SettingsViewModel>();
        })
        .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
        startupForm.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }

}