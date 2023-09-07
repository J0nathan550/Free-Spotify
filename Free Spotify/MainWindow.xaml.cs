using Free_Spotify.Pages;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Shell;

namespace Free_Spotify
{
    public sealed partial class MainWindow : Page
    {
        public MainWindow()
        {
            // Title bar modification
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(BackgroundElement);
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            titleBar.ForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            titleBar.InactiveBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            titleBar.InactiveForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            titleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            this.InitializeComponent();
            


            LoadPageAnimation();
        }

        // Dull animation of when you start application, it transitions to the home page.
        #region Loading main page animation.
        private async void LoadPageAnimation()
        {
            await LoadPageAnimationAsync();
        }

        private async Task LoadPageAnimationAsync()
        {
            loadPage.Navigate(typeof(LoadingPageAnimation), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            await Task.Delay(250);
            loadPage.Navigate(typeof(PlaylistView), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
        }
        #endregion

        /// <summary>
        /// Home button, when you click it transfers you to home page, if you are already in that page, and you click it, it will not transition to it again. Only if you will switch.
        /// </summary>
        private void Home_Clicked(object sender, PointerRoutedEventArgs e)
        {
            if (loadPage.SourcePageType == typeof(PlaylistView))
            {
                return;
            }
            loadPage.Navigate(typeof(PlaylistView), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
        /// <summary>
        /// Search button, when you click it transfers you to search page, if you are already in that page, and you click it, it will not transition to it again. Only if you will switch.
        /// </summary>
        private void Search_Clicked(object sender, PointerRoutedEventArgs e)
        {
            if (loadPage.SourcePageType == typeof(SearchView))
            {
                return;
            }
            loadPage.Navigate(typeof(SearchView), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
        }
    }
}