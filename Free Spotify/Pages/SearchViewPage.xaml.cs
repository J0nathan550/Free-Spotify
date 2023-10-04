using FontAwesome.WPF;
using Free_Spotify.Classes;
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
using System.Windows.Shell;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Free_Spotify.Pages
{
    public partial class SearchViewPage : Page
    {
        public static SearchViewPage instance;

        public System.Timers.Timer progressSongTimer = new System.Timers.Timer() { Interval = 1000 }; // used to render every single millisecond the progress bar of player.
        public CancellationTokenSource cancelProgressSongTimer = new CancellationTokenSource();

        private List<TrackSearchResult> trackList = new List<TrackSearchResult>();
        private int currentSongIndex = 0;

        private SearchFilter filter = SearchFilter.Track;    // used to search something certain in Search Engine.
        private bool isTextErased = false;                   // used to remove the tip
        private MediaPlayer mediaPlayer = new();             // new feature to play sounds, removes many rookie booleans, and prevents spamming the song
        private bool IsSongRepeat = false;                   // boolean to define if we are repeating sound
        private bool IsSongPaused = false;                   // since we do not use NAudio anymore we need just one boolean to track if we paused. 

        public SearchViewPage()
        {
            InitializeComponent();
            instance = this;
            Utils.LoadSettings();
            mediaPlayer.Volume = Utils.settings.volume;
            MainWindow.window.volumeSlider.Value = mediaPlayer.Volume;

            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            // lambda render timer, inside you can see the updating player visual.

            progressSongTimer.Elapsed += new System.Timers.ElapsedEventHandler(async (o, i) =>
            {
                try
                {
                    if (cancelProgressSongTimer.IsCancellationRequested)
                    {
                        return;
                    }
                }
                catch
                {
                    cancelProgressSongTimer.Cancel();
                }
                await Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (mediaPlayer.Source != null)
                        {
                            if (mediaPlayer.NaturalDuration.HasTimeSpan)
                            {
                                Utils.ContinueDiscordPresence(trackList[currentSongIndex]);
                                MainWindow.window.musicProgress.Value = mediaPlayer.Position.TotalMilliseconds;

                                MainWindow.window.musicProgress.Maximum = trackList[currentSongIndex].DurationMs;

                                MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Normal;
                                MainWindow.window.progressSongTaskBar.ProgressValue = mediaPlayer.Position.TotalMilliseconds / mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;

                                if (mediaPlayer.Position.Hours > 0)
                                {
                                    MainWindow.window.startOfSong.Content = mediaPlayer.Position.ToString(@"h\:m\:ss");
                                }
                                else
                                {
                                    MainWindow.window.startOfSong.Content = mediaPlayer.Position.ToString(@"m\:ss");
                                }
                            }
                        }
                    }
                    catch
                    {
                        StopSound();
                    }
                });
            });

            // pause button, switches the sprite of playing (pausing) song.
            MainWindow.window.musicToggle.MouseDown += new MouseButtonEventHandler((o, i) =>
            {
                PausingSong();
            });

            // Handler that if you press on thumb of the slider, it will change the position of the song. Also pauses the song to prevent sound buffer to break.
            MainWindow.window.musicProgress.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(async (sender, e) =>
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (!IsSongPaused)
                        {
                            PausingSong();
                            return;
                        }
                        if (mediaPlayer.Source != null)
                        {
                            if (mediaPlayer.NaturalDuration.HasTimeSpan)
                            {
                                if (mediaPlayer.Position.Hours > 0)
                                {
                                    MainWindow.window.startOfSong.Content = new TimeSpan(0, 0, 0, 0, (int)MainWindow.window.musicProgress.Value).ToString(@"h\:m\:ss");
                                }
                                else
                                {
                                    MainWindow.window.startOfSong.Content = new TimeSpan(0, 0, 0, 0, (int)MainWindow.window.musicProgress.Value).ToString(@"m\:ss");
                                }
                            }
                        }
                    }
                    catch
                    {
                        StopSound();
                    }
                });
            }));

            // unpauses the song and renders once again the player when you release the thumb button.
            MainWindow.window.musicProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, e) =>
            {
                try
                {
                    if (mediaPlayer.Source != null)
                    {
                        Utils.ContinueDiscordPresence(trackList[currentSongIndex]);
                        if (mediaPlayer.NaturalDuration.HasTimeSpan)
                        {
                            mediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)MainWindow.window.musicProgress.Value);
                        }
                    }
                    PausingSong();
                }
                catch
                {
                    StopSound();
                }
            }));

            // volume slider, manages the volume of all songs. Later will be stored in saving system.
            MainWindow.window.volumeSlider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(async (sender, e) =>
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    mediaPlayer.Volume = (float)MainWindow.window.volumeSlider.Value;
                    Utils.settings.volume = mediaPlayer.Volume;
                });
            }));

            // repeat song button, repeats the song.
            MainWindow.window.repeatSong.MouseDown += (sender, e) =>
            {
                RepeatSongBehavior();
            };


            // right button switches to the next music
            MainWindow.window.rightSong.MouseDown += async (sender, e) =>
            {
                currentSongIndex++;
                if (currentSongIndex >= trackList.Count) currentSongIndex = 0;
                await Task.Run(() =>
                {
                    Utils.ContinueDiscordPresence(trackList[currentSongIndex]);
                    PlaySound();
                    UpdateStatusPlayerBar();
                });
            };

            // left button switches to the previous music
            MainWindow.window.leftSong.MouseDown += async (sender, e) =>
            {
                currentSongIndex--;
                if (currentSongIndex < 0) currentSongIndex = trackList.Count - 1;
                await Task.Run(() =>
                {
                    Utils.ContinueDiscordPresence(trackList[currentSongIndex]);
                    PlaySound();
                    UpdateStatusPlayerBar();
                });
            };
        }

        /// <summary>
        /// An event that if the mediaplayer reaches the end of the song it will fire this event. Can repeat the song, or stop it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            Utils.ContinueDiscordPresence(trackList[currentSongIndex]);
            await Dispatcher.BeginInvoke(async () =>
            {
                if (mediaPlayer.Source != null)
                {
                    try
                    {
                        if (IsSongRepeat)
                        {
                            mediaPlayer.Position = TimeSpan.Zero;
                            return;
                        }
                        await Task.Run(() =>
                        {
                            currentSongIndex++;
                            if (currentSongIndex >= trackList.Count) currentSongIndex = 0;
                            PlaySound();
                            UpdateStatusPlayerBar();
                        });
                    }
                    catch
                    {
                        StopSound();
                    }
                }
            });
        }

        /// <summary>
        /// Function used to switch between if you want to repeat song or not, IS NOT USED TO LITERALLY REPEAT THE SAME SONG, ONLY TO SWITCH THE REPEAT BUTTON!
        /// </summary>
        private async void RepeatSongBehavior()
        {
            IsSongRepeat = !IsSongRepeat;
            if (IsSongRepeat)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    MainWindow.window.repeatSong.Foreground = new SolidColorBrush(Colors.Lime);
                });
            }
            else
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    MainWindow.window.repeatSong.Foreground = new SolidColorBrush(Colors.White);
                });
            }
        }



        /// <summary>
        /// Function that pauses the song.
        /// </summary>
        private async void PausingSong()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (mediaPlayer.Source != null)
                    {
                        try
                        {
                            if (IsSongPaused)
                            {
                                try
                                {
                                    IsSongPaused = false;
                                    mediaPlayer.Play();
                                    progressSongTimer.Start();
                                    Utils.ContinueDiscordPresence(trackList[currentSongIndex]);
                                    MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Pause;
                                    if (mediaPlayer.Source != null)
                                    {
                                        MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Normal;
                                        if (mediaPlayer.NaturalDuration.HasTimeSpan)
                                        {
                                            MainWindow.window.progressSongTaskBar.ProgressValue = mediaPlayer.Position.TotalMilliseconds / mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                                        }
                                    }
                                    else
                                    {
                                        MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Error;
                                    }
                                }
                                catch
                                {
                                    StopSound();
                                }
                                return;
                            }
                            try
                            {
                                Utils.PauseDiscordPresence(trackList[currentSongIndex]);
                                IsSongPaused = true;
                                mediaPlayer.Pause();
                                progressSongTimer.Stop();
                                MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Play;
                                if (mediaPlayer.Source != null)
                                {
                                    MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Paused;
                                    if (mediaPlayer.NaturalDuration.HasTimeSpan)
                                    {
                                        MainWindow.window.progressSongTaskBar.ProgressValue = mediaPlayer.Position.TotalMilliseconds / mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                                    }
                                }
                                else
                                {
                                    MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Error;
                                }
                            }
                            catch
                            {
                                StopSound();
                            }
                        }
                        catch
                        {
                            StopSound();
                        }
                    }
                    else
                    {
                        MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.None;
                        MainWindow.window.progressSongTaskBar.ProgressValue = 0;
                    }
                }
                catch
                {
                    StopSound();
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
        /// A system, that when you type returns you the possible songs that you can listen in spotify.
        /// </summary>
        private async void SearchingSystem()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                WrapPanel wrapPanelVisual = new WrapPanel();
                try
                {
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
                }
                catch
                {
                    StopSound();
                }

                try
                {
                    /// searching for the song or else.
                    int indexLoop = 0;
                    var spotifyClient = new SpotifyClient();
                    List<ISearchResult> searchMatches = await spotifyClient.Search.GetResultsAsync(SearchBarTextBox.Text, filter);
                    List<TrackSearchResult> newSongs = new();

                    if (searchMatches.Count != 0)
                    {
                        foreach (var result in searchMatches)
                        {
                            // Use pattern matching to handle different results (albums, artists, tracks, playlists)
                            switch (result)
                            {
                                case TrackSearchResult track:
                                    {
                                        newSongs.Add(track);
                                        await Dispatcher.BeginInvoke(() =>
                                        {
                                            try
                                            {
                                                Border background = new Border();
                                                background.Name = $"i{indexLoop}";
                                                background.CornerRadius = new CornerRadius(6);
                                                background.Cursor = Cursors.Hand;
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

                                                background.MouseDown += new MouseButtonEventHandler(async (o, i) =>
                                                {
                                                    await Dispatcher.BeginInvoke(() =>
                                                    {
                                                        try
                                                        {
                                                            if (mediaPlayer.Source != null)
                                                            {
                                                                if (IsSongRepeat)
                                                                {
                                                                    RepeatSongBehavior();
                                                                }
                                                                StopSound();
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            StopSound();
                                                        }
                                                    });
                                                    await Task.Run(async () =>
                                                    {
                                                        try
                                                        {

                                                            trackList.Clear();
                                                            trackList.AddRange(newSongs);
                                                            int symbolToRemove = 1;
                                                            await Dispatcher.BeginInvoke(() =>
                                                            {
                                                                currentSongIndex = int.Parse(background.Name.Substring(symbolToRemove));
                                                            });
                                                            PlaySound();
                                                            UpdateStatusPlayerBar();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            StopSound();
                                                            MessageBox.Show(ex.GetType().Name);
                                                            MessageBox.Show(ex.Message);
                                                        }
                                                    });
                                                });
                                            }
                                            catch
                                            {
                                            }
                                        });
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
                            indexLoop++;
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
                    StopSound();
                    MessageBox.Show(e.GetType().Name);
                    MessageBox.Show(e.Message);
                }
            });
        }

        /// <summary>
        /// Updates the images, the length of song of mp3player.
        /// </summary>
        private async void UpdateStatusPlayerBar()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Pause;
                    MainWindow.window.songTitle.Content = trackList[currentSongIndex].Title;
                    MainWindow.window.songAuthor.Content = trackList[currentSongIndex].Artists[0].Name;


                    BitmapImage sourceImageOfTrack = new BitmapImage();
                    sourceImageOfTrack.BeginInit();
                    sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                    sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                    sourceImageOfTrack.UriSource = new Uri(trackList[currentSongIndex].Album.Images[0].Url);
                    sourceImageOfTrack.EndInit();

                    MainWindow.window.iconTrack.Source = sourceImageOfTrack;

                    uint hourMillisecond = 3600000;
                    if (trackList[currentSongIndex].DurationMs > hourMillisecond)
                    {
                        MainWindow.window.endOfSong.Content = $"{TimeSpan.FromMilliseconds(trackList[currentSongIndex].DurationMs).ToString(@"h\:m\:ss")}";
                    }
                    else
                    {
                        MainWindow.window.endOfSong.Content = $"{TimeSpan.FromMilliseconds(trackList[currentSongIndex].DurationMs).ToString(@"m\:ss")}";
                    }
                }
                catch
                {
                    StopSound();
                }
            });
        }

        /// <summary>
        /// Function that stops the sound in case of selecting new song.
        /// </summary>
        private async void StopSound()
        {
            try
            {
                Utils.IdleDiscordPresence();
                IsSongPaused = true;
                PausingSong();
                await Dispatcher.BeginInvoke(() =>
                {
                    if (mediaPlayer.Source != null)
                    {
                        mediaPlayer.Position = TimeSpan.Zero;
                        mediaPlayer.Stop();
                        mediaPlayer.Close();
                    }
                    progressSongTimer.Stop();
                });
                await Dispatcher.BeginInvoke(() =>
                {
                    MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.None;
                    MainWindow.window.progressSongTaskBar.ProgressValue = 0;
                });
                GC.Collect();
            }
            catch (Exception ex)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
                progressSongTimer.Stop();
                IsSongPaused = true;
                PausingSong();
                MessageBox.Show(ex.GetType().ToString());
                MessageBox.Show(ex.Message);
                GC.Collect();
            }
        }

        /// <summary>
        /// Tries to play the sound from the youtube. If exist it plays if not then unfortunately you cannot play it :(
        /// </summary>
        private async void PlaySound()
        {
            try
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    if (mediaPlayer.Source != null)
                    {
                        StopSound();
                        return;
                    }
                });
                await Dispatcher.BeginInvoke(() =>
                {
                    MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Indeterminate;
                    MainWindow.window.progressSongTaskBar.ProgressValue = 0;
                });
                SpotifyClient spotifyYouTubeRetrive = new SpotifyClient();
                string? youtubeID = await spotifyYouTubeRetrive.Tracks.GetYoutubeIdAsync(trackList[currentSongIndex].Url);
                YoutubeClient? youtube = new YoutubeClient();
                var video = youtube.Videos.GetAsync($"https://youtube.com/watch?v={youtubeID}");
                var streamManifest = youtube.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={youtubeID}");
                var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                await Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        mediaPlayer.Open(new Uri(streamInfo.Url));
                        mediaPlayer.Play();
                        progressSongTimer.Start();
                        mediaPlayer.Volume = (float)MainWindow.window.volumeSlider.Value;
                        Utils.StartDiscordPresence(trackList[currentSongIndex]);
                    }
                    catch
                    {
                        StopSound();
                    }
                });
            }
            catch (ArgumentException) // exception that arrives when song doesn't exist in YouTube and it simply tries to switch to another one. 
            {
                try
                {
                    currentSongIndex++;
                    if (currentSongIndex >= trackList.Count) currentSongIndex = 0;
                    await Task.Run(() =>
                    {
                        PlaySound();
                        UpdateStatusPlayerBar();
                    });
                }
                catch
                {
                    StopSound();
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
            catch (NullReferenceException)
            {
                StopSound();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: {ex.GetType()},\n Message: {ex.Message},\n Please screenshot this error, and send me in github!\nLINK: {Utils.githubLink}");
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
    }
}