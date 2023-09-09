using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Free_Spotify.Pages
{
    public partial class MainScreenPage : Page
    {
        private HomeViewPage? homePage = new();
        private SearchViewPage? searchPage = new();

        public MainScreenPage()
        {
            InitializeComponent();
        }

        private async void buttonHomePage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                searchPage = null;
                homePage = new();
                GC.Collect();
                LoadPageMainView.Navigate(homePage);
            });
        }

        private async void buttonSearchPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                searchPage = new();
                homePage = null;
                GC.Collect();
                LoadPageMainView.Navigate(searchPage);
            });
        }
    }
}