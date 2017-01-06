using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

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

            Photo currentWallpaper = app().currentWallpaper;
            if(currentWallpaper == null)
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
                            Image image = Image.FromFile(Wallpaper.GetWallpaperFile());
                            if (image != null)
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
                                imagePreview.Source = bitmapSource;

                                GC.Collect();

                                loadingView.Visibility = Visibility.Hidden;
                            }
                        }));
                    }
                });

            bw.RunWorkerAsync();
        }

        public void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }

        public void onLoadStart()
        {
            loadingView.Visibility = Visibility.Visible;
        }

        public void MoreInfo_Click(object sender, RoutedEventArgs e)
        {
            string url = app().currentWallpaper.Url;
            int end = url.IndexOf("/sizes/");
            string mainFlickrUrl;
            if (end > -1)
            {
                mainFlickrUrl = url.Substring(0, end);
            }
            else
            {
                mainFlickrUrl = url;
            }

            Process.Start(mainFlickrUrl);
        }

        private App app()
        {
            return ((App)Application.Current);
        }
    }
}   
