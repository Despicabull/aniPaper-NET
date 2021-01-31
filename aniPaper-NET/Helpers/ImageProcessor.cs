using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace aniPaper_NET
{
    static class ImageProcessor
    {
        public static Bitmap ConvertImageToBitmap(Image image, int target_width, int target_height)
        {
            float image_ratio = (float)image.Width / image.Height;
            float target_ratio = (float)target_width / target_height;
            // Chooses the largest value
            float x = Math.Max(image.Width, image.Height);

            int ratio_value;
            Bitmap bitmap = new Bitmap(target_width, target_height);
            Bitmap temp_bitmap;
            RectangleF dest_rect, src_rect;

            // Converts the image's aspect ratio to targeted aspect ratio
            if (image.Width > image.Height)
            {
                ratio_value = (int)(x * target_ratio / image_ratio);
                temp_bitmap = new Bitmap(ratio_value, image.Height);
                dest_rect = new RectangleF(0, 0, ratio_value, image.Height);
                src_rect = new RectangleF((image.Width - ratio_value) / 2, 0, ratio_value, image.Height);
            }
            else
            {
                ratio_value = (int)(x / target_ratio * image_ratio);
                temp_bitmap = new Bitmap(image.Width, ratio_value);
                dest_rect = new RectangleF(0, 0, image.Width, ratio_value);
                src_rect = new RectangleF(0, (image.Height - ratio_value) / 2, image.Width, ratio_value);
            }

            using (Graphics graphics = Graphics.FromImage(temp_bitmap))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.DrawImage(image, dest_rect, src_rect, GraphicsUnit.Pixel);
            };

            // Resize the converted image to specified size
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.DrawImage(temp_bitmap, 0, 0, target_width, target_height);
            }

            image.Dispose();

            return bitmap;
        }

        public static BitmapImage ConvertImageToBitmapImage(Image image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmap_image = new BitmapImage();
                bitmap_image.BeginInit();
                bitmap_image.StreamSource = memory;
                bitmap_image.CacheOption = BitmapCacheOption.OnLoad;
                bitmap_image.EndInit();
                bitmap_image.Freeze();

                image.Dispose();

                return bitmap_image;
            };
        }
    }
}
