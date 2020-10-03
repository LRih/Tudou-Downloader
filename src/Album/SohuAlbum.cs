using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public class SohuAlbum : Album
    {
        //===================================================================== VARIABLES
        private dynamic Json;

        //===================================================================== INITIALIZE
        public SohuAlbum(string url) : base(url)
        {
        }

        //===================================================================== FUNCTIONS
        protected override void GenerateInfo(string src)
        {
            Json = GetJson(src);

            CoverUrl = Json.pic240_330;
            Title = Json.albumName;
            IsMovie = false;
            TotalEpisodes = Json.size;
            Region = Json.area;
            Year = Json.publishYear;
        }

        protected override List<Episode> GetMovie(string src)
        {
            throw new NotImplementedException();
        }
        protected override List<Episode> GetDrama(string src)
        {
            List<Episode> results = new List<Episode>();

            for (int i = 0; i < Json.videos.Count; i++)
            {
                var video = Json.videos[i];
                results.Add(new Episode((string)video.pageUrl, (int)video.order, Title));
            }

            return results;
        }

        private dynamic GetJson(string src)
        {
            string id = Regex.Match(src, "var playlistId\\s*=\\s*\"(.+?)\";").Groups[1].Value;
            string url = "http://pl.hd.sohu.com/videolist?playlistid=" + id;

            Console.WriteLine("Sohu json url: " + url);

            return JsonConvert.DeserializeObject<dynamic>(Http.Get(url, Encoding.GetEncoding(936)));
        }
    }
}
