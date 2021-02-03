using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace aniPaper_NET
{
    static class Config
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

        public static bool run_on_startup = true;
        public static int volume = 50;
        public static WallpaperStyle style = WallpaperStyle.Fill;

        private static void SetStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (run_on_startup) key.SetValue(Assembly.GetEntryAssembly().GetName().Name, Application.ExecutablePath);
            else key.DeleteValue(Assembly.GetEntryAssembly().GetName().Name, false);
        }
        
        public static void LoadConfig()
        {
            string ini_path = Path.Combine(Directory.GetCurrentDirectory(), "config.ini");
            IniFile ini_file = new IniFile(ini_path);

            // Loads the .ini file and checks if every key is valid, otherwise write a new key
            if (ini_file.KeyExists("RunOnStartup", "General")) run_on_startup = bool.Parse(ini_file.Read("RunOnStartup", "General"));
            else ini_file.Write("RunOnStartup", run_on_startup.ToString(), "General");
            if (ini_file.KeyExists("Style", "Image")) style = (WallpaperStyle)int.Parse(ini_file.Read("Style", "Image"));
            else ini_file.Write("Style", ((int)style).ToString(), "Image");
            if (ini_file.KeyExists("Volume", "Video")) volume = int.Parse(ini_file.Read("Volume", "Video"));
            else ini_file.Write("Volume", volume.ToString(), "Video");

            SetStartup();
        }

        public static void SaveConfig()
        {
            string ini_path = Path.Combine(Directory.GetCurrentDirectory(), "config.ini");
            IniFile ini_file = new IniFile(ini_path);

            // Saves the wallpaper configuration into .ini file
            ini_file.Write("RunOnStartup", run_on_startup.ToString(), "General");
            ini_file.Write("Style", ((int)style).ToString(), "Image");
            ini_file.Write("Volume", volume.ToString(), "Video");

            SetStartup();
        }
    }
}
