using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Free_Spotify.Pages
{
    public partial class MainScreenPage : Page
    {
        public static MainScreenPage? instance;
        public HomeViewPage homePage = new();
        public SearchViewPage searchPage = new();

        public MainScreenPage()
        {
            InitializeComponent();
            instance = this;
        }

        private async void buttonHomePage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                GC.Collect();
                LoadPageMainView.Navigate(homePage);
            });
        }

        private async void buttonSearchPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                GC.Collect();
                LoadPageMainView.Navigate(searchPage);
            });
        }
    }
}