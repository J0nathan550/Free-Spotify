﻿using Free_Spotify.Pages;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Free_Spotify
{
    /// <summary>
    /// Main window class, handles all sorts of events related to keybindings, loading different pages.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Function that moves window arround. Without it, regular window that has no style will not move.
        /// </summary>
        private async void MoveWindow_Down(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { DragMove(); });
        }

        /// <summary>
        /// Function that if you click minimize button, it will minimize window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private async void MinimizeIcon_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { WindowState = WindowState.Minimized; });
        }

        /// <summary>
        /// Function that if you click maximize button, it will maximize window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
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

        /// <summary>
        /// Function that if you click close button, it will close/quit window. In no style window you have to create own buttons that handle those functions.
        /// </summary>
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}