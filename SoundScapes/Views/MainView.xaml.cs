﻿using ModernWpf.Controls;

namespace SoundScapes.Views
{
    public partial class MainView : System.Windows.Controls.UserControl
    {
        private Type? lastPageOpened;

        public MainView()
        {
            InitializeComponent();

            ContentFrame.Navigate(typeof(SearchView));
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            lastPageOpened = typeof(SearchView);
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
    }
}