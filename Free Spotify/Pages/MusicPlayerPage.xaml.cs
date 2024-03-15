using FontAwesome.WPF;
using Free_Spotify.Ballons;
using Free_Spotify.Classes;
using Free_Spotify.Dialogs;
using Free_Spotify.Interfaces;
using SpotifyExplode;
using SpotifyExplode.Search;
using System;
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
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;
using static Free_Spotify.Classes.SettingsData.Playlist;

namespace Free_Spotify.Pages
{
    public partial class MusicPlayerPage : Page
    {

        // Timer for rendering the progress bar of the player.
        public System.Timers.Timer progressSongTimer = new() { Interval = 1000 };

        // Cancellation token source for canceling the progress timer.
        public CancellationTokenSource cancelProgressSongTimer = new();

        // Metadata for tracks and videos, used to specify the search engine (Spotify or YouTube).
        public TrackSearchResult? trackSpotify = null;
        public VideoSearchResult? trackYouTube = null;
        public YouTubeTrackItem? trackYouTubePlaylist = null;

        public ISongManager? songManager;

        // MediaPlayer for playing music, offering quality and preventing stacking.
        public MediaPlayer MusicMediaPlayer { get; private set; } = new();

        // Singleton instance of the MusicPlayerPage.
        public static MusicPlayerPage? Instance { get; set; }

        // Boolean indicating if the song should repeat.
        public bool IsSongRepeat = false;

        // Boolean indicating if the song is paused.
        public bool IsSongPaused = false;

        // Boolean indicating if shuffle mode is enabled.
        public bool isShuffleEnabled = false;

        // Ballon object for music exploration.
        public MusicExplorerBallon? ballon;

