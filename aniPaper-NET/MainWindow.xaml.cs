using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using static aniPaper_NET.Config;
using static aniPaper_NET.Program;
using static aniPaper_NET.WallpaperManager;

namespace aniPaper_NET
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

            // Initializes the app settings to default
            navigation = Navigation.Installed;
            is_downloading = false;
            is_searching = false;
            search_flag = false;
            Current_Browser_Page = 1;

            // Loads configuration
            LoadConfig();

            // Sets the list view items source
            list_view_wallpapers.ItemsSource = InstalledWallpapers;

            // Disables the configuration, download, and wallpaper button
            btn_delete_wallpaper.IsEnabled = false;
            btn_download_wallpaper.IsEnabled = false;
            btn_set_wallpaper.IsEnabled = false;

            // Disables the previous, and next button
            btn_previous_page.IsEnabled = false;
            btn_next_page.IsEnabled = false;

            // Sets the current browser page
            label_page.Content = $"{Current_Browser_Page} / {max_browser_page}";

            // Loads the wallpaper
            LoadWallpaperFromFolder();

            #endregion
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Assembly.GetEntryAssembly().GetName().Version.ToString(), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            FileDialog file_dialog = new OpenFileDialog()
            {
                DefaultExt = ".bmp",
                Filter = "Images (*.bmp;*.jpeg;*.jpg;*.png)|*.bmp;*.jpeg;*.jpg;*.png|Videos (*.mp4;*.wmv;*.mov;*.avi)|*.mp4;*.wmv;*.mov;*.avi",
            };
            bool? dialog_ok = file_dialog.ShowDialog();

            if (dialog_ok == true)
            {
                // Resets list_view_wallpaper selection
                if (list_view_wallpapers.SelectedIndex != -1) list_view_wallpapers.SelectedIndex = -1;

                string wallpaper_file = file_dialog.FileName;
                string wallpaper_title = Path.GetFileNameWithoutExtension(wallpaper_file);
                WallpaperType wallpaper_type = (WallpaperType)file_dialog.FilterIndex;

                CreateWallpaperFolder(wallpaper_file, wallpaper_title, wallpaper_type);
            }
        }

        private void ConfigWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (_ConfigurationWindow == null)
            {
                _ConfigurationWindow = new Configuration.MainWindow();
                _ConfigurationWindow.Show();
            }
        }

        private void DeleteWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (list_view_wallpapers.SelectedIndex != -1)
            {
                DeleteWallpaper((Wallpaper)list_view_wallpapers.SelectedItem);

                list_view_wallpapers.SelectedIndex = -1;
            }
        }

        private void DownloadWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (list_view_wallpapers.SelectedIndex != -1)
            {
                if (!is_downloading)
                {
                    is_downloading = true;
                    DownloadWallpaperFromUrl((Wallpaper)list_view_wallpapers.SelectedItem);
                }
                else
                {
                    MessageBox.Show("Download is currently in progress", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Exit_Click(object sendeer, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ListViewWallpapers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (list_view_wallpapers.SelectedIndex != -1)
            {
                switch (list_view_navigation.SelectedIndex)
                {
                    case 0:
                        btn_delete_wallpaper.IsEnabled = true;
                        btn_download_wallpaper.IsEnabled = false;
                        btn_set_wallpaper.IsEnabled = true;
                        break;
                    case 1:
                        btn_delete_wallpaper.IsEnabled = false;
                        btn_download_wallpaper.IsEnabled = true;
                        btn_set_wallpaper.IsEnabled = false;
                        break;
                }

                Wallpaper wallpaper = (Wallpaper)list_view_wallpapers.SelectedItem;
                img_wallpaper.Source = wallpaper.Thumbnail;
            }
            else
            {
                btn_delete_wallpaper.IsEnabled = false;
                btn_download_wallpaper.IsEnabled = false;
                btn_set_wallpaper.IsEnabled = false;

                img_wallpaper.Source = null;
            }
        }

        private void ListViewNavigation_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (list_view_wallpapers != null)
            {
                // Resets list_view_wallpaper selection
                if (list_view_wallpapers.SelectedIndex != -1) list_view_wallpapers.SelectedIndex = -1;

                switch (list_view_navigation.SelectedIndex)
                {
                    case 0: // Installed
                        btn_next_page.IsEnabled = false;
                        btn_previous_page.IsEnabled = false;

                        navigation = Navigation.Installed;
                        list_view_wallpapers.ItemsSource = InstalledWallpapers;
                        break;
                    case 1: // Discover
                        btn_next_page.IsEnabled = true;
                        btn_previous_page.IsEnabled = true;

                        navigation = Navigation.Discover;
                        list_view_wallpapers.ItemsSource = DiscoveredWallpapers;
                        break;
                    default:
                        break;
                }
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (!is_searching)
            {
                is_searching = true;
                Current_Browser_Page++;
                label_page.Content = $"{Current_Browser_Page} / {max_browser_page}";
            }
            else
            {
                MessageBox.Show("Searching is currently in progress", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (!is_searching)
            {
                is_searching = true;
                Current_Browser_Page--;
                label_page.Content = $"{Current_Browser_Page} / {max_browser_page}";
            }
            else
            {
                MessageBox.Show("Searching is currently in progress", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (list_view_wallpapers.SelectedIndex != -1) SetWallpaper((Wallpaper)list_view_wallpapers.SelectedItem);
        }

        private void TextBoxSearch_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            string key = text_box_search.Text.Trim().ToLower();
            if (key == "search" && !search_flag)
            {
                search_flag = true;
                text_box_search.Text = "";
            }
        }

        private void TextBoxSearch_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            string key = text_box_search.Text.Trim().ToLower();
            // Resets the search hint
            if (key == "")
            {
                search_flag = false;
                text_box_search.Text = "Search";
            }
        }

        private void TextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (list_view_wallpapers != null)
            {
                // Resets list_view_wallpaper selection
                if (list_view_wallpapers.SelectedIndex != -1) list_view_wallpapers.SelectedIndex = -1;

                string key = text_box_search.Text.Trim().ToLower();
                if (search_flag)
                {
                    ObservableCollection<Wallpaper> DisplayedWallpapers = new ObservableCollection<Wallpaper>();
                    switch (list_view_navigation.SelectedIndex)
                    {
                        case 0: // Installed
                            foreach (Wallpaper wallpaper in InstalledWallpapers)
                            {
                                string t = wallpaper.Title.Trim().ToLower();
                                if (t.Contains(key))
                                {
                                    DisplayedWallpapers.Add(wallpaper);
                                }
                            }
                            break;
                        case 1: // Discover
                            foreach (Wallpaper wallpaper in DiscoveredWallpapers)
                            {
                                string t = wallpaper.Title.Trim().ToLower();
                                if (t.Contains(key))
                                {
                                    DisplayedWallpapers.Add(wallpaper);
                                }
                            }
                            break;
                    }

                    list_view_wallpapers.ItemsSource = DisplayedWallpapers;
                }
            }
        }
    }
}
