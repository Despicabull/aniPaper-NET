using System;
using System.Windows;
using static aniPaper_NET.Program;

namespace aniPaper_NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon notify_icon;

        private void CreateContextMenu()
        {
            notify_icon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            notify_icon.ContextMenuStrip.Items.Add("Manage Wallpaper").Click += (s, e) => ManageWallpaper();
            notify_icon.ContextMenuStrip.Items.Add("-");
            notify_icon.ContextMenuStrip.Items.Add("Show / Hide Wallpaper").Click += (s, e) => ToggleWallpaperVisibility(); // Video only
            notify_icon.ContextMenuStrip.Items.Add("Play Wallpaper").Click += (s, e) => PlayWallpaper(); // Video only
            notify_icon.ContextMenuStrip.Items.Add("Pause Wallpaper").Click += (s, e) => PauseWallpaper(); // Video only
            notify_icon.ContextMenuStrip.Items.Add("-");
            notify_icon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) =>
            {
                Environment.Exit(0);
            };
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
            if (_VLCPlayerWindow != null && _VLCPlayerWindow.IsVisible)
            {
                _VLCPlayerWindow.PausePlayer();
            }
        }

        private void PlayWallpaper()
        {
            if (_VLCPlayerWindow != null && _VLCPlayerWindow.IsVisible)
            {
                _VLCPlayerWindow.PlayMedia();
            }
        }

        private void ToggleWallpaperVisibility()
        {
            if (_VLCPlayerWindow != null && !_VLCPlayerWindow.IsVisible)
            {
                _VLCPlayerWindow.Show();
                _VLCPlayerWindow.PlayMedia();
            }
            else if (_VLCPlayerWindow != null && _VLCPlayerWindow.IsVisible)
            {
                _VLCPlayerWindow.Hide();
                _VLCPlayerWindow.PausePlayer();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            notify_icon = new System.Windows.Forms.NotifyIcon();
            notify_icon.DoubleClick += (s, args) => ManageWallpaper();
            notify_icon.Icon = aniPaper_NET.Properties.Resources.AppIcon;
            notify_icon.Visible = true;

            CreateContextMenu();
        }
    }
}
