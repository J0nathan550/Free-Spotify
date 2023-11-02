using Free_Spotify.Classes;
using Free_Spotify.Dialogs;
using Free_Spotify.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

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
            CreateTracksInPlaylist();
            if (Utils.IsPlayingFromPlaylist) playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Pause;
        }

        /// <summary>
        /// Updates the view to display tracks in the current playlist, including track information and context menus.
        /// </summary>
        public void CreateTracksInPlaylist()
        {
            // Update the playlist view, title, and amount of tracks.
            // Display track details and context menus for each track in the current playlist.

            PlayListView.Instance?.CreatePlaylists();
            playListTitle.Text = Settings.SettingsData.playlists[playListCurrentIndex].Title;
            playListAmountTracks.Text = $"{Settings.GetLocalizationString("AmountTracksDefaultText")} {Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Count} | {Settings.SettingsData.playlists[playListCurrentIndex].CalculateAmountOfTimeToListenWholePlayList()}";

            try
            {
                Uri uri = new(Settings.SettingsData.playlists[playListCurrentIndex].ImagePath, UriKind.RelativeOrAbsolute);
                BitmapImage bitmapImage = new(uri);
                ImageBehavior.SetAnimatedSource(playListImage, bitmapImage);
                ImageBehavior.SetRepeatBehavior(playListImage, RepeatBehavior.Forever);
                RenderOptions.SetBitmapScalingMode(bitmapImage, BitmapScalingMode.Fant);
            }
            catch
            {
                BitmapImage bitmapImage = new(new Uri(Utils.DefaultImagePath));
                ImageBehavior.SetAnimatedSource(playListImage, bitmapImage);
                ImageBehavior.SetRepeatBehavior(playListImage, RepeatBehavior.Forever);
                RenderOptions.SetBitmapScalingMode(bitmapImage, BitmapScalingMode.Fant);
            }

            if (PlayListView.Instance != null)
            {
                trackPanelDisplay.Children.Clear();
                int index = 0;
                foreach (var track in Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist)
                {
                    // Grid
                    Grid grid = new()
                    {
                        Height = 50,
                        Name = $"i{index}",
                        Cursor = Cursors.Hand
                    };

                    grid.MouseLeftButtonDown += (sender, e) =>
                    {
                        currentSongIndex = int.Parse(grid.Name[1..]);
                        if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack != null)
                        {
                            MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack?.GetVideoSearchResult(), Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].YouTubeTrack, this);
                            SearchViewPage.SearchWindow?.ClearSearchView();
                            playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Pause;
                        }
                        else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
                        {
                            MusicPlayerPage.Instance?.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                            SearchViewPage.SearchWindow?.ClearSearchView();
                            playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Pause;
                        }
                        Utils.IsPlayingFromPlaylist = true;
                    };

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
                        int index = int.Parse(grid.Name[1..]);
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
                        int index = int.Parse(grid.Name[1..]);
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
                            Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist.Remove(track);
                            Settings.SaveSettings();
                            CreateTracksInPlaylist();
                        }
                    };

                    contextMenu.Items.Add(moveUpItem);
                    contextMenu.Items.Add(moveDownItem);
                    contextMenu.Items.Add(deleteTrackItem);
                    grid.ContextMenu = contextMenu;

                    // Column Definitions
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

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

                    // Rectangle (Column 1)
                    Rectangle rectangle1 = new()
                    {
                        SnapsToDevicePixels = true,
                        Fill = Brushes.White
                    };

                    // Image (Column 2)
                    Image? actualImage = new();
                    if (!Settings.SettingsData.economTraffic)
                    {
                        RenderOptions.SetBitmapScalingMode(actualImage, BitmapScalingMode.Fant);
                        try
                        {
                            if (track.YouTubeTrack != null && track.YouTubeTrack.Thumbnails != null)
                            {
                                Uri uri = new(track.YouTubeTrack.Thumbnails[0].Url, UriKind.RelativeOrAbsolute);
                                BitmapImage bitmapImage = new(uri);

                                // Create an Image
                                Image image = new()
                                {
                                    Margin = new Thickness(5),
                                    Source = bitmapImage,
                                    Stretch = Stretch.Uniform
                                };
                                ImageBehavior.SetAnimatedSource(image, bitmapImage);
                                ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                                actualImage = image;
                            }
                            else if (track.SpotifyTrack != null)
                            {
                                Uri uri = new(track.SpotifyTrack.Album.Images[0].Url, UriKind.RelativeOrAbsolute);
                                BitmapImage bitmapImage = new(uri);

                                // Create an Image
                                Image image = new()
                                {
                                    Margin = new Thickness(5),
                                    Source = bitmapImage,
                                    Stretch = Stretch.Uniform
                                };

                                ImageBehavior.SetAnimatedSource(image, bitmapImage);
                                ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                                actualImage = image;
                            }
                        }
                        catch
                        {
                            BitmapImage bitmapImage = new(new Uri(Utils.DefaultImagePath));

                            // Create an Image
                            Image image = new()
                            {
                                Margin = new Thickness(5),
                                Source = bitmapImage,
                                Stretch = Stretch.Uniform
                            };

                            ImageBehavior.SetAnimatedSource(image, bitmapImage);
                            ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                            actualImage = image;
                        }
                    }
                    else
                    {
                        actualImage = null;
                    }


                    // Rectangle (Column 3)
                    Rectangle rectangle3 = new()
                    {
                        SnapsToDevicePixels = true,
                        Fill = Brushes.White
                    };

                    // Grid (Column 4)
                    Grid grid4 = new();

                    // Row Definitions
                    grid4.RowDefinitions.Add(new RowDefinition());
                    grid4.RowDefinitions.Add(new RowDefinition());

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
                        Content = new TextBlock
                        {
                            TextTrimming = TextTrimming.CharacterEllipsis,
                        }
                    };

                    if (track.YouTubeTrack != null) label4Row0.Content = track.YouTubeTrack.Title;
                    else if (track.SpotifyTrack != null) label4Row0.Content = track.SpotifyTrack.Title;

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
                        Content = new TextBlock
                        {
                            TextTrimming = TextTrimming.CharacterEllipsis,
                        }
                    };

                    if (track.YouTubeTrack != null) label4Row1.Content = track.YouTubeTrack.Author;
                    else if (track.SpotifyTrack != null) label4Row1.Content = track.SpotifyTrack.Artists[0].Name;

                    // Rectangle (Column 5)
                    Rectangle rectangle5 = new()
                    {
                        Fill = Brushes.White
                    };

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


                    uint hourMillisecond = 3600000;
                    if (track.YouTubeTrack != null)
                    {
                        if (track.YouTubeTrack.Duration != null)
                        {
                            if (track.YouTubeTrack.Duration.Value.TotalMilliseconds > hourMillisecond)
                            {
                                label6.Content = TimeSpan.FromMilliseconds(track.YouTubeTrack.Duration.Value.TotalMilliseconds).ToString(@"h\:mm\:ss");
                            }
                            else
                            {
                                label6.Content = TimeSpan.FromMilliseconds(track.YouTubeTrack.Duration.Value.TotalMilliseconds).ToString(@"m\:ss");
                            }
                        }
                    }
                    else if (track.SpotifyTrack != null)
                    {
                        if (track.SpotifyTrack.DurationMs > hourMillisecond)
                        {
                            label6.Content = TimeSpan.FromMilliseconds(track.SpotifyTrack.DurationMs).ToString(@"h\:mm\:ss");
                        }
                        else
                        {
                            label6.Content = TimeSpan.FromMilliseconds(track.SpotifyTrack.DurationMs).ToString(@"m\:ss");
                        }
                    }

                    // Add elements to the grid
                    grid.Children.Add(label0);
                    grid.Children.Add(rectangle1);
                    if (!Settings.SettingsData.economTraffic)
                    {
                        grid.Children.Add(actualImage);
                    }
                    grid.Children.Add(rectangle3);
                    grid.Children.Add(grid4);
                    grid.Children.Add(rectangle5);
                    grid.Children.Add(label6);

                    Grid.SetColumn(label0, 0);
                    Grid.SetColumn(rectangle1, 1);
                    if (!Settings.SettingsData.economTraffic)
                    {
                        Grid.SetColumn(actualImage, 2);
                    }
                    Grid.SetColumn(actualImage, 2);
                    Grid.SetColumn(rectangle3, 3);
                    Grid.SetColumn(grid4, 4);
                    Grid.SetColumn(rectangle5, 5);
                    Grid.SetColumn(label6, 6);

                    Grid.SetRow(label4Row0, 0);
                    Grid.SetRow(label4Row1, 1);

                    grid4.Children.Add(label4Row0);
                    grid4.Children.Add(label4Row1);

                    // Border
                    Border border = new()
                    {
                        BorderThickness = new Thickness(0.5),
                        BorderBrush = Brushes.White
                    };

                    // Add the grid and border to the stack panel
                    trackPanelDisplay.Children.Add(grid);
                    trackPanelDisplay.Children.Add(border);
                    index++;
                }
                if (trackPanelDisplay.Children.Count == 0)
                {
                    return;
                }
                trackPanelDisplay.Children.RemoveAt(trackPanelDisplay.Children.Count - 1);
            }
        }

        /// <summary>
        /// Navigates back to the search page in the application.
        /// </summary>
        private void BackToSearchPage_Click(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.Window == null)
            {
                return;
            }
            SearchViewPage.SearchWindow = null;
            SearchViewPage.SearchWindow = new();
            Content = null;
            MainWindow.Window.LoadingPagesFrame.Content = null;
            MainWindow.Window.LoadingPagesFrame.NavigationService.Navigate(null);
            MainWindow.Window.LoadingPagesFrame.NavigationService.RemoveBackEntry();
            MainWindow.Window.LoadingPagesFrame.Navigate(SearchViewPage.SearchWindow);
        }

        /// <summary>
        /// Plays the first track in the current playlist.
        /// </summary>
        private void PlayButtonBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
            SearchViewPage.SearchWindow?.ClearSearchView();
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
            playIconPlaylist.Icon = FontAwesome.WPF.FontAwesomeIcon.Pause;
            Utils.IsPlayingFromPlaylist = true;
        }

        /// <summary>
        /// Plays the previous track in the current playlist and updates the player's status bar.
        /// </summary>
        public void PreviousSong()
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
                MusicPlayerPage.Instance?.UpdateStatusPlayerBar();
            }
            else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
            {
                MusicPlayerPage.Instance.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                MusicPlayerPage.Instance.UpdateStatusPlayerBar();
            }
        }

        /// <summary>
        /// Plays the next track in the current playlist and updates the player's status bar.
        /// </summary>
        public void NextSong()
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
                MusicPlayerPage.Instance?.UpdateStatusPlayerBar();
            }
            else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
            {
                MusicPlayerPage.Instance.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                MusicPlayerPage.Instance.UpdateStatusPlayerBar();
            }
        }

        /// <summary>
        /// Shuffles the songs in the current playlist and plays the shuffled track while updating the player's status bar.
        /// </summary>
        public void ShuffleSongs()
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
                MusicPlayerPage.Instance?.UpdateStatusPlayerBar();
            }
            else if (Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack != null)
            {
                currentSongIndex = pos;
                MusicPlayerPage.Instance.PlaySound(Settings.SettingsData.playlists[playListCurrentIndex].TracksInPlaylist[currentSongIndex].SpotifyTrack?.GetTrackSearchResult(), this);
                MusicPlayerPage.Instance.UpdateStatusPlayerBar();
            }
        }
    }
}