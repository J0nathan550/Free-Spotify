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
        Core.Initialize();
        Environment.SetEnvironmentVariable("SLAVA_UKRAINI", "1");
        
        AppHost = Host.CreateDefaultBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            // Views
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainView>();
            services.AddSingleton<MusicPlayerView>();
            services.AddSingleton<PlaylistView>();
            services.AddTransient<PlaylistAddItemView>();
            services.AddTransient<PlaylistEditItemView>();
            services.AddTransient<PlaylistAddSongItemView>();
            services.AddTransient<PlaylistInstallSongView>();
            services.AddSingleton<SearchView>();

            // View Models
            services.AddSingleton<SearchViewModel>();
            services.AddSingleton<MusicPlayerViewModel>();
            services.AddSingleton<PlaylistViewModel>();
            services.AddTransient<PlaylistAddItemViewModel>();
            services.AddTransient<PlaylistEditItemViewModel>();
            services.AddTransient<PlaylistAddSongItemViewModel>();
            services.AddTransient<PlaylistInstallSongViewModel>();

            // Services
            services.AddSingleton<IMusicPlayer, MusicPlayerService>();
            services.AddSingleton<ISettings, SettingsService>();
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