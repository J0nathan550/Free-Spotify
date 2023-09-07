﻿using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NAudio.Wave;
using SpotifyExplode;
using SpotifyExplode.Tracks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams ;

namespace Spotify_Fetch
{
    public partial class MainWindow : Window
    {
        public SpotifyClient SpotifyFreeClient = new SpotifyClient();
        private YoutubeClient youtube;

        public MainWindow()
        {
            InitializeComponent();
            LoadingTracks();
        }

        private Track track;
        private string YouTubeID;

        private async void LoadingTracks()
        {
            track = await SpotifyFreeClient.Tracks.GetAsync("https://open.spotify.com/track/7rhOaEGYyPzY81e7acou1k?si=23b434cff5274100");
            YouTubeID = await SpotifyFreeClient.Tracks.GetYoutubeIdAsync("https://open.spotify.com/track/7rhOaEGYyPzY81e7acou1k?si=23b434cff5274100");

            TextBlock title = new TextBlock();
            title.Text = track.Title;
            title.Foreground = new SolidColorBrush(Color.FromArgb(255, 0,0 ,0));
            visuals.Children.Add(title);
            Button button = new Button();
            button.Content = "Play";
            button.Click += Button_Click;
            visuals.Children.Add(button);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlayMp3FromUrl(YouTubeID);
        }

        public async void PlayMp3FromUrl(string url)
        {
            await Task.Run(() =>
            {
                var youtube = new YoutubeClient();
                var video = youtube.Videos.GetAsync($"https://youtube.com/watch?v={YouTubeID}");
                var streamManifest = youtube.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={YouTubeID}");
                var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new MediaFoundationReader(streamInfo.Url))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Volume = 0.05f;
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }

            });
        }
    }
}