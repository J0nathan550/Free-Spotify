using SpotifyExplode;
using SpotifyExplode.Search;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Free_Spotify.Pages
{
    public partial class SearchViewPage : Page
    {

        public SearchViewPage()
        {
            InitializeComponent();
        }

        private bool isTextErased = false; // used to remove the tip

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
        /// An a event that is used in search bar to represent if you are lost focus, if you have no text inside, it will add the tip.
        /// </summary>
        private async void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
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
            await Dispatcher.InvokeAsync(async () =>
            {
                if (searchVisual.Children.Count != 0)
                {
                    searchVisual.Children.Clear();
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
                    List<ISearchResult> searchMatches = await spotifyClient.Search.GetResultsAsync(SearchBarTextBox.Text);
                    if (searchMatches.Count != 0)
                    {
                        foreach (var result in await spotifyClient.Search.GetResultsAsync(SearchBarTextBox.Text))
                        {
                            // Use pattern matching to handle different results (albums, artists, tracks, playlists)
                            switch (result)
                            {
                                case TrackSearchResult track:
                                    {
                                        var id = track.Id;
                                        var title = track.Title;
                                        var duration = track.DurationMs;
                                        searchVisual.Children.Clear();


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
                        await Dispatcher.InvokeAsync(() =>
                        {
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
                catch (HttpRequestException)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
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
                catch(ArgumentException) 
                {
                    // ignore for now, shows error that track does not exist to download.
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.GetType().Name);
                    MessageBox.Show(e.Message);
                }
            });
        }

        /// <summary>
        /// Button that removes everything from the textbox, nice little feature.
        /// </summary>
        private async void removeEverythingFromSearchBoxButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                SearchBarTextBox.Text = string.Empty;
                Keyboard.ClearFocus(); // removes focus from the textbox.
                searchVisual.Children.Clear();
                removeEverythingFromSearchBoxButton.Visibility = Visibility.Hidden;
            });
        }

        /*
          var youtube = new YoutubeClient();
                var video = youtube.Videos.GetAsync($"https://youtube.com/watch?v={YouTubeID}");
                var streamManifest = youtube.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={YouTubeID}");
                var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                using (WaveStream blockAlignedStream = new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new MediaFoundationReader(streamInfo.Url))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Volume = 0.05f;
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        songIsRunning = true;
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                        songIsRunning = false;
                        waveOut.Dispose();
                        youtube = null;
                        GC.Collect();
                    }
                }
        */
    }
}