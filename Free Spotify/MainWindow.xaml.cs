using AutoUpdaterDotNET;
using DiscordRPC;
using Free_Spotify.Classes;
using Free_Spotify.Pages;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Free_Spotify
{
    /// <summary>
    /// Main window class, handles all sorts of events related to keybindings, loading different pages.
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow window;
        public DiscordRpcClient discordClient = new DiscordRpcClient("1154023388805873744");
        public MainWindow()
        {
            CheckForUpdates();

            window = this;

            Utils.LoadSettings();
            Utils.UpdateLanguage();

            InitializeComponent();
            var assembly = Assembly.GetEntryAssembly();
            currentVersion_Item.Header = $"{Utils.GetLocalizationString("AppCurrentVersionDefaultText")} {assembly?.GetName().Version}";
            songTitle.Text = Utils.GetLocalizationString("SongTitleDefaultText");
            songAuthor.Text = Utils.GetLocalizationString("SongAuthorDefaultText");
            settingsMenuItem.Header = Utils.GetLocalizationString("SettingsMenuItemHeader");
            checkUpdatesMenuItem.Header = Utils.GetLocalizationString("CheckUpdatesMenuItemHeader");
            discordClient.Initialize();
            Utils.IdleDiscordPresence();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            timer.Tick += delegate { CheckForUpdates(); };
            timer.Start();
        }

        /// <summary>
        /// Function that if you click minimize button, it will minimize window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private async void MinimizeIcon_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.BeginInvoke(() => { WindowState = WindowState.Minimized; });
        }

        /// <summary>
        /// Function that if you click maximize button, it will maximize window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private async void MaximizedIcon_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                if (WindowState != WindowState.Maximized)
                {
                    WindowState = WindowState.Maximized;

                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            });
        }

        /// <summary>
        /// Function that if you click close button, it will close/quit window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void WindowProcedure(object sender, EventArgs e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                if (WindowState != WindowState.Maximized)
                {
                    iconMaximizedDefault.Visibility = Visibility.Visible;
                    iconMaximizedSelected.Visibility = Visibility.Hidden;
                    windowBackground.Padding = new Thickness(0);
                }
                else
                {
                    iconMaximizedDefault.Visibility = Visibility.Hidden;
                    iconMaximizedSelected.Visibility = Visibility.Visible;
                    windowBackground.Padding = new Thickness(10);
                }
                if (Utils.settings.musicPlayerBallonTurnOn && WindowState != WindowState.Minimized && SearchViewPage.searchWindow.ballon != null)
                {
                    myNotifyIcon.CloseBalloon();
                }
                else if (Utils.settings.musicPlayerBallonTurnOn && WindowState == WindowState.Minimized && SearchViewPage.searchWindow.ballon != null)
                {
                    myNotifyIcon.ShowCustomBalloon(SearchViewPage.searchWindow.ballon, PopupAnimation.Slide, null);
                }
            });
        }

        private void Closing_Window(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SearchViewPage.searchWindow.cancelProgressSongTimer.Cancel();
            Utils.SaveSettings();
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();
        }

        private void currentVersion_Item_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Utils.GithubLink,
                UseShellExecute = true
            });
        }

        private void CheckForUpdates()
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
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            LoadingPagesFrame.Navigate(settingsPage);
        }
    }
}