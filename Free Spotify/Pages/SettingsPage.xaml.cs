using Free_Spotify.Classes;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace Free_Spotify.Pages
{
    public partial class SettingsPage : Page
    {
        /// <summary>
        /// Constructor of this page.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            Settings.LoadSettings();
            languageComboBox.SelectedIndex = Settings.SettingsData.languageIndex;

            Settings.UpdateLanguage();

            discordRPCCheckBox.IsChecked = Settings.SettingsData.discordRPC;
            trafficEconomicCheckBox.IsChecked = Settings.SettingsData.economTraffic;

            ballonPlayerCheckBox.IsChecked = Settings.SettingsData.musicPlayerBallonTurnOn;
            ballonPlayerText.Text = Settings.GetLocalizationString("BallonPlayerText");
            ballonPlayerCheckBox.ToolTip = Settings.GetLocalizationString("BallonPlayerCheckBoxToolTip");
            languageLabel.Text = Settings.GetLocalizationString("LanguageSettingsDefaultText");

            trafficEconomic.Text = Settings.GetLocalizationString("TrafficDefaultText");
            settingsLabel.Text = Settings.GetLocalizationString("SettingsMenuItemHeader");
            trafficEconomicCheckBox.ToolTip = Settings.GetLocalizationString("TrafficToolTipDefaultText");

            searchEngineLabel.Text = Settings.GetLocalizationString("SearchEngineLabelText");
            searchEngineCheckBox.ToolTip = Settings.GetLocalizationString("SearchEngineToolTipText");
            searchEngineCheckBox.SelectedIndex = Settings.SettingsData.searchEngineIndex;

            topMostWindowCheckBox.IsChecked = Settings.SettingsData.isWindowTopMost;
            topMostWindowCheckBox.ToolTip = Settings.GetLocalizationString("TopMostWindowCheckBoxText");
            topMostWindowText.Text = Settings.GetLocalizationString("TopMostWindowText");
        }

        /// <summary>
        /// Handles a click event, checks for the existence of the main window, clears the content of a frame, removes a navigation entry, and navigates to the search view page.
        /// </summary>
        private void BackToSearchPage_Click(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.Window == null)
            {
                return;
            }
            if (SearchViewPage.SearchWindow != null)
            {
                SearchViewPage.SearchWindow = null;
                SearchViewPage.SearchWindow = new();
            }
            MainWindow.Window.LoadingPagesFrame.Content = null;
            _ = MainWindow.Window.LoadingPagesFrame.NavigationService.Navigate(null);
            _ = MainWindow.Window.LoadingPagesFrame.NavigationService.RemoveBackEntry();
            _ = MainWindow.Window.LoadingPagesFrame.Navigate(SearchViewPage.SearchWindow);
        }

        private bool loadingPenalty = false; // removing null reference because this func is shit
        /// <summary>
        /// Handles a selection change event for a language ComboBox. It updates the application's language settings based on the selected language and updates 
        /// various UI elements accordingly.
        /// </summary>
        private void LanguageChanged_ComboBox(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingPenalty)
            {
                loadingPenalty = true;
                return;
            }

            Settings.SettingsData.languageIndex = languageComboBox.SelectedIndex;
            switch (languageComboBox.SelectedIndex)
            {
                case 0: // eng
                    Settings.ChangeLanguage("en");
                    break;
                case 1: // ru
                    Settings.ChangeLanguage("ru");
                    break;
                case 2: // ua
                    Settings.ChangeLanguage("uk");
                    break;
                case 3: // japanese
                    Settings.ChangeLanguage("ja");
                    break;
                default:
                    Settings.SettingsData.languageIndex = 0;
                    Settings.ChangeLanguage("en");
                    break;
            }

            languageLabel.Text = Settings.GetLocalizationString("LanguageSettingsDefaultText");
            trafficEconomic.Text = Settings.GetLocalizationString("TrafficDefaultText");
            settingsLabel.Text = Settings.GetLocalizationString("SettingsMenuItemHeader");
            trafficEconomicCheckBox.ToolTip = Settings.GetLocalizationString("TrafficToolTipDefaultText");
            searchEngineLabel.Text = Settings.GetLocalizationString("SearchEngineLabelText");
            searchEngineCheckBox.ToolTip = Settings.GetLocalizationString("SearchEngineToolTipText");
            ballonPlayerText.Text = Settings.GetLocalizationString("BallonPlayerText");
            ballonPlayerCheckBox.ToolTip = Settings.GetLocalizationString("BallonPlayerCheckBoxToolTip");
            topMostWindowCheckBox.ToolTip = Settings.GetLocalizationString("TopMostWindowText");
            topMostWindowText.Text = Settings.GetLocalizationString("TopMostWindowCheckBoxText");

            if (PlayListView.Instance != null)
            {
                PlayListView.Instance.yourPlaylist.Text = Settings.GetLocalizationString("YourPlaylistText");
                PlayListView.Instance.CreatePlaylists();
            }
            var assembly = Assembly.GetEntryAssembly();
            DiscordStatuses.IdleDiscordPresence();

            if (MainWindow.Window != null)
            {
                MainWindow.Window.currentVersion_Item.Header = $"{Settings.GetLocalizationString("AppCurrentVersionDefaultText")} {assembly?.GetName().Version}";
                MainWindow.Window.settingsMenuItem.Header = Settings.GetLocalizationString("SettingsMenuItemHeader");
                MainWindow.Window.checkUpdatesMenuItem.Header = Settings.GetLocalizationString("CheckUpdatesMenuItemHeader");
            }
            Settings.SaveSettings();
        }

        /// <summary>
        ///  Handles a UI option related to Discord Rich Presence. Toggles the option on or off and updates the Discord presence status accordingly.
        /// </summary>
        private void DiscordRPC_Option(object sender, System.Windows.RoutedEventArgs e)
        {
            discordRPCCheckBox.IsChecked = Settings.SettingsData.discordRPC;
            Settings.SettingsData.discordRPC = !Settings.SettingsData.discordRPC;
            DiscordStatuses.IdleDiscordPresence();
            discordRPCCheckBox.IsChecked = Settings.SettingsData.discordRPC;
            Settings.SaveSettings();
        }


        /// <summary>
        /// Handles a UI option related to economical traffic. Toggles the option on or off and updates the Discord presence status accordingly.
        /// </summary>
        private void EconomTraffic_Option(object sender, System.Windows.RoutedEventArgs e)
        {
            trafficEconomicCheckBox.IsChecked = Settings.SettingsData.economTraffic;
            Settings.SettingsData.economTraffic = !Settings.SettingsData.economTraffic;
            DiscordStatuses.IdleDiscordPresence();
            trafficEconomicCheckBox.IsChecked = Settings.SettingsData.economTraffic;
            Settings.SaveSettings();
        }

        private bool loadingPenaltySearchingEngine = false;
        /// <summary>
        /// Handles a selection change event for a search engine ComboBox. Updates the application's search engine settings based on the selected option.
        /// </summary>
        private void SearchEngine_ComboBox(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingPenaltySearchingEngine)
            {
                loadingPenaltySearchingEngine = true;
                return;
            }
            Settings.SettingsData.searchEngineIndex = searchEngineCheckBox.SelectedIndex;
            Settings.SaveSettings();
        }

        /// <summary>
        /// Handles a click event for a CheckBox related to a music player feature. Toggles the music player's balloon display option and updates Discord presence status.
        /// </summary>
        private void BallonPlayerCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ballonPlayerCheckBox.IsChecked = Settings.SettingsData.musicPlayerBallonTurnOn;
            Settings.SettingsData.musicPlayerBallonTurnOn = !Settings.SettingsData.musicPlayerBallonTurnOn;
            DiscordStatuses.IdleDiscordPresence();
            ballonPlayerCheckBox.IsChecked = Settings.SettingsData.musicPlayerBallonTurnOn;
            Settings.SaveSettings();
        }

        private void TopMostWindowCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainWindow.Window != null)
            {
                Settings.SettingsData.isWindowTopMost = !Settings.SettingsData.isWindowTopMost;
                topMostWindowCheckBox.IsChecked = Settings.SettingsData.isWindowTopMost;
                MainWindow.Window.Topmost = Settings.SettingsData.isWindowTopMost;
                Settings.SaveSettings();
            }
        }
    }
}