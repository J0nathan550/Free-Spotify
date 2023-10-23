using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Free_Spotify.Ballons
{
    public partial class MusicExplorerBallon : UserControl
    {
        private Timer windowAutoCloseAFKTimer = new Timer() { Interval = 1000 };
        private int countDownAFK = 5;

        public MusicExplorerBallon()
        {
            InitializeComponent();
            windowAutoCloseAFKTimer.Elapsed += WindowAutoCloseAFKTimer_Elapsed;
            countDownAFK = 5;
            windowAutoCloseAFKTimer.Start();
        }

        private async void WindowAutoCloseAFKTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            await Dispatcher.BeginInvoke(async () =>
            {
                if (MainWindow.window.WindowState == System.Windows.WindowState.Normal || MainWindow.window.WindowState == System.Windows.WindowState.Maximized)
                {
                    countDownAFK = 5;
                    return;
                }
                countDownAFK--;
                if (countDownAFK == 0)
                {
                    Storyboard sb = FindResource("FadeOutAnimation") as Storyboard;
                    sb.Begin(entireBaloon); // Start the animation on your user control (mp3PlayerHolder)
                    await Task.Delay(1000);
                    MainWindow.window.myNotifyIcon.CloseBalloon();
                    windowAutoCloseAFKTimer.Stop();
                }
            });
        }

        private async void BallonClose(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Storyboard sb = FindResource("FadeOutAnimation") as Storyboard;
            sb.Begin(entireBaloon); // Start the animation on your user control (mp3PlayerHolder)
            await Task.Delay(1000);
            MainWindow.window.myNotifyIcon.CloseBalloon();
        }

        private void Ballon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            countDownAFK = 5;
            if (windowAutoCloseAFKTimer.Enabled == true)
            {
                windowAutoCloseAFKTimer.Stop();
            }
        }

        private void Ballon_MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            countDownAFK = 5;
            if (windowAutoCloseAFKTimer.Enabled == true)
            {
                windowAutoCloseAFKTimer.Stop();
            }
        }
    }
}