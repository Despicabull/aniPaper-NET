using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using static aniPaper_NET.Program;

namespace aniPaper_NET
{
    class Config
    {
        public enum WallpaperStyle : int
        {
            Fill,
            Fit,
            Stretch,
            Tile,
            Center,
            Span,
        }

        private static bool startup = false;
        private static int volume = 50; // Video only
        private static WallpaperStyle wstyle = WallpaperStyle.Fill; // Image only

        public static bool Startup
        {
            get
            {
                return startup;
            }
            set
            {
                startup = value;
                SetStartup();
            }
        }
        public static int Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                if (_VLCPlayerWindow != null) _VLCPlayerWindow.SetVolume(volume);
            }
        }
        public static WallpaperStyle WStyle
        {
            get
            {
                return wstyle;
            }
            set
            {
                wstyle = value;
                SetStyle();
            }
        }

        private static void SetStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (Startup) key.SetValue(Assembly.GetEntryAssembly().GetName().Name, Application.ExecutablePath);
            else key.DeleteValue(Assembly.GetEntryAssembly().GetName().Name, false);

            key.Dispose();
        }

        private static void SetStyle()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (WStyle)
            {
                case WallpaperStyle.Fill:
                    key.SetValue(@"WallpaperStyle", 10.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                case WallpaperStyle.Fit:
                    key.SetValue(@"WallpaperStyle", 6.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                case WallpaperStyle.Span: // Windows 8 or newer only
                    key.SetValue(@"WallpaperStyle", 22.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                case WallpaperStyle.Stretch:
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                case WallpaperStyle.Tile:
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                    break;
                case WallpaperStyle.Center:
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                default:
                    break;
            }

            key.Dispose();
        }
        
        public static void LoadConfig()
        {
            string ini_path = Path.Combine(Directory.GetCurrentDirectory(), "config.ini");
            IniFile ini_file = new IniFile(ini_path);

            // Loads the .ini file and checks if every key is valid, otherwise write a new key
            if (ini_file.KeyExists("RunOnStartup", "General")) Startup = bool.Parse(ini_file.Read("RunOnStartup", "General"));
            else ini_file.Write("RunOnStartup", Startup.ToString(), "General");
            if (ini_file.KeyExists("Style", "Image")) WStyle = (WallpaperStyle)int.Parse(ini_file.Read("Style", "Image"));
            else ini_file.Write("Style", ((int)WStyle).ToString(), "Image");
            if (ini_file.KeyExists("Volume", "Video")) Volume = int.Parse(ini_file.Read("Volume", "Video"));
            else ini_file.Write("Volume", Volume.ToString(), "Video");
        }

        public static void SaveConfig()
        {
            string ini_path = Path.Combine(Directory.GetCurrentDirectory(), "config.ini");
            IniFile ini_file = new IniFile(ini_path);

            // Saves the wallpaper configuration into .ini file
            ini_file.Write("RunOnStartup", Startup.ToString(), "General");
            ini_file.Write("Style", ((int)WStyle).ToString(), "Image");
            ini_file.Write("Volume", Volume.ToString(), "Video");
        }
    }
}
