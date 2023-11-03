using Free_Spotify.Classes;
using Free_Spotify.Pages;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using XamlAnimatedGif;

namespace Free_Spotify.Dialogs
{
    public partial class PlayListRenameDialog : Window
    {
        /// <summary>
        /// The index of the playlist being renamed.
        /// </summary>
        private readonly int renamePlayListIndex = -1;

        /// <summary>
        /// Initializes a new instance of the PlayListRenameDialog.
        /// </summary>
        public PlayListRenameDialog(int playListIndex)
        {
            InitializeComponent();

            renamePlayListIndex = playListIndex;
            titleDemoPlaylist.Text = Settings.SettingsData.playlists[renamePlayListIndex].Title;
            titleTextBox.Text = Settings.SettingsData.playlists[renamePlayListIndex].Title;
            imageTextBox.Text = Settings.SettingsData.playlists[renamePlayListIndex].ImagePath;
            renamePlaylist.Text = Settings.GetLocalizationString("RenamePlaylistText");
            title.Text = Settings.GetLocalizationString("TitlePlaylistDefaultText");
            imageOptional.Text = Settings.GetLocalizationString("ImagePlaylistDefaultText");
            renamePlaylistButton.Text = Settings.GetLocalizationString("RenamePlaylistText");
            amountOfTracksText.Text = $"{Settings.GetLocalizationString("AmountTracksDefaultText")} 0";

            try
            {
                Uri uri = new(Settings.SettingsData.playlists[renamePlayListIndex].ImagePath, UriKind.RelativeOrAbsolute);
                AnimationBehavior.SetSourceUri(demoImagePlaylist, uri);
                AnimationBehavior.SetRepeatBehavior(demoImagePlaylist, RepeatBehavior.Forever);
            }
            catch
            {
                LoadDefaultImage();
            }

        }

        /// <summary>
        /// Closes the PlayListRenameDialog and sets the DialogResult to false.
        /// </summary>
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the TextChanged event of the TitleTextBox to update the displayed playlist title.
        /// </summary>
        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(titleTextBox.Text))
            {
                titleDemoPlaylist.Text = Settings.GetLocalizationString("EnterPlaylistDefaultText");
                return;
            }
            titleDemoPlaylist.Text = titleTextBox.Text;
        }

        /// <summary>
        /// Handles the MouseDown event when renaming the playlist's image.
        /// </summary>
        private void RenameImageToPlayList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Uri uri;
            try
            {
                OpenFileDialog ofd = new()
                {
                    AddExtension = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = Settings.GetLocalizationString("FilterForDialogDefaultText"),
                    Title = Settings.GetLocalizationString("SelectImageDefaultText")
                };
                if (ofd.ShowDialog() == true)
                {
                    uri = new Uri(ofd.FileName, UriKind.RelativeOrAbsolute);
                    AnimationBehavior.SetSourceUri(demoImagePlaylist, uri);
                    AnimationBehavior.SetRepeatBehavior(demoImagePlaylist, RepeatBehavior.Forever);
                    imageTextBox.Text = ofd.FileName;
                    return;
                }
                LoadDefaultImage();
            }
            catch
            {
                LoadDefaultImage();
            }
        }

        /// <summary>
        /// Handles the click event to rename the playlist. It updates the playlist title and image.
        /// </summary>
        private void RenamePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(titleTextBox.Text))
            {
                MessageBox.Show(Settings.GetLocalizationString("ErrorEnterTheTitleDefaultText"), Settings.GetLocalizationString("ErrorText"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string image;
            if (!string.IsNullOrEmpty(imageTextBox.Text))
            {
                image = imageTextBox.Text;
            }
            else
            {
                image = Utils.DefaultImagePath;
            }
            Settings.SettingsData.playlists[renamePlayListIndex].ImagePath = image;
            Settings.SettingsData.playlists[renamePlayListIndex].Title = titleTextBox.Text;
            if (PlayListView.Instance != null && MainWindow.Window != null)
            {
                if (PlayListView.Instance.playListBriefView != null)
                {
                    PlayListView.Instance.playListBriefView = null;
                }
                PlayListView.Instance.playListBriefView = new(PlayListView.Instance.currentSelectedPlaylist);
                MainWindow.Window.LoadingPagesFrame.Content = null;
                MainWindow.Window.LoadingPagesFrame.NavigationService.Navigate(null);
                MainWindow.Window.LoadingPagesFrame.NavigationService.RemoveBackEntry();
                MainWindow.Window.LoadingPagesFrame.Navigate(PlayListView.Instance.playListBriefView);
            }
            Settings.SaveSettings();
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// A handy function to load the default playlist image.
        /// </summary>
        private void LoadDefaultImage()
        {
            Uri uri = new(Utils.DefaultImagePath);
            AnimationBehavior.SetSourceUri(demoImagePlaylist, uri);
            AnimationBehavior.SetRepeatBehavior(demoImagePlaylist, RepeatBehavior.Forever);
            imageTextBox.Text = string.Empty;
        }

    }
}