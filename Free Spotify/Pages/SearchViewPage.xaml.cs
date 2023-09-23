using DiscordRPC;
using FontAwesome.WPF;
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
        public System.Timers.Timer countTimer = new System.Timers.Timer() { Interval = 1 }; // used to render every single millisecond the progress bar of player.
        public CancellationTokenSource cancelTimer = new CancellationTokenSource();
        private TrackSearchResult track = new TrackSearchResult(); // a track info to prevent stacking songs.
        private SearchFilter filter = SearchFilter.Track; // used to search something certain in Search Engine.
        private bool isTextErased = false; // used to remove the tip
        private string githubLink = "https://github.com/J0nathan550"; // later will add the link to the public repository (IF EVER!)s
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private bool IsSongRepeat = false; // boolean to define if we are repeating sound
        private bool IsSongPaused = false;

        public SearchViewPage()
        {
            InitializeComponent();
            // lambda render timer, inside you can see the updating player visual.
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            countTimer.Elapsed += new System.Timers.ElapsedEventHandler(async (o, i) =>
            {
                if (cancelTimer.IsCancellationRequested)
                {
                    return;
                }
                await Dispatcher.BeginInvoke(() =>
                {
                    if (mediaPlayer.Source != null)
                    {
                        if (mediaPlayer.NaturalDuration.HasTimeSpan)
                        {
                            MainWindow.window.musicProgress.Value = mediaPlayer.Position.TotalMilliseconds;
                            MainWindow.window.musicProgress.Maximum = track.DurationMs;

                            MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Normal;
                            MainWindow.window.progressSongTaskBar.ProgressValue = mediaPlayer.Position.TotalMilliseconds / track.DurationMs;

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
                });
            }));

            // unpauses the song and renders once again the player when you release the thumb button.
            MainWindow.window.musicProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, e) =>
            {
                if (mediaPlayer.Source != null)
                {
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = "Слушает музыку...",
                        State = $"{track.Artists[0].Name} - {track.Title}",
                        Assets = new Assets()
                        {
                            LargeImageKey = "logo",
                            LargeImageText = "Free Spotify",
                        },
                        Timestamps = new Timestamps()
                        {
                            Start = DateTime.UtcNow.AddMilliseconds(-MainWindow.window.musicProgress.Value),
                            End = DateTime.UtcNow.AddMilliseconds(MainWindow.window.musicProgress.Maximum - MainWindow.window.musicProgress.Value)
                        },
                        Buttons = new DiscordRPC.Button[]
                        {
                            new DiscordRPC.Button()
                            {
                                Label = "Послушать трек...",
                                Url = track.Url,
                            },
                            new DiscordRPC.Button()
                            {
                                Label = "Free Spotify...",
                                Url = githubLink
                            }
                        }
                    });
                    if (mediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        mediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)MainWindow.window.musicProgress.Value);
                    }
                }
                PausingSong();
            }));

            // volume slider, manages the volume of all songs. Later will be stored in saving system.
            MainWindow.window.volumeSlider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(async (sender, e) =>
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    mediaPlayer.Volume = (float)MainWindow.window.volumeSlider.Value;    
                });
            }));

            // repeat song button, repeats the song.
            MainWindow.window.repeatSong.MouseDown += (sender, e) =>
            {
                RepeatSongBehavior();
            };
        }

        private async void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                if (mediaPlayer.Source != null)
                {
                    if (IsSongRepeat)
                    {
                        mediaPlayer.Position = TimeSpan.Zero;
                        return;
                    }
                    StopSound();
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
                if (mediaPlayer.Source != null)
                {
                    if (IsSongPaused)
                    {
                        IsSongPaused = false;
                        mediaPlayer.Play();
                        countTimer.Start();
                        MainWindow.window.discordClient.SetPresence(new RichPresence()
                        {
                            Details = "Слушает музыку...",
                            State = $"{track.Artists[0].Name} - {track.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = "logo",
                                LargeImageText = "Free Spotify",
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = DateTime.UtcNow.AddMilliseconds(-MainWindow.window.musicProgress.Value),
                                End = DateTime.UtcNow.AddMilliseconds(MainWindow.window.musicProgress.Maximum - MainWindow.window.musicProgress.Value)
                            },
                            Buttons = new DiscordRPC.Button[]
    {
                            new DiscordRPC.Button()
                            {
                                Label = "Послушать трек...",
                                Url = track.Url,
                            },
                            new DiscordRPC.Button()
                            {
                                Label = "Free Spotify...",
                                Url = githubLink
                            }
    }
                        });
                        MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Pause;
                        if (mediaPlayer.Source != null)
                        {
                            MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Normal;
                            if (mediaPlayer.NaturalDuration.HasTimeSpan)
                            {
                                MainWindow.window.progressSongTaskBar.ProgressValue = mediaPlayer.Position.TotalMilliseconds / track.DurationMs;
                            }
                        }
                        else
                        {
                            MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Error;
                        }
                        return;
                    }
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = "Поставил музыку на паузу...",
                        State = $"{track.Artists[0].Name} - {track.Title}",
                        Assets = new Assets()
                        {
                            LargeImageKey = "logo",
                            LargeImageText = "Free Spotify",
                        },
                        Timestamps = Timestamps.Now
                    });
                    IsSongPaused = true;
                    mediaPlayer.Pause();
                    countTimer.Stop();
                    MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Play;
                    if (mediaPlayer.Source != null)
                    {
                        MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Paused;
                        if (mediaPlayer.NaturalDuration.HasTimeSpan)
                        {
                            MainWindow.window.progressSongTaskBar.ProgressValue = mediaPlayer.Position.TotalMilliseconds / track.DurationMs;
                        }
                    }
                    else
                    {
                        MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Error;
                    }
                }
                else
                {
                    MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.None;
                    MainWindow.window.progressSongTaskBar.ProgressValue = 0;
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
                                            try
                                            {
                                                Border background = new Border();
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
                                                        if (mediaPlayer.Source != null)
                                                        {
                                                            if (IsSongRepeat)
                                                            {
                                                                RepeatSongBehavior();
                                                            }
                                                            StopSound();
                                                        }
                                                    });
                                                    await Task.Run(async () =>
                                                    {
                                                        try
                                                        {
                                                            this.track = track;
                                                            PlaySound();
                                                            await Dispatcher.BeginInvoke(() =>
                                                            {
                                                                MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Pause;
                                                                MainWindow.window.songTitle.Content = track.Title;
                                                                MainWindow.window.songAuthor.Content = track.Artists[0].Name;

                                                                MainWindow.window.favoriteSongButton.Visibility = Visibility.Visible;

                                                                BitmapImage sourceImageOfTrack = new BitmapImage();
                                                                sourceImageOfTrack.BeginInit();
                                                                sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                                                                sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                                                                sourceImageOfTrack.UriSource = new Uri(track.Album.Images[0].Url);
                                                                sourceImageOfTrack.EndInit();

                                                                MainWindow.window.iconTrack.Source = sourceImageOfTrack;

                                                                uint hourMillisecond = 3600000;
                                                                if (track.DurationMs > hourMillisecond)
                                                                {
                                                                    MainWindow.window.endOfSong.Content = $"{TimeSpan.FromMilliseconds(track.DurationMs).ToString(@"h\:m\:ss")}";
                                                                }
                                                                else
                                                                {
                                                                    MainWindow.window.endOfSong.Content = $"{TimeSpan.FromMilliseconds(track.DurationMs).ToString(@"m\:ss")}";
                                                                }
                                                            });
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
        /// Function that stops the sound in case of selecting new song.
        /// </summary>
        private async void StopSound()
        {
            try
            {
                MainWindow.window.discordClient.SetPresence(new RichPresence()
                {
                    Details = "Ничего не делает...",
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo",
                        LargeImageText = "Free Spotify",
                    },
                    Timestamps = Timestamps.Now
                });
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
                    countTimer.Stop();
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
                countTimer.Stop();
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
                string? youtubeID = await spotifyYouTubeRetrive.Tracks.GetYoutubeIdAsync(track.Url);
                YoutubeClient? youtube = new YoutubeClient();
                var video = youtube.Videos.GetAsync($"https://youtube.com/watch?v={youtubeID}");
                var streamManifest = youtube.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={youtubeID}");
                var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                await Dispatcher.BeginInvoke(() =>
                {
                    mediaPlayer.Open(new Uri(streamInfo.Url));
                    mediaPlayer.Play();
                    countTimer.Start();
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = "Слушает музыку...",
                        State = $"{track.Artists[0].Name} - {track.Title}",
                        Assets = new Assets()
                        {
                            LargeImageKey = "logo",
                            LargeImageText = "Free Spotify",
                        },
                        Timestamps = new Timestamps()
                        {
                            Start = Timestamps.Now.Start,
                            End = Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(track.DurationMs)).End
                        },
                        Buttons = new DiscordRPC.Button[]
                        {
                            new DiscordRPC.Button()
                            {
                                Label = "Послушать трек...",
                                Url = track.Url,
                            },
                            new DiscordRPC.Button()
                            {
                                Label = "Free Spotify...",
                                Url = githubLink
                            }
                        }
                    });
                });
            }
            catch (ArgumentException)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Error;
                    MainWindow.window.progressSongTaskBar.ProgressValue = 0;
                });
                StopSound();
                MessageBox.Show(@"К сожалению, эта песня не существует на YouTube чтобы её можно было проиграть. Поищите другую песню ¯\_(ツ)_/¯", "☹", MessageBoxButton.OK);
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
                MessageBox.Show($"ERROR: {ex.GetType()},\n Message: {ex.Message},\n Please screenshot this error, and send me in github!\nLINK: {githubLink}");
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