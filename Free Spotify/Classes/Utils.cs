using DiscordRPC;
using Newtonsoft.Json;
using SpotifyExplode.Search;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using YoutubeExplode.Search;

namespace Free_Spotify.Classes
{
    public static class Utils
    {
        public static Settings settings = new Settings(); // an ability to access to settings of this app

        public static string savePath { get; private set; } = "settings.json"; // constant string to represent file of settings. 
        public static string GithubLink { get; private set; } = "https://github.com/J0nathan550/Free-Spotify"; // later will add the link to the public repository (IF EVER!)

        // Link updates for different arch. 
        public static string DownloadAutoUpdateLinkX64 { get; private set; } = "https://raw.githubusercontent.com/J0nathan550/Free-Spotify/master/AutoUpdateX64.xml";
        public static string DownloadAutoUpdateLinkX86 { get; private set; } = "https://raw.githubusercontent.com/J0nathan550/Free-Spotify/master/AutoUpdateX86.xml";

        // Used to localize project to different languages. 
        public static ResourceManager localizationManager = new ResourceManager("Free_Spotify.Localization.Localization", Assembly.GetExecutingAssembly());

        /// <summary>
        /// Handy function to retrive codename of localization
        /// </summary>
        public static string GetLocalizationString(string name)
        {
            return localizationManager.GetString(name);
        }

        /// <summary>
        /// Used in settings, to change the UI to what you selected in settings. 
        /// </summary>
        public static void ChangeLanguage(string language)
        {
            var cultureInfo = new CultureInfo(language);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }

        public static void UpdateLanguage()
        {
            switch (settings.languageIndex)
            {
                case 0: // eng
                    ChangeLanguage("en");
                    break;
                case 1: // ru
                    ChangeLanguage("ru");
                    break;
                case 2: // ukr
                    ChangeLanguage("uk");
                    break;
                case 3: // japanese
                    ChangeLanguage("ja");
                    break;
                default:
                    settings.languageIndex = 0;
                    ChangeLanguage("en");
                    break;
            }
        }

