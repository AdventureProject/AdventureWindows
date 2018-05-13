using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Adventure
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AdventureUtils.checkVersion();

            // Clear out the design time values
            photoTitle.Text = "";
            descriptionLabel.Text = "";
            photoDateLabel.Text = "";


            Photo currentWallpaper = app().currentWallpaper;
            if (currentWallpaper == null)
            {
                setImagePreviewAsync(AdventureUtils.URL_TODAYS_WALLPAPER);
            }
            else
            {
                setImagePreviewAsync(currentWallpaper);
            }
        }

        private void WallpaperSet(object target, ExecutedRoutedEventArgs e)
        {
            setImagePreviewAsync(app().currentWallpaper);
        }

        private void setImagePreviewAsync(string url)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    Photo photo = AdventureUtils.GetPhoto(AdventureUtils.URL_TODAYS_WALLPAPER);
                    setImagePreviewAsync(photo);
                });

            bw.RunWorkerAsync();
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public void setImagePreviewAsync(Photo photo)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    if (photo != null)
                    {
                        imagePreview.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Image wallpaperImage = Image.FromFile(Wallpaper.GetWallpaperFile());
                            if (wallpaperImage != null)
                            {
                                BitmapSource wallpaperSource = convertToBitmapSource(wallpaperImage);

                                Image zoomedInMapimage = AdventureUtils.GetZoomedInMapImage(photo.Location);
                                Image zoomedOutMapimage = AdventureUtils.GetZoomedOutMapImage(photo.Location);

                                BitmapSource zoomedInMapSource = convertToBitmapSource(zoomedInMapimage);
                                BitmapSource zoomedOutMapSource = convertToBitmapSource(zoomedOutMapimage);

                                photoTitle.Text = photo.Title;

                                if (string.IsNullOrEmpty(photo.Description.Trim()))
                                {
                                    descriptionLabel.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    descriptionLabel.Visibility = Visibility.Visible;
                                }
                                descriptionLabel.Text = photo.Description;
                                DateTime dateTime = DateTime.Parse(photo.Date);
                                photoDateLabel.Text = dateTime.ToString("dd MMMM yyyy");
                                imagePreview.Source = wallpaperSource;

                                //zoomedInImage.Source = new BitmapImage(AdventureUtils.GetZoomedInMapUrl(photo.Location));
                                //zoomedOutImage.Source = new BitmapImage(AdventureUtils.GetZoomedOutMapUrl(photo.Location));

                                zoomedInImage.Source = zoomedInMapSource;
                                zoomedOutImage.Source = zoomedOutMapSource;

                                GC.Collect();

                                loadingView.Visibility = Visibility.Hidden;
                            }
                        }));
                    }
                });

            bw.RunWorkerAsync();
        }

        private BitmapSource convertToBitmapSource(Image image)
        {
            var oldBitmap = image as Bitmap ?? new Bitmap(image);

            IntPtr ip = oldBitmap.GetHbitmap(Color.Transparent);

            var bitmapSource =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ip,
                    IntPtr.Zero,
                    new Int32Rect(0, 0, oldBitmap.Width, oldBitmap.Height),
                    null);

            DeleteObject(ip);
            oldBitmap.Dispose();
            image.Dispose();

            bitmapSource.Freeze();

            return bitmapSource;
        }

        public void Settings_Click(object sender, RoutedEventArgs e)
        {
            app().ToggleSettingsWindow();
        }

        public void onLoadStart()
        {
            loadingView.Visibility = Visibility.Visible;
        }

        public void MoreInfo_Click(object sender, RoutedEventArgs e)
        {
            string url = app().currentWallpaper.Url;

            Process.Start(url);
        }

        private App app()
        {
            return ((App)Application.Current);
        }

        private void zoomedInImage_MouseEnter(object sender, MouseEventArgs e)
        {
            zoomedInImage.Opacity = 1.0;
        }

        private void zoomedInImage_MouseLeave(object sender, MouseEventArgs e)
        {
            zoomedInImage.Opacity = 0.5;
        }

        private void zoomedOutImage_MouseEnter(object sender, MouseEventArgs e)
        {
            zoomedOutImage.Opacity = 1.0;
        }

        private void zoomedOutImage_MouseLeave(object sender, MouseEventArgs e)
        {
            zoomedOutImage.Opacity = 0.5;
        }
    }
}
