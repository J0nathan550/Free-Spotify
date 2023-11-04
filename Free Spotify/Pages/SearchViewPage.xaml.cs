using Free_Spotify.Classes;
using Free_Spotify.Interfaces;
using SpotifyExplode;
using SpotifyExplode.Search;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace Free_Spotify.Pages
{
    public partial class SearchViewPage : Page, ISongManager
    {
        // singleton to do things in MainWindow script.
        private static SearchViewPage? searchWindow;

        // used to do query every second after you typed character
        private readonly System.Timers.Timer searchFieldTimer = new() { Interval = 1000, AutoReset = false, Enabled = false };

        // used to search something certain in Search Engine.
        private readonly SpotifyExplode.Search.SearchFilter filter = SpotifyExplode.Search.SearchFilter.Track;

        // used to remove default placeholder text in Search Tab.
        private bool isTextErased = false;

        // Lists with meta-data of tracks / videos, used to specify which Search Engine to use, Spotify or YouTube. 
        public List<TrackSearchResult> trackSpotifyList = new();
        public List<VideoSearchResult> trackYouTubeList = new();

        // handy value to determine the currentIndex of song that is playing.
        public int currentSongIndex = 0;

        // Buffer Lists
        private readonly List<TrackSearchResult> newSongsSpotify = new();
        private readonly List<VideoSearchResult> newSongsYouTube = new();

        public static SearchViewPage? SearchWindow { get => searchWindow; set => searchWindow = value; }

        /// <summary>
        /// Search View Page constructor.
        /// </summary>
        public SearchViewPage()
        {
            // Loading Settings to handle data. 
            Settings.LoadSettings();
            Settings.UpdateLanguage();

            // Loading entire page with functions and everything.
            InitializeComponent();

            searchFieldTimer.Elapsed += SearchFieldTimer_Elapsed;
            SearchBarTextBox.Text = Settings.GetLocalizationString("SearchBarTextBoxDefaultText");

            SearchWindow = this;
        }

        // Timer that start to search if for the second there was no key press. 
        private void SearchFieldTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            SearchingSystem();
        }

        /// <summary>
        /// An a event that is used in search bar to represent if you are lost focus, if you have no text inside, it will add the tip.
        /// </summary>
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBarTextBox.Text.Length == 0)
            {
                SearchBarTextBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x80, 0x80, 0x80));
                SearchBarTextBox.Text = Settings.GetLocalizationString("SearchBarTextBoxDefaultText");
                isTextErased = false;
            }
        }

        /// <summary>
        /// An event that tries to give the result of certain track.
        /// </summary>
        private void SearchBarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBarTextBox.Text.Length != 0)
            {
                removeEverythingFromSearchBoxButton.Visibility = Visibility.Visible;
                if (SearchBarTextBox.Text.Length > 2)
                {
                    searchFieldTimer.Start();
                }
            }
            else
            {
                removeEverythingFromSearchBoxButton.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// An a event that is used in search bar to represent if you are clicked it, if you have no text inside, it will remove the tip.
        /// </summary>
        private void SearchTextBox_Focus(object sender, RoutedEventArgs e)
        {
            if (!isTextErased)
            {
                SearchBarTextBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                SearchBarTextBox.Text = string.Empty;
                isTextErased = true;
            }
        }

        /// <summary>
        /// A system, that when you type returns you the possible songs that you can listen in spotify.
        /// </summary>
        private void SearchingSystem()
        {
            newSongsSpotify.Clear();
            newSongsYouTube.Clear();
            Dispatcher.BeginInvoke(async () =>
            {
                await Dispatcher.BeginInvoke(async () =>
                {
                    // Create Wrap panel
                    WrapPanel wrapPanelVisual = new();
                    wrapPanelVisual.Orientation = Orientation.Horizontal;

                    if (searchVisual.Children.Count != 0)
                    {
                        searchVisual.Children.Clear();
                    }
                    searchVisual.Children.Add(wrapPanelVisual);

                    if (!isTextErased)
                    {
                        return;
                    }

                    if (Settings.SettingsData.searchEngineIndex == 1) // YouTube
                    {
                        try
                        {
                            /// searching for the song or else.
                            int indexLoop = 0;
                            var youTubeClient = new YoutubeClient();

                            await foreach (var result in youTubeClient.Search.GetVideosAsync(SearchBarTextBox.Text))
                            {
                                if (indexLoop >= 50)
                                {
                                    break;
                                }
                                // Use pattern matching to handle different results (albums, artists, tracks, playlists)
                                switch (result)
                                {
                                    case VideoSearchResult video:
                                        {
                                            newSongsYouTube.Add(video);
                                            await Dispatcher.BeginInvoke(() =>
                                            {
                                                try
                                                {
                                                    Border background = new()
                                                    {
                                                        Name = $"i{indexLoop}",
                                                        CornerRadius = new CornerRadius(6),
                                                        Cursor = Cursors.Hand,
                                                        Background = new SolidColorBrush(Color.FromArgb(0x00, 0x21, 0x21, 0x21)),
                                                        Width = 256,
                                                        Height = 256,
                                                    };
                                                    RenderOptions.SetBitmapScalingMode(background, BitmapScalingMode.Fant);

                                                    Grid mainVisualGrid = new();
                                                    RenderOptions.SetBitmapScalingMode(mainVisualGrid, BitmapScalingMode.Fant);

                                                    background.Child = mainVisualGrid;

                                                    RowDefinition rowDefinition = new()
                                                    {
                                                        Height = new GridLength(3, GridUnitType.Star)
                                                    };
                                                    mainVisualGrid.RowDefinitions.Add(rowDefinition);

                                                    RowDefinition textDefinition = new()
                                                    {
                                                        Height = new GridLength(1, GridUnitType.Star)
                                                    };
                                                    mainVisualGrid.RowDefinitions.Add(textDefinition);

                                                    if (!Settings.SettingsData.economTraffic)
                                                    {
                                                        BitmapImage sourceImageOfTrack = new();
                                                        sourceImageOfTrack.BeginInit();
                                                        sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                                                        sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                                                        sourceImageOfTrack.UriSource = new Uri(video.Thumbnails[0].Url);
                                                        sourceImageOfTrack.EndInit();

                                                        Image actualImageOfTrack = new()
                                                        {
                                                            Source = sourceImageOfTrack,
                                                            HorizontalAlignment = HorizontalAlignment.Center,
                                                            VerticalAlignment = VerticalAlignment.Center,
                                                            //actualImageOfTrack.Stretch = Stretch.Fill; later option
                                                            CacheMode = null
                                                        };
                                                        RenderOptions.SetBitmapScalingMode(actualImageOfTrack, BitmapScalingMode.Fant);
                                                        mainVisualGrid.Children.Add(actualImageOfTrack);

                                                        Grid.SetZIndex(actualImageOfTrack, -2);

                                                        Grid.SetRow(actualImageOfTrack, 0);
                                                        Grid.SetRowSpan(actualImageOfTrack, 2);
                                                    }

                                                    Border captionBorder = new()
                                                    {
                                                        Background = new SolidColorBrush(Color.FromArgb(180, 0x00, 0x00, 0x00))
                                                    };
                                                    RenderOptions.SetBitmapScalingMode(captionBorder, BitmapScalingMode.Fant);

                                                    TextBlock DescriptionOfTrack = new()
                                                    {
                                                        Text = $"{Settings.GetLocalizationString("ArtistDefaultText")} {video.Author.ChannelTitle}\n" +
                                                    $"{Settings.GetLocalizationString("TrackDefaultText")} {video.Title}\n",
                                                        Foreground = new SolidColorBrush(Colors.White)
                                                    };
                                                    RenderOptions.SetBitmapScalingMode(DescriptionOfTrack, BitmapScalingMode.Fant);
                                                    DescriptionOfTrack.Style = (Style)DescriptionOfTrack.FindResource("fontMontserrat");
                                                    DescriptionOfTrack.FontSize = 14;
                                                    DescriptionOfTrack.TextWrapping = TextWrapping.WrapWithOverflow;
                                                    DescriptionOfTrack.VerticalAlignment = VerticalAlignment.Center;
                                                    DescriptionOfTrack.HorizontalAlignment = HorizontalAlignment.Center;
                                                    DescriptionOfTrack.TextTrimming = TextTrimming.CharacterEllipsis;
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
                                                            if (MusicPlayerPage.Instance != null)
                                                            {
                                                                MusicPlayerPage.Instance.favoriteSongButton.Visibility = Visibility.Visible;
                                                                try
                                                                {
                                                                    if (MusicPlayerPage.Instance.MusicMediaPlayer.Source != null)
                                                                    {
                                                                        if (MusicPlayerPage.Instance.IsSongRepeat)
                                                                        {
                                                                            MusicPlayerPage.Instance.RepeatSongBehavior();
                                                                        }
                                                                        MusicPlayerPage.Instance.StopSound();
                                                                    }
                                                                }
                                                                catch
                                                                {
                                                                    MusicPlayerPage.Instance?.StopSound();
                                                                }
                                                            }
                                                        });
                                                        await Task.Run(async () =>
                                                        {
                                                            try
                                                            {
                                                                if (MusicPlayerPage.Instance != null)
                                                                {
                                                                    trackYouTubeList.Clear();
                                                                    trackYouTubeList.AddRange(newSongsYouTube);
                                                                    int symbolToRemove = 1;
                                                                    await Dispatcher.BeginInvoke(() =>
                                                                    {
                                                                        currentSongIndex = int.Parse(background.Name[symbolToRemove..]);
                                                                    });

                                                                    Utils.IsPlayingFromPlaylist = false;

                                                                    MusicPlayerPage.Instance.PlaySound(trackYouTubeList[currentSongIndex], null, this);
                                                                    MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                                                                }
                                                            }
                                                            catch
                                                            {
                                                                NextSong();
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
                                                
                                                searchVisual.Children.Clear();
                                                TextBlock resultTextBlock = new()
                                                {
                                                    Text = $"{Settings.GetLocalizationString("ErrorNothingFoundText")} \"{SearchBarTextBox.Text}\" {Settings.GetLocalizationString("ErrorNothingFoundText1")}",
                                                    FontSize = 14,
                                                    HorizontalAlignment = HorizontalAlignment.Center,
                                                    VerticalAlignment = VerticalAlignment.Center,
                                                    Foreground = new SolidColorBrush(Colors.White),
                                                    TextWrapping = TextWrapping.Wrap,
                                                    TextTrimming = TextTrimming.CharacterEllipsis,
                                                    Style = (Style)FindResource("fontMontserrat")
                                                };
                                                searchVisual.Children.Add(resultTextBlock);
                                            });
                                            break;
                                        }
                                }
                                indexLoop++;
                            }
                        }
                        catch (HttpRequestException request)
                        {
                            if (request.Message.Contains("400"))
                            {
                                await Dispatcher.BeginInvoke(() =>
                                {
                                    
                                    searchVisual.Children.Clear();
                                    TextBlock resultTextBlock = new()
                                    {
                                        Text = Settings.GetLocalizationString("TipStartWrittingText"),
                                        FontSize = 14,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Foreground = new SolidColorBrush(Colors.White),
                                        TextWrapping = TextWrapping.Wrap,
                                        TextTrimming = TextTrimming.CharacterEllipsis,
                                        Style = (Style)FindResource("fontMontserrat")
                                    };
                                    searchVisual.Children.Add(resultTextBlock);
                                });
                            }
                            else
                            {
                                await Dispatcher.BeginInvoke(() =>
                                {
                                    
                                    searchVisual.Children.Clear();
                                    TextBlock resultTextBlock = new()
                                    {
                                        Text = Settings.GetLocalizationString("ErrorNoInternetText"),
                                        FontSize = 14,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Foreground = new SolidColorBrush(Colors.White),
                                        TextWrapping = TextWrapping.Wrap,
                                        TextTrimming = TextTrimming.CharacterEllipsis,
                                        Style = (Style)FindResource("fontMontserrat")
                                    };
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
                        catch
                        {
                            NextSong();
                        }
                    }
                    else
                    {
                        try
                        {
                            /// searching for the song or else.
                            int indexLoop = 0;
                            var spotifyClient = new SpotifyClient();
                            List<SpotifyExplode.Search.ISearchResult> searchMatches = await spotifyClient.Search.GetResultsAsync(SearchBarTextBox.Text, filter);

                            if (searchMatches.Count != 0)
                            {
                                foreach (var result in searchMatches)
                                {
                                    // Use pattern matching to handle different results (albums, artists, tracks, playlists)
                                    switch (result)
                                    {
                                        case TrackSearchResult track:
                                            {
                                                newSongsSpotify.Add(track);
                                                await Dispatcher.BeginInvoke(() =>
                                                {
                                                    try
                                                    {
                                                        Border background = new()
                                                        {
                                                            Name = $"i{indexLoop}",
                                                            CornerRadius = new CornerRadius(6),
                                                            Cursor = Cursors.Hand,
                                                            Background = new SolidColorBrush(Color.FromArgb(0x00, 0x21, 0x21, 0x21)),
                                                            Width = 256,
                                                            Height = 256
                                                        };
                                                        RenderOptions.SetBitmapScalingMode(background, BitmapScalingMode.Fant);

                                                        Grid mainVisualGrid = new();
                                                        RenderOptions.SetBitmapScalingMode(mainVisualGrid, BitmapScalingMode.Fant);

                                                        background.Child = mainVisualGrid;

                                                        RowDefinition rowDefinition = new()
                                                        {
                                                            Height = new GridLength(3, GridUnitType.Star)
                                                        };
                                                        mainVisualGrid.RowDefinitions.Add(rowDefinition);

                                                        RowDefinition textDefinition = new()
                                                        {
                                                            Height = new GridLength(1, GridUnitType.Star)
                                                        };
                                                        mainVisualGrid.RowDefinitions.Add(textDefinition);

                                                        if (!Settings.SettingsData.economTraffic)
                                                        {
                                                            BitmapImage sourceImageOfTrack = new();
                                                            sourceImageOfTrack.BeginInit();
                                                            sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                                                            sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                                                            sourceImageOfTrack.UriSource = new Uri(track.Album.Images[0].Url);
                                                            sourceImageOfTrack.EndInit();

                                                            Image actualImageOfTrack = new()
                                                            {
                                                                Source = sourceImageOfTrack,
                                                                HorizontalAlignment = HorizontalAlignment.Center,
                                                                VerticalAlignment = VerticalAlignment.Center,
                                                                //actualImageOfTrack.Stretch = Stretch.Fill; later option
                                                                CacheMode = null
                                                            };
                                                            RenderOptions.SetBitmapScalingMode(actualImageOfTrack, BitmapScalingMode.Fant);
                                                            mainVisualGrid.Children.Add(actualImageOfTrack);

                                                            Grid.SetZIndex(actualImageOfTrack, -2);

                                                            Grid.SetRow(actualImageOfTrack, 0);
                                                            Grid.SetRowSpan(actualImageOfTrack, 2);
                                                        }

                                                        Border captionBorder = new()
                                                        {
                                                            Background = new SolidColorBrush(Color.FromArgb(180, 0x00, 0x00, 0x00))
                                                        };
                                                        RenderOptions.SetBitmapScalingMode(captionBorder, BitmapScalingMode.Fant);

                                                        TextBlock DescriptionOfTrack = new()
                                                        {
                                                            Text = $"{Settings.GetLocalizationString("ArtistDefaultText")} {track.Artists[0].Name}\n" +
                                                        $"{Settings.GetLocalizationString("TrackDefaultText")} {track.Title}\n",
                                                            Foreground = new SolidColorBrush(Colors.White)
                                                        };
                                                        DescriptionOfTrack.Style = (Style)DescriptionOfTrack.FindResource("fontMontserrat");
                                                        DescriptionOfTrack.FontSize = 14;
                                                        DescriptionOfTrack.TextWrapping = TextWrapping.WrapWithOverflow;
                                                        DescriptionOfTrack.VerticalAlignment = VerticalAlignment.Center;
                                                        DescriptionOfTrack.HorizontalAlignment = HorizontalAlignment.Center;
                                                        DescriptionOfTrack.TextTrimming = TextTrimming.CharacterEllipsis;
                                                        DescriptionOfTrack.Padding = new Thickness(0, 10, 0, 0);
                                                        RenderOptions.SetBitmapScalingMode(DescriptionOfTrack, BitmapScalingMode.Fant);

                                                        Grid.SetZIndex(captionBorder, -1);
                                                        Grid.SetZIndex(DescriptionOfTrack, 1);

                                                        mainVisualGrid.Children.Add(captionBorder);
                                                        mainVisualGrid.Children.Add(DescriptionOfTrack);
                                                        Grid.SetRow(captionBorder, 1);
                                                        Grid.SetRow(DescriptionOfTrack, 1);
                                                        wrapPanelVisual.Children.Add(background);

                                                        background.MouseDown += new MouseButtonEventHandler(async (o, i) =>
                                                        {
                                                            if (MusicPlayerPage.Instance != null)
                                                            {
                                                                MusicPlayerPage.Instance.favoriteSongButton.Visibility = Visibility.Visible;
                                                                await Dispatcher.BeginInvoke(() =>
                                                                {
                                                                    try
                                                                    {
                                                                        if (MusicPlayerPage.Instance.MusicMediaPlayer.Source != null)
                                                                        {
                                                                            if (MusicPlayerPage.Instance.IsSongRepeat)
                                                                            {
                                                                                MusicPlayerPage.Instance.RepeatSongBehavior();
                                                                            }
                                                                            MusicPlayerPage.Instance.StopSound();
                                                                        }
                                                                    }
                                                                    catch
                                                                    {
                                                                        MusicPlayerPage.Instance.StopSound();
                                                                    }
                                                                });
                                                            }
                                                            await Task.Run(async () =>
                                                            {
                                                                try
                                                                {
                                                                    if (MusicPlayerPage.Instance != null)
                                                                    {
                                                                        trackSpotifyList.Clear();
                                                                        trackSpotifyList.AddRange(newSongsSpotify);
                                                                        int symbolToRemove = 1;
                                                                        await Dispatcher.BeginInvoke(() =>
                                                                        {
                                                                            currentSongIndex = int.Parse(background.Name[symbolToRemove..]);

                                                                            Utils.IsPlayingFromPlaylist = false;

                                                                            MusicPlayerPage.Instance.PlaySound(trackSpotifyList[currentSongIndex], this);
                                                                            MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                                                                        });
                                                                    }
                                                                }
                                                                catch
                                                                {
                                                                    MusicPlayerPage.Instance?.ClearMusic();
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
                                                    
                                                    searchVisual.Children.Clear();
                                                    TextBlock resultTextBlock = new()
                                                    {
                                                        Text = $"{Settings.GetLocalizationString("ErrorNothingFoundText")} \"{SearchBarTextBox.Text}\" {Settings.GetLocalizationString("ErrorNothingFoundText1")}",
                                                        FontSize = 14,
                                                        HorizontalAlignment = HorizontalAlignment.Center,
                                                        VerticalAlignment = VerticalAlignment.Center,
                                                        Foreground = new SolidColorBrush(Colors.White),
                                                        TextWrapping = TextWrapping.Wrap,
                                                        TextTrimming = TextTrimming.CharacterEllipsis,
                                                        Style = (Style)FindResource("fontMontserrat")
                                                    };
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
                                    
                                    searchVisual.Children.Clear();
                                    TextBlock resultTextBlock = new()
                                    {
                                        Text = $"{Settings.GetLocalizationString("ErrorNothingFoundText")} \"{SearchBarTextBox.Text}\" {Settings.GetLocalizationString("ErrorNothingFoundText1")}",
                                        FontSize = 14,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Foreground = new SolidColorBrush(Colors.White),
                                        TextWrapping = TextWrapping.Wrap,
                                        TextTrimming = TextTrimming.CharacterEllipsis,
                                        Style = (Style)FindResource("fontMontserrat")
                                    };
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
                                    
                                    searchVisual.Children.Clear();
                                    TextBlock resultTextBlock = new()
                                    {
                                        Text = Settings.GetLocalizationString("TipStartWrittingText"),
                                        FontSize = 14,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Foreground = new SolidColorBrush(Colors.White),
                                        TextWrapping = TextWrapping.Wrap,
                                        TextTrimming = TextTrimming.CharacterEllipsis,
                                        Style = (Style)FindResource("fontMontserrat")
                                    };
                                    searchVisual.Children.Add(resultTextBlock);
                                });
                            }
                            else
                            {
                                await Dispatcher.BeginInvoke(() =>
                                {
                                    
                                    searchVisual.Children.Clear();
                                    TextBlock resultTextBlock = new()
                                    {
                                        Text = Settings.GetLocalizationString("ErrorNoInternetText"),
                                        FontSize = 14,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Foreground = new SolidColorBrush(Colors.White),
                                        TextWrapping = TextWrapping.Wrap,
                                        TextTrimming = TextTrimming.CharacterEllipsis,
                                        Style = (Style)FindResource("fontMontserrat")
                                    };
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
                        catch
                        {
                        }
                    }
                });
            });

        }



        /// <summary>
        /// Button that removes everything from the textbox, nice little feature.
        /// </summary>
        private void RemoveEverythingFromSearchBoxButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchBarTextBox.Text = string.Empty;
            Keyboard.ClearFocus(); // removes focus from the textbox.
            searchVisual.Children.Clear();
            removeEverythingFromSearchBoxButton.Visibility = Visibility.Hidden;
            newSongsSpotify.Clear();
            newSongsYouTube.Clear();
        }

        /// <summary>
        /// Advances to the previous song in the playlist and updates the player's status bar
        /// </summary>
        public void PreviousSong()
        {
            try
            {
                if (MusicPlayerPage.Instance == null)
                {
                    return;
                }
                currentSongIndex--;

                // Check the search engine index and update the current song accordingly
                if (Settings.SettingsData.searchEngineIndex == 1)
                {
                    if (currentSongIndex < 0)
                    {
                        currentSongIndex = trackYouTubeList.Count - 1;
                    }
                    MusicPlayerPage.Instance.PlaySound(trackYouTubeList[currentSongIndex], null, this);
                    MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                }
                else
                {
                    if (currentSongIndex < 0)
                    {
                        currentSongIndex = trackSpotifyList.Count - 1;
                    }
                    MusicPlayerPage.Instance.PlaySound(trackSpotifyList[currentSongIndex], this);
                    MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                }
                newSongsSpotify.Clear();
                newSongsYouTube.Clear();
            }
            catch
            {
                MusicPlayerPage.Instance?.StopSound();
            }
        }

        /// <summary>
        /// Advances to the next song in the playlist and updates the player's status bar
        /// </summary>
        public void NextSong()
        {
            try
            {
                if (MusicPlayerPage.Instance == null)
                {
                    return;
                }
                currentSongIndex++;

                // Check the search engine index and update the current song accordingly
                if (Settings.SettingsData.searchEngineIndex == 1)
                {
                    if (trackYouTubeList.Count <= currentSongIndex)
                    {
                        currentSongIndex = 0;
                    }
                    MusicPlayerPage.Instance.PlaySound(trackYouTubeList[currentSongIndex], null, this);
                    MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                }
                else
                {
                    if (trackSpotifyList.Count <= currentSongIndex)
                    {
                        currentSongIndex = 0;
                    }
                    MusicPlayerPage.Instance.PlaySound(trackSpotifyList[currentSongIndex], this);
                    MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                }
                newSongsSpotify.Clear();
                newSongsYouTube.Clear();
            }
            catch
            {
                MusicPlayerPage.Instance?.StopSound();
            }
        }

        /// <summary>
        /// Shuffles the songs in the playlist and updates the player's status bar
        /// </summary>
        public void ShuffleSongs()
        {
            try
            {
                if (MusicPlayerPage.Instance == null)
                {
                    return;
                }

                // Create a random number generator
                Random rand = new();

                // Check the search engine index and shuffle the current song accordingly
                if (Settings.SettingsData.searchEngineIndex == 1)
                {
                    int pos = rand.Next(0, trackYouTubeList.Count);
                    int times = 1000;
                    while (pos == currentSongIndex && times > 0)
                    {
                        pos = rand.Next(0, trackYouTubeList.Count);
                        times--;
                    }
                    currentSongIndex = pos;
                    MusicPlayerPage.Instance.PlaySound(trackYouTubeList[currentSongIndex], null, this);
                    MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                }
                else
                {
                    int pos = rand.Next(0, trackSpotifyList.Count);
                    int times = 1000;
                    while (pos == currentSongIndex && times > 0)
                    {
                        pos = rand.Next(0, trackSpotifyList.Count);
                        times--;
                    }
                    currentSongIndex = pos;
                    MusicPlayerPage.Instance.PlaySound(trackSpotifyList[currentSongIndex], this);
                    MusicPlayerPage.Instance.UpdateStatusPlayerBar();
                }
                newSongsSpotify.Clear();
                newSongsYouTube.Clear();
            }
            catch
            {
                MusicPlayerPage.Instance?.StopSound();
            }
        }

        public void ClearSearchView()
        {
            newSongsSpotify.Clear();
            newSongsYouTube.Clear();
            trackSpotifyList.Clear();
            trackYouTubeList.Clear();
            Content = null;
            SearchWindow = null;
        }
    }
}