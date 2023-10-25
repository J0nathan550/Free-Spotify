using FontAwesome.WPF;
using Free_Spotify.Ballons;
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
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace Free_Spotify.Pages
{
    public partial class SearchViewPage : Page
    {
        // singleton to do things in MainWindow script.
        public static SearchViewPage searchWindow;

        // used to render every single millisecond the progress bar of player.
        public System.Timers.Timer progressSongTimer = new System.Timers.Timer() { Interval = 1000 };

        // used to do query every second after you typed character
        private System.Timers.Timer searchFieldTimer = new System.Timers.Timer() { Interval = 1000 };

        // canceling the time of timer progress, pretty useless since it used only to cancel it when app is closing (crushes in VS when you close app)
        public CancellationTokenSource cancelProgressSongTimer = new CancellationTokenSource();

        // Lists with meta-data of tracks / videos, used to specify which Search Engine to use, Spotify or YouTube. 
        private List<TrackSearchResult> trackSpotifyList = new();
        private List<VideoSearchResult> trackYouTubeList = new();

        // handy value to determine the currentIndex of song that is playing.
        private int currentSongIndex = 0;

        // used to search something certain in Search Engine.
        private SpotifyExplode.Search.SearchFilter filter = SpotifyExplode.Search.SearchFilter.Track;

        // used to remove default placeholder text in Search Tab.
        private bool isTextErased = false;

        // Old but really good working MediaPlayer, prevents stacking, has awesome quality, MediaPlayer > NAudio.
        public MediaPlayer mediaPlayer { get; private set; } = new();

        // boolean to represent if we want to repeat the same song.
        private bool IsSongRepeat = false;

        // boolean to represent if we are paused the song.
        private bool IsSongPaused = false;

        // boolean to represent if we want to listen to random songs.
        private bool isShuffleEnabled = false;

        public MusicExplorerBallon ballon;

        public SearchViewPage()
        {
            // Loading Settings to handle data. 
            Utils.LoadSettings();
            Utils.UpdateLanguage();

            // Loading entire page with functions and everything.
            InitializeComponent();
            searchWindow = this;

            SearchBarTextBox.Text = Utils.GetLocalizationString("SearchBarTextBoxDefaultText");
            mediaPlayer.Volume = Utils.settings.volume;
            MainWindow.window.volumeSlider.Value = mediaPlayer.Volume;

            // Event that represents if MediaPlayer reach the end of song. 
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            // Timer that optimizes little bit search system (I can't say this is optimizing, but when you are typing it's good)
            searchFieldTimer.Elapsed += SearchFieldTimer_Elapsed;

            // Event that switches if we want (at the end of the song) turn on random one.
            MainWindow.window.randomSongButton.MouseDown += RandomSongButton_MouseDown;

            // Timer that is updating UI progress of song. 
            progressSongTimer.Elapsed += ProgressSongTimer_Elapsed;

            // pause button, switches the sprite of playing (pausing) song.
            MainWindow.window.musicToggle.MouseDown += MusicToggle_MouseDown;

            // Handler that if you press on thumb of the slider, it will change the position of the song. Also pauses the song to prevent sound buffer to break.
            MainWindow.window.musicProgress.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((sender, e) =>
            {
                MusicProgress_DragDeltaProgress();
            }));

            // unpauses the song and renders once again the player when you release the thumb button.
            MainWindow.window.musicProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, e) =>
            {
                MusicProgress_DragCompletedEvent();
            }));

            MainWindow.window.musicProgress.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(Slider_MouseLeftButtonDownEvent), true);

            // volume slider, manages the volume of all songs. Later will be stored in saving system.
            MainWindow.window.volumeSlider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((sender, e) =>
            {
                VolumeSlider_DragDeltaEvent();
            }));

            MainWindow.window.volumeSlider.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(Slider_MouseLeftButtonDownEvent), true);

            // mute song and do not change volume value on click
            MainWindow.window.volumeIcon.MouseDown += volumeIcon_Click;

            // repeat song button, repeats the song.
            MainWindow.window.repeatSong.MouseDown += RepeatSong_MouseDown;

            // right button switches to the next music
            MainWindow.window.rightSong.MouseDown += RightSong_MouseDown;

            // left button switches to the previous music
            MainWindow.window.leftSong.MouseDown += LeftSong_MouseDown;
        }

        // left button switches to the previous music
        private async void LeftSong_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int tempCurSongIndex = currentSongIndex;
            currentSongIndex--;
            if (Utils.settings.searchEngineIndex == 1)
            {
                if (trackYouTubeList.Count == 0)
                {
                    currentSongIndex = tempCurSongIndex;
                    return;
                }
                if (currentSongIndex < 0) currentSongIndex = trackYouTubeList.Count - 1;
                Utils.ContinueDiscordPresence(trackYouTubeList[currentSongIndex]);
            }
            else
            {
                if (trackSpotifyList.Count == 0)
                {
                    currentSongIndex = tempCurSongIndex;
                    return;
                }
                if (currentSongIndex < 0) currentSongIndex = trackSpotifyList.Count - 1;
                Utils.ContinueDiscordPresence(trackSpotifyList[currentSongIndex]);
            }
            await Task.Run(() =>
            {
                PlaySound();
                UpdateStatusPlayerBar();
            });
        }

        private void MusicProgress_DragCompletedEvent()
        {
            try
            {
                if (mediaPlayer.Source != null)
                {
                    if (Utils.settings.searchEngineIndex == 1)
                    {
                        Utils.ContinueDiscordPresence(trackYouTubeList[currentSongIndex]);
                    }
                    else
                    {
                        Utils.ContinueDiscordPresence(trackSpotifyList[currentSongIndex]);
                    }

                    if (mediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        if (Utils.settings.musicPlayerBallonTurnOn && ballon != null && MainWindow.window.WindowState == WindowState.Minimized)
                        {
                            mediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)ballon.musicProgress.Value);
                            PausingSong();
                            return;
                        }
                        mediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)MainWindow.window.musicProgress.Value);
                    }
                }
                PausingSong();
            }
            catch
            {
                StopSound();
            }
        }

        private async void MusicProgress_DragDeltaProgress()
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
                    if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        string format = mediaPlayer.Position.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss";
                        if (Utils.settings.musicPlayerBallonTurnOn && ballon != null && MainWindow.window.WindowState == WindowState.Minimized)
                        {
                            ballon.startOfSong.Content = new TimeSpan(0, 0, 0, 0, (int)ballon.musicProgress.Value).ToString(format);
                            return;
                        }
                        MainWindow.window.startOfSong.Content = new TimeSpan(0, 0, 0, 0, (int)MainWindow.window.musicProgress.Value).ToString(format);
                    }
                }
                catch
                {
                    StopSound();
                }
            });
        }

        private async void VolumeSlider_DragDeltaEvent()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                mediaPlayer.Volume = (float)MainWindow.window.volumeSlider.Value;
                Utils.settings.volume = mediaPlayer.Volume;
            });
        }

        // generic slider event that simulates one button click and drag operations on a Slider's thumb if IsMoveToPointEnabled is true
        private void Slider_MouseLeftButtonDownEvent(object sender, MouseButtonEventArgs e)
        {
            var slider = (Slider)sender;
            Track track = slider.Template.FindName("PART_Track", slider) as Track;
            if (!slider.IsMoveToPointEnabled || track == null || track.Thumb == null || track.Thumb.IsMouseOver)
            {
                return;
            }
            track.Thumb.UpdateLayout();

            track.Thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = MouseLeftButtonDownEvent,
                Source = track.Thumb
            });
            track.Thumb.RaiseEvent(new DragDeltaEventArgs(0, 0)
            {
                RoutedEvent = Thumb.DragDeltaEvent,
                Source = track.Thumb
            });
        }

        // mute|unmute mediaplayer and change icon when user click on the volume icon
        private void volumeIcon_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.IsMuted)
            {
                MainWindow.window.volumeIcon.Icon = FontAwesomeIcon.VolumeUp;
                mediaPlayer.IsMuted = false;
            }
            else
            {
                MainWindow.window.volumeIcon.Icon = FontAwesomeIcon.VolumeOff;
                mediaPlayer.IsMuted = true;
            }
        }


        // right button switches to the next music
        private async void RightSong_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int tempCurSongIndex = currentSongIndex;
            currentSongIndex++;
            await Task.Run(async () =>
            {
                if (Utils.settings.searchEngineIndex == 1)
                {
                    if (trackYouTubeList.Count == 0)
                    {
                        currentSongIndex = tempCurSongIndex;
                        return;
                    }
                    if (currentSongIndex >= trackYouTubeList.Count) currentSongIndex = 0;
                    Utils.ContinueDiscordPresence(trackYouTubeList[currentSongIndex]);
                }
                else
                {
                    if (trackSpotifyList.Count == 0)
                    {
                        currentSongIndex = tempCurSongIndex;
                        return;
                    }
                    if (currentSongIndex >= trackSpotifyList.Count) currentSongIndex = 0;
                    Utils.ContinueDiscordPresence(trackSpotifyList[currentSongIndex]);
                }
                await Task.Run(() =>
                {
                    PlaySound();
                    UpdateStatusPlayerBar();
                });
            });
        }

        // repeat song button, repeats the song.
        private void RepeatSong_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RepeatSongBehavior();
        }

        // A 'Music Toggle' literally means pause button. 
        private void MusicToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PausingSong();
        }

        // Timer that updates UI every single second. 
        private async void ProgressSongTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
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
                            if (Utils.settings.searchEngineIndex == 1)
                            {
                                Utils.ContinueDiscordPresence(trackYouTubeList[currentSongIndex]);
                            }
                            else
                            {
                                Utils.ContinueDiscordPresence(trackSpotifyList[currentSongIndex]);
                            }

                            MainWindow.window.musicProgress.Value = mediaPlayer.Position.TotalMilliseconds;
                            if (ballon != null && Utils.settings.musicPlayerBallonTurnOn) ballon.musicProgress.Value = MainWindow.window.musicProgress.Value;

                            if (Utils.settings.searchEngineIndex == 1)
                            {
                                MainWindow.window.musicProgress.Maximum = trackYouTubeList[currentSongIndex].Duration.Value.TotalMilliseconds;
                                if (ballon != null && Utils.settings.musicPlayerBallonTurnOn) ballon.musicProgress.Maximum = trackYouTubeList[currentSongIndex].Duration.Value.TotalMilliseconds;
                            }
                            else
                            {
                                MainWindow.window.musicProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                                if (ballon != null && Utils.settings.musicPlayerBallonTurnOn) ballon.musicProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                            }

                            MainWindow.window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Normal;
                            MainWindow.window.progressSongTaskBar.ProgressValue = mediaPlayer.Position.TotalMilliseconds / mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;

                            if (mediaPlayer.Position.Hours > 0)
                            {
                                MainWindow.window.startOfSong.Content = mediaPlayer.Position.ToString(@"h\:mm\:ss");
                                if (ballon != null && Utils.settings.musicPlayerBallonTurnOn) ballon.startOfSong.Content = mediaPlayer.Position.ToString(@"h\:mm\:ss");
                            }
                            else
                            {
                                MainWindow.window.startOfSong.Content = mediaPlayer.Position.ToString(@"m\:ss");
                                if (ballon != null && Utils.settings.musicPlayerBallonTurnOn) ballon.startOfSong.Content = mediaPlayer.Position.ToString(@"m\:ss");
                            }
                        }
                    }
                }
                catch
                {
                    StopSound();
                }
            });
        }

        // Shuffling, at the end of the song it picks random music. 
        private void RandomSongButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isShuffleEnabled = !isShuffleEnabled;
            if (isShuffleEnabled)
            {
                MainWindow.window.randomSongButton.Foreground = new SolidColorBrush(Colors.Lime);
                if (Utils.settings.musicPlayerBallonTurnOn && ballon != null) ballon.randomSongButton.Foreground = new SolidColorBrush(Colors.Lime);
            }
            else
            {
                MainWindow.window.randomSongButton.Foreground = new SolidColorBrush(Colors.White);
                if (Utils.settings.musicPlayerBallonTurnOn && ballon != null) ballon.randomSongButton.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        // Timer that start to search if for the second there was no key press. 
        private void SearchFieldTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            searchFieldTimer.Stop();
            SearchingSystem();
        }

        /// <summary>
        /// An event that if the mediaplayer reaches the end of the song it will fire this event. Can repeat the song, or stop it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            if (Utils.settings.searchEngineIndex == 1)
            {
                Utils.ContinueDiscordPresence(trackYouTubeList[currentSongIndex]);
            }
            else
            {
                Utils.ContinueDiscordPresence(trackSpotifyList[currentSongIndex]);
            }
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
                        if (isShuffleEnabled)
                        {
                            if (Utils.settings.searchEngineIndex == 1)
                            {
                                Random rand = new Random();
                                currentSongIndex = rand.Next(0, trackYouTubeList.Count - 1);
                                await Task.Run(() =>
                                {
                                    PlaySound();
                                    UpdateStatusPlayerBar();
                                });
                            }
                            else
                            {
                                Random rand = new Random();
                                currentSongIndex = rand.Next(0, trackSpotifyList.Count - 1);
                                await Task.Run(() =>
                                {
                                    PlaySound();
                                    UpdateStatusPlayerBar();
                                });
                            }
                            return;
                        }
                        await Task.Run(() =>
                        {
                            if (Utils.settings.searchEngineIndex == 1)
                            {
                                currentSongIndex++;
                                if (currentSongIndex >= trackYouTubeList.Count) currentSongIndex = 0;
                                PlaySound();
                                UpdateStatusPlayerBar();
                            }
                            else
                            {
                                currentSongIndex++;
                                if (currentSongIndex >= trackSpotifyList.Count) currentSongIndex = 0;
                                PlaySound();
                                UpdateStatusPlayerBar();
                            }

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
                    if (Utils.settings.musicPlayerBallonTurnOn && ballon != null) ballon.repeatSong.Foreground = new SolidColorBrush(Colors.Lime);
                });
            }
            else
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    MainWindow.window.repeatSong.Foreground = new SolidColorBrush(Colors.White);
                    if (Utils.settings.musicPlayerBallonTurnOn && ballon != null) ballon.repeatSong.Foreground = new SolidColorBrush(Colors.White);
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
                                    if (Utils.settings.searchEngineIndex == 1)
                                    {
                                        Utils.ContinueDiscordPresence(trackYouTubeList[currentSongIndex]);
                                    }
                                    else
                                    {
                                        Utils.ContinueDiscordPresence(trackSpotifyList[currentSongIndex]);
                                    }
                                    MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Pause;
                                    if (Utils.settings.musicPlayerBallonTurnOn && ballon != null) ballon.musicToggle.Icon = FontAwesomeIcon.Pause;
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
                                if (Utils.settings.searchEngineIndex == 1)
                                {
                                    Utils.PauseDiscordPresence(trackYouTubeList[currentSongIndex]);
                                }
                                else
                                {
                                    Utils.PauseDiscordPresence(trackSpotifyList[currentSongIndex]);
                                }
                                IsSongPaused = true;
                                mediaPlayer.Pause();
                                progressSongTimer.Stop();
                                MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Play;
                                if (Utils.settings.musicPlayerBallonTurnOn && ballon != null) ballon.musicToggle.Icon = FontAwesomeIcon.Play;
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
                    SearchBarTextBox.Text = Utils.GetLocalizationString("SearchBarTextBoxDefaultText");
                    isTextErased = false;
                }
            });
        }

        /// <summary>
        /// An event that tries to give the result of certain track.
        /// </summary>
        private void SearchBarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchFieldTimer.Stop();
            searchFieldTimer.Start();
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
                if (SearchBarTextBox.Text.Length < 3)
                {
                    return;
                }
                WrapPanel wrapPanelVisual = new WrapPanel();
                try
                {
                    wrapPanelVisual.Orientation = Orientation.Horizontal;

                    if (searchVisual.Children.Count != 0)
                    {
                        GC.Collect();
                        searchVisual.Children.Clear();
                    }
                    searchVisual.Children.Add(wrapPanelVisual);

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
                if (Utils.settings.searchEngineIndex == 1) // YouTube
                {
                    try
                    {
                        /// searching for the song or else.
                        int indexLoop = 0;
                        var youTubeClient = new YoutubeClient();
                        List<VideoSearchResult> newSongs = new();

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
                                        newSongs.Add(video);
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

                                                if (!Utils.settings.economTraffic)
                                                {
                                                    BitmapImage sourceImageOfTrack = new BitmapImage();
                                                    sourceImageOfTrack.BeginInit();
                                                    sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                                                    sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                                                    sourceImageOfTrack.UriSource = new Uri(video.Thumbnails[1].Url);
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
                                                }

                                                Border captionBorder = new Border();
                                                captionBorder.Background = new SolidColorBrush(Color.FromArgb(180, 0x00, 0x00, 0x00));

                                                TextBlock DescriptionOfTrack = new TextBlock();
                                                DescriptionOfTrack.Text = $"{Utils.GetLocalizationString("ArtistDefaultText")} {video.Author.ChannelTitle}\n" +
                                                $"{Utils.GetLocalizationString("TrackDefaultText")} {video.Title}\n";
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

                                                            trackYouTubeList.Clear();
                                                            trackYouTubeList.AddRange(newSongs);
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
                                            resultTextBlock.Text = $"{Utils.GetLocalizationString("ErrorNothingFoundText")} \"{SearchBarTextBox.Text}\" {Utils.GetLocalizationString("ErrorNothingFoundText1")}";
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
                    catch (HttpRequestException request)
                    {
                        if (request.Message.Contains("400"))
                        {
                            await Dispatcher.BeginInvoke(() =>
                            {
                                GC.Collect();
                                searchVisual.Children.Clear();
                                TextBlock resultTextBlock = new TextBlock();
                                resultTextBlock.Text = Utils.GetLocalizationString("TipStartWrittingText");
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
                                resultTextBlock.Text = Utils.GetLocalizationString("ErrorNoInternetText");
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
                }
                else
                {
                    try
                    {
                        /// searching for the song or else.
                        int indexLoop = 0;
                        var spotifyClient = new SpotifyClient();
                        List<SpotifyExplode.Search.ISearchResult> searchMatches = await spotifyClient.Search.GetResultsAsync(SearchBarTextBox.Text, filter);
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

                                                    if (!Utils.settings.economTraffic)
                                                    {
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
                                                    }

                                                    Border captionBorder = new Border();
                                                    captionBorder.Background = new SolidColorBrush(Color.FromArgb(180, 0x00, 0x00, 0x00));

                                                    TextBlock DescriptionOfTrack = new TextBlock();
                                                    DescriptionOfTrack.Text = $"{Utils.GetLocalizationString("ArtistDefaultText")} {track.Artists[0].Name}\n" +
                                                    $"{Utils.GetLocalizationString("TrackDefaultText")} {track.Title}\n";
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

                                                                trackSpotifyList.Clear();
                                                                trackSpotifyList.AddRange(newSongs);
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
                                                resultTextBlock.Text = $"{Utils.GetLocalizationString("ErrorNothingFoundText")} \"{SearchBarTextBox.Text}\" {Utils.GetLocalizationString("ErrorNothingFoundText1")}";
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
                                resultTextBlock.Text = $"{Utils.GetLocalizationString("ErrorNothingFoundText")} \"{SearchBarTextBox.Text}\" {Utils.GetLocalizationString("ErrorNothingFoundText1")}";
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
                                resultTextBlock.Text = Utils.GetLocalizationString("TipStartWrittingText");
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
                                resultTextBlock.Text = Utils.GetLocalizationString("ErrorNoInternetText");
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
                    if (Utils.settings.searchEngineIndex == 1)
                    {
                        MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Pause;
                        MainWindow.window.songTitle.Content = trackYouTubeList[currentSongIndex].Title;
                        MainWindow.window.songAuthor.Content = trackYouTubeList[currentSongIndex].Author.ChannelTitle;

                        if (Utils.settings.musicPlayerBallonTurnOn)
                        {
                            if (ballon != null) ballon = null;
                            ballon = new MusicExplorerBallon();
                        }

                        if (!Utils.settings.economTraffic)
                        {
                            BitmapImage sourceImageOfTrack = new BitmapImage();
                            sourceImageOfTrack.BeginInit();
                            sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                            sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                            sourceImageOfTrack.UriSource = new Uri(trackYouTubeList[currentSongIndex].Thumbnails[1].Url);
                            sourceImageOfTrack.EndInit();

                            MainWindow.window.iconTrack.Source = sourceImageOfTrack;
                            if (Utils.settings.musicPlayerBallonTurnOn) ballon.songIcon.Source = sourceImageOfTrack;
                        }

                        uint hourMillisecond = 3600000;
                        double trackDuration = trackYouTubeList[currentSongIndex].Duration.Value.TotalMilliseconds;
                        string format = trackDuration > hourMillisecond ? @"h\:mm\:ss" : @"m\:ss";
                        MainWindow.window.endOfSong.Content = $"{TimeSpan.FromMilliseconds(trackDuration).ToString(format)}";

                        if (Utils.settings.musicPlayerBallonTurnOn)
                        {
                            ballon.songDescription.Text = $"{Utils.GetLocalizationString("ArtistDefaultText")} {trackYouTubeList[currentSongIndex].Author.ChannelTitle}\n{Utils.GetLocalizationString("TrackDefaultText")} {trackYouTubeList[currentSongIndex].Title}";
                            ballon.endOfSong.Content = MainWindow.window.endOfSong.Content;
                            ballon.leftSong.MouseDown += LeftSong_MouseDown;
                            ballon.rightSong.MouseDown += RightSong_MouseDown;
                            ballon.musicToggle.MouseDown += MusicToggle_MouseDown;
                            ballon.randomSongButton.MouseDown += RandomSongButton_MouseDown;
                            ballon.repeatSong.MouseDown += RepeatSong_MouseDown;
                            ballon.musicProgress.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((sender, e) =>
                            {
                                MusicProgress_DragDeltaProgress();
                            }));
                            ballon.musicProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, e) =>
                            {
                                MusicProgress_DragCompletedEvent();
                            }));
                            if (MainWindow.window.WindowState == WindowState.Minimized)
                            {
                                MainWindow.window.myNotifyIcon.ShowCustomBalloon(ballon, PopupAnimation.Slide, null);
                            }
                        }
                    }
                    else
                    {
                        MainWindow.window.musicToggle.Icon = FontAwesomeIcon.Pause;
                        MainWindow.window.songTitle.Content = trackSpotifyList[currentSongIndex].Title;
                        MainWindow.window.songAuthor.Content = trackSpotifyList[currentSongIndex].Artists[0].Name;

                        if (Utils.settings.musicPlayerBallonTurnOn)
                        {
                            if (ballon != null) ballon = null;
                            ballon = new MusicExplorerBallon();
                        }

                        if (!Utils.settings.economTraffic)
                        {
                            BitmapImage sourceImageOfTrack = new BitmapImage();
                            sourceImageOfTrack.BeginInit();
                            sourceImageOfTrack.CreateOptions = BitmapCreateOptions.None;
                            sourceImageOfTrack.CacheOption = BitmapCacheOption.None;
                            sourceImageOfTrack.UriSource = new Uri(trackSpotifyList[currentSongIndex].Album.Images[0].Url);
                            sourceImageOfTrack.EndInit();

                            MainWindow.window.iconTrack.Source = sourceImageOfTrack;
                            if (Utils.settings.musicPlayerBallonTurnOn) ballon.songIcon.Source = sourceImageOfTrack;
                        }

                        uint hourMillisecond = 3600000;
                        double trackDuration = trackSpotifyList[currentSongIndex].DurationMs;
                        string format = trackDuration > hourMillisecond ? @"h\:mm\:ss" : @"m\:ss";
                        MainWindow.window.endOfSong.Content = $"{TimeSpan.FromMilliseconds(mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds).ToString(format)}";


                        if (Utils.settings.musicPlayerBallonTurnOn)
                        {
                            ballon.songDescription.Text = $"{Utils.GetLocalizationString("ArtistDefaultText")} {trackSpotifyList[currentSongIndex].Artists[0].Name}\n{Utils.GetLocalizationString("TrackDefaultText")} {trackSpotifyList[currentSongIndex].Title}";
                            ballon.endOfSong.Content = MainWindow.window.endOfSong.Content;
                            ballon.leftSong.MouseDown += LeftSong_MouseDown;
                            ballon.rightSong.MouseDown += RightSong_MouseDown;
                            ballon.musicToggle.MouseDown += MusicToggle_MouseDown;
                            ballon.randomSongButton.MouseDown += RandomSongButton_MouseDown;
                            ballon.repeatSong.MouseDown += RepeatSong_MouseDown;
                            ballon.musicProgress.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((sender, e) =>
                            {
                                MusicProgress_DragDeltaProgress();
                            }));
                            ballon.musicProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, e) =>
                            {
                                MusicProgress_DragCompletedEvent();
                            }));
                            if (MainWindow.window.WindowState == WindowState.Minimized)
                            {
                                MainWindow.window.myNotifyIcon.ShowCustomBalloon(ballon, PopupAnimation.Slide, null);
                            }
                        }
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
                if (Utils.settings.searchEngineIndex == 1)
                {
                    YoutubeClient youtubeClient = new YoutubeClient();
                    var streamManifest = youtubeClient.Videos.Streams.GetManifestAsync(trackYouTubeList[currentSongIndex].Url);
                    var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                    await Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            mediaPlayer.Open(new Uri(streamInfo.Url));
                            mediaPlayer.Play();
                            progressSongTimer.Start();
                            mediaPlayer.Volume = (float)MainWindow.window.volumeSlider.Value;
                            Utils.StartDiscordPresence(trackYouTubeList[currentSongIndex]);
                        }
                        catch
                        {
                            StopSound();
                        }
                    });
                }
                else
                {
                    SpotifyClient spotifyYouTubeRetrive = new SpotifyClient();
                    string? youtubeID = await spotifyYouTubeRetrive.Tracks.GetYoutubeIdAsync(trackSpotifyList[currentSongIndex].Url);
                    YoutubeClient? youtube = new YoutubeClient();
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
                            Utils.StartDiscordPresence(trackSpotifyList[currentSongIndex]);
                        }
                        catch
                        {
                            StopSound();
                        }
                    });
                }
            }
            catch (ArgumentException) // exception that arrives when song doesn't exist in YouTube and it simply tries to switch to another one. 
            {
                try
                {
                    currentSongIndex++;
                    if (currentSongIndex >= trackSpotifyList.Count) currentSongIndex = 0;
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
                        resultTextBlock.Text = Utils.GetLocalizationString("TipStartWrittingText");
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
                        resultTextBlock.Text = Utils.GetLocalizationString("ErrorNoInternetText");
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
                MessageBox.Show($"ERROR: {ex.GetType()},\n Message: {ex.Message},\n Please screenshot this error, and send me in github!\nLINK: {Utils.GithubLink}");
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