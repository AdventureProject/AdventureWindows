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
                            imagePreview.Source = bitmapSource;
                        }));
                    }
                });

            bw.RunWorkerAsync();
        }

        public void TodaysWallpaper_Click(object sender, RoutedEventArgs e)
        {
            app().FetchTodaysWallpaper();
        }

        public void RandomWallpaper_Click(object sender, RoutedEventArgs e)
        {
            app().FetchRandomWallpaper();
        }

        public void Schedule_Click(object sender, RoutedEventArgs e)
        {
            app().SetUpTimer();
            MessageBox.Show("Schedule set");
        }

        public void StartBoot_Click(object sender, RoutedEventArgs e)
        {
            App.StartOnBoot();
            MessageBox.Show("Start on boot");
        }

        public void RemoveBoot_Click(object sender, RoutedEventArgs e)
        {
            App.RemoveOnBoot();
            MessageBox.Show("Cancel on boot");
        }

        public void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void LatestVersion_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private App app()
        {
            return ((App)Application.Current);
        }
    }
}   
