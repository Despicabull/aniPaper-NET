namespace aniPaper_NET
{
    class AppSettings
    {
        private static readonly int max_browser_page = 500;
        private static readonly int max_folder_page = 500;
        private static int current_browser_page = 1;
        private static int current_folder_page = 1;

        public static Configuration.MainWindow _ConfigurationWindow;
        public static VLCPlayer.MainWindow _VLCPlayerWindow;

        public enum Navigation : int
        {
            Installed,
            Discover,
        }

        public static bool is_exit;
        public static bool search_flag = false;
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
            }
        }
        public static int Current_Folder_Page
        {
            get
            {
                return current_folder_page;
            }
            set
            {
                if (value < 1) value = 1;
                else if (value > max_folder_page) value = max_folder_page;
                current_folder_page = value;
            }
        }

        public static Navigation navigation = Navigation.Installed;
    }
}
