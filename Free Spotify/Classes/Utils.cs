using DiscordRPC;
using SpotifyExplode.Search;
using System;

namespace Free_Spotify.Classes
{
    public static class Utils
    {
        public static string githubLink { get; private set; } = "https://github.com/J0nathan550"; // later will add the link to the public repository (IF EVER!)
        /// <summary>
        /// If progress of the song is changed, it changes the progress to current position
        /// </summary>
        public static void ContinueDiscordPresence(TrackSearchResult track)
        {
            try
            {
                MainWindow.window.discordClient.SetPresence(new RichPresence()
                {
                    Details = "Слушает музыку...",
                    State = $"{track.Artists[0].Name} - {track.Title}",
                    Assets = new Assets()
                    {
                        LargeImageKey = track.Album.Images[0].Url,
                        LargeImageText = $"{track.Artists[0].Name} - {track.Title}",
                    },
                    Timestamps = new Timestamps()
                    {
                        Start = DateTime.UtcNow.AddMilliseconds(-MainWindow.window.musicProgress.Value),
                        End = DateTime.UtcNow.AddMilliseconds(MainWindow.window.musicProgress.Maximum - MainWindow.window.musicProgress.Value)
                    },
                    Buttons = new Button[]
                    {
                        new Button()
                        {
                            Label = "Послушать трек...",
                            Url = track.Url,
                        },
                        new Button()
                        {
                            Label = "Free Spotify...",
                            Url = githubLink
                        }
                    }
                });
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
                MainWindow.window.discordClient.SetPresence(new RichPresence()
                {
                    Details = "Ничего не делает...",
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo",
                        LargeImageText = "Free Spotify",
                    },
                    Timestamps = Timestamps.Now,
                    Buttons = new Button[]
                    {
                        new Button()
                        {
                            Label = "Free Spotify...",
                            Url = githubLink
                        }
                    }
                });
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */  }
        }

        /// <summary>
        /// Shows status when song is paused. 
        /// </summary>
        public static void PauseDiscordPresence(TrackSearchResult track)
        {
            try
            {
                MainWindow.window.discordClient.SetPresence(new RichPresence()
                {
                    Details = "Поставил музыку на паузу...",
                    State = $"{track.Artists[0].Name} - {track.Title}",
                    Assets = new Assets()
                    {
                        LargeImageKey = track.Album.Images[0].Url,
                        LargeImageText = $"{track.Artists[0].Name} - {track.Title}",
                    },
                    Timestamps = Timestamps.Now,
                    Buttons = new Button[]
                    {
                        new Button()
                        {
                            Label = "Послушать трек...",
                            Url = track.Url,
                        },
                        new Button()
                        {
                            Label = "Free Spotify...",
                            Url = githubLink
                        }
                    }
                });
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */  }
        }

        /// <summary>
        /// Used to call a reset of song to show which song is playing
        /// </summary>
        public static void StartDiscordPresence(TrackSearchResult track)
        {
            try
            {
                MainWindow.window.discordClient.SetPresence(new RichPresence()
                {
                    Details = "Слушает музыку...",
                    State = $"{track.Artists[0].Name} - {track.Title}",
                    Assets = new Assets()
                    {
                        LargeImageKey = track.Album.Images[0].Url,
                        LargeImageText = $"{track.Artists[0].Name} - {track.Title}",
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
                            Label = "Послушать трек...",
                            Url = track.Url,
                        },
                        new Button()
                        {
                            Label = "Free Spotify...",
                            Url = githubLink
                        }
                    }
                });
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */ }
        }
    }
}