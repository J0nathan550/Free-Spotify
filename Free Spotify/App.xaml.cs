using Free_Spotify.Classes;
using Free_Spotify.Pages;
using System.Windows;

namespace Free_Spotify
{
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            SearchViewPage.searchWindow.cancelProgressSongTimer.Cancel();
            Utils.SaveSettings();
            base.OnExit(e);
        }
    }
}