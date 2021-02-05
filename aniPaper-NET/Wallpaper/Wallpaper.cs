using System.IO;
using System.Windows.Media.Imaging;
using static aniPaper_NET.WallpaperManager;

namespace aniPaper_NET
{
    abstract class Wallpaper
    {
        public enum WallpaperType : int
        {
            Image = 1,
            Video = 2,
        };

        private readonly string Link;

        // For data binding purposes
        public string Title { get; }
        public WallpaperType Type { get; }
        public BitmapImage Thumbnail { get; }

        public Wallpaper(string Title, WallpaperType Type, BitmapImage Thumbnail = null)
        {
            this.Title = Title;
            this.Type = Type;
            this.Thumbnail = Thumbnail;
        }

        public Wallpaper(string Title, string Link, WallpaperType Type, BitmapImage Thumbnail = null)
        {
            this.Title = Title;
            this.Link = Link;
            this.Type = Type;
            this.Thumbnail = Thumbnail;
        }

        public string GetDirectory()
        {
            return Path.Combine(wallpapers_directory.FullName, Title);
        }

        public abstract string GetFile();

        public string GetThumbnail()
        {
            return Path.Combine(GetDirectory(), "thumbnail");
        }

        public string GetWallpaperLink()
        {
            return Link;
        }
    }

    sealed class ImageWallpaper : Wallpaper
    {
        public ImageWallpaper(string Title, BitmapImage Thumbnail = null, WallpaperType Type = WallpaperType.Image) : base(Title, Type, Thumbnail) { }
        public ImageWallpaper(string Title, string Link, BitmapImage Thumbnail = null, WallpaperType Type = WallpaperType.Image) : base(Title, Link, Type, Thumbnail) { }

        public override string GetFile()
        {
            return Path.Combine(GetDirectory(), "wallpaper-image");
        }
    }

    sealed class VideoWallpaper : Wallpaper
    {
        public VideoWallpaper(string Title, BitmapImage Thumbnail = null, WallpaperType Type = WallpaperType.Video) : base(Title, Type, Thumbnail) { }
        public VideoWallpaper(string Title, string Link, BitmapImage Thumbnail = null, WallpaperType Type = WallpaperType.Video) : base(Title, Link, Type, Thumbnail) { }

        public override string GetFile()
        {
            return Path.Combine(GetDirectory(), "wallpaper-video");
        }
    }
}
