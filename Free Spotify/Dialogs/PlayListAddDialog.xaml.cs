using Free_Spotify.Classes;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Free_Spotify.Dialogs
{
    public partial class PlayListAddDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the PlayListAddDialog.
        /// </summary>
        public PlayListAddDialog()
        {
            InitializeComponent();
            addPlaylistText.Text = Settings.GetLocalizationString("AddPlaylistText");
            title.Text = Settings.GetLocalizationString("TitlePlaylistDefaultText");
            imageOptional.Text = Settings.GetLocalizationString("ImagePlaylistDefaultText");
            createPlaylistText.Text = Settings.GetLocalizationString("CreatePlaylistText");
            LoadDefaultImage();
        }

        /// <summary>
        /// Closes the window without creating a playlist.
        /// </summary>
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close(); // Close the custom window
        }

        /// <summary>
        /// Opens a file explorer to select an image or GIF for the playlist.
        /// If another file type is selected, the program loads the default icon.
        /// </summary>
        private void AddImageToPlayList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Uri uri;
            BitmapImage bitmapImage;
            try
            {
                OpenFileDialog ofd = new()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    Filter = Settings.GetLocalizationString("FilterForDialogDefaultText"),
                    Title = Settings.GetLocalizationString("SelectImageDefaultText")
                };
                if (ofd.ShowDialog() == true)
                {
                    uri = new Uri(ofd.FileName, UriKind.RelativeOrAbsolute);
                    bitmapImage = new BitmapImage(uri);
                    demoImagePlaylist.Source = bitmapImage;
                    ImageBehavior.SetAnimatedSource(demoImagePlaylist, bitmapImage);
                    ImageBehavior.SetRepeatBehavior(demoImagePlaylist, RepeatBehavior.Forever);
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
        /// Shows the user how their playlist will look like.
        /// </summary>
        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(titleTextBox.Text))
            {
                titleDemoPlaylist.Text = Settings.GetLocalizationString("EnterPlaylistTitleDefaultText");
                return;
            }
            titleDemoPlaylist.Text = titleTextBox.Text;
        }

        /// <summary>
        /// Creates a playlist in PlayListView by adding values to JSON and then fetching them in PlayListView.
        /// </summary>
        private void CreatePlaylist_Click(object sender, RoutedEventArgs e)
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
            Settings.SettingsData.playlists.Add(new SettingsData.Playlist() { Title = titleTextBox.Text, ImagePath = image });
            Settings.SaveSettings();
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handy function to load the default playlist image.
        /// </summary>
        private void LoadDefaultImage()
        {
            BitmapImage bitmapImage = new(new Uri(Utils.DefaultImagePath));
            demoImagePlaylist.Source = bitmapImage;
            ImageBehavior.SetAnimatedSource(demoImagePlaylist, bitmapImage);
            ImageBehavior.SetRepeatBehavior(demoImagePlaylist, RepeatBehavior.Forever);
            imageTextBox.Text = string.Empty;
        }


    }
}