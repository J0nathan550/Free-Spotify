using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Free_Spotify.Ballons
{
    public partial class MusicExplorerBallon : UserControl
    {

        /// <summary>
        /// Timer for automatically closing the balloon after a period of inactivity.
        /// </summary>
        public System.Timers.Timer windowAutoCloseAFKTimer = new() { Interval = 1000 };

        public CancellationTokenSource cancelBallon = new();

        /// <summary>
        /// Countdown value for auto-closing the balloon.
        /// </summary>
        private int countDownAFK = 5;

        /// <summary>
        /// Gets or sets the singleton instance of MusicExplorerBallon.
        /// </summary>
        public static MusicExplorerBallon? Instance { get; set; }

        /// <summary>
        /// Initializes a new instance of the MusicExplorerBallon.
        /// </summary>
        public MusicExplorerBallon()
        {
            InitializeComponent();
            windowAutoCloseAFKTimer.Elapsed += WindowAutoCloseAFKTimer_Elapsed;
            countDownAFK = 5;
            windowAutoCloseAFKTimer.Start();
            Instance = this;
        }

        /// <summary>
        /// Handles the elapsed event of the auto-close timer, ensuring the balloon closes after inactivity.
        /// </summary>
        private async void WindowAutoCloseAFKTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                if (cancelBallon.IsCancellationRequested)
                {
                    return;
                }
            }
            catch
            {
                cancelBallon.Cancel();
            }
            try
            {
                await Dispatcher.BeginInvoke(async () =>
                {
                    if (MainWindow.Window != null)
                    {
                        if (MainWindow.Window.WindowState is System.Windows.WindowState.Normal or System.Windows.WindowState.Maximized)
                        {
                            countDownAFK = 5;
                            return;
                        }
                        countDownAFK--;
                        if (countDownAFK == 0)
                        {
                            Storyboard? sb = FindResource("FadeOutAnimation") as Storyboard;
                            sb?.Begin(entireBaloon); // Start the animation on your user control (mp3PlayerHolder)
                            await Task.Delay(1000);
                            MainWindow.Window.myNotifyIcon.CloseBalloon();
                            windowAutoCloseAFKTimer.Stop();
                        }
                    }
                });
            }
            catch { /* stupid ass error that I don't know how to fix.*/ }
        }

        /// <summary>
        /// Handles the event when the user clicks to close the notification balloon.
        /// </summary>
        private async void BallonClose(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (cancelBallon.IsCancellationRequested)
                {
                    return;
                }
            }
            catch
            {
                cancelBallon.Cancel();
            }
            Storyboard? sb = FindResource("FadeOutAnimation") as Storyboard;
            sb?.Begin(entireBaloon); // Start the animation on your user control (mp3PlayerHolder)
            await Task.Delay(1000);
            MainWindow.Window?.myNotifyIcon.CloseBalloon();
        }

        /// <summary>
        /// Handles the event when the user clicks or hovers over the notification balloon, resetting the auto-close countdown.
        /// </summary>
        private void Ballon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (cancelBallon.IsCancellationRequested)
                {
                    return;
                }
            }
            catch
            {
                cancelBallon.Cancel();
            }
            countDownAFK = 5;
            if (windowAutoCloseAFKTimer.Enabled == true)
            {
                windowAutoCloseAFKTimer.Stop();
            }
        }

        /// <summary>
        /// Handles the event when the user hovers over the notification balloon, resetting the auto-close countdown.
        /// </summary>
        private void Ballon_MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (cancelBallon.IsCancellationRequested)
                {
                    return;
                }
            }
            catch
            {
                cancelBallon.Cancel();
            }
            countDownAFK = 5;
            if (windowAutoCloseAFKTimer.Enabled == true)
            {
                windowAutoCloseAFKTimer.Stop();
            }
        }

    }
}