using DiscordRPC;
using Free_Spotify.Pages;
using SpotifyExplode.Search;
using System;
using YoutubeExplode.Search;

namespace Free_Spotify.Classes
{
    /// <summary>
    /// Class that will change status in specific case.
    /// </summary>
    public static class DiscordStatuses
    {

        /// <summary>
        /// If progress of the song is changed, it changes the progress to current position
        /// </summary>
        public static void ContinueDiscordPresence(TrackSearchResult track)
        {
            if (track == null)
            {
                return;
            }
            try
            {
                if (Settings.SettingsData.discordRPC && MusicPlayerPage.Instance != null)
                {
                    if (track.Url != null)
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{track.Artists[0].Name} - {track.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = track.Album.Images[0].Url,
                                LargeImageText = $"{track.Artists[0].Name} - {track.Title}",
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = DateTime.UtcNow.AddMilliseconds(-MusicPlayerPage.Instance.musicProgress.Value),
                                End = DateTime.UtcNow.AddMilliseconds(MusicPlayerPage.Instance.musicProgress.Maximum - MusicPlayerPage.Instance.musicProgress.Value)
                            },
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = track.Url,
                                },
                                new Button()
                                {
                                    Label = "Free Spotify...",
                                    Url = Utils.GithubLink
                                }
                            }
                        });
                    }
                    else
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{track.Artists[0].Name} - {track.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = track.Album.Images[0].Url,
                                LargeImageText = $"{track.Artists[0].Name} - {track.Title}",
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = DateTime.UtcNow.AddMilliseconds(-MusicPlayerPage.Instance.musicProgress.Value),
                                End = DateTime.UtcNow.AddMilliseconds(MusicPlayerPage.Instance.musicProgress.Maximum - MusicPlayerPage.Instance.musicProgress.Value)
                            },
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = Utils.GithubLink,
                                },
                                new Button()
                                {
                                    Label = "Free Spotify...",
                                    Url = Utils.GithubLink
                                }
                            }
                        });
                    }
                }
                else
                {
                    MainWindow.Window?.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */ }
        }
        /// <summary>
        /// If progress of the song is changed, it changes the progress to current position
        /// </summary>
        public static void ContinueDiscordPresence(VideoSearchResult video)
        {
            if (video == null)
            {
                return;
            }
            try
            {
                if (Settings.SettingsData.discordRPC && MusicPlayerPage.Instance != null)
                {
                    if (video.Url != null)
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{video.Author.ChannelTitle} - {video.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = video.Thumbnails[0].Url,
                                LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}",
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = DateTime.UtcNow.AddMilliseconds(-MusicPlayerPage.Instance.musicProgress.Value),
                                End = DateTime.UtcNow.AddMilliseconds(MusicPlayerPage.Instance.musicProgress.Maximum - MusicPlayerPage.Instance.musicProgress.Value)
                            },
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = video.Url + "&t=" + (int)TimeSpan.FromMilliseconds(MusicPlayerPage.Instance.musicProgress.Value).TotalSeconds, // = + timespan -> adds ability to follow on which position user is playing the song.
                                },
                                new Button()
                                {
                                    Label = "Free Spotify...",
                                    Url = Utils.GithubLink
                                }
                            },
                        });
                    }
                    else
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{video.Author.ChannelTitle} - {video.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = video.Thumbnails[0].Url,
                                LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}",
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = DateTime.UtcNow.AddMilliseconds(-MusicPlayerPage.Instance.musicProgress.Value),
                                End = DateTime.UtcNow.AddMilliseconds(MusicPlayerPage.Instance.musicProgress.Maximum - MusicPlayerPage.Instance.musicProgress.Value)
                            },
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                            Url = Utils.GithubLink
                                },
                                new Button()
                                {
                                    Label = "Free Spotify...",
                                    Url = Utils.GithubLink
                                }
                            },
                        });
                    }
                }
                else
                {
                    MainWindow.Window?.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */ }
        }
        /// <summary>
        /// Shows status of idling in song.
        /// </summary>
        public static void IdleDiscordPresence()
        {
            try
            {
                if (Settings.SettingsData.discordRPC)
                {
                    MainWindow.Window?.discordClient.SetPresence(new RichPresence() { Details = Settings.GetLocalizationString("DiscordRPCAFK"), Assets = new Assets() { LargeImageKey = "logo", LargeImageText = "Free Spotify" }, Timestamps = Timestamps.Now, Buttons = new Button[] { new Button() { Label = "Free Spotify...", Url = Utils.GithubLink } } });
                }
                else
                {
                    MainWindow.Window?.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */  }
        }

        /// <summary>
        /// Shows status when song is paused. 
        /// </summary>
        public static void PauseDiscordPresence(TrackSearchResult track)
        {
            if (track == null)
            {
                return;
            }
            try
            {
                if (Settings.SettingsData.discordRPC)
                {
                    if (track.Url != null)
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCPausedTrack"),
                            State = $"{track.Artists[0].Name} - {track.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = track.Album.Images[0].Url,
                                LargeImageText = $"{track.Artists[0].Name} - {track.Title}"
                            },
                            Timestamps = Timestamps.Now,
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = track.Url
                                },
                                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                            }
                        });
                    }
                    else
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCPausedTrack"),
                            State = $"{track.Artists[0].Name} - {track.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = track.Album.Images[0].Url,
                                LargeImageText = $"{track.Artists[0].Name} - {track.Title}"
                            },
                            Timestamps = Timestamps.Now,
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = Utils.GithubLink
                                },
                                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                            }
                        });
                    }

                }
                else
                {
                    MainWindow.Window?.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */  }
        }

        /// <summary>
        /// Shows status when song is paused. 
        /// </summary>
        public static void PauseDiscordPresence(VideoSearchResult video)
        {
            if (video == null)
            {
                return;
            }
            try
            {
                if (Settings.SettingsData.discordRPC && MusicPlayerPage.Instance != null)
                {
                    if (video.Url != null)
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCPausedTrack"),
                            State = $"{video.Author.ChannelTitle} - {video.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = video.Thumbnails[0].Url,
                                LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}"
                            },
                            Timestamps = Timestamps.Now,
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = video.Url + "&t=" + (int)TimeSpan.FromMilliseconds(MusicPlayerPage.Instance.musicProgress.Value).TotalSeconds
                                },
                                new Button()
                                {
                                    Label = "Free Spotify...",
                                    Url = Utils.GithubLink
                                }
                            }
                        });
                    }
                    else
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCPausedTrack"),
                            State = $"{video.Author.ChannelTitle} - {video.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = video.Thumbnails[0].Url,
                                LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}"
                            },
                            Timestamps = Timestamps.Now,
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = Utils.GithubLink
                                },
                                new Button()
                                {
                                    Label = "Free Spotify...",
                                    Url = Utils.GithubLink
                                }
                            }
                        });
                    }

                }
                else
                {
                    MainWindow.Window?.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */  }
        }

        /// <summary>
        /// Used to call a reset of song to show which song is playing
        /// </summary>
        public static void StartDiscordPresence(TrackSearchResult track)
        {
            if (track == null)
            {
                return;
            }
            try
            {
                if (Settings.SettingsData.discordRPC)
                {
                    if (track.Url != null)
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{track.Artists[0].Name} - {track.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = track.Album.Images[0].Url,
                                LargeImageText = $"{track.Artists[0].Name} - {track.Title}"
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = Timestamps.Now.Start,
                                End = Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(track.DurationMs)).End
                            },
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = track.Url
                                },
                                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                            }
                        });
                    }
                    else
                    {
                        MainWindow.Window?.discordClient.SetPresence(new RichPresence()
                        {
                            Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{track.Artists[0].Name} - {track.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = track.Album.Images[0].Url,
                                LargeImageText = $"{track.Artists[0].Name} - {track.Title}"
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = Timestamps.Now.Start,
                                End = Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(track.DurationMs)).End
                            },
                            Buttons = new Button[]
                            {
                                new Button()
                                {
                                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                                    Url = Utils.GithubLink
                                },
                                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                            }
                        });
                    }

                }
                else
                {
                    MainWindow.Window?.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */ }
        }

        /// <summary>
        /// Used to call a reset of song to show which song is playing
        /// </summary>
        public static void StartDiscordPresence(VideoSearchResult video)
        {
            try
            {
                if (Settings.SettingsData.discordRPC && MusicPlayerPage.Instance != null)
                {
                    if (video.Url != null)
                    {
                        if (video.Duration.HasValue)
                        {
                            MainWindow.Window?.discordClient.SetPresence(
                                new RichPresence()
                                {
                                    Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                                    State = $"{video.Author.ChannelTitle} - {video.Title}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = video.Thumbnails[0].Url,
                                        LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}"
                                    },
                                    Timestamps = new Timestamps()
                                    {
                                        Start = Timestamps.Now.Start,
                                        End =
                                            Timestamps.FromTimeSpan(
                                                TimeSpan.FromMilliseconds(video.Duration.Value.TotalMilliseconds)
                                            ).End
                                    },
                                    Buttons = new Button[]
                                    {
                new Button()
                {
                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                    Url =
                        video.Url
                        + "&t="
                        + (int)TimeSpan.FromMilliseconds(
                            MusicPlayerPage.Instance.musicProgress.Value
                        ).TotalSeconds
                },
                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                                    }
                                }
                            );
                        }
                        else
                        {
                            MainWindow.Window?.discordClient.SetPresence(
                                new RichPresence()
                                {
                                    Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                                    State = $"{video.Author.ChannelTitle} - {video.Title}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = video.Thumbnails[0].Url,
                                        LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}"
                                    },
                                    Timestamps = new Timestamps()
                                    {
                                        Start = Timestamps.Now.Start,
                                        End = Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(0)).End
                                    },
                                    Buttons = new Button[]
                                    {
                new Button()
                {
                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                    Url = video.Url
                },
                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                                    }
                                }
                            );
                        }
                    }
                    else
                    {
                        if (video.Duration.HasValue)
                        {
                            MainWindow.Window?.discordClient.SetPresence(
                                new RichPresence()
                                {
                                    Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                                    State = $"{video.Author.ChannelTitle} - {video.Title}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = video.Thumbnails[0].Url,
                                        LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}"
                                    },
                                    Timestamps = new Timestamps()
                                    {
                                        Start = Timestamps.Now.Start,
                                        End =
                                            Timestamps.FromTimeSpan(
                                                TimeSpan.FromMilliseconds(video.Duration.Value.TotalMilliseconds)
                                            ).End
                                    },
                                    Buttons = new Button[]
                                    {
                new Button()
                {
                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                    Url =Utils.GithubLink
                },
                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                                    }
                                }
                            );
                        }
                        else
                        {
                            MainWindow.Window?.discordClient.SetPresence(
                                new RichPresence()
                                {
                                    Details = Settings.GetLocalizationString("DiscordRPCListeningTo"),
                                    State = $"{video.Author.ChannelTitle} - {video.Title}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = video.Thumbnails[0].Url,
                                        LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}"
                                    },
                                    Timestamps = new Timestamps()
                                    {
                                        Start = Timestamps.Now.Start,
                                        End = Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(0)).End
                                    },
                                    Buttons = new Button[]
                                    {
                new Button()
                {
                    Label = Settings.GetLocalizationString("DiscordRPCListenToTrack"),
                    Url = Utils.GithubLink
                },
                new Button() { Label = "Free Spotify...", Url = Utils.GithubLink }
                                    }
                                }
                            );
                        }
                    }


                }
                else
                {
                    MainWindow.Window?.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */ }
        }
    }
}