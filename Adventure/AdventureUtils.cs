using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;

namespace Adventure
{
    class AdventureUtils
    {
        private static DateTime m_lastVersionCheck;

        public static Stream MakeRequest(string requestUrl)
        {
            Stream responseStream;

            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                    "Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));

                responseStream = response.GetResponseStream();
                return responseStream;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static Photo GetPhoto(string url)
        {
            Photo photo = null;

            using (Stream response = MakeRequest(url))
            {
                if (response != null)
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Photo));
                    object objResponse = jsonSerializer.ReadObject(response);
                    photo = objResponse as Photo;
                }
            }

            return photo;
        }

        public static Image DownloadImage(String url)
        {
            Uri uri = new Uri(url);
            return DownloadImage(uri);
        }

        public static Image DownloadImage(Uri uri)
        {
            Image image;

            using (Stream s = new System.Net.WebClient().OpenRead(uri.ToString()))
            {
                image = Image.FromStream(s);
            }

            return image;
        }

        public static Image GetImage(String url)
        {
            Uri uri = new Uri(url);
            return GetImage(uri);
        }

        private const string GOOGLE_MAPS_API_KEY = "AIzaSyBQEg1oMy2cc1AqyYAf7HVaGwTYmVVOYtg";

        public static Uri GetZoomedInMapUrl(string location)
        {
            string baseUrl =
                    "http://maps.googleapis.com/maps/api/staticmap?center={1:s}&zoom=15&scale=1&size=400x480&maptype=terrain&key={0:s}&format=png&visual_refresh=true&markers=size:mid%7Ccolor:0xff0000%7Clabel:%7C{1:s}";
            string url = String.Format(baseUrl, GOOGLE_MAPS_API_KEY, location);
            return new Uri(url);
        }

        public static Uri GetZoomedOutMapUrl(string location)
        {
            string baseUrl =
                    "http://maps.googleapis.com/maps/api/staticmap?center={1:s}&zoom=6&scale=1&size=400x480&maptype=terrain&key={0:s}&format=png&visual_refresh=true&markers=size:mid%7Ccolor:0xff0000%7Clabel:%7C{1:s}";
            string url = String.Format(baseUrl, GOOGLE_MAPS_API_KEY, location);
            return new Uri(url);
        }

        public static Image GetZoomedInMapImage(string location)
        {
            if (!String.IsNullOrEmpty(location))
            {
                Uri mapUri = GetZoomedInMapUrl(location);
                return GetImage(mapUri);
            }
            else
            {
                return null;
            }
        }

        public static Image GetZoomedOutMapImage(string location)
        {
            if (!String.IsNullOrEmpty(location))
            {
                Uri mapUri = GetZoomedOutMapUrl(location);
                return GetImage(mapUri);
            }
            else
            {
                return null;
            }
        }

        public static Image GetImage(Uri uri)
        {
            string tempPath = System.IO.Path.GetTempPath();
            string urlHash = GetStringSha256Hash(uri.AbsoluteUri);
            string filePath = tempPath + Path.DirectorySeparatorChar + urlHash;

            Image image;
            if (File.Exists(filePath))
            {
                image = Image.FromFile(filePath);
            }
            else
            {
                image = DownloadImage(uri);
                if (image != null)
                {
                    image.Save(filePath);
                }
            }

            return image;
        }

        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public static string URL_TODAYS_WALLPAPER = @"http://wethinkadventure.rocks/todayswallpaper";
        public static string URL_RANDOM_WALLPAPER = @"http://wethinkadventure.rocks/random";

        public static Photo GetTodaysPhoto()
        {
            return GetPhoto(URL_TODAYS_WALLPAPER);
        }

        public static Photo GetRandomPhoto()
        {
            return GetPhoto(URL_RANDOM_WALLPAPER);
        }

        public static string VERSION = @"6";
        public static async Task<Octokit.Release> getLatestVersion()
        {
            var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("AdventureWindows"));
            var releases = await github.Repository.Release.GetAll("Wavesonics", "AdventureWindows");
            return releases[0];
        }

        public static async void checkVersion()
        {
            if (m_lastVersionCheck == null || m_lastVersionCheck.AddHours(12).CompareTo(DateTime.Now) < 0)
            {
                Console.Out.WriteLine("Checking GitHub for newer version");

                try
                {
                    var latest = await getLatestVersion();

                    if (!latest.Name.Equals(VERSION, StringComparison.Ordinal))
                    {
                        MessageBox.Show("New version avalible:\nLatest: " + latest.Name + "\nCurrent: " + VERSION + "\n\nGo to Settings to download the latest version.", "Adventure.Rocks", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Octokit.RateLimitExceededException e)
                {
                    Console.Out.WriteLine("Rate limited by GitHub, backing off...");
                }
                catch(System.Net.Http.HttpRequestException e)
                {
                    Console.Out.WriteLine("Failed to connect to GitHub.");
                }
                finally
                {
                    m_lastVersionCheck = DateTime.Now;
                }
            }
            else
            {
                Console.Out.WriteLine("Not checking GitHub for newer version. Too soon.");
            }
        }
    }
}
