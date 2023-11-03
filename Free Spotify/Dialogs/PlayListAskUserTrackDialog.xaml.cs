using Free_Spotify.Classes;
using Free_Spotify.Pages;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using XamlAnimatedGif;

namespace Free_Spotify.Dialogs
{
    public partial class PlayListAskUserTrackDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the PlayListAskUserTrackDialog and creates checkboxes for playlists.
        /// </summary>
        public PlayListAskUserTrackDialog()
        {
            InitializeComponent();
            addSongTitle.Text = Settings.GetLocalizationString("AddSongToPlaylistText");
            addButtonText.Text = Settings.GetLocalizationString("AddButtonText");
            cancelButtonText.Text = Settings.GetLocalizationString("CancelButtonText");
            CreatePlaylists();
        }

        private readonly List<CheckBox> checkBoxes = new();

        /// <summary>
        /// Dynamically generates and displays checkboxes for each playlist, allowing user selection.
        /// </summary>
        private void CreatePlaylists()
        {
            renderPlaylistPanel.Children.Clear();
            if (Settings.SettingsData.playlists.Count == 0)
            {
                TextBlock textBlock = new()
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14,
                    Style = (Style)FindResource("fontMontserrat"),
                    FontWeight = FontWeights.Medium,
                    Text = Settings.GetLocalizationString("NoPlaylistDefaultText"),
                    Foreground = new SolidColorBrush(Colors.Gray),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                };
                renderPlaylistPanel.Children.Add(textBlock);
                return;
            }
            int index = 0;
            foreach (var playlist in Settings.SettingsData.playlists)
            {
                // Border
                Border border = new()
                {
                    Margin = new Thickness(5),
                    Height = 50,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.White
                };

                // CheckBox (Column 3)
                CheckBox checkBox = new() { Name = $"i{index}" };

                // Grid
                Grid grid = new()
                {
                    Cursor = Cursors.Hand
                };
                grid.MouseLeftButtonDown += (sender, e) =>
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                };

                // Column Definitions
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

                // Image
                Image actualImage = new();
                try
                {
                    Uri uri = new(playlist.ImagePath, UriKind.RelativeOrAbsolute);

                    // Create an Image
                    Image image = new()
                    {
                        Margin = new Thickness(5),
                        Stretch = Stretch.Uniform
                    };

                    AnimationBehavior.SetSourceUri(image, uri);
                    AnimationBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                    actualImage = image;
                }
                catch
                {
                    Uri uri = new(Utils.DefaultImagePath);

                    // Create an Image
                    Image image = new()
                    {
                        Margin = new Thickness(5),
                        Stretch = Stretch.Uniform
                    };

                    AnimationBehavior.SetSourceUri(image, uri);
                    AnimationBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                    actualImage = image;
                }

                // Grid (Column 1)
                Grid gridInColumn1 = new();

                // Row Definitions
                gridInColumn1.RowDefinitions.Add(new RowDefinition());
                gridInColumn1.RowDefinitions.Add(new RowDefinition());

                // Label (Row 1)
                Label label1 = new()
                {
                    Style = (Style)Application.Current.Resources["fontLabelMontserrat"],
                    Margin = new Thickness(5, 0, 0, 0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Medium,
                    Padding = new Thickness(0),
                    FontSize = 14,
                    Content = new TextBlock
                    {
                        Text = playlist.Title,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    }
                };

                // Label (Row 2)
                Label label2 = new()
                {
                    Style = (Style)Application.Current.Resources["fontLabelMontserrat"],
                    Margin = new Thickness(5, 0, 0, 0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    Foreground = new SolidColorBrush(Color.FromRgb(144, 144, 144)),
                    FontWeight = FontWeights.Medium,
                    Padding = new Thickness(0),
                    FontSize = 12,
                    Content = new TextBlock
                    {
                        Text = $"{Settings.GetLocalizationString("AmountTracksDefaultText")} {playlist.TracksInPlaylist.Count} | {playlist.CalculateAmountOfTimeToListenWholePlayList()}",
                        TextTrimming = TextTrimming.CharacterEllipsis
                    }
                };

                Label labelInColumn3 = new()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Content = checkBox
                };

                // Add elements to the grid
                Grid.SetColumn(actualImage, 0);
                Grid.SetColumn(gridInColumn1, 1);
                Grid.SetColumn(labelInColumn3, 2);

                grid.Children.Add(actualImage);
                grid.Children.Add(gridInColumn1);
                grid.Children.Add(labelInColumn3);

                Grid.SetRow(label1, 0);
                Grid.SetRow(label2, 1);

                gridInColumn1.Children.Add(label1);
                gridInColumn1.Children.Add(label2);

                // Add the grid to the border
                border.Child = grid;

                checkBoxes.Add(checkBox);
                renderPlaylistPanel.Children.Add(border);
                index++;
            }
        }


        /// <summary>
        /// Closes the window and sets the dialog result to false.
        /// </summary>
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Cancels the operation and closes the window with the dialog result set to false.
        /// </summary>
        private void PlaylistCancelAddingMusic_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Adds selected music tracks to the chosen playlist and updates the playlist view.
        /// </summary>
        private void PlaylistAddMusic_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxes.Count == 0)
            {
                MessageBox.Show(Settings.GetLocalizationString("ErrorNoPlaylist"), Settings.GetLocalizationString("ErrorText"), MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = true;
                Close();
                return;
            }
            bool canAdd = false;
            foreach (var checkBox in checkBoxes)
            {
                if (checkBox.IsChecked == true && MusicPlayerPage.Instance != null && SearchViewPage.SearchWindow != null)
                {
                    int index = int.Parse(checkBox.Name[1..]);
                    var item = new SettingsData.Playlist.PlaylistItem();
                    if (Settings.SettingsData.searchEngineIndex == 1)
                    {
                        if (MusicPlayerPage.Instance.trackYouTube != null)
                        {
                            item.YouTubeTrack = new(MusicPlayerPage.Instance.trackYouTube);
                        }
                    }
                    else
                    {
                        if (MusicPlayerPage.Instance.trackSpotify != null)
                        {
                            item.SpotifyTrack = new(MusicPlayerPage.Instance.trackSpotify);
                        }
                    }
                    Settings.SettingsData.playlists[index].TracksInPlaylist.Add(item);
                    canAdd = true;
                }
            }
            if (!canAdd)
            {
                MessageBox.Show(Settings.GetLocalizationString("AddTrackErrorText"), Settings.GetLocalizationString("ErrorText"), MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = true;
                Close();
                return;
            }

            PlayListView.Instance?.CreatePlaylists();
            Settings.SaveSettings();
            DialogResult = true;
            Close();
        }

    }
}