using Newtonsoft.Json;
using SpotifyExplode.Albums;
using SpotifyExplode.Artists;
using SpotifyExplode.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace Free_Spotify.Classes
{
    /// <summary>
    /// Settings of this project. 
    /// </summary>
    public static class Settings
    {
        private static SettingsData settingsData = new();
        /// <summary>
        /// Function that is used to load data from .json file
        /// </summary>

        // Used to localize project to different languages. 
        private static ResourceManager localizationManager = new("Free_Spotify.Localization.Localization", Assembly.GetExecutingAssembly());

        public static SettingsData SettingsData { get => settingsData; set => settingsData = value; }
        public static ResourceManager LocalizationManager { get => localizationManager; set => localizationManager = value; }

        /// <summary>
        /// Handy function to retrive codename of localization
        /// </summary>
        public static string GetLocalizationString(string name)
        {
            string? word = LocalizationManager.GetString(name);
            if (word == null) return string.Empty;
            return word;
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

        /// <summary>
        /// Updates the language in app
        /// </summary>
        public static void UpdateLanguage()
        {
            switch (SettingsData.languageIndex)
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
                    SettingsData.languageIndex = 0;
                    ChangeLanguage("en");
                    break;
            }
        }

        /// <summary>
        /// Loads all of the data from the JSON.  
        /// </summary>
        public static void LoadSettings()
        {
            if (!File.Exists(Utils.savePath))
            {
                File.WriteAllText(Utils.savePath, JsonConvert.SerializeObject(SettingsData));
                return;
            }
            string jsonInfo = File.ReadAllText(Utils.savePath);
            if (!string.IsNullOrEmpty(jsonInfo))
            {
                try
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    SettingsData = JsonConvert.DeserializeObject<SettingsData>(jsonInfo);
#pragma warning restore CS8601 // Possible null reference assignment.
                }
                catch
                {
                    MessageBox.Show(GetLocalizationString("ErrorJSONCorrupted"), GetLocalizationString("ErrorText"), MessageBoxButton.OK, MessageBoxImage.Error);
                    File.Delete(Utils.savePath);
                    File.WriteAllText(Utils.savePath, JsonConvert.SerializeObject(SettingsData));
                }
            }
            else
            {
                File.Delete(Utils.savePath);
                File.WriteAllText(Utils.savePath, JsonConvert.SerializeObject(SettingsData));
            }
        }

        /// <summary>
        /// Self-explanatory, saved in .json format.
        /// </summary>
        public static void SaveSettings()
        {
            File.WriteAllText(Utils.savePath, JsonConvert.SerializeObject(SettingsData));
        }
    }

    /// <summary>
    /// Settings Data -> class with all of the saving info from this app.
    /// </summary>
    public class SettingsData
    {
        public double volume = 0.5f;
        public int languageIndex = 0; // 0 -> English, 1 -> ru, 2 -> UA, 3 -> Japan
        public int searchEngineIndex = 0; // 0 -> Spotify, 1 -> YouTube
        public bool discordRPC = true;
        public bool economTraffic = false;
        public bool musicPlayerBallonTurnOn = true;
        public List<Playlist> playlists = new();

        /// <summary>
        /// A main Playlist class that can store Title, Image, Tracks. And handy function to calculate time. 
        /// </summary>
        public class Playlist
        {
            public string Title { get; set; } = string.Empty;
            public string ImagePath { get; set; } = string.Empty;
            public List<PlaylistItem> TracksInPlaylist { get; set; } = new List<PlaylistItem>();

            /// <summary>
            /// Calculates the amount of time you have to spend to listen whole playlist. 
            /// </summary>
            /// <returns>Amount of time to end playlist.</returns>
            public string CalculateAmountOfTimeToListenWholePlayList()
            {
                long duration = 0;
                foreach (var track in TracksInPlaylist)
                {
                    if (track.SpotifyTrack != null)
                    {
                        duration += track.SpotifyTrack.DurationMs;
                    }
                    else if (track.YouTubeTrack != null && track.YouTubeTrack.Duration != null)
                    {
                        duration += (long)track.YouTubeTrack.Duration.Value.TotalMilliseconds;
                    }
                }

                long seconds = duration / 1000;
                if (seconds < 60)
                {
                    return seconds + $" {Settings.GetLocalizationString("SecondsText")}.";
                }
                if (seconds < 3600)
                {
                    return (seconds / 60) + $" {Settings.GetLocalizationString("MinutesText")}, " + (seconds % 60) + $" {Settings.GetLocalizationString("SecondsText")}.";
                }
                return (seconds / 3600) + $" {Settings.GetLocalizationString("HoursText")}, " + (seconds % 3600 / 60) + $" {Settings.GetLocalizationString("MinutesText")}";

            }

            /// <summary>
            /// An class to store two types of tracks.
            /// </summary>
            public class PlaylistItem
            {
                // the reason why spotify track is also created as another item, is because the Spotify Explode Track class cannot give me set info. And because of that I cannot save the TrackSearchResult.
                public SpotifyTrackItem? SpotifyTrack { get; set; } = null;
                public YouTubeTrackItem? YouTubeTrack { get; set; } = null;
            }

            /// <summary>
            /// Items that I really need to store from TrackSearchResult.
            /// </summary>
            public class SpotifyTrackItem
            {
                public string Id { get; set; } = string.Empty;

                public string Url { get; set; } = string.Empty;

                public string Title { get; set; } = string.Empty;

                public long DurationMs { get; set; } = 0;

                public List<Artist> Artists { get; set; } = new List<Artist>();

                public Album Album { get; set; } = new Album();

                /// <summary>
                /// Do not remove it, because it breaks the loading of JSON info of this class.
                /// </summary>
                public SpotifyTrackItem() { }

                /// <summary>
                /// Setter
                /// </summary>
                public SpotifyTrackItem(TrackSearchResult result)
                {
                    Id = result.Id;
                    Url = result.Url;
                    Title = result.Title;
                    DurationMs = result.DurationMs;
                    Artists = result.Artists;
                    Album = result.Album;
                }

                /// <summary>
                /// Getter.
                /// </summary>
                public TrackSearchResult GetTrackSearchResult()
                {
                    return new()
                    {
                        Title = Title,
                        Id = Id,
                        DurationMs = DurationMs,
                        Artists = Artists,
                        Album = Album
                    };
                }

            }

            /// <summary>
            /// Items that I really need to store from VideoSearchResult.
            /// </summary>
            public class YouTubeTrackItem
            {
                public VideoId Id { get; set; }

                public string Url { get; set; } = string.Empty;

                public string Title { get; set; } = string.Empty;

                public Author? Author { get; set; }

                public TimeSpan? Duration { get; set; }

                public IReadOnlyList<Thumbnail>? Thumbnails { get; set; }

                /// <summary>
                /// Do not remove it, because it breaks the loading of JSON info of this class.
                /// </summary>
                public YouTubeTrackItem() { }

                /// <summary>
                /// Setter
                /// </summary>
                public YouTubeTrackItem(VideoSearchResult result)
                {
                    Id = result.Id;
                    Url = result.Url;
                    Title = result.Title;
                    Author = result.Author;
                    Duration = result.Duration;
                    Thumbnails = result.Thumbnails;
                }

                /// <summary>
                /// Getter.
                /// </summary>
                public VideoSearchResult? GetVideoSearchResult()
                {
                    if (Author != null && Thumbnails != null)
                    {
                        VideoSearchResult video = new(Id, Title, Author, Duration, Thumbnails);
                        return video;
                    }
                    return null;
                }
            }
        }
    }
}