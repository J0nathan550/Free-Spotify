using AutoUpdaterDotNET;
using DiscordRPC;
using Free_Spotify.Classes;
using Free_Spotify.Pages;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Free_Spotify
{
    /// <summary>
    /// Main window class, handles all sorts of events related to keybindings, loading different pages.
    /// </summary>
    public partial class MainWindow : Window
    {
        public DiscordRpcClient discordClient = new("1154023388805873744");

        public MainWindow()
        {
            CheckForUpdates();

            Window = this;

            Settings.LoadSettings();
            Settings.UpdateLanguage();

            InitializeComponent();

            Topmost = Settings.SettingsData.isWindowTopMost;

            var assembly = Assembly.GetEntryAssembly();
            currentVersion_Item.Header = $"{Settings.GetLocalizationString("AppCurrentVersionDefaultText")} {assembly?.GetName().Version}";
            settingsMenuItem.Header = Settings.GetLocalizationString("SettingsMenuItemHeader");
            checkUpdatesMenuItem.Header = Settings.GetLocalizationString("CheckUpdatesMenuItemHeader");

            _ = discordClient.Initialize();
            DiscordStatuses.IdleDiscordPresence();
        }

        /// <summary>
        /// Function that if you click minimize button, it will minimize window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private void MinimizeIcon_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Function that if you click maximize button, it will maximize window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private void MaximizedIcon_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
        }

        /// <summary>
        /// Function that if you click close button, it will close/quit window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Check for the updates. If you click in the menu on check updates (it will hopefully start the update)
        /// </summary>
        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();
        }

        /// <summary>
        /// Resends to Git page.
        /// </summary>
        private void CurrentVersion_Item_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo()
            {
                FileName = Utils.GithubLink,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Function that if update is happen creates special modal window.
        /// </summary>
        private static void CheckForUpdates()
        {
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.ShowRemindLaterButton = false;
            AutoUpdater.Mandatory = true;
            AutoUpdater.UpdateMode = Mode.Forced;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Icon = new System.Drawing.Bitmap("Assets/spotify-icon-png-15398-Windows.ico");
            switch (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture)
            {
                case System.Runtime.InteropServices.Architecture.X86:
                    AutoUpdater.Start(Utils.DownloadAutoUpdateLinkX86);
                    break;
                case System.Runtime.InteropServices.Architecture.X64:
                    AutoUpdater.Start(Utils.DownloadAutoUpdateLinkX64);
                    break;
            }

        }

        private SettingsPage settingsPage = new SettingsPage();

        public static MainWindow? Window { get; set; }

        /// <summary>
        /// Menu selection of settings.
        /// </summary>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            LoadingPagesFrame.Content = null;
            _ = LoadingPagesFrame.NavigationService.Navigate(null);
            _ = LoadingPagesFrame.NavigationService.RemoveBackEntry();
            _ = LoadingPagesFrame.Navigate(settingsPage);
        }
    }
}