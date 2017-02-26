using System.Windows;
using System.Windows.Documents;
using System.Diagnostics;


namespace Adventure
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            versionTextBlock.Text = "V " + AdventureUtils.VERSION;
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

        public void VisitAdventure_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        public void TodaysWallpaper_Click(object sender, RoutedEventArgs e)
        {
            app().FetchTodaysWallpaper();
        }

        public void RandomWallpaper_Click(object sender, RoutedEventArgs e)
        {
            app().FetchRandomWallpaper();
        }

        private App app()
        {
            return ((App)Application.Current);
        }
    }
}
