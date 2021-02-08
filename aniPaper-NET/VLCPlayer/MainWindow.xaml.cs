using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using static aniPaper_NET.Config;
using static aniPaper_NET.Program;
using static aniPaper_NET.Win32;

namespace aniPaper_NET.VLCPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool media_ready;
        private string file_path;
        private LibVLC lib_vlc;
        private Media media;
        private MediaPlayer media_player;

        public MainWindow()
        {
            InitializeComponent();

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                lib_vlc.Dispose();
                media_ready = false;
                media_player.EndReached -= Media_Player_EndReached;
                media_player.Dispose();
                media.Dispose();
                _VLCPlayerWindow = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            IntPtr progman = FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;

            // Sends 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing 
            // happens.
            SendMessageTimeout(progman,
                            0x052C,
                            new IntPtr(0),
                            IntPtr.Zero,
                            SendMessageTimeoutFlags.SMTO_NORMAL,
                            1000,
                            out result);

            // Spy++ output
            // .....
            // 0x00010190 "" WorkerW
            //   ...
            //   0x000100EE "" SHELLDLL_DefView
            //     0x000100F0 "FolderView" SysListView32
            // 0x00100B8A "" WorkerW       <-- This is the WorkerW instance we are after!
            // 0x000100EC "Program Manager" Progman
            IntPtr worker_w = IntPtr.Zero;

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = FindWindowEx(tophandle,
                                    IntPtr.Zero,
                                    "SHELLDLL_DefView",
                                    IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    worker_w = FindWindowEx(IntPtr.Zero,
                                        tophandle,
                                        "WorkerW",
                                        IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);

            // Sets the window parent to WorkerW
            IntPtr window_handle = new WindowInteropHelper(this).Handle;
            SetParent(window_handle, worker_w);

            // Hides the window from the alt-tab switcher
            long style_new_window_extended = (long)WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            SetWindowLongPtr(new HandleRef(null, window_handle), GWL_EXSTYLE, (IntPtr)style_new_window_extended);
        }

        private void Media_Player_EndReached(object sender, EventArgs e)
        {
            if (media_player == null) return;

            // Loops the video played
            ThreadPool.QueueUserWorkItem(t => media_player.Play(media));
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            // Resets window position
            Top = 0;
            Left = 0;
            // Resets window size
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
        }

        private void VideoView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Core.Initialize();

                lib_vlc = new LibVLC("--no-disable-screensaver");
                media_player = new MediaPlayer(lib_vlc)
                {
                    AspectRatio = "Fill",
                    EnableHardwareDecoding = true,
                    Volume = Volume,
                };
                // Adds event handler when media player has ended
                media_player.EndReached += Media_Player_EndReached;

                video_view_wallpaper.MediaPlayer = media_player;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ChangeWallpaper(string[] args)
        {
            Dispatcher.Invoke(delegate
            {
                Show();
            });

            file_path = args[0];
            media = new Media(lib_vlc, file_path, FromType.FromPath);
            media_player.Play(media);

            media_ready = true;
        }

        public void PausePlayer()
        {
            if (media_player == null) return;

            if (media_player.IsPlaying && media_ready) media_player.Pause();
        }

        public void PlayMedia()
        {
            if (media_player == null) return;

            if (!media_player.IsPlaying && media_ready) media_player.Play();
        }

        public void SetVolume(int volume)
        {
            if (media_player == null) return;

            if (media_ready) media_player.Volume = volume;
        }

        public void StopPlayer()
        {
            Dispatcher.Invoke(delegate
            {
                Hide();
            });

            if (media_player == null) return;

            if (media_ready) media_player.Stop();
        }
    }
}
