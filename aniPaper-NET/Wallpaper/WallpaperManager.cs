using HtmlAgilityPack;
using Microsoft.Win32;
using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static aniPaper_NET.Config;
using static aniPaper_NET.ImageProcessor;
using static aniPaper_NET.Program;
using static aniPaper_NET.Wallpaper;
using static aniPaper_NET.Win32;

namespace aniPaper_NET
{
    static partial class WallpaperManager
    {
        private delegate void WallpaperUpdater(ObservableCollection<Wallpaper> wallpapers, Wallpaper wallpaper);

        private static WallpaperUpdater update_wallpaper;

        private static readonly string web_url = "http://www.wallpapermaiden.com";
        private static readonly Regex illegal_characters = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);

        public static readonly DirectoryInfo downloads_directory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Downloads"));
        public static readonly DirectoryInfo wallpapers_directory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Wallpapers"));

        // A list where all installed and discovered wallpaper files are stored
        public static readonly ObservableCollection<Wallpaper> InstalledWallpapers = new ObservableCollection<Wallpaper>();
        public static readonly ObservableCollection<Wallpaper> DiscoveredWallpapers = new ObservableCollection<Wallpaper>();

        private static void AddWallpaper(ObservableCollection<Wallpaper> wallpapers, Wallpaper wallpaper)
        {
            wallpapers.Add(wallpaper);
        }

        private static void RemoveWallpaper(ObservableCollection<Wallpaper> wallpapers, Wallpaper wallpaper)
        {
            wallpapers.Remove(wallpaper);
        }

        private static void ValidateFolder()
        {
            // Creates a wallpapers path
            if (!wallpapers_directory.Exists) wallpapers_directory.Create();
            // Creates a download directory
            if (!downloads_directory.Exists) downloads_directory.Create();
        }

        public static void SetWallpaper(Wallpaper wallpaper)
        {
            ValidateFolder();

            switch (wallpaper.Type)
            {
                case (WallpaperType.Image):
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                    switch (style)
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
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaper.GetFile(), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                    break;
                case (WallpaperType.Video):
                    if (_VLCPlayerWindow == null)
                    {
                        _VLCPlayerWindow = new VLCPlayer.MainWindow(new string[] { wallpaper.GetFile() })
                        {
                            Width = SystemParameters.VirtualScreenWidth,
                            Height = SystemParameters.VirtualScreenHeight,
                        };
                        _VLCPlayerWindow.Show();
                    }
                    else
                    {
                        _VLCPlayerWindow.ChangeWallpaper(new string[] { wallpaper.GetFile() });
                    }
                    break;
                default:
                    break;
            }

            SaveConfig();
        }

