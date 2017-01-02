using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        public void setImagePreviewAsync(Photo photo)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    if (photo != null)
                    {
                        Uri uri = new Uri(photo.Image);
                        Stream s = new System.Net.WebClient().OpenRead(uri.ToString());
                        Image image = Image.FromStream(s);

                        var oldBitmap =
                            image as Bitmap ?? new Bitmap(image);

                        var bitmapSource =
                            System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                oldBitmap.GetHbitmap(Color.Transparent),
                                IntPtr.Zero,
                                new Int32Rect(0, 0, oldBitmap.Width, oldBitmap.Height),
                                null);

                        bitmapSource.Freeze();
                        imagePreview.Dispatcher.BeginInvoke(new Action(() => {
                            photoTitle.Content = photo.Title;

                            if (string.IsNullOrEmpty(photo.Description.Trim()))
                            {
                                descriptionLabel.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                descriptionLabel.Visibility = Visibility.Visible;
                            }
                            descriptionLabel.Content = photo.Description;
                            DateTime dateTime = DateTime.Parse(photo.Date);
                            photoDateLabel.Content = dateTime.ToString("dd MMMM yyyy");
                            imagePreview.Source = bitmapSource;

                            loadingView.Visibility = Visibility.Hidden;
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