        /// <summary>
        /// If progress of the song is changed, it changes the progress to current position
        /// </summary>
        public static void ContinueDiscordPresence(TrackSearchResult track)
        {
            try
            {
                if (settings.discordRPC)
                {
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = GetLocalizationString("DiscordRPCListeningTo"),
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
                            Label = GetLocalizationString("DiscordRPCListenToTrack"),
                            Url = track.Url,
                        },
                        new Button()
                        {
                            Label = "Free Spotify...",
                            Url = GithubLink
                        }
                        },
                    });
                }
                else
                {
                    MainWindow.window.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */ }
        }
        /// <summary>
        /// If progress of the song is changed, it changes the progress to current position
        /// </summary>
        public static void ContinueDiscordPresence(VideoSearchResult video)
        {
            try
            {
                if (settings.discordRPC)
                {
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = GetLocalizationString("DiscordRPCListeningTo"),
                        State = $"{video.Author.ChannelTitle} - {video.Title}",
                        Assets = new Assets()
                        {
                            LargeImageKey = video.Thumbnails[1].Url,
                            LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}",
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
                            Label = GetLocalizationString("DiscordRPCListenToTrack"),
                            Url = video.Url + "&t=" + (int)TimeSpan.FromMilliseconds(MainWindow.window.musicProgress.Value).TotalSeconds, // = + timespan -> adds ability to follow on which position user is playing the song.
                        },
                        new Button()
                        {
                            Label = "Free Spotify...",
                            Url = GithubLink
                        }
                        },
                    });
                }
                else
                {
                    MainWindow.window.discordClient.ClearPresence();
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
                if (settings.discordRPC)
                {
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = GetLocalizationString("DiscordRPCAFK"),
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
                            Url = GithubLink
                        }
                    }
                    });
                }
                else
                {
                    MainWindow.window.discordClient.ClearPresence();
                }
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
                if (settings.discordRPC)
                {
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = GetLocalizationString("DiscordRPCPausedTrack"),
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
                                Label = GetLocalizationString("DiscordRPCListenToTrack"),
                                Url = track.Url,
                            },
                            new Button()
                            {
                                Label = "Free Spotify...",
                                Url = GithubLink
                            }
                        }
                    });
                }
                else
                {
                    MainWindow.window.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */  }
        }

        /// <summary>
        /// Shows status when song is paused. 
        /// </summary>
        public static void PauseDiscordPresence(VideoSearchResult video)
        {
            try
            {
                if (settings.discordRPC)
                {
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = GetLocalizationString("DiscordRPCPausedTrack"),
                        State = $"{video.Author.ChannelTitle} - {video.Title}",
                        Assets = new Assets()
                        {
                            LargeImageKey = video.Thumbnails[1].Url,
                            LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}",
                        },
                        Timestamps = Timestamps.Now,
                        Buttons = new Button[]
                        {
                            new Button()
                            {
                                Label = GetLocalizationString("DiscordRPCListenToTrack"),
                                Url = video.Url + "&t=" + (int)TimeSpan.FromMilliseconds(MainWindow.window.musicProgress.Value).TotalSeconds, // = + timespan -> adds ability to follow on which position user is playing the song.
                            },
                            new Button()
                            {
                                Label = "Free Spotify...",
                                Url = GithubLink
                            }
                        }
                    });
                }
                else
                {
                    MainWindow.window.discordClient.ClearPresence();
                }
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
                if (settings.discordRPC)
                {
                    MainWindow.window.discordClient.SetPresence(new RichPresence()
                    {
                        Details = GetLocalizationString("DiscordRPCListeningTo"),
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
                                Label = GetLocalizationString("DiscordRPCListenToTrack"),
                                Url = track.Url,
                            },
                            new Button()
                            {
                                Label = "Free Spotify...",
                                Url = GithubLink
                            }
                        }
                    });
                }
                else
                {
                    MainWindow.window.discordClient.ClearPresence();
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
                if (settings.discordRPC)
                {
                    if (video.Duration.HasValue)
                    {
                        MainWindow.window.discordClient.SetPresence(new RichPresence()
                        {
                            Details = GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{video.Author.ChannelTitle} - {video.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = video.Thumbnails[1].Url,
                                LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}",
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = Timestamps.Now.Start,
                                End = Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(video.Duration.Value.TotalMilliseconds)).End
                            },
                            Buttons = new Button[]
                        {
                            new Button()
                            {
                                Label = GetLocalizationString("DiscordRPCListenToTrack"),
                                Url = video.Url + "&t=" + (int)TimeSpan.FromMilliseconds(MainWindow.window.musicProgress.Value).TotalSeconds, // = + timespan -> adds ability to follow on which position user is playing the song.
                            },
                            new Button()
                            {
                                Label = "Free Spotify...",
                                Url = GithubLink
                            }
                        }
                        });
                    }
                    else
                    {
                        MainWindow.window.discordClient.SetPresence(new RichPresence()
                        {
                            Details = GetLocalizationString("DiscordRPCListeningTo"),
                            State = $"{video.Author.ChannelTitle} - {video.Title}",
                            Assets = new Assets()
                            {
                                LargeImageKey = video.Thumbnails[1].Url,
                                LargeImageText = $"{video.Author.ChannelTitle} - {video.Title}",
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
                                Label = GetLocalizationString("DiscordRPCListenToTrack"),
                                Url = video.Url,
                            },
                            new Button()
                            {
                                Label = "Free Spotify...",
                                Url = GithubLink
                            }
                        }
                        });
                    }
                }
                else
                {
                    MainWindow.window.discordClient.ClearPresence();
                }
            }
            catch { /* Sometimes buttons can crash whole application because URL inside of button can be null. */ }
        }

        /// <summary>
        /// Function that is used to load data from .json file
        /// </summary>
        public static void LoadSettings()
        {
            if (!File.Exists(savePath))
            {
                File.WriteAllText(savePath, JsonConvert.SerializeObject(settings));
                return;
            }
            string jsonInfo = File.ReadAllText(savePath);
            if (!string.IsNullOrEmpty(jsonInfo))
            {
                try
                {
                    settings = JsonConvert.DeserializeObject<Settings>(jsonInfo);
                }
                catch
                {
                    MessageBox.Show("Файл с настройками был повреждён, настройки сброшены по умолчанию.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    File.Delete(savePath);
                    File.WriteAllText(savePath, JsonConvert.SerializeObject(settings));
                }
            }
            else
            {
                File.Delete(savePath);
                File.WriteAllText(savePath, JsonConvert.SerializeObject(settings));
            }
        }

        /// <summary>
        /// Self-explanatory, saved in .json format.
        /// </summary>
        public static void SaveSettings()
        {
            File.WriteAllText(savePath, JsonConvert.SerializeObject(settings));
        }

        public class Settings
        {
            public double volume = 0.5f;
            public int languageIndex = 0; // 0 -> English, 1 -> ru, 2 -> UA, 3 -> Japan
            public int searchEngineIndex = 0; // 0 -> Spotify, 1 -> YouTube
            public bool discordRPC = true;
            public bool economTraffic = false;
            public bool musicPlayerBallonTurnOn = true;
        }
    }
}