        public static async void CreateWallpaperFolder(string file, string wallpaper_title, WallpaperType wallpaper_type)
        {
            ValidateFolder();

            DirectoryInfo directory = wallpapers_directory.CreateSubdirectory(wallpaper_title);

            string thumbnail_file = Path.Combine(directory.FullName, "thumbnail");
            string wallpaper_image_file = Path.Combine(directory.FullName, "wallpaper-image");
            string wallpaper_video_file = Path.Combine(directory.FullName, "wallpaper-video");

            Image wallpaper_image, thumbnail_image;
            Bitmap thumbnail_bitmap;
            BitmapImage thumbnail_bitmap_image;
            Wallpaper wallpaper;

            update_wallpaper = AddWallpaper;

            await Task.Run(() =>
            {
                try
                {
                    switch (wallpaper_type)
                    {
                        case WallpaperType.Image:
                            // Saves the wallpaper image and creating its thumbnail
                            File.Copy(file, wallpaper_image_file);
                            wallpaper_image = Image.FromFile(file);
                            thumbnail_bitmap = ConvertImageToBitmap(wallpaper_image, 270, 170);
                            thumbnail_bitmap.Save(thumbnail_file);

                            thumbnail_image = Image.FromFile(thumbnail_file);
                            thumbnail_bitmap_image = ConvertImageToBitmapImage(thumbnail_image);
                            wallpaper = new ImageWallpaper(wallpaper_title, thumbnail_bitmap_image);

                            wallpaper_image.Dispose();
                            thumbnail_image.Dispose();
                            thumbnail_bitmap.Dispose();

                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                update_wallpaper(InstalledWallpapers, wallpaper);
                            });
                            break;
                        case WallpaperType.Video:
                            // Saves the wallpaper video and creating its thumbnail
                            File.Copy(file, wallpaper_video_file);
                            using (MemoryStream stream = new MemoryStream())
                            {
                                FFMpegConverter ffmpeg = new FFMpegConverter();
                                ffmpeg.GetVideoThumbnail(file, stream);
                                wallpaper_image = Image.FromStream(stream);
                                thumbnail_bitmap = ConvertImageToBitmap(wallpaper_image, 270, 170);
                                thumbnail_bitmap.Save(thumbnail_file);

                                thumbnail_image = Image.FromFile(thumbnail_file);
                                thumbnail_bitmap_image = ConvertImageToBitmapImage(thumbnail_image);
                                wallpaper = new VideoWallpaper(wallpaper_title, thumbnail_bitmap_image);

                                wallpaper_image.Dispose();
                                thumbnail_image.Dispose();
                                thumbnail_bitmap.Dispose();

                                Application.Current.Dispatcher.Invoke(delegate
                                {
                                    update_wallpaper(InstalledWallpapers, wallpaper);
                                });
                            };
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public static async void DeleteWallpaper(Wallpaper wallpaper)
        {
            ValidateFolder();

            update_wallpaper = RemoveWallpaper;

            await Task.Run(() =>
            {
                try
                {
                    Directory.Delete(wallpaper.GetDirectory(), true);

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        update_wallpaper(InstalledWallpapers, wallpaper);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public static async void LoadWallpaperFromFolder()
        {
            ValidateFolder();

            InstalledWallpapers.Clear();

            IEnumerable<DirectoryInfo> wallpaper_directories = wallpapers_directory.EnumerateDirectories();

            update_wallpaper = AddWallpaper;

            await Task.Run(() =>
            {
                try
                {
                    foreach (DirectoryInfo directory in wallpaper_directories)
                    {
                        string thumbnail_file = Path.Combine(directory.FullName, "thumbnail");
                        string wallpaper_image_file = Path.Combine(directory.FullName, "wallpaper-image");
                        string wallpaper_video_file = Path.Combine(directory.FullName, "wallpaper-video");

                        string wallpaper_title = directory.Name;
                        Image thumbnail_image;
                        BitmapImage thumbnail_bitmap_image;
                        Wallpaper wallpaper;

                        if (File.Exists(wallpaper_image_file))
                        {
                            thumbnail_image = Image.FromFile(thumbnail_file);
                            thumbnail_bitmap_image = ConvertImageToBitmapImage(thumbnail_image);
                            wallpaper = new ImageWallpaper(wallpaper_title, thumbnail_bitmap_image);

                            thumbnail_image.Dispose();

                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                update_wallpaper(InstalledWallpapers, wallpaper);
                            });
                        }
                        else if (File.Exists(wallpaper_video_file))
                        {
                            thumbnail_image = Image.FromFile(thumbnail_file);
                            thumbnail_bitmap_image = ConvertImageToBitmapImage(thumbnail_image);
                            wallpaper = new VideoWallpaper(wallpaper_title, thumbnail_bitmap_image);

                            thumbnail_image.Dispose();

                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                update_wallpaper(InstalledWallpapers, wallpaper);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }).ContinueWith(t =>
            {
                is_searching = false;
            });
        }

        public static async void LoadWallpaperFromUrl()
        {
            ValidateFolder();

            DiscoveredWallpapers.Clear();

            string web_url_page = $"{web_url}/?page={Current_Browser_Page}";

            HtmlDocument document = new HtmlDocument();

            using (WebClient client = new WebClient())
            {
                string html_document = await client.DownloadStringTaskAsync(web_url_page);
                document.LoadHtml(html_document);
            }
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes(string.Format("//div[@class='wallpaperBg']"));

            update_wallpaper = AddWallpaper;

            await Task.Run(() =>
            {
                try
                {
                    foreach (HtmlNode node in nodes)
                    {
                        HtmlNode link_node = node.SelectSingleNode("a[@href]");
                        HtmlNode image_node = node.SelectSingleNode(".//img");
                        if (link_node != null && image_node != null)
                        {
                            // Gets the wallpaper title and removing illegal characters
                            string wallpaper_title = illegal_characters.Replace(link_node.Attributes["title"].Value, "");
                            string wallpaper_path = image_node.Attributes["src"].Value;
                            Image thumbnail_image;
                            BitmapImage thumbnail_bitmap_image;
                            Wallpaper wallpaper;

                            WebRequest request = WebRequest.Create($"{web_url}{wallpaper_path}");

                            using (WebResponse response = request.GetResponse())
                            {
                                using (Stream stream = response.GetResponseStream())
                                {
                                    thumbnail_image = Image.FromStream(stream);
                                    thumbnail_bitmap_image = ConvertImageToBitmapImage(thumbnail_image);
                                    wallpaper = new ImageWallpaper(wallpaper_title, wallpaper_path, thumbnail_bitmap_image);

                                    thumbnail_image.Dispose();

                                    Application.Current.Dispatcher.Invoke(delegate
                                    {
                                        update_wallpaper(DiscoveredWallpapers, wallpaper);
                                    });
                                };
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }).ContinueWith(t =>
            {
                is_searching = false;
            });
        }
    }
}
