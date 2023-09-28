using Free_Spotify.Classes;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Free_Spotify.Pages
{
    public partial class MainScreenPage : Page
    {
        public static MainScreenPage instance;
        public HomeViewPage homePage = new();
        public SearchViewPage searchPage = new();
        public PlaylistPage playListPage = new();

        public MainScreenPage()
        {
            InitializeComponent();
            instance = this;
        }

        private async void buttonHomePage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                GC.Collect();
                LoadPageMainView.Navigate(homePage);
            });
        }

        private async void buttonSearchPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                GC.Collect();
                LoadPageMainView.Navigate(searchPage);
            });
        }

        public async void PlaylistPage()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                GC.Collect();
                LoadPageMainView.Navigate(playListPage);
            });
        }

        private void CreatePlaylistVisual(object sender, MouseButtonEventArgs e)
        {
            buttonHomePage_MouseDown(sender, e);
            Playlist_Holder.CreatePlaylist($"Мой плейлист #{Playlist_Holder.GetLastPlaylistID()}");
        }
    }
}