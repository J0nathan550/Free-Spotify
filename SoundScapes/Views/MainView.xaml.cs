using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views
{
    public partial class MainView : UserControl
    {
        private Type? lastPageOpened;
        private readonly MusicPlayerViewModel? musicPlayerView;

        public MainView()
        {
            InitializeComponent();
            ContentFrame.Navigate(typeof(SearchView));
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            lastPageOpened = typeof(SearchView);
            musicPlayerView = App.AppHost?.Services.GetService<MusicPlayerViewModel>();
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type? pageType = null;

            if (args.IsSettingsInvoked) pageType = typeof(SettingsView);
            else if (args.InvokedItemContainer?.Tag is string typeName) pageType = Type.GetType(typeName);
            if (pageType != null && pageType != lastPageOpened)
            {
                if (pageType == typeof(SearchView))
                {
                    if (musicPlayerView != null)
                    {
                        musicPlayerView.IsPlayingFromPlaylist = false;
                    }
                }
                else if (pageType == typeof(PlaylistView))
                {
                    if (musicPlayerView != null)
                    {
                        musicPlayerView.IsPlayingFromPlaylist = true;
                    }
                }
                ContentFrame.Navigate(pageType);
                lastPageOpened = pageType;
            }
        }
    }
}