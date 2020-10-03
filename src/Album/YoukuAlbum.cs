using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public class YoukuAlbum : Album
    {
        //===================================================================== INITIALIZE
        public YoukuAlbum(string url) : base(url)
        {
        }

        //===================================================================== FUNCTIONS
        protected override void GenerateInfo(string src)
        {
            CoverUrl = Regex.Match(src, @"<li class=""thumb""><img src='(.+?)'").Groups[1].Value;
            Title = Regex.Match(src, "<span class=\"name\">(.+?)</span>").Groups[1].Value;
            IsMovie = Regex.IsMatch(src, "电影</a>:</span>");
            TotalEpisodes = (IsMovie ? "1" : Regex.Match(src, @"<div class=""basenotice"">[\n\s]*?共(\d+)集").Groups[1].Value);
            Region = Regex.Match(src, @"<label>地区:</label>.+?>(.+?)</a>", RegexOptions.Singleline).Groups[1].Value;
            Year = Regex.Match(src, @"<span class=""pub"">(\d{4})</span>").Groups[1].Value;
        }

        protected override List<Episode> GetMovie(string src)
        {
            List<Episode> results = new List<Episode>();
            Match match = Regex.Match(src, @"<a class=""btnShow btnplayposi""  charset=""420-2-3"" href=""(.+?)""");
            results.Add(new Episode(match.Groups[1].Value, 0, Title, new Youku()));
            return results;
        }
        protected override List<Episode> GetDrama(string src)
        {
            List<Episode> results = new List<Episode>();

            string id = Regex.Match(src, @"id:'(.+?)'").Groups[1].Value;
            string json = Http.Get(string.Format("http://www.youku.com/show_episode/id_{0}.html?dt=json", id));

            MatchCollection matches = Regex.Matches(json, @"<a href=""(.+?)"".+?>(\d+)</a>");
            foreach (Match match in matches)
            {
                int episodeNumber = int.Parse(match.Groups[2].Value);

                // only add episode if list doesn't already include episode
                if (results.Count(info => info.Number == episodeNumber) == 0)
                    results.Add(new Episode(match.Groups[1].Value, episodeNumber, Title, new Youku()));
            }

            return results;
        }

        //===================================================================== PROPERTIES
        public override int MaxSimultaneousDownloads
        {
            get { return 2; }
        }
    }
}
