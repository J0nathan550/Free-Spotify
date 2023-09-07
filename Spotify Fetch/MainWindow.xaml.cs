using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NAudio.Wave;
using SpotifyExplode;
using SpotifyExplode.Tracks;

namespace Spotify_Fetch
{
    public partial class MainWindow : Window
    {
        public SpotifyClient SpotifyFreeClient = new SpotifyClient();

        public MainWindow()
        {
            InitializeComponent();
            LoadingTracks();
        }

        private Track track;

        private async void LoadingTracks()
        {
            track = await SpotifyFreeClient.Tracks.GetAsync("https://open.spotify.com/track/5f6XpCkieq8sbwqvBrqIlN?si=2a53db6377f34c4d");
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
            PlayMp3FromUrl(track.PreviewUrl);
        }

        public async static void PlayMp3FromUrl(string url)
        {
            await Task.Run(() =>
            {
                using (Stream ms = new MemoryStream())
                {
                    using (Stream stream = WebRequest.Create(url)
                        .GetResponse().GetResponseStream())
                    {
                        byte[] buffer = new byte[32768];
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                    }

                    ms.Position = 0;
                    using (WaveStream blockAlignedStream =
                        new BlockAlignReductionStream(
                            WaveFormatConversionStream.CreatePcmStream(
                                new Mp3FileReader(ms))))
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
                }
            });
        }
    }
}