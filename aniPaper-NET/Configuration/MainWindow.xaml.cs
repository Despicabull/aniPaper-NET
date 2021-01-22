using System.Windows;
using static aniPaper_NET.AppSettings;
using static aniPaper_NET.WallpaperConfig;

namespace aniPaper_NET.Configuration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Initialize
            LoadConfig();
            combo_box_wallpaper_style.SelectedIndex = (int)style;
            slider_volume.Value = volume;
        }

        private void CloseConfig_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _ConfigurationWindow = null;
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            style = (WallpaperStyle)combo_box_wallpaper_style.SelectedIndex;
            volume = (int)slider_volume.Value;
            SaveConfig();
            Close();
        }
    }
}
