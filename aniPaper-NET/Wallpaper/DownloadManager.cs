using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using static aniPaper_NET.Program;

namespace aniPaper_NET
{
    static partial class WallpaperManager
    {
        public static void DownloadWallpaperFromUrl(Wallpaper wallpaper)
        {
            ValidateFolder();

            try
            {
                if (!Directory.Exists(wallpaper.GetDirectory()))
                {
                    DirectoryInfo directory = downloads_directory.CreateSubdirectory(wallpaper.Title);

                    Task.WhenAll(DownloadThumbnailFromUrl(directory, wallpaper.GetWallpaperLink()),
                                DownloadImageFromUrl(directory, wallpaper.GetWallpaperLink().Replace("-thumb", "")))
                                .ContinueWith(delegate
                                {
                                    directory.MoveTo(wallpaper.GetDirectory());

                                    Application.Current.Dispatcher.Invoke(delegate
                                    {
                                        InstalledWallpapers.Add(wallpaper);
                                    });

                                    is_downloading = false;
                                });
                }
                else throw new Exception("Specified wallpaper already exists in your library");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                is_downloading = false;
            }
        }

        private static async Task DownloadImageFromUrl(DirectoryInfo wallpaper_directory, string wallpaper_link)
        {
            try
            {
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
    }
}