        public MusicPlayerPage()
        {
            InitializeComponent();

            songTitle.Text = Settings.GetLocalizationString("SongTitleDefaultText");
            songAuthor.Text = Settings.GetLocalizationString("SongAuthorDefaultText");

            // Initialize the music player volume with the saved value from settings.
            MusicMediaPlayer.Volume = Settings.SettingsData.volume;
            volumeSlider.Value = MusicMediaPlayer.Volume;

            // Subscribe to various events and handlers for music playback control.
            MusicMediaPlayer.MediaEnded += MediaPlayer_MediaEnded; // Handle when the song ends.
            randomSongButton.MouseDown += RandomSongButton_MouseDown; // Enable random song selection.
            progressSongTimer.Elapsed += ProgressSongTimer_Elapsed; // Update the UI progress of the song.
            musicToggle.MouseDown += MusicToggle_MouseDown; // Toggle play/pause.
            musicProgress.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((sender, e) => MusicProgress_DragDeltaProgress())); // Change song position when dragging the slider.
            musicProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, e) => MusicProgress_DragCompletedEvent())); // Resume playback after dragging the slider.
            musicProgress.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(MusicProgressSlider_MouseLeftButtonDownEvent), true); // Handle music slider interactions.
            volumeSlider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((sender, e) => VolumeSlider_DragDeltaEvent())); // Adjust the volume when dragging the volume slider.
            volumeSlider.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(VolumeSlider_MouseLeftButtonDownEvent), true); // Handle volume slider interactions.
            volumeIcon.MouseDown += VolumeIcon_Click; // Mute the song without changing volume.
            repeatSong.MouseDown += RepeatSong_MouseDown; // Enable song repeat.
            rightSong.MouseDown += RightSong_MouseDown; // Skip to the next song.
            leftSong.MouseDown += LeftSong_MouseDown; // Skip to the previous song.

            Instance = this; // Set the instance of the MusicPlayerPage.
        }

        /// <summary>
        /// Calls the "PreviousSong" method of the song manager to switch to the previous music
        /// </summary>
        public void LeftSong_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (songManager == null)
                {
                    return;
                }
                songManager.PreviousSong();
            }
            catch
            {
                StopSound();
            }
        }

        /// <summary>
        /// Handles drag completion event for music progress bar
        /// Updates the Discord presence based on the playing track
        /// Adjusts the music player position and state
        /// </summary>
        private void MusicProgress_DragCompletedEvent()
        {
            try
            {
                if (MusicMediaPlayer.Source != null)
                {
                    if (trackYouTube != null)
                    {
                        DiscordStatuses.ContinueDiscordPresence(trackYouTube);
                    }
                    else if (trackSpotify != null)
                    {
                        DiscordStatuses.ContinueDiscordPresence(trackSpotify);
                    }

                    if (MusicMediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null)
                        {
                            MusicMediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)ballon.musicProgress.Value);
                            PausingSong();
                            return;
                        }
                        MusicMediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)musicProgress.Value);
                    }
                }
                PausingSong();
            }
            catch
            {
                StopSound();
            }
        }

        /// <summary>
        /// Asynchronously handles the drag delta event for the MusicProgress control
        /// </summary>
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
                    if (MusicMediaPlayer.Source != null && MusicMediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        string format = MusicMediaPlayer.Position.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss";
                        if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null)
                        {
                            ballon.startOfSong.Content = new TimeSpan(0, 0, 0, 0, (int)ballon.musicProgress.Value).ToString(format);
                            return;
                        }
                        startOfSong.Content = new TimeSpan(0, 0, 0, 0, (int)musicProgress.Value).ToString(format);
                    }
                }
                catch
                {
                    StopSound();
                }
            });
        }

        /// <summary>
        /// Handles the drag delta event of a volume slider by updating the volume of a music player and the corresponding application setting.
        /// </summary>
        private void VolumeSlider_DragDeltaEvent()
        {
            MusicMediaPlayer.Volume = (float)volumeSlider.Value;
            Settings.SettingsData.volume = MusicMediaPlayer.Volume;
        }

        /// <summary>
        /// Handles the mouse left button down event for a volume slider. This method checks whether the volume slider is enabled for move-to-point interaction, and if not, or if the mouse is over the thumb, it takes no action. If the conditions are met, it simulates a left mouse button down event on the slider's thumb.
        /// </summary>
        private void VolumeSlider_MouseLeftButtonDownEvent(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!volumeSlider.IsMoveToPointEnabled || volumeSlider.Template.FindName("PART_Track", volumeSlider) is not Track track || track.Thumb == null || track.Thumb.IsMouseOver)
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
            catch
            {
                StopSound();
            }
        }

        /// <summary>
        /// Handles the mouse left button down event for a volume slider. This method checks whether the music slider is enabled for 
        /// move-to-point interaction, and if not, or if the mouse is over the thumb, it takes no action. If the conditions are met, it simulates a left mouse button down event on the slider's thumb.
        /// </summary>
        private void MusicProgressSlider_MouseLeftButtonDownEvent(object sender, MouseButtonEventArgs e)
        {

            try
            {
                //if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null)
                //{
                //    MusicMediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)ballon.musicProgress.Value);
                //    if (!musicProgress.IsMoveToPointEnabled || musicProgress.Template.FindName("PART_Track", musicProgress) is not Track trackBallon || trackBallon.Thumb == null || trackBallon.Thumb.IsMouseOver)
                //    {
                //        return;
                //    }
                //    trackBallon.Thumb.UpdateLayout();
                    
                //    trackBallon.Thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                //    {
                //        RoutedEvent = MouseLeftButtonDownEvent,
                //        Source = trackBallon.Thumb
                //    });

                //    trackBallon.Thumb.RaiseEvent(new DragDeltaEventArgs(0, 0)
                //    {
                //        RoutedEvent = Thumb.DragDeltaEvent,
                //        Source = trackBallon.Thumb
                //    });
                //    return;
                //}

                if (!musicProgress.IsMoveToPointEnabled || musicProgress.Template.FindName("PART_Track", musicProgress) is not Track track || track.Thumb == null || track.Thumb.IsMouseOver)
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
            catch
            {
                StopSound();
            }
        }

        /// <summary>
        /// Handles a click event on a volume icon. This method toggles the mute state of the music player and updates the icon accordingly. If the music player is muted, 
        /// it unmutes it and changes the icon to represent the unmuted state. If the music player is not muted, it mutes it and changes the icon to represent the muted state.
        /// </summary>
        public void VolumeIcon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MusicMediaPlayer.IsMuted)
                {
                    volumeIcon.Icon = FontAwesomeIcon.VolumeUp;
                    MusicMediaPlayer.IsMuted = false;
                }
                else
                {
                    volumeIcon.Icon = FontAwesomeIcon.VolumeOff;
                    MusicMediaPlayer.IsMuted = true;
                }
            }
            catch
            {
                StopSound();
            }
        }


        /// <summary>
        /// Handles a mouse down event on the right button, which switches to the next music track.
        /// </summary>
        public void RightSong_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (songManager == null)
                {
                    return;
                }
                songManager.NextSong();
            }
            catch
            {
                StopSound();
            }
        }

        /// <summary>
        /// Handles a mouse down event on the repeat song button. This method repeats the currently playing song.
        /// </summary>
        public void RepeatSong_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RepeatSongBehavior();
        }

        /// <summary>
        /// Handles a mouse down event on the 'Music Toggle' button, typically a pause button. This method pauses the currently playing song.
        /// </summary>
        public void MusicToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PausingSong();
        }

        /// <summary>
        /// A timer that updates the user interface every second. This method updates the progress of the currently playing song and other related UI elements.
        /// </summary> 
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
                    if (MusicMediaPlayer.Source != null && MainWindow.Window != null)
                    {
                        if (MusicMediaPlayer.NaturalDuration.HasTimeSpan)
                        {
                            if (trackYouTube != null)
                            {
                                DiscordStatuses.ContinueDiscordPresence(trackYouTube);
                            }
                            else if (trackSpotify != null)
                            {
                                DiscordStatuses.ContinueDiscordPresence(trackSpotify);
                            }

                            musicProgress.Value = MusicMediaPlayer.Position.TotalMilliseconds;
                            if (ballon != null && Settings.SettingsData.musicPlayerBallonTurnOn) ballon.musicProgress.Value = musicProgress.Value;

                            if (trackYouTube != null && trackYouTube.Duration != null)
                            {
                                musicProgress.Maximum = trackYouTube.Duration.Value.TotalMilliseconds;
                                if (ballon != null && Settings.SettingsData.musicPlayerBallonTurnOn) ballon.musicProgress.Maximum = trackYouTube.Duration.Value.TotalMilliseconds;
                            }
                            else if (trackSpotify != null)
                            {
                                musicProgress.Maximum = MusicMediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                                if (ballon != null && Settings.SettingsData.musicPlayerBallonTurnOn) ballon.musicProgress.Maximum = MusicMediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                            }

                            MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Normal;
                            MainWindow.Window.progressSongTaskBar.ProgressValue = MusicMediaPlayer.Position.TotalMilliseconds / MusicMediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;

                            if (MusicMediaPlayer.Position.Hours > 0)
                            {
                                startOfSong.Content = MusicMediaPlayer.Position.ToString(@"h\:mm\:ss");
                                if (ballon != null && Settings.SettingsData.musicPlayerBallonTurnOn) ballon.startOfSong.Content = MusicMediaPlayer.Position.ToString(@"h\:mm\:ss");
                            }
                            else
                            {
                                startOfSong.Content = MusicMediaPlayer.Position.ToString(@"m\:ss");
                                if (ballon != null && Settings.SettingsData.musicPlayerBallonTurnOn) ballon.startOfSong.Content = MusicMediaPlayer.Position.ToString(@"m\:ss");
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
        /// Handles a mouse down event on the random song button, which toggles the shuffle feature. When enabled, the player picks a random song at the end of the current one.
        /// </summary>
        public void RandomSongButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isShuffleEnabled = !isShuffleEnabled;
            if (isShuffleEnabled)
            {
                randomSongButton.Foreground = new SolidColorBrush(Colors.Lime);
                if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.randomSongButton.Foreground = new SolidColorBrush(Colors.Lime);
            }
            else
            {
                randomSongButton.Foreground = new SolidColorBrush(Colors.White);
                if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.randomSongButton.Foreground = new SolidColorBrush(Colors.White);
            }
        }


        /// <summary>
        /// An event that fires when the media player reaches the end of a song. It can repeat the song or stop it, depending on the current settings.
        /// </summary>
        private async void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            if (trackYouTube != null)
            {
                DiscordStatuses.ContinueDiscordPresence(trackYouTube);
            }
            else if (trackSpotify != null)
            {
                DiscordStatuses.ContinueDiscordPresence(trackSpotify);
            }
            await Dispatcher.BeginInvoke(async () =>
            {
                if (MusicMediaPlayer.Source != null)
                {
                    try
                    {
                        if (IsSongRepeat)
                        {
                            MusicMediaPlayer.Position = TimeSpan.Zero;
                            return;
                        }
                        if (isShuffleEnabled)
                        {
                            songManager?.ShuffleSongs();
                            return;
                        }
                        await Task.Run(() =>
                        {
                            songManager?.NextSong();
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
        /// Toggles between repeating the current song and not repeating it. This function switches the repeat button state and updates the UI accordingly.
        /// </summary>
        public async void RepeatSongBehavior()
        {
            IsSongRepeat = !IsSongRepeat;
            if (IsSongRepeat)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    repeatSong.Foreground = new SolidColorBrush(Colors.Lime);
                    if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.repeatSong.Foreground = new SolidColorBrush(Colors.Lime);
                });
            }
            else
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    repeatSong.Foreground = new SolidColorBrush(Colors.White);
                    if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.repeatSong.Foreground = new SolidColorBrush(Colors.White);
                });
            }
        }

        /// <summary>
        /// Pauses or resumes the currently playing song. This function manages the pause and play states, updates UI elements, 
        /// and communicates with external services like Discord presence.
        /// </summary>
        public async void PausingSong()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (MusicMediaPlayer.Source != null)
                    {
                        try
                        {
                            if (IsSongPaused)
                            {
                                try
                                {
                                    IsSongPaused = false;
                                    MusicMediaPlayer.Play();
                                    progressSongTimer.Start();
                                    if (trackYouTube != null)
                                    {
                                        DiscordStatuses.ContinueDiscordPresence(trackYouTube);
                                    }
                                    else if (trackSpotify != null)
                                    {
                                        DiscordStatuses.ContinueDiscordPresence(trackSpotify);
                                    }
                                    musicToggle.Icon = FontAwesomeIcon.Pause;
                                    if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.musicToggle.Icon = FontAwesomeIcon.Pause;
                                    if (MusicMediaPlayer.Source != null && MainWindow.Window != null)
                                    {
                                        MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Normal;
                                        if (MusicMediaPlayer.NaturalDuration.HasTimeSpan)
                                        {
                                            MainWindow.Window.progressSongTaskBar.ProgressValue = MusicMediaPlayer.Position.TotalMilliseconds / MusicMediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                                        }
                                    }
                                    else
                                    {
                                        if (MainWindow.Window == null)
                                        {
                                            return;
                                        }
                                        MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Error;
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
                                if (trackYouTube != null)
                                {
                                    DiscordStatuses.ContinueDiscordPresence(trackYouTube);
                                }
                                else if (trackSpotify != null)
                                {
                                    DiscordStatuses.ContinueDiscordPresence(trackSpotify);
                                }
                                IsSongPaused = true;
                                MusicMediaPlayer.Pause();
                                progressSongTimer.Stop();
                                musicToggle.Icon = FontAwesomeIcon.Play;
                                if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.musicToggle.Icon = FontAwesomeIcon.Play;
                                if (MusicMediaPlayer.Source != null && MainWindow.Window != null)
                                {
                                    MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Paused;
                                    if (MusicMediaPlayer.NaturalDuration.HasTimeSpan)
                                    {
                                        MainWindow.Window.progressSongTaskBar.ProgressValue = MusicMediaPlayer.Position.TotalMilliseconds / MusicMediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                                    }
                                }
                                else
                                {
                                    if (MainWindow.Window == null)
                                    {
                                        return;
                                    }
                                    MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Error;
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
                        if (MainWindow.Window == null)
                        {
                            return;
                        }
                        MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.None;
                        MainWindow.Window.progressSongTaskBar.ProgressValue = 0;
                    }
                }
                catch
                {
                    StopSound();
                }
            });
        }

        /// <summary>
        /// Updates the status and information on the player bar, including song details, duration, and album artwork. 
        /// </summary>
        public async void UpdateStatusPlayerBar(VideoSearchResult? youTubeTrack, TrackSearchResult? spotifyTrackItem)
        {
            await Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    if (youTubeTrack != null && youTubeTrack.Duration != null && MainWindow.Window != null)
                    {
                        musicToggle.Icon = FontAwesomeIcon.Pause;
                        songTitle.Text = youTubeTrack.Title;
                        songAuthor.Text = youTubeTrack.Author?.ChannelTitle;

                        if (Settings.SettingsData.musicPlayerBallonTurnOn)
                        {
                            if (ballon != null) ballon = null;
                            ballon = new MusicExplorerBallon();
                        }

                        if (!Settings.SettingsData.economTraffic)
                        {
                            if (youTubeTrack.Thumbnails != null)
                            {
                                iconTrack.Source = new BitmapImage(new Uri(youTubeTrack.Thumbnails[0].Url));
                                RenderOptions.SetBitmapScalingMode(iconTrack, BitmapScalingMode.Fant);
                                if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.songIcon.Source = iconTrack.Source;
                            }
                        }

                        uint hourMillisecond = 3600000;
                        double trackDuration = youTubeTrack.Duration.Value.TotalMilliseconds;
                        string format = trackDuration > hourMillisecond ? @"h\:mm\:ss" : @"m\:ss";
                        endOfSong.Content = $"{TimeSpan.FromMilliseconds(trackDuration).ToString(format)}";

                        if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null && youTubeTrack.Author != null)
                        {
                            await Dispatcher.BeginInvoke(() =>
                            {
                                ballon.songDescription.Text = $"{Settings.GetLocalizationString("ArtistDefaultText")} {youTubeTrack.Author.ChannelTitle}\n{Settings.GetLocalizationString("TrackDefaultText")} {youTubeTrack.Title}";
                                ballon.endOfSong.Content = endOfSong.Content;
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
                            });
                            MainWindow.Window.myNotifyIcon.ShowCustomBalloon(ballon, PopupAnimation.Slide, null);

                        }
                    }
                    else if (spotifyTrackItem != null && MainWindow.Window != null)
                    {
                        musicToggle.Icon = FontAwesomeIcon.Pause;
                        songTitle.Text = spotifyTrackItem.Title;
                        songAuthor.Text = spotifyTrackItem.Artists[0].Name;

                        if (Settings.SettingsData.musicPlayerBallonTurnOn)
                        {
                            if (ballon != null) ballon = null;
                            ballon = new MusicExplorerBallon();
                        }

                        if (!Settings.SettingsData.economTraffic)
                        {
                            iconTrack.Source = new BitmapImage(new Uri(spotifyTrackItem.Album.Images[0].Url));
                            RenderOptions.SetBitmapScalingMode(iconTrack, BitmapScalingMode.Fant);
                            if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null) ballon.songIcon.Source = iconTrack.Source;
                        }

                        uint hourMillisecond = 3600000;
                        double trackDuration = spotifyTrackItem.DurationMs;
                        string format = trackDuration > hourMillisecond ? @"h\:mm\:ss" : @"m\:ss";
                        endOfSong.Content = TimeSpan.FromMilliseconds(spotifyTrackItem.DurationMs).ToString(format);

                        if (Settings.SettingsData.musicPlayerBallonTurnOn && ballon != null)
                        {
                            await Dispatcher.BeginInvoke(() =>
                            {
                                ballon.songDescription.Text = $"{Settings.GetLocalizationString("ArtistDefaultText")} {spotifyTrackItem.Artists[0].Name}\n{Settings.GetLocalizationString("TrackDefaultText")} {spotifyTrackItem.Title}";
                                ballon.endOfSong.Content = endOfSong.Content;
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
                                ballon.musicProgress.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(MusicProgressSlider_MouseLeftButtonDownEvent), true);

                            });
                            await Dispatcher.BeginInvoke(() =>
                            {
                                MainWindow.Window.myNotifyIcon.ShowCustomBalloon(ballon, PopupAnimation.Slide, null);
                            });
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
        /// Stops the sound and resets the player when a new song is selected or an error occurs.
        /// </summary>
        public async void StopSound()
        {
            try
            {
                DiscordStatuses.IdleDiscordPresence();
                IsSongPaused = false;
                PausingSong();
                await Dispatcher.BeginInvoke(() =>
                {
                    if (MusicMediaPlayer.Source != null)
                    {
                        MusicMediaPlayer.Position = TimeSpan.Zero;
                        MusicMediaPlayer.Stop();
                        MusicMediaPlayer.Close();
                    }
                    progressSongTimer.Stop();
                    if (MainWindow.Window == null)
                    {
                        return;
                    }
                    MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.None;
                    MainWindow.Window.progressSongTaskBar.ProgressValue = 0;
                });

            }
            catch
            {
                ClearMusic();
            }
        }


        /// <summary>
        /// Tries to play a sound from Spotify. Stops the current sound, updates the song manager, and plays the selected Spotify track.
        /// </summary>
        public async void PlaySound(TrackSearchResult? spotifyTrack, ISongManager songManager)
        {
            try
            {
                StopSound();
                this.songManager = songManager;
                trackSpotify = spotifyTrack;
                trackYouTube = null;
                await Task.Run(() =>
                {
                    if (spotifyTrack != null)
                    {
                        UpdateStatusPlayerBar(null, spotifyTrack);
                    }
                    PlaySound();
                });
            }
            catch
            {
                StopSound();
            }
        }
        /// <summary>
        /// Tries to play a sound from YouTube. Stops the current sound, updates the song manager, and plays the selected YouTube track.
        /// </summary>
        public async void PlaySound(VideoSearchResult? youTubeTrack, YouTubeTrackItem? youTubeTrackReserv, ISongManager songManager)
        {
            try
            {
                StopSound();
                this.songManager = songManager;
                trackYouTube = youTubeTrack;
                trackYouTubePlaylist = youTubeTrackReserv;
                trackSpotify = null;
                await Task.Run(() =>
                {
                    if (trackYouTube != null)
                    {
                        UpdateStatusPlayerBar(trackYouTube, null);
                    }
                    PlaySound();
                });
            }
            catch
            {
                StopSound();
            }
        }
        /// <summary>
        /// Plays the selected track, updates the player bar, and manages audio stream selection and playback.
        /// </summary>
        private async void PlaySound()
        {
            try
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    if (Utils.IsPlayingFromPlaylist)
                    {
                        favoriteSongButton.Visibility = Visibility.Hidden;
                    }
                    if (MusicMediaPlayer.Source != null)
                    {
                        StopSound();
                        return;
                    }
                });
                await Dispatcher.BeginInvoke(() =>
                {
                    if (MainWindow.Window == null)
                    {
                        return;
                    }
                    MainWindow.Window.progressSongTaskBar.ProgressState = TaskbarItemProgressState.Indeterminate;
                    MainWindow.Window.progressSongTaskBar.ProgressValue = 0;
                });
                if (trackYouTube != null)
                {
                    ValueTask<StreamManifest> streamManifest = new();
                    try
                    {
                        YoutubeClient youtubeClient = new();
                        if (trackYouTube.Id.Value == null)
                        {
                            if (trackYouTubePlaylist != null)
                            {
                                streamManifest = youtubeClient.Videos.Streams.GetManifestAsync(trackYouTubePlaylist.Url);
                            }
                        }
                        else
                        {
                            streamManifest = youtubeClient.Videos.Streams.GetManifestAsync(trackYouTube.Url);
                        }
                    }
                    catch
                    {
                        StopSound();
                    }
                    await Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                            MusicMediaPlayer.Open(new Uri(streamInfo.Url));
                            MusicMediaPlayer.Play();
                            progressSongTimer.Start();
                            MusicMediaPlayer.Volume = (float)volumeSlider.Value;
                            DiscordStatuses.StartDiscordPresence(trackYouTube);
                        }
                        catch
                        {
                            StopSound();
                        }
                    });
                }
                else if (trackSpotify != null)
                {
                    SpotifyClient spotifyYouTubeRetrive = new();
                    string? youtubeID = await spotifyYouTubeRetrive.Tracks.GetYoutubeIdAsync(trackSpotify.Url);
                    YoutubeClient? youtube = new();
                    var streamManifest = youtube.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={youtubeID}");
                    var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                    await Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            MusicMediaPlayer.Open(new Uri(streamInfo.Url));
                            MusicMediaPlayer.Play();
                            progressSongTimer.Start();
                            MusicMediaPlayer.Volume = (float)volumeSlider.Value;
                            DiscordStatuses.StartDiscordPresence(trackSpotify);
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
                    songManager?.NextSong();
                }
                catch
                {
                    StopSound();
                }
            }
            catch (HttpRequestException request)
            {
                if (request.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await Dispatcher.BeginInvoke(() =>
                    {

                        SearchViewPage.SearchWindow?.searchVisual.Children.Clear();
                        TextBlock resultTextBlock = new()
                        {
                            Text = Settings.GetLocalizationString("TipStartWrittingText"),
                            FontSize = 14,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Foreground = new SolidColorBrush(Colors.White),
                            TextWrapping = TextWrapping.Wrap,
                            Style = (Style)FindResource("fontMontserrat")
                        };
                        _ = (SearchViewPage.SearchWindow?.searchVisual.Children.Add(resultTextBlock));
                    });
                }
                else
                {
                    await Dispatcher.BeginInvoke(() =>
                    {

                        SearchViewPage.SearchWindow?.searchVisual.Children.Clear();
                        TextBlock resultTextBlock = new()
                        {
                            Text = Settings.GetLocalizationString("ErrorNoInternetText"),
                            FontSize = 14,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Foreground = new SolidColorBrush(Colors.White),
                            TextWrapping = TextWrapping.Wrap,
                            Style = (Style)FindResource("fontMontserrat")
                        };
                        _ = (SearchViewPage.SearchWindow?.searchVisual.Children.Add(resultTextBlock));
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
                _ = MessageBox.Show($"ERROR: {ex.GetType()},\n Message: {ex.Message},\n Please screenshot this error, and send me in github!\nLINK: {Utils.GithubLink}");
                StopSound();
            }
        }

        public void ClearMusic()
        {
            _ = Dispatcher.BeginInvoke(() =>
            {
                StopSound();
                MusicMediaPlayer.Close();
                trackSpotify = null;
                trackYouTube = null;
                trackYouTubePlaylist = null;
                IsSongPaused = false;
                IsSongRepeat = false;
                isShuffleEnabled = false;
                ballon = null;
            });
        }

        /// <summary>
        /// Handles the user's interaction with the favorite song button.
        /// </summary>
        private void FavoriteSongButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlayListAskUserTrackDialog playListAskUserTrackDialog = new();
            _ = playListAskUserTrackDialog.ShowDialog();
        }
    }
}