using ModernWpf.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundScapes.Views;

public partial class MainView : UserControl
{
    private Type? lastPageOpened;

    public MainView()
    {
        InitializeComponent();
        NavigationView.Focus();
        ContentFrame.Navigate(typeof(MusicHubView));
        NavigationView.SelectedItem = NavigationView.MenuItems[0];
        lastPageOpened = typeof(MusicHubView);
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        Type? pageType = null;

        if (args.InvokedItemContainer?.Tag is string typeName) pageType = Type.GetType(typeName);
        if (pageType != null && pageType != lastPageOpened)
        {
            ContentFrame.Navigate(pageType);
            lastPageOpened = pageType;
        }
    }

    private void MainView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F1 && lastPageOpened != typeof(HelpView))
        {
            ContentFrame.Navigate(typeof(HelpView));
            NavigationView.SelectedItem = NavigationView.MenuItems[1];
            lastPageOpened = typeof(HelpView);
        }
    }
}