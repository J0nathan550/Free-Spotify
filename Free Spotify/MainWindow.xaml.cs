using DiscordRPC;
using Free_Spotify.Classes;
using Free_Spotify.Pages;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

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
            InitializeComponent();
            window = this;
            discordClient.Initialize();
            Utils.IdleDiscordPresence();
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

        private async void WindowProcedure(object sender, System.EventArgs e)
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
                    windowBackground.Padding = new Thickness(3);
                }
            });
        }

        private void Closing_Window(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SearchViewPage.instance.cancelProgressSongTimer.Cancel();
            Utils.SaveSettings();
        }
    }
}