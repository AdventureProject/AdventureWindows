using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public static Photo GetPhoto( string url )
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

        public static string VERSION = @"3";
        public static async Task<Octokit.Release> getLatestVersion()
        {
            var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("AdventureWindows"));
            var releases = await github.Repository.Release.GetAll("Wavesonics", "AdventureWindows");
            return releases[0];
        }

        public static async void checkVersion()
        {
            if(m_lastVersionCheck == null || m_lastVersionCheck.AddHours(12).CompareTo( DateTime.Now ) < 0)
            {
                Console.Out.WriteLine("Checking GitHub for newer version");

                try
                {
                    var latest = await getLatestVersion();

                    if (!latest.Name.Equals(VERSION, StringComparison.Ordinal))
                    {
                        MessageBox.Show("New version avalible:\nLatest: " + latest.Name + "\nCurrent: " + VERSION);
                    }
                }
                catch(Octokit.RateLimitExceededException e )
                {
                    Console.Out.WriteLine("Rate limited by GitHub, backing off...");
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
