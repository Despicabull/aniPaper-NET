using static aniPaper_NET.WallpaperManager;

namespace aniPaper_NET
{
    class Program
    {
        private static int current_browser_page = 1;

        public static Configuration.MainWindow _ConfigurationWindow;
        public static VLCPlayer.MainWindow _VLCPlayerWindow;

        public enum Navigation : int
        {
            Installed,
            Discover,
        }

        public static bool is_downloading;
        public static bool is_searching;
        public static bool search_flag;
        public static int Current_Browser_Page
        {
            get
            {
                return current_browser_page;
            }
            set
            {
                if (value < 1) value = 1;
                else if (value > max_browser_page) value = max_browser_page;
                current_browser_page = value;
                LoadWallpaperFromUrl();
            }
        }
        public static readonly int max_browser_page = 500;

        public static Navigation navigation;
    }
}
