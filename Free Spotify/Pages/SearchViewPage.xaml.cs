using FontAwesome.WPF;
using NAudio.Wave;
using SpotifyExplode;
using SpotifyExplode.Search;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Free_Spotify.Pages
{
    public partial class SearchViewPage : Page
    {
        private System.Timers.Timer countTimer = new System.Timers.Timer() { Interval = 1 }; // used to render every single millisecond the progress bar of player.
        private TrackSearchResult track = new TrackSearchResult(); // a track info to prevent stacking songs.
        private MediaFoundationReader? mediaFoundationReader = null; // used to render precise position of song and length.  
        private WaveStream? blockAlignedStream = null; // stream of sound to prevent stacking
        private WaveOut waveOut = new WaveOut(); // current song to handle pause system and volume.
        private SearchFilter filter = SearchFilter.Track; // used to search something certain in Search Engine.
        private bool isTextErased = false; // used to remove the tip
        private bool isSongPlaying = false; // nice little boolean to stop song when you change song. 
        private DateTime lastClick = DateTime.MinValue; // a penalty if you try to spam, used to remove stacking songs.
        private string githubLink = string.Empty; // later will add the link to the public repository (IF EVER!)

        public SearchViewPage()
        {
            InitializeComponent();

            // lambda render timer, inside you can see the updating player visual.
            countTimer.Elapsed += new System.Timers.ElapsedEventHandler(async (o, i) =>
            {
                await Dispatcher.BeginInvoke(() =>
                {   
                    if (mediaFoundationReader != null)
                    {
                        MainWindow.GetMainWindow(null).musicProgress.Value = mediaFoundationReader.CurrentTime.TotalMilliseconds;
                        MainWindow.GetMainWindow(null).musicProgress.Maximum = track.DurationMs;

                        if (mediaFoundationReader.CurrentTime.Hours > 0)
                        {
                            MainWindow.GetMainWindow(null).startOfSong.Content = mediaFoundationReader.CurrentTime.ToString(@"h\:m\:ss");
                        }
                        else
                        {
                            MainWindow.GetMainWindow(null).startOfSong.Content = mediaFoundationReader.CurrentTime.ToString(@"m\:ss");
                        }
                    }
                });
            });

            // pause button, switches the sprite of playing (pausing) song.
            MainWindow.GetMainWindow(null).musicToggle.MouseDown += new MouseButtonEventHandler((o, i) =>
            {
                PausingSong();
            });

            // Handler that if you press on thumb of the slider, it will change the position of the song. Also pauses the song to prevent sound buffer to break.
            MainWindow.GetMainWindow(null).musicProgress.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(async (sender, e) =>
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    if (waveOut.PlaybackState != PlaybackState.Paused)
                    {
                        PausingSong();
                        return;
                    }
                    if (mediaFoundationReader != null)
                    {

                        if (mediaFoundationReader.CurrentTime.Hours > 0)
                        {
                            MainWindow.GetMainWindow(null).startOfSong.Content = new TimeSpan(0,0,0,0,(int)MainWindow.GetMainWindow(null).musicProgress.Value).ToString(@"h\:m\:ss");
                        }
                        else
                        {
                            MainWindow.GetMainWindow(null).startOfSong.Content = new TimeSpan(0, 0, 0, 0, (int)MainWindow.GetMainWindow(null).musicProgress.Value).ToString(@"m\:ss");
                        }
                    }
                });
            }));

            // unpauses the song and renders once again the player when you release the thumb button.
            MainWindow.GetMainWindow(null).musicProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, e) =>
            {
                if (mediaFoundationReader != null)
                {
                    mediaFoundationReader.CurrentTime = new TimeSpan(0,0,0,0, (int)MainWindow.GetMainWindow(null).musicProgress.Value);
                }
                PausingSong();
            }));

            // volume slider, manages the volume of all songs. Later will be stored in saving system.
            MainWindow.GetMainWindow(null).volumeSlider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(async (sender, e) =>
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    waveOut.Volume = (float)MainWindow.GetMainWindow(null).volumeSlider.Value;
                });
            }));
        }

        /// <summary>
        /// An a event that is used in search bar to represent if you are clicked it, if you have no text inside, it will remove the tip.
        /// </summary>
        private async void SearchTextBox_Focus(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (!isTextErased)
                {
                    SearchBarTextBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    SearchBarTextBox.Text = string.Empty;
                    isTextErased = true;
                }
            });
        }

        /// <summary>
        /// Function that pauses the song.
        /// </summary>
        private async void PausingSong()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                if (waveOut != null)
                {
                    if (waveOut.PlaybackState == PlaybackState.Paused)
                    {
                        waveOut.Resume();
                        countTimer.Start();
                    MainWindow.GetMainWindow(null).musicToggle.Icon = FontAwesomeIcon.Pause;
                        return;
                    }
                    waveOut.Pause();
                    countTimer.Stop();
                    MainWindow.GetMainWindow(null).musicToggle.Icon = FontAwesomeIcon.Play;
                }
            });
        }

        /// <summary>
        /// An a event that is used in search bar to represent if you are lost focus, if you have no text inside, it will add the tip.
        /// </summary>
        private async void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                if (SearchBarTextBox.Text.Length == 0)
                {
                    SearchBarTextBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x80, 0x80, 0x80));
                    SearchBarTextBox.Text = "Что хочешь послушать?";
                    isTextErased = false;
                }
            });
        }

        /// <summary>
        /// An event that tries to give the result of certain track.
        /// </summary>
        private async void SearchBarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                SearchingSystem();
            });
        }

        /// <summary>
        /// A system, that when you type returns you the possible songs that you can listen in spotify.
        /// </summary>
        private async void SearchingSystem()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                WrapPanel wrapPanelVisual = new WrapPanel();
                wrapPanelVisual.Orientation = Orientation.Horizontal;

                if (searchVisual.Children.Count != 0)
                {
                    GC.Collect();
                    searchVisual.Children.Clear();
                    searchVisual.Children.Add(wrapPanelVisual);
                }

                if (!isTextErased)
                {
                    return;
                }

                if (SearchBarTextBox.Text.Length != 0)
                {
                    removeEverythingFromSearchBoxButton.Visibility = Visibility.Visible;
                }
                else
                {
                    removeEverythingFromSearchBoxButton.Visibility = Visibility.Hidden;
                }
                try
                {
                    var spotifyClient = new SpotifyClient();
                    /// searching for the song or else.
                    List<ISearchResult> searchMatches = await spotifyClient.Search.GetResultsAsync(SearchBarTextBox.Text, filter);
                    if (searchMatches.Count != 0)
                    {
                        foreach (var result in searchMatches)
                        {
                            // Use pattern matching to handle different results (albums, artists, tracks, playlists)
                            switch (result)
                            {
                                case TrackSearchResult track:
                                    {
                                
                                        await Dispatcher.BeginInvoke(() =>
                                        {
                                            Border background = new Border();
                                            background.CornerRadius = new CornerRadius(6);
                                            background.Cursor = Cursors.Hand;
                                            background.MouseDown += new MouseButtonEventHandler(async (o, i) =>
                                            {
                                                DateTime now = DateTime.Now;
                                                if ((now - lastClick).TotalMilliseconds < 1500)
                                                {
                                                    lastClick = now;    
                                                    return;
                                                }
                                                lastClick = now;
                                                if (isSongPlaying)
                                                {
                                                    StopSound();
                                                    return;
                                                }
                                                isSongPlaying = true; 
                                                await Task.Run(async () =>
                                                {
                                                    try
                                                    {

                                                        this.track = track;
                                                        PlaySound();
                                                        await Dispatcher.BeginInvoke(() =>
                                                        {

                                                            MainWindow.GetMainWindow(null).songTitle.Content = track.Title;
                                                            MainWindow.GetMainWindow(null).songAuthor.Content = track.Artists[0].Name;
                                                            
                                                            BitmapImage sourceImageOfTrack = new BitmapImage();
                                                            sourceImageOfTrack.BeginInit();
                                                            sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                                                            sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                                                            sourceImageOfTrack.UriSource = new Uri(track.Album.Images[0].Url);
                                                            sourceImageOfTrack.EndInit();

                                                            MainWindow.GetMainWindow(null).iconTrack.Source = sourceImageOfTrack;

                                                            uint hourMillisecond = 3600000;
                                                            if (track.DurationMs > hourMillisecond)
                                                            {
                                                                MainWindow.GetMainWindow(null).endOfSong.Content = $"{TimeSpan.FromMilliseconds(track.DurationMs).ToString(@"h\:m\:ss")}";
                                                            }
                                                            else
                                                            {
                                                                MainWindow.GetMainWindow(null).endOfSong.Content = $"{TimeSpan.FromMilliseconds(track.DurationMs).ToString(@"m\:ss")}";
                                                            }
                                                        });
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        MessageBox.Show(ex.GetType().Name);
                                                        MessageBox.Show(ex.Message);
                                                        waveOut?.Stop();
                                                        isSongPlaying = false;
                                                        countTimer.Stop();
                                                        GC.Collect();
                                                    }
                                                });
                                            });
                                            background.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x21, 0x21, 0x21));
                                            background.Width = 256;
                                            background.Height = 256;

                                            Grid mainVisualGrid = new Grid();

                                            background.Child = mainVisualGrid;

                                            RowDefinition rowDefinition = new RowDefinition();
                                            rowDefinition.Height = new GridLength(3, GridUnitType.Star);
                                            mainVisualGrid.RowDefinitions.Add(rowDefinition);

                                            RowDefinition textDefinition = new RowDefinition();
                                            textDefinition.Height = new GridLength(1, GridUnitType.Star);
                                            mainVisualGrid.RowDefinitions.Add(textDefinition);

                                            BitmapImage sourceImageOfTrack = new BitmapImage();
                                            sourceImageOfTrack.BeginInit();
                                            sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                                            sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                                            sourceImageOfTrack.UriSource = new Uri(track.Album.Images[0].Url);
                                            sourceImageOfTrack.EndInit();

                                            Image actualImageOfTrack = new Image();
                                            actualImageOfTrack.Source = sourceImageOfTrack;
                                            actualImageOfTrack.HorizontalAlignment = HorizontalAlignment.Center;
                                            actualImageOfTrack.VerticalAlignment = VerticalAlignment.Center;
                                            //actualImageOfTrack.Stretch = Stretch.Fill; later option
                                            actualImageOfTrack.CacheMode = null;
                                            mainVisualGrid.Children.Add(actualImageOfTrack);

                                            Grid.SetZIndex(actualImageOfTrack, -2);

                                            Grid.SetRow(actualImageOfTrack, 0);
                                            Grid.SetRowSpan(actualImageOfTrack, 2);

                                            Border captionBorder = new Border();
                                            captionBorder.Background = new SolidColorBrush(Color.FromArgb(180, 0x00, 0x00, 0x00));

                                            TextBlock DescriptionOfTrack = new TextBlock();
                                            DescriptionOfTrack.Text = $"Artist: {track.Artists[0].Name}\n" +
                                            $"Track: {track.Title}\n";
                                            DescriptionOfTrack.Foreground = new SolidColorBrush(Colors.White);
                                            DescriptionOfTrack.Style = (Style)DescriptionOfTrack.FindResource("fontMontserrat");
                                            DescriptionOfTrack.FontSize = 14;
                                            DescriptionOfTrack.TextWrapping = TextWrapping.WrapWithOverflow;
                                            DescriptionOfTrack.VerticalAlignment = VerticalAlignment.Center;
                                            DescriptionOfTrack.HorizontalAlignment = HorizontalAlignment.Center;
                                            DescriptionOfTrack.Padding = new Thickness(0, 10, 0, 0);

                                            Grid.SetZIndex(captionBorder, -1);
                                            Grid.SetZIndex(DescriptionOfTrack, 1);

                                            mainVisualGrid.Children.Add(captionBorder);
                                            mainVisualGrid.Children.Add(DescriptionOfTrack);
                                            Grid.SetRow(captionBorder, 1);
                                            Grid.SetRow(DescriptionOfTrack, 1);
                                            wrapPanelVisual.Children.Add(background);
                                        });
                                        break;
                                    }
                                case PlaylistSearchResult playlist:
                                    {
                                        var id = playlist.Id;
                                        var title = playlist.Name;
                                        break;
                                    }
                                case AlbumSearchResult album:
                                    {
                                        var id = album.Id;
                                        var title = album.Name;
                                        var artists = album.Artists;
                                        var tracks = album.Tracks;
                                        break;
                                    }
                                case ArtistSearchResult artist:
                                    {
                                        var id = artist.Id;
                                        var title = artist.Name;
                                        break;
                                    }
                                default:
                                    {
                                        await Dispatcher.InvokeAsync(() =>
                                        {
                                            GC.Collect();
                                            searchVisual.Children.Clear();
                                            TextBlock resultTextBlock = new TextBlock();
                                            resultTextBlock.Text = $"По запросу: \"{SearchBarTextBox.Text}\" ничего не найдено.";
                                            resultTextBlock.FontSize = 14;
                                            resultTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                                            resultTextBlock.VerticalAlignment = VerticalAlignment.Center;
                                            resultTextBlock.Foreground = new SolidColorBrush(Colors.White);
                                            resultTextBlock.TextWrapping = TextWrapping.Wrap;
                                            resultTextBlock.Style = (Style)FindResource("fontMontserrat");
                                            searchVisual.Children.Add(resultTextBlock);
                                        });
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        await Dispatcher.BeginInvoke(() =>
                        {
                            GC.Collect();
                            searchVisual.Children.Clear();
                            TextBlock resultTextBlock = new TextBlock();
                            resultTextBlock.Text = $"По запросу: \"{SearchBarTextBox.Text}\" ничего не найдено.";
                            resultTextBlock.FontSize = 14;
                            resultTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            resultTextBlock.VerticalAlignment = VerticalAlignment.Center;
                            resultTextBlock.Foreground = new SolidColorBrush(Colors.White);
                            resultTextBlock.TextWrapping = TextWrapping.Wrap;
                            resultTextBlock.Style = (Style)FindResource("fontMontserrat");
                            searchVisual.Children.Add(resultTextBlock);
                        });
                    }

                }
                catch (HttpRequestException request)
                {
                    if (request.Message.Contains("400"))
                    {
                        await Dispatcher.BeginInvoke(() =>
                        {
                            GC.Collect();
                            searchVisual.Children.Clear();
                            TextBlock resultTextBlock = new TextBlock();
                            resultTextBlock.Text = $"Начните печатать что-то, чтобы насладится приятной музыкой!";
                            resultTextBlock.FontSize = 14;
                            resultTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            resultTextBlock.VerticalAlignment = VerticalAlignment.Center;
                            resultTextBlock.Foreground = new SolidColorBrush(Colors.White);
                            resultTextBlock.TextWrapping = TextWrapping.Wrap;
                            resultTextBlock.Style = (Style)FindResource("fontMontserrat");
                            searchVisual.Children.Add(resultTextBlock);
                        });
                    }
                    else
                    {
                        await Dispatcher.BeginInvoke(() =>
                        {
                            GC.Collect();
                            searchVisual.Children.Clear();
                            TextBlock resultTextBlock = new TextBlock();
                            resultTextBlock.Text = $"Похоже у вас проблемы с интернетом!";
                            resultTextBlock.FontSize = 14;
                            resultTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            resultTextBlock.VerticalAlignment = VerticalAlignment.Center;
                            resultTextBlock.Foreground = new SolidColorBrush(Colors.White);
                            resultTextBlock.TextWrapping = TextWrapping.Wrap;
                            resultTextBlock.Style = (Style)FindResource("fontMontserrat");
                            searchVisual.Children.Add(resultTextBlock);
                        });
                    }
                }
                catch (ArgumentException)
                {
                    // ignore for now, shows error that track does not exist to download.
                }
                catch (InvalidOperationException)
                {
                    // ignore for now, appears something related to user? json issue.
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.GetType().Name);
                    MessageBox.Show(e.Message);
                }
            });
        }

        /// <summary>
        /// Function that stops the sound in case of ending the song, or if you change the song
        /// </summary>
        private void StopSound()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }
            if (blockAlignedStream != null)
            {
                blockAlignedStream.Flush();
                blockAlignedStream.Close();
            }
            countTimer.Stop();
            isSongPlaying = false;  
            GC.Collect();
        }

        /// <summary>
        /// Tries to play the sound from the youtube. If exist it plays if not then unfortunately you cannot play it :(
        /// </summary>
        private async void PlaySound()
        {
            try
            {
                SpotifyClient spotifyYouTubeRetrive = new SpotifyClient();
                string? youtubeID = await spotifyYouTubeRetrive.Tracks.GetYoutubeIdAsync(track.Url);
                YoutubeClient? youtube = new YoutubeClient();
                var video = youtube.Videos.GetAsync($"https://youtube.com/watch?v={youtubeID}");
                var streamManifest = youtube.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={youtubeID}");
                var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                using (blockAlignedStream = new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(mediaFoundationReader =
                        new MediaFoundationReader(streamInfo.Url))))
                {
                    using (waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        if (waveOut != null)
                        {
                            waveOut.Init(blockAlignedStream);
                            waveOut.Play();
                            countTimer.Start();
                            while (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
                            {
                                if (waveOut == null)
                                {
                                    break;
                                }
                                Thread.Sleep(100);
                            }
                            StopSound();
                        }
                        else
                        {
                            StopSound();
                        }
                    }

                }
            }
            catch (HttpRequestException request)
            {
                if (request.Message.Contains("400"))
                {
                    await Dispatcher.BeginInvoke(() =>
                    {
                        GC.Collect();
                        searchVisual.Children.Clear();
                        TextBlock resultTextBlock = new TextBlock();
                        resultTextBlock.Text = $"Начните печатать что-то, чтобы насладится приятной музыкой!";
                        resultTextBlock.FontSize = 14;
                        resultTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                        resultTextBlock.VerticalAlignment = VerticalAlignment.Center;
                        resultTextBlock.Foreground = new SolidColorBrush(Colors.White);
                        resultTextBlock.TextWrapping = TextWrapping.Wrap;
                        resultTextBlock.Style = (Style)FindResource("fontMontserrat");
                        searchVisual.Children.Add(resultTextBlock);
                    });
                }
                else
                {
                    await Dispatcher.BeginInvoke(() =>
                    {
                        GC.Collect();
                        searchVisual.Children.Clear();
                        TextBlock resultTextBlock = new TextBlock();
                        resultTextBlock.Text = $"Похоже у вас проблемы с интернетом!";
                        resultTextBlock.FontSize = 14;
                        resultTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                        resultTextBlock.VerticalAlignment = VerticalAlignment.Center;
                        resultTextBlock.Foreground = new SolidColorBrush(Colors.White);
                        resultTextBlock.TextWrapping = TextWrapping.Wrap;
                        resultTextBlock.Style = (Style)FindResource("fontMontserrat");
                        searchVisual.Children.Add(resultTextBlock);
                    });
                }
                StopSound();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"ERROR: {ex.GetType()}, Message: {ex.Message}, Please screenshot this error, and send me in github!\nLINK: {githubLink}");
                StopSound();
            }
        }

        /// <summary>
        /// Button that removes everything from the textbox, nice little feature.
        /// </summary>
        private async void removeEverythingFromSearchBoxButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                GC.Collect();
                SearchBarTextBox.Text = string.Empty;
                Keyboard.ClearFocus(); // removes focus from the textbox.
                searchVisual.Children.Clear();
                removeEverythingFromSearchBoxButton.Visibility = Visibility.Hidden;
            });
        }

        /// <summary>
        /// Function that if you will press it, will show you all information about artists that you were looking for.
        /// </summary>
        private async void ArtistsTag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                artistsTag.Background = new SolidColorBrush(Colors.White);
                artistsTagText.Foreground = new SolidColorBrush(Colors.Black);
                playlistTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                playlistTagText.Foreground = new SolidColorBrush(Colors.White);
                trackTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                trackTagText.Foreground = new SolidColorBrush(Colors.White);
                albumsTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                albumsTagText.Foreground = new SolidColorBrush(Colors.White);
            });
            filter = SearchFilter.Artist;
            SearchingSystem();
        }
        /// <summary>
        /// Function that if you will press it, will show you all possible playlists that were created by author.
        /// </summary>
        private async void PlaylistTag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                artistsTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                artistsTagText.Foreground = new SolidColorBrush(Colors.White);
                playlistTag.Background = new SolidColorBrush(Colors.White);
                playlistTagText.Foreground = new SolidColorBrush(Colors.Black);
                trackTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                trackTagText.Foreground = new SolidColorBrush(Colors.White);
                albumsTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                albumsTagText.Foreground = new SolidColorBrush(Colors.White);
            });
            filter = SearchFilter.Playlist;
            SearchingSystem();
        }
        /// <summary>
        /// Function that if you will press it, will show you all possible tracks that were created by author.
        /// </summary>
        private async void TrackTag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                artistsTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                artistsTagText.Foreground = new SolidColorBrush(Colors.White);
                playlistTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                playlistTagText.Foreground = new SolidColorBrush(Colors.White);
                trackTag.Background = new SolidColorBrush(Colors.White);
                trackTagText.Foreground = new SolidColorBrush(Colors.Black);
                albumsTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                albumsTagText.Foreground = new SolidColorBrush(Colors.White);
            });
            filter = SearchFilter.Track;
            SearchingSystem();
        }
        /// <summary>
        /// Function that if you will press it, will show you all possible albums that were created by author.
        /// </summary>
        private async void AlbumsTag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                artistsTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                artistsTagText.Foreground = new SolidColorBrush(Colors.White);
                playlistTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                playlistTagText.Foreground = new SolidColorBrush(Colors.White);
                trackTag.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x23, 0x23, 0x23));
                trackTagText.Foreground = new SolidColorBrush(Colors.White);
                albumsTag.Background = new SolidColorBrush(Colors.White);
                albumsTagText.Foreground = new SolidColorBrush(Colors.Black);
            });
            filter = SearchFilter.Album;
            SearchingSystem();
        }

    }
}