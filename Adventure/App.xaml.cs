﻿using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Adventure
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex s_mutex = null;

        private Timer timer;
        private TaskbarIcon tb;

        public Photo currentWallpaper { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //initialize NotifyIcon
            // http://www.codeproject.com/Articles/36468/WPF-NotifyIcon
            tb = (TaskbarIcon)FindResource("WallpaperNotifyIcon");

            CommandBinding OpenCommandBinding = new CommandBinding(ApplicationCommands.Open, OpenCommandExecuted);
            CommandManager.RegisterClassCommandBinding(typeof(object), OpenCommandBinding);

            CommandBinding HelpCommandBinding = new CommandBinding(ApplicationCommands.Help, HelpCommandExecuted);
            CommandManager.RegisterClassCommandBinding(typeof(object), HelpCommandBinding);

            CommandBinding CloseCommandBinding = new CommandBinding(ApplicationCommands.Close, CloseCommandExecuted);
            CommandManager.RegisterClassCommandBinding(typeof(object), CloseCommandBinding);

            StartOnBoot();
            SetUpTimer();

            FetchTodaysWallpaper();
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            const string appName = "Adventure.Rocks";
            bool createdNew;

            s_mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("The application is already running :)\n\nLook for the icon in your System Tray.", "Adventure.Rocks", MessageBoxButton.OK, MessageBoxImage.Information);

                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }
        }

        private void OpenCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ToggleMainWindow();
        }

        private void HelpCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ToggleSettingsWindow();
        }

        public void ToggleMainWindow()
        {
            if (!IsWindowOpen<MainWindow>())
            {
                MainWindow window = new MainWindow();
                window.Show();
            }
            else
            {
                GetOpenWindow<MainWindow>().Close();
            }
        }

        public void ToggleSettingsWindow()
        {
            if (!IsWindowOpen<SettingsWindow>())
            {
                SettingsWindow window = new SettingsWindow();
                window.Show();
            }
            else
            {
                GetOpenWindow<SettingsWindow>().Close();
            }
        }

        private void CloseCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public static void StartOnBoot()
        {
            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            key.SetValue("Adventure", getExePath());
        }

        public static void RemoveOnBoot()
        {
            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            key.DeleteValue("Adventure", false);
        }

        private static string getExePath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static Window GetOpenWindow<T>() where T : Window
        {
            return Application.Current.Windows.OfType<T>().First(); ;
        }

        public void FetchTodaysWallpaper()
        {
            loadingStarted();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    Photo photo = AdventureUtils.GetTodaysPhoto();
                    SetWallpaper(photo);
                });

            bw.RunWorkerAsync();
        }

        public void FetchRandomWallpaper()
        {
            loadingStarted();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    Photo photo = AdventureUtils.GetRandomPhoto();
                    SetWallpaper(photo);
                });

            bw.RunWorkerAsync();
        }

        public void SetWallpaper(Photo photo)
        {
            if (photo != null)
            {
                currentWallpaper = photo;

                Uri uri = new Uri(photo.Image);
                Wallpaper.Set(uri, Wallpaper.Style.Centered);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(delegate
                {
                    if (MainWindow != null)
                    {
                        ((MainWindow)MainWindow).setImagePreviewAsync(currentWallpaper);
                    }
                }));
            }
        }

        public void loadingStarted()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(delegate
            {
                if (MainWindow != null)
                {
                    ((MainWindow)MainWindow).onLoadStart();
                }
            }));
        }

        public void SetUpTimer()
        {
            SetUpTimer(new TimeSpan(1, 0, 00));
        }

        public void SetUpTimer(TimeSpan alertTime)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                TimeSpan timeLeftToday = TimeSpan.FromDays(1.0) - current.TimeOfDay;
                timeToGo = timeLeftToday + alertTime;
            }

            this.timer = new Timer(x =>
            {
                FetchTodaysWallpaper();
                SetUpTimer();
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }
    }
}
