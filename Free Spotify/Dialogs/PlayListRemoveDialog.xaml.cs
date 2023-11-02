using Free_Spotify.Classes;
using System.Windows;

namespace Free_Spotify.Dialogs
{
    public partial class PlayListRemoveDialog : Window
    {
        /// <summary>
        /// Represents the types of removal: Playlist or Track.
        /// </summary>
        public enum TypeRemove
        {
            Playlist = 0,
            Track = 1
        }

        /// <summary>
        /// Initializes a new instance of the PlayListRemoveDialog class based on the specified removal type.
        /// </summary>
        /// <param name="type">The removal type, either TypeRemove.Playlist or TypeRemove.Track.</param>
        public PlayListRemoveDialog(TypeRemove type)
        {
            InitializeComponent();
            removePlaylist.Text = Settings.GetLocalizationString("RemovePlaylistText");
            removeCancelText.Text = Settings.GetLocalizationString("CancelButtonText");
            removeButtonText.Text = Settings.GetLocalizationString("RemoveButtonText");
            switch (type)
            {
                case TypeRemove.Playlist:
                    whatToRemove.Content = Settings.GetLocalizationString("QuestionPlaylist");
                    break;
                case TypeRemove.Track:
                    whatToRemove.Content = Settings.GetLocalizationString("QuestionTrack");
                    break;
                default:
                    MessageBox.Show("Wrong type specified.");
                    break;
            }
        }

        /// <summary>
        /// Handles the click event to remove the playlist or track, returning true.
        /// </summary>
        private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles the click event to close the dialog and return false in PlayListView.
        /// </summary>
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the click event to cancel the removal and return false in PlayListView.
        /// </summary>
        private void RemovePlaylistCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}