using System.Windows;
using static aniPaper_NET.AppSettings;

namespace aniPaper_NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private System.Windows.Forms.NotifyIcon notify_icon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            /*
            notify_icon = new System.Windows.Forms.NotifyIcon();
            notify_icon.DoubleClick += (s, args) => ManageWallpaper();
            notify_icon.Icon = aniPaper_NET.Properties.Resources.AppIcon;
            notify_icon.Visible = true;

            CreateContextMenu();
            */
        }

        private void CreateContextMenu()
        {
            /*
            notify_icon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            notify_icon.ContextMenuStrip.Items.Add("Manage Wallpaper").Click += (s, e) => ManageWallpaper();
            notify_icon.ContextMenuStrip.Items.Add("Play Wallpaper").Click += (s, e) => PlayWallpaper();
            notify_icon.ContextMenuStrip.Items.Add("Pause Wallpaper").Click += (s, e) => PauseWallpaper();
            notify_icon.ContextMenuStrip.Items.Add("-");
            notify_icon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
            */
        }

        /*
        private void ExitApplication()
        {
            is_exit = true;
            MainWindow.Close();
            notify_icon.Dispose();
            notify_icon = null;
        }

        private void ManageWallpaper()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void PauseWallpaper()
        {
            if (_VLCPlayerWindow != null)
            {
                _VLCPlayerWindow.PausePlayer();
            }
        }

        private void PlayWallpaper()
        {
            if (_VLCPlayerWindow != null)
            {
                _VLCPlayerWindow.PlayMedia();
            }
        }
        */
    }
}
