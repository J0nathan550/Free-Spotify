using Free_Spotify.Classes;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace Free_Spotify.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            Utils.LoadSettings();
            languageComboBox.SelectedIndex = Utils.settings.languageIndex;
            Utils.UpdateLanguage();
            discordRPCCheckBox.IsChecked = Utils.settings.discordRPC;
            trafficEconomicCheckBox.IsChecked = Utils.settings.economTraffic;
            ballonPlayerCheckBox.IsChecked = Utils.settings.musicPlayerBallonTurnOn;
            ballonPlayerText.Text = Utils.GetLocalizationString("BallonPlayerText");
            ballonPlayerCheckBox.ToolTip = Utils.GetLocalizationString("BallonPlayerCheckBoxToolTip");
            languageLabel.Text = Utils.GetLocalizationString("LanguageSettingsDefaultText");
            trafficEconomic.Text = Utils.GetLocalizationString("TrafficDefaultText");
            settingsLabel.Text = Utils.GetLocalizationString("SettingsMenuItemHeader");
            trafficEconomicCheckBox.ToolTip = Utils.GetLocalizationString("TrafficToolTipDefaultText");
            searchEngineLabel.Text = Utils.GetLocalizationString("SearchEngineLabelText");
            searchEngineCheckBox.ToolTip = Utils.GetLocalizationString("SearchEngineToolTipText");
            searchEngineCheckBox.SelectedIndex = Utils.settings.searchEngineIndex;
        }

        private void backToSearchPage_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.window.LoadingPagesFrame.Navigate(SearchViewPage.searchWindow);
        }

        private bool loadingPenalty = false; // removing null reference because this func is shit
        private async void LanguageChanged_ComboBox(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingPenalty)
            {
                loadingPenalty = true;
                return;
            }
            await Dispatcher.BeginInvoke(() =>
            {
                Utils.settings.languageIndex = languageComboBox.SelectedIndex;
                switch (languageComboBox.SelectedIndex)
                {
                    case 0: // eng
                        Utils.ChangeLanguage("en");
                        break;
                    case 1: // ru
                        Utils.ChangeLanguage("ru");
                        break;
                    case 3: // japanese
                        Utils.ChangeLanguage("ja");
                        break;
                    default:
                        Utils.settings.languageIndex = 0;
                        Utils.ChangeLanguage("en");
                        break;
                }

                languageLabel.Text = Utils.GetLocalizationString("LanguageSettingsDefaultText");
                trafficEconomic.Text = Utils.GetLocalizationString("TrafficDefaultText");
                settingsLabel.Text = Utils.GetLocalizationString("SettingsMenuItemHeader");
                trafficEconomicCheckBox.ToolTip = Utils.GetLocalizationString("TrafficToolTipDefaultText");
                searchEngineLabel.Text = Utils.GetLocalizationString("SearchEngineLabelText");
                searchEngineCheckBox.ToolTip = Utils.GetLocalizationString("SearchEngineToolTipText");
                ballonPlayerText.Text = Utils.GetLocalizationString("BallonPlayerText");
                ballonPlayerCheckBox.ToolTip = Utils.GetLocalizationString("BallonPlayerCheckBoxToolTip");
                var assembly = Assembly.GetEntryAssembly();
                Utils.IdleDiscordPresence();
                MainWindow.window.currentVersion_Item.Header = $"{Utils.GetLocalizationString("AppCurrentVersionDefaultText")} {assembly?.GetName().Version}";
                MainWindow.window.settingsMenuItem.Header = Utils.GetLocalizationString("SettingsMenuItemHeader");
                MainWindow.window.checkUpdatesMenuItem.Header = Utils.GetLocalizationString("CheckUpdatesMenuItemHeader");
                MainWindow.window.songTitle.Text = Utils.GetLocalizationString("SongTitleDefaultText");
                MainWindow.window.songAuthor.Text = Utils.GetLocalizationString("SongAuthorDefaultText");
                Utils.SaveSettings();
            });
        }

        private void DiscordRPC_Option(object sender, System.Windows.RoutedEventArgs e)
        {
            discordRPCCheckBox.IsChecked = Utils.settings.discordRPC;
            Utils.settings.discordRPC = !Utils.settings.discordRPC;
            Utils.IdleDiscordPresence();
            discordRPCCheckBox.IsChecked = Utils.settings.discordRPC;
            Utils.SaveSettings();
        }

        private void EconomTraffic_Option(object sender, System.Windows.RoutedEventArgs e)
        {
            trafficEconomicCheckBox.IsChecked = Utils.settings.economTraffic;
            Utils.settings.economTraffic = !Utils.settings.economTraffic;
            Utils.IdleDiscordPresence();
            trafficEconomicCheckBox.IsChecked = Utils.settings.economTraffic;
            Utils.SaveSettings();
        }

        private bool loadingPenaltySearchingEngine = false;
        private void SearchEngine_ComboBox(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingPenaltySearchingEngine)
            {
                loadingPenaltySearchingEngine = true;
                return;
            }
            Utils.settings.searchEngineIndex = searchEngineCheckBox.SelectedIndex;
            Utils.SaveSettings();
        }

        private void ballonPlayerCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ballonPlayerCheckBox.IsChecked = Utils.settings.musicPlayerBallonTurnOn;
            Utils.settings.musicPlayerBallonTurnOn = !Utils.settings.musicPlayerBallonTurnOn;
            Utils.IdleDiscordPresence();
            ballonPlayerCheckBox.IsChecked = Utils.settings.musicPlayerBallonTurnOn;
            Utils.SaveSettings();
        }
    }
}