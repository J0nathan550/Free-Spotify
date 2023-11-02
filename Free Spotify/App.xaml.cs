using Free_Spotify.Ballons;
using Free_Spotify.Classes;
using Free_Spotify.Pages;
using System.Windows;

namespace Free_Spotify
{
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                MusicPlayerPage.Instance?.cancelProgressSongTimer.Cancel();
                MusicExplorerBallon.Instance?.cancelBallon.Cancel();
                Settings.SaveSettings();
                base.OnExit(e);
            }
            catch
            {
                Settings.SaveSettings();
                base.OnExit(e);
            }
        }
    }
}