using Free_Spotify.Classes;
using Free_Spotify.Dialogs;
using Free_Spotify.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

namespace Free_Spotify.Pages
{
    public partial class PlayListBriefView : Page, ISongManager
    {
        // Stores the index of the currently playing song within a playlist.
        private int currentSongIndex = 0;

        // Represents the index of the currently selected playlist.
        private readonly int playListCurrentIndex = -1;

        /// <summary>
        /// Initializes a new instance of the PlayListBriefView and associates it with a specific playlist.
        /// </summary>
        public PlayListBriefView(int playListCurrentIndex)
        {
            InitializeComponent();
            this.playListCurrentIndex = playListCurrentIndex;
            // Calls a method to populate the view with tracks from the selected playlist.
            GC.Collect();
            CreateTracksInPlaylist();
            if (Utils.IsPlayingFromPlaylist) playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Stop;
        }

        /// <summary>
        /// Updates the view to display tracks in the current playlist, including track information and context menus.
        /// </summary>
        public async void CreateTracksInPlaylist()
        {
            // Update the playlist view, title, and amount of tracks.
            // Display track details and context menus for each track in the current playlist.
            PlayListView.Instance?.CreatePlaylists();
            playListTitle.Text = Settings.SettingsData.playlists[playListCurrentIndex].Title;
            playListAmountTracks.Text = $"{Settings.GetLocalizationString("AmountTracksDefaultText")} {Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count} | {Settings.SettingsData.playlists[playListCurrentIndex].CalculateAmountOfTimeToListenWholePlayList()}";

            try
            {
                Uri uri = new(Settings.SettingsData.playlists[playListCurrentIndex].ImagePath, UriKind.RelativeOrAbsolute);
                FileInfo info = new(Settings.SettingsData.playlists[playListCurrentIndex].ImagePath);
                if (info.Extension == ".gif")
                {
                    AnimationBehavior.SetSourceUri(playListImage, uri);
                    AnimationBehavior.SetRepeatBehavior(playListImage, RepeatBehavior.Forever);
                }
                else
                {
                    BitmapImage bitmapImage = new(uri);
                    playListImage.Source = bitmapImage;
                }
            }
            catch
            {
                Uri uri = new(Utils.DefaultImagePath);
                BitmapImage bitmapImage = new(uri);
                playListImage.Source = bitmapImage;
            }

            mainVisualGrid.Children.Clear();
            mainVisualGrid.RowDefinitions.Clear();
            mainVisualGrid.ColumnDefinitions.Clear();

            int maxColumns = 7;
            int maxRows = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count + Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count;

            if (maxRows == 0)
            {
                return;
            }

            Dictionary<string, Image> images = new();

            for (int i = 0; i < maxColumns; i++)
            {
                if (i % 2 != 0)
                {
                    mainVisualGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1) });
                    GridSplitter splitter = new()
                    {
                        Background = new SolidColorBrush(Colors.White),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    Grid.SetColumn(splitter, i);
                    Grid.SetRowSpan(splitter, maxRows);
                    Grid.SetZIndex(splitter, 2);
                    _ = mainVisualGrid.Children.Add(splitter);
                }
                else
                {
                    mainVisualGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
            }

