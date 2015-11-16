﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class AdventureUtils
    {
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

            Stream response = MakeRequest(url);
            using (response)
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
    }
}
