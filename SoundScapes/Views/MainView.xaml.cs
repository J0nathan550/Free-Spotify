using ModernWpf.Controls;
using System.Windows.Controls;

namespace SoundScapes.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            ContentFrame.Navigate(typeof(SearchView));
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked) ContentFrame.Navigate(typeof(SettingsView));
            else if (args.InvokedItemContainer != null)
            {
                Type? navPageType = Type.GetType(args.InvokedItemContainer.Tag?.ToString() ?? string.Empty);
                if (navPageType != null) ContentFrame.Navigate(navPageType);
            }
        }
    }
}