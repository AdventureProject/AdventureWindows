using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Adventure
{
    public sealed class Wallpaper
    {
        public static readonly string WALLPAPER_FILE = "wallpaper.bmp";
        public static string GetWallpaperFile()
        {
            return Path.Combine(Path.GetTempPath(), WALLPAPER_FILE);
        }

        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        private static Point getMaxScreenSize()
        {
            Point size = new Point();

            int maxSize = 0;
            foreach (var screen in Screen.AllScreens)
            {
                int curSize = screen.Bounds.Width * screen.Bounds.Height;
                if (curSize > maxSize)
                {
                    maxSize = curSize;
                    size = new Point(screen.Bounds.Width, screen.Bounds.Height);
                }
            }

            return size;
        }

        public static void Set(Uri uri, Style style)
        {
            Image image = AdventureUtils.GetImage(uri);

            if (image != null)
            {
                Point maxSize = getMaxScreenSize();

                double ratioX = (double)maxSize.X / (double)image.Width;

                Image resizedImage = ResizeImage(image, (int)(image.Width * ratioX), (int)(image.Height * ratioX));
                image.Dispose();

                /*
                // Figure out the ratio
                double ratioX = (double)maxWidth / (double)image.Width;
                double ratioY = (double)maxHeight / (double)image.Height;
                // use whichever multiplier is smaller
                double ratio = ratioX < ratioY ? ratioX : ratioY;

                int newWidth = (int)((double)image.Width * ratio);
                int newHeight = (int)((double)image.Height * ratio);
                */

                int heightDelta = resizedImage.Height - maxSize.Y;

                Rectangle cropRect = new Rectangle(0, 0, maxSize.X, resizedImage.Height - heightDelta);
                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(resizedImage,
                                    new Rectangle(0, 0, target.Width, target.Height),
                                    cropRect,
                                    GraphicsUnit.Pixel);
                }

                string tempPath = GetWallpaperFile();
                if (!Directory.Exists(Path.GetTempPath()))
                {
                    Directory.CreateDirectory(Path.GetTempPath());
                }
                target.Save(tempPath, ImageFormat.Bmp);
                target.Dispose();

                resizedImage.Dispose();

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (style == Style.Stretched)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Centered)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Tiled)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }

                SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    tempPath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