            int index = 0;
            for (int i = 0; i < maxRows; i++)
            {
                if (i % 2 != 0)
                {
                    mainVisualGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1) });
                    Border line = new()
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Colors.White)
                    };
                    Grid.SetColumnSpan(line, maxColumns);
                    Grid.SetRow(line, i);
                    _ = mainVisualGrid.Children.Add(line);
                }
                else
                {
                    if (PlayListView.Instance != null)
                    {
                        mainVisualGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

                        Grid gridToBeAdded = new()
                        {
                            Name = $"i{index}",
                            Cursor = Cursors.Hand,
                            IsHitTestVisible = true,
                            Background = new SolidColorBrush(Colors.Transparent)
                        };


                        gridToBeAdded.MouseLeftButtonDown += (sender, e) =>
                        {
                            currentSongIndex = int.Parse(gridToBeAdded.Name[1..]);
                            SearchViewPage.SearchWindow?.ClearListsWhenPlayingInPlaylist();
                            if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack != null)
                            {
                                MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack?.GetVideoSearchResult(), Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack, this);
                                playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Stop;
                            }
                            else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
                            {
                                MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                                playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Stop;
                            }
                            Utils.IsPlayingFromPlaylist = true;
                        };

                        Grid.SetColumnSpan(gridToBeAdded, maxColumns);
                        Grid.SetRow(gridToBeAdded, i);
                        Grid.SetZIndex(gridToBeAdded, 1);

                        // ContextMenu
                        ContextMenu contextMenu = new()
                        {
                            Background = new SolidColorBrush(Colors.Black),
                            Foreground = new SolidColorBrush(Colors.White),
                        };

                        MenuItem moveUpItem = new()
                        {
                            Header = Settings.GetLocalizationString("MoveUpDefaultText"),
                            Background = new SolidColorBrush(Colors.Black),
                            Foreground = new SolidColorBrush(Colors.White),
                            BorderThickness = new Thickness(0)
                        };
                        moveUpItem.Click += (o, e) =>
                        {
                            int index = int.Parse(gridToBeAdded.Name[1..]);
                            int oldIndex = index;
                            index--;
                            if (index < 0)
                            {
                                index = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count - 1;
                                var item = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[oldIndex];
                                Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.RemoveAt(oldIndex);
                                Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Insert(index, item);
                                Settings.SaveSettings();
                                CreateTracksInPlaylist();
                                return;
                            }
                            var item1 = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[oldIndex];
                            Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.RemoveAt(oldIndex);
                            Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Insert(index, item1);
                            Settings.SaveSettings();
                            CreateTracksInPlaylist();
                        };

                        MenuItem moveDownItem = new()
                        {
                            Header = Settings.GetLocalizationString("MoveDownDefaultText"),
                            Background = new SolidColorBrush(Colors.Black),
                            Foreground = new SolidColorBrush(Colors.White),
                            BorderThickness = new Thickness(0)
                        };
                        moveDownItem.Click += (o, e) =>
                        {
                            int index = int.Parse(gridToBeAdded.Name[1..]);
                            int oldIndex = index;
                            index++;
                            if (index >= Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count)
                            {
                                index = 0;
                                var item = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[oldIndex];
                                Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.RemoveAt(oldIndex);
                                Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Insert(index, item);
                                Settings.SaveSettings();
                                CreateTracksInPlaylist();
                                return;
                            }
                            var item1 = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[oldIndex];
                            Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.RemoveAt(oldIndex);
                            Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Insert(index, item1);
                            Settings.SaveSettings();
                            CreateTracksInPlaylist();
                        };

                        MenuItem deleteTrackItem = new()
                        {
                            Header = Settings.GetLocalizationString("DeleteDefaultText"),
                            Background = new SolidColorBrush(Colors.Black),
                            Foreground = new SolidColorBrush(Colors.White),
                            BorderThickness = new Thickness(0)
                        };
                        deleteTrackItem.Click += (o, e) =>
                        {
                            PlayListRemoveDialog removeDialog = new(PlayListRemoveDialog.TypeRemove.Track);
                            if (removeDialog.ShowDialog() == true)
                            {
                                _ = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Remove(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[int.Parse(gridToBeAdded.Name.Substring(1))]);
                                Settings.SaveSettings();
                                CreateTracksInPlaylist();
                            }
                        };

                        _ = contextMenu.Items.Add(moveUpItem);
                        _ = contextMenu.Items.Add(moveDownItem);
                        _ = contextMenu.Items.Add(deleteTrackItem);
                        gridToBeAdded.ContextMenu = contextMenu;

                        // Label (Column 0)
                        Label label0 = new()
                        {
                            Foreground = Brushes.White,
                            Style = (Style)Application.Current.Resources["fontLabelMontserrat"],
                            FontWeight = FontWeights.Bold,
                            FontSize = 18,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Content = new TextBlock
                            {
                                TextTrimming = TextTrimming.CharacterEllipsis,
                                Text = (index + 1).ToString()
                            }
                        };
                        Grid.SetColumn(label0, 0);
                        Grid.SetRow(label0, i);

                        SettingsData.Playlist.YouTubeTrackItem? youTubeTrack = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[index].YouTubeTrack;
                        SettingsData.Playlist.SpotifyTrackItem? spotifyTrack = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[index].SpotifyTrack;

                        // Image (Column 2)
                        Image? actualImage = new();
                        if (!Settings.SettingsData.economTraffic)
                        {
                            try
                            {
                                if (youTubeTrack != null && youTubeTrack.Thumbnails != null)
                                {
                                    Image image = new()
                                    {
                                        Stretch = Stretch.Uniform,
                                        Source = new BitmapImage(new(Utils.DefaultImagePath))
                                    };
                                    images[youTubeTrack.Thumbnails[0].Url] = image;
                                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                                    actualImage = image;
                                }
                                else if (spotifyTrack != null)
                                {
                                    Image image = new()
                                    {
                                        Stretch = Stretch.Uniform,
                                        Source = new BitmapImage(new(Utils.DefaultImagePath))
                                    };
                                    images[spotifyTrack.Album.Images[0].Url] = image;
                                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                                    actualImage = image;
                                }
                            }
                            catch
                            {
                                Uri uri = new(Utils.DefaultImagePath);

                                // Create an Image
                                Image image = new()
                                {
                                    Stretch = Stretch.Uniform,
                                    Source = new BitmapImage(uri)
                                };
                                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                                actualImage = image;
                            }
                        }
                        Grid.SetColumn(actualImage, 2);
                        Grid.SetRow(actualImage, i);

                        // Grid (Column 4)
                        Grid grid4 = new();
                        Grid.SetColumn(grid4, 4);
                        Grid.SetRow(grid4, i);
                        // Row Definitions
                        grid4.RowDefinitions.Add(new RowDefinition());
                        grid4.RowDefinitions.Add(new RowDefinition());

                        TextBlock label4Row0TextBlock = new()
                        {
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };

                        // Label (Row 0)
                        Label label4Row0 = new()
                        {
                            Margin = new Thickness(5, 0, 0, 0),
                            Padding = new Thickness(0),
                            Style = (Style)Application.Current.Resources["fontLabelMontserrat"],
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Medium,
                            FontSize = 18,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Content = label4Row0TextBlock
                        };


                        if (youTubeTrack != null) label4Row0TextBlock.Text = youTubeTrack.Title;
                        else if (spotifyTrack != null) label4Row0TextBlock.Text = spotifyTrack.Title;

                        TextBlock label4Row1TextBlock = new()
                        {
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };

                        // Label (Row 1)
                        Label label4Row1 = new()
                        {
                            Margin = new Thickness(5, 0, 0, 0),
                            Padding = new Thickness(0),
                            Style = (Style)Application.Current.Resources["fontLabelMontserrat"],
                            Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                            FontWeight = FontWeights.Medium,
                            FontSize = 14,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Content = label4Row1TextBlock
                        };

                        if (youTubeTrack != null && youTubeTrack.Author != null) label4Row1TextBlock.Text = youTubeTrack.Author.ToString();
                        else if (spotifyTrack != null) label4Row1TextBlock.Text = spotifyTrack.Artists[0].Name;

                        // Label (Column 6)
                        Label label6 = new()
                        {
                            Style = (Style)Application.Current.Resources["fontLabelMontserrat"],
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Medium,
                            FontSize = 18,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            Content = new TextBlock
                            {
                                TextTrimming = TextTrimming.CharacterEllipsis,
                            }
                        };
                        Grid.SetColumn(label6, 6);
                        Grid.SetRow(label6, i);

                        uint hourMillisecond = 3600000;
                        if (youTubeTrack != null)
                        {
                            if (youTubeTrack.Duration != null)
                            {
                                label6.Content = youTubeTrack.Duration.Value.TotalMilliseconds > hourMillisecond
                                    ? TimeSpan.FromMilliseconds(youTubeTrack.Duration.Value.TotalMilliseconds).ToString(@"h\:mm\:ss")
                                    : (object)TimeSpan.FromMilliseconds(youTubeTrack.Duration.Value.TotalMilliseconds).ToString(@"m\:ss");
                            }
                        }
                        else if (spotifyTrack != null)
                        {
                            label6.Content = spotifyTrack.DurationMs > hourMillisecond
                                ? TimeSpan.FromMilliseconds(spotifyTrack.DurationMs).ToString(@"h\:mm\:ss")
                                : (object)TimeSpan.FromMilliseconds(spotifyTrack.DurationMs).ToString(@"m\:ss");
                        }

                        // Add elements to the grid
                        _ = mainVisualGrid.Children.Add(gridToBeAdded);
                        _ = mainVisualGrid.Children.Add(label0);
                        if (!Settings.SettingsData.economTraffic)
                        {
                            _ = mainVisualGrid.Children.Add(actualImage);
                        }
                        _ = mainVisualGrid.Children.Add(grid4);
                        _ = mainVisualGrid.Children.Add(label6);

                        if (!Settings.SettingsData.economTraffic)
                        {
                            Grid.SetColumn(actualImage, 2);
                        }

                        Grid.SetRow(label4Row0, 0);
                        Grid.SetRow(label4Row1, 1);

                        _ = grid4.Children.Add(label4Row0);
                        _ = grid4.Children.Add(label4Row1);

                        index++;
                    }
                }
            }


            foreach (string key in images.Keys)
            {
                images[key].Source = await Utils.LoadImageAsync(key);
            }

        }

        /// <summary>
        /// Navigates back to the search page in the application.
        /// </summary>
        private void BackToSearchPage_Click(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.Window != null)
            {
                MainWindow.Window.LoadingPagesFrame.Content = null;
                _ = MainWindow.Window.LoadingPagesFrame.NavigationService.Navigate(null);
                _ = MainWindow.Window.LoadingPagesFrame.NavigationService.RemoveBackEntry();
                _ = MainWindow.Window.LoadingPagesFrame.Navigate(SearchViewPage.SearchWindow);
            }
        }

        /// <summary>
        /// Plays the first track in the current playlist.
        /// </summary>
        private void PlayButtonBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(() =>
            {
                SearchViewPage.SearchWindow?.ClearListsWhenPlayingInPlaylist();
                // If a music player instance exists and the playlist is not empty, play the first track.
                if (MusicPlayerPage.Instance == null)
                {
                    return;
                }
                if (MusicPlayerPage.Instance.progressSongTimer.Enabled)
                {
                    MusicPlayerPage.Instance?.ClearMusic();
                    playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Play;
                    Utils.IsPlayingFromPlaylist = false;
                    return;
                }
                currentSongIndex = 0;
                if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count == 0)
                {
                    return;
                }
                if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack != null)
                {
                    MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack?.GetVideoSearchResult(), Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack, this);
                }
                else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
                {
                    MusicPlayerPage.Instance.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                }
                playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Stop;
                Utils.IsPlayingFromPlaylist = true;
            });
        }

        /// <summary>
        /// Plays the previous track in the current playlist and updates the player's status bar.
        /// </summary>
        public void PreviousSong()
        {
            _ = Dispatcher.BeginInvoke(() =>
            {
                if (MusicPlayerPage.Instance == null)
                {
                    return;
                }
                currentSongIndex--;
                if (currentSongIndex < 0)
                {
                    currentSongIndex = Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count - 1;
                }
                if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack != null)
                {
                    MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack?.GetVideoSearchResult(), Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack, this);
                }
                else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
                {
                    MusicPlayerPage.Instance.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                }
            });
        }

        /// <summary>
        /// Plays the next track in the current playlist and updates the player's status bar.
        /// </summary>
        public void NextSong()
        {
            _ = Dispatcher.BeginInvoke(() =>
            {
                if (MusicPlayerPage.Instance == null)
                {
                    return;
                }
                currentSongIndex++;
                if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count <= currentSongIndex)
                {
                    currentSongIndex = 0;
                }
                if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack != null)
                {
                    MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack?.GetVideoSearchResult(), Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack, this);
                }
                else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
                {
                    MusicPlayerPage.Instance.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                }
            });
        }

        /// <summary>
        /// Shuffles the songs in the current playlist and plays the shuffled track while updating the player's status bar.
        /// </summary>
        public void ShuffleSongs()
        {
            _ = Dispatcher.BeginInvoke(() =>
            {
                if (MusicPlayerPage.Instance == null)
                {
                    return;
                }
                Random rand = new();
                int pos = rand.Next(0, Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count);
                int times = 1000;
                while (pos == currentSongIndex && times > 0)
                {
                    pos = rand.Next(0, Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count);
                    times--;
                }
                if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack != null)
                {
                    currentSongIndex = pos;
                    MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack?.GetVideoSearchResult(), Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack, this);
                }
                else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
                {
                    currentSongIndex = pos;
                    MusicPlayerPage.Instance.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                }
            });
        }
    }
}