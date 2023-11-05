using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Free_Spotify.Classes
{
    /// <summary>
    /// Handy class to store some strings that are need to be used globally.
    /// </summary>
    public static class Utils
    {
        public const string savePath = "settings.json"; // constant string to represent file of settings. 
        public const string GithubLink = "https://github.com/J0nathan550/Free-Spotify";// later will add the link to the public repository (IF EVER!)
        // Link updates for different arch. 
        public const string DownloadAutoUpdateLinkX64 = "https://raw.githubusercontent.com/J0nathan550/Free-Spotify/master/AutoUpdateX64.xml";
        public const string DownloadAutoUpdateLinkX86 = "https://raw.githubusercontent.com/J0nathan550/Free-Spotify/master/AutoUpdateX86.xml";
        public const string ProjectName = "Free Spotify";

        public static bool IsPlayingFromPlaylist { get; set; } = false;
        public static string DefaultImagePath { get; set; } = $@"{System.AppDomain.CurrentDomain.BaseDirectory}\Assets\default-playlist-icon.png";
        public static async Task<BitmapImage> LoadImageAsync(string imageUrl)
        {
            using HttpClient client = new();
            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            BitmapImage image = new();

            using (MemoryStream stream = new(imageData))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }
    }
}