using System.IO;
using System.Windows.Media.Imaging;
using static aniPaper_NET.WallpaperManager;

namespace aniPaper_NET
{
    public enum WallpaperType : int
    {
        Image = 1,
        Video = 2,
    };

    abstract class Wallpaper
    {
        private readonly string title;
        private readonly string link;
        private readonly WallpaperType type;
        private readonly BitmapImage thumbnail;

        // For data binding purposes
        public string Title
        {
            get
            {
                return title;
            }
        }
        public string Link
        {
            get
            {
                return link;
            }
        }
        public WallpaperType Type
        {
            get
            {
                return type;
            }
        }
        public BitmapImage Thumbnail
        {
            get
            {
                return thumbnail;
            }
        }

        public Wallpaper(string title, WallpaperType type, BitmapImage thumbnail = null)
        {
            this.title = title;
            this.type = type;
            this.thumbnail = thumbnail;
        }

        public Wallpaper(string title, string link, WallpaperType type, BitmapImage thumbnail = null)
        {
            this.title = title;
            this.link = link;
            this.type = type;
            this.thumbnail = thumbnail;
        }

        public abstract string GetFile();

        public string GetDirectory()
        {
            return Path.Combine(wallpapers_directory.FullName, Title);
        }

        public string GetThumbnail()
        {
            return Path.Combine(GetDirectory(), "thumbnail");
        }
    }

    sealed class ImageWallpaper : Wallpaper
    {
        public ImageWallpaper(string title, BitmapImage thumbnail = null, WallpaperType type = WallpaperType.Image) : base(title, type, thumbnail) { }
        public ImageWallpaper(string title, string link, BitmapImage thumbnail = null, WallpaperType type = WallpaperType.Image) : base(title, link, type, thumbnail) { }

        public override string GetFile()
        {
            return Path.Combine(GetDirectory(), "wallpaper-image");
        }
    }

    sealed class VideoWallpaper : Wallpaper
    {
        public VideoWallpaper(string title, BitmapImage thumbnail = null, WallpaperType type = WallpaperType.Video) : base(title, type, thumbnail) { }
        public VideoWallpaper(string title, string link, BitmapImage thumbnail = null, WallpaperType type = WallpaperType.Video) : base(title, link, type, thumbnail) { }

        public override string GetFile()
        {
            return Path.Combine(GetDirectory(), "wallpaper-video");
        }
    }
}
