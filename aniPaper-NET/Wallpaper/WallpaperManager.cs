﻿using HtmlAgilityPack;
using Microsoft.Win32;
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
using static aniPaper_NET.AppSettings;
using static aniPaper_NET.Helpers.Win32;
using static aniPaper_NET.ImageProcessor;
using static aniPaper_NET.Wallpaper;
using static aniPaper_NET.WallpaperConfig;

namespace aniPaper_NET
{
    class WallpaperManager
    {
        public static readonly DirectoryInfo downloads_directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Downloads"));
        public static readonly DirectoryInfo wallpapers_directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Wallpapers"));

        // A list where all installed and discovered wallpaper files are stored
        public static readonly ObservableCollection<Wallpaper> InstalledWallpapers = new ObservableCollection<Wallpaper>();
        public static readonly ObservableCollection<Wallpaper> DiscoveredWallpapers = new ObservableCollection<Wallpaper>();

        private static readonly string web_url = "http://www.wallpapermaiden.com";
        private static readonly Regex illegal_characters = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);

        private static void ValidateFolder()
        {
            // Creates a wallpapers path
            if (!wallpapers_directory.Exists) wallpapers_directory.Create();
            // Creates a download directory
            if (!downloads_directory.Exists) downloads_directory.Create();
        }

        /// <summary>
        /// TODO: Complete function fix bug {this}
        /// </summary>
        public static void CreateWallpaperFolder(string file, string wallpaper_title, WallpaperType wallpaper_type)
        {
            try
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
                            InstalledWallpapers.Add(wallpaper);
                        });
                        break;
                    case WallpaperType.Video:
                        // Saves the wallpaper video and creating its thumbnail
                        File.Copy(file, wallpaper_video_file);

                        wallpaper = new VideoWallpaper(wallpaper_title);

                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            InstalledWallpapers.Add(wallpaper);
                        });
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void DeleteWallpaper(Wallpaper wallpaper)
        {
            try
            {
                ValidateFolder();

                Directory.Delete(wallpaper.GetDirectory(), true);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    InstalledWallpapers.Remove(wallpaper);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void DownloadWallpaperFromUrl(Wallpaper wallpaper)
        {
            try
            {
                DirectoryInfo directory = downloads_directory.CreateSubdirectory(wallpaper.Title);

                Task.WhenAll(DownloadThumbnailFromUrl(directory, wallpaper.Link),
                            DownloadImageFromUrl(directory, wallpaper.Link.Replace("-thumb", "")))
                            .ContinueWith(t =>
                            {
                                directory.MoveTo(wallpaper.GetDirectory());

                                Application.Current.Dispatcher.Invoke(delegate
                                {
                                    InstalledWallpapers.Add(wallpaper);
                                });
                            });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// TODO: Complete function
        /// </summary>
        public static void SetWallpaper(Wallpaper wallpaper)
        {
            try
            {
                ValidateFolder();

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                switch (wallpaper.Type)
                {
                    case (WallpaperType.Image):
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
                            case WallpaperStyle.Span:
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
                        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaper.GetWallpaper(), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                        break;
                    case (WallpaperType.Video):
                        if (_VLCPlayerWindow == null)
                        {
                            _VLCPlayerWindow = new VLCPlayer.MainWindow(new string[]{ wallpaper.GetWallpaper() })
                            {
                                Width = SystemParameters.VirtualScreenWidth,
                                Height = SystemParameters.VirtualScreenHeight,
                            };
                            _VLCPlayerWindow.Show();
                        }
                        else
                        {
                            _VLCPlayerWindow.ChangeWallpaper(new string[] { wallpaper.GetWallpaper() });
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async Task DownloadImageFromUrl(DirectoryInfo wallpaper_directory, string wallpaper_link)
        {
            try
            {
                ValidateFolder();

                using (WebClient client = new WebClient())
                {
                    // Removes the -thumb value in the url to download the original image size
                    string url = $"{web_url}{wallpaper_link}";
                    string temp_file = Path.Combine(wallpaper_directory.FullName, "wallpaper-image");

                    await client.DownloadFileTaskAsync(new Uri(url), temp_file);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async Task DownloadThumbnailFromUrl(DirectoryInfo wallpaper_directory, string wallpaper_link)
        {
            try
            {
                ValidateFolder();

                using (WebClient client = new WebClient())
                {
                    // Removes the -thumb value in the url to download the original image size
                    string url = $"{web_url}{wallpaper_link}";
                    string temp_file = Path.Combine(wallpaper_directory.FullName, "thumbnail");

                    await client.DownloadFileTaskAsync(new Uri(url), temp_file);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// TODO: Complete function
        /// </summary>
        public static async void LoadWallpaperFromFolder()
        {
            try
            {
                ValidateFolder();

                InstalledWallpapers.Clear();

                IEnumerable<DirectoryInfo> wallpaper_directories = wallpapers_directory.EnumerateDirectories();

                foreach (DirectoryInfo directory in wallpaper_directories)
                {
                    await Task.Run(() =>
                    {
                        string thumbnail_file = Path.Combine(directory.FullName, "thumbnail");
                        string wallpaper_image_file = Path.Combine(directory.FullName, "wallpaper-image");
                        string wallpaper_video_file = Path.Combine(directory.FullName, "wallpaper-video");

                        string wallpaper_title = directory.Name;
                        Wallpaper wallpaper;

                        if (File.Exists(wallpaper_image_file))
                        {
                            Image thumbnail_image = Image.FromFile(thumbnail_file);
                            BitmapImage thumbnail_bitmap_image = ConvertImageToBitmapImage(thumbnail_image);
                            wallpaper = new ImageWallpaper(wallpaper_title, thumbnail_bitmap_image);

                            thumbnail_image.Dispose();

                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                InstalledWallpapers.Add(wallpaper);
                            });
                        }
                        else if (File.Exists(wallpaper_video_file))
                        {
                            wallpaper = new VideoWallpaper(wallpaper_title);

                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                InstalledWallpapers.Add(wallpaper);
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// TODO: Complete function {this}
        /// </summary>
        public static async void LoadWallpaperFromUrl()
        {
            try
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

                foreach (HtmlNode node in nodes)
                {
                    await Task.Run(() =>
                    {
                        HtmlNode link_node = node.SelectSingleNode("a[@href]");
                        HtmlNode image_node = node.SelectSingleNode(".//img");
                        if (link_node != null && image_node != null)
                        {
                            // Gets the wallpaper title and removing illegal characters
                            string wallpaper_title = illegal_characters.Replace(link_node.Attributes["title"].Value, "");
                            string wallpaper_path = image_node.Attributes["src"].Value;

                            WebRequest request = WebRequest.Create($"{web_url}{wallpaper_path}");

                            using (WebResponse response = request.GetResponse())
                            {
                                using (Stream stream = response.GetResponseStream())
                                {
                                    Image thumbnail_image = Image.FromStream(stream);
                                    BitmapImage thumbnail_bitmap_image = ConvertImageToBitmapImage(thumbnail_image);
                                    ImageWallpaper image_wallpaper = new ImageWallpaper(wallpaper_title, wallpaper_path, thumbnail_bitmap_image);

                                    thumbnail_image.Dispose();

                                    Application.Current.Dispatcher.Invoke(delegate
                                    {
                                        DiscoveredWallpapers.Add(image_wallpaper);
                                    });
                                };
                            };
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
