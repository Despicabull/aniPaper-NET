using System.Windows;
using static aniPaper_NET.Config;
using static aniPaper_NET.Program;

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

            #region Initialization

            combo_box_wallpaper_style.SelectedIndex = (int)WStyle;
            slider_volume.Value = Volume;
            checkbox_run_on_startup.IsChecked = Startup;

            #endregion
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
            WStyle = (WallpaperStyle)combo_box_wallpaper_style.SelectedIndex;
            Volume = (int)slider_volume.Value;
            Startup = (bool)checkbox_run_on_startup.IsChecked;
            SaveConfig();
            Close();
        }
    }
}
