using ModernWpf.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundScapes.Views;

/// <summary>
/// Клас, який представляє головний вигляд програми.
/// </summary>
public partial class MainView : UserControl
{
    private Type? lastPageOpened;

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="MainView"/>.
    /// </summary>
    public MainView()
    {
        InitializeComponent();
        NavigationView.Focus();
        ContentFrame.Navigate(typeof(SearchView));
        NavigationView.SelectedItem = NavigationView.MenuItems[0];
        lastPageOpened = typeof(SearchView);
    }

    /// <summary>
    /// Обробляє подію, коли вибрано пункт навігації в навігаційному вигляді.
    /// </summary>
    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        Type? pageType = null;

        if (args.InvokedItemContainer?.Tag is string typeName)
            pageType = Type.GetType(typeName);

        // Переходить на вибрану сторінку, якщо вона відрізняється від попередньої сторінки
        if (pageType != null && pageType != lastPageOpened)
        {
            ContentFrame.Navigate(pageType);
            lastPageOpened = pageType;
        }
    }

    /// <summary>
    /// Обробляє подію, коли натиснута клавіша.
    /// </summary>
    private void MainView_KeyDown(object sender, KeyEventArgs e)
    {
        // Переходить на сторінку HelpView, коли натиснута клавіша F1
        if (e.Key == Key.F1 && lastPageOpened != typeof(HelpView))
        {
            ContentFrame.Navigate(typeof(HelpView));
            NavigationView.SelectedItem = NavigationView.MenuItems[2];
            lastPageOpened = typeof(HelpView);
        }
    }
}