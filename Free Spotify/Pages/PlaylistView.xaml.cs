using Free_Spotify.Classes;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Free_Spotify.Pages
{
    public sealed partial class PlaylistView : Page
    {
        private SpotifyExplode.Tracks.Track track;

        public PlaylistView()
        {
            this.InitializeComponent();
            LoadingTracks();
        }

        private async void LoadingTracks()
        {
            track = await FreeSpotifyClient.SpotifyFreeClient.Tracks.GetAsync("https://open.spotify.com/track/01PX1U2dua0zNgqiRlykKP");
            TextBlock title = new TextBlock();
            title.Text = track.Title;
            title.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            visuals.Children.Add(title);
            Button button = new Button();
            button.Content = "Play";
            button.Click += Button_Click;
            visuals.Children.Add(button);
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //PlayMp3FromUrl(track.Url);
        }

        public static void PlayMp3FromUrl(string url)
        {

            //using (Stream ms = new MemoryStream())
            //{
            //    using (Stream stream = WebRequest.Create(url)
            //        .GetResponse().GetResponseStream())
            //    {
            //        byte[] buffer = new byte[32768];
            //        int read;
            //        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            //        {
            //            ms.Write(buffer, 0, read);
            //        }
            //    }

            //    ms.Position = 0;
            //    using (WaveStream blockAlignedStream =
            //        new BlockAlignReductionStream(
            //            WaveFormatConversionStream.CreatePcmStream(
            //                new Mp3FileReader(ms))))
            //    {
            //        using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
            //        {
            //            waveOut.Init(blockAlignedStream);
            //            waveOut.Play();
            //            while (waveOut.PlaybackState == PlaybackState.Playing)
            //            {
            //                Thread.Sleep(100);
            //            }
            //        }

        }
    }
}