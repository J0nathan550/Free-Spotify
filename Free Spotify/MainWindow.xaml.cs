using System.Windows;

namespace Free_Spotify
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MoveWindow_Down(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { DragMove(); });
        }

        private async void MinimizeIcon_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { WindowState = WindowState.Minimized; });
        }

        private async void MaximizedIcon_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => {
                if (WindowState != WindowState.Maximized)
                {
                    WindowState = WindowState.Maximized;
                    iconMaximizedDefault.Visibility = Visibility.Hidden;
                    iconMaximizedSelected.Visibility = Visibility.Visible;
                }
                else
                {
                    WindowState = WindowState.Normal;
                    iconMaximizedDefault.Visibility = Visibility.Visible;
                    iconMaximizedSelected.Visibility = Visibility.Hidden;
                }
            });
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}