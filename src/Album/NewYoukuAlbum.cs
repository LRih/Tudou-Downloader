using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public class NewYoukuAlbum : Album
    {
        //===================================================================== INITIALIZE
        public NewYoukuAlbum(string url) : base(url)
        {
        }

        //===================================================================== FUNCTIONS
        protected override void GenerateInfo(string src)
        {
            CoverUrl = Regex.Match(src, @"<i class=""bg""></i><img src=""(.+?)""").Groups[1].Value;
            Title = Regex.Match(src, @"<a title=""(.+?)""").Groups[1].Value;
            IsMovie = Regex.IsMatch(src, "电影</a>:</span>");
            TotalEpisodes = (IsMovie ? "1" : Regex.Match(src, @"共(\d+)集").Groups[1].Value);
            Region = Regex.Match(src, @"<li>地区：<a href="".+?"" target=""_blank"">(.+?)</a></li>", RegexOptions.Singleline).Groups[1].Value;
            Year = Regex.Match(src, @"<span class=""sub-title"">(\d{4})</span>").Groups[1].Value;
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

            string playlistUrl = "http://" + Regex.Match(src, @"<a href=""(.+?)"" class=""p-btn""").Groups[1].Value;
            string playlistSrc = Http.Get(playlistUrl);

            // get links
            MatchCollection matches = Regex.Matches(playlistSrc, @"name=""tvlist"" flag=""(\d+?)"" seq="".+?"" id=""item_(.+?)""");
            foreach (Match match in matches)
            {
                int episodeNumber = int.Parse(match.Groups[1].Value);

                // only add episode if list doesn't already include episode
                if (results.Count(info => info.Number == episodeNumber) == 0)
                {
                    string url = "http://v.youku.com/v_show/id_" + match.Groups[2].Value;
                    results.Add(new Episode(url, episodeNumber, Title, new Youku()));
                }
            }

            return results;
        }

        //protected override List<Episode> GetDrama(string src)
        //{
        //    List<Episode> results = new List<Episode>();

        //    string id = Regex.Match(src, @"showid:""(.+?)""").Groups[1].Value;
        //    string json = Http.Get(string.Format("http://list.youku.com/show/module?id={0}&tab=showInfo", id));

        //    // get reloads
        //    MatchCollection matches = Regex.Matches(json, @"data-id=\\""(reload_\d+)\\""");
        //    foreach (Match match in matches)
        //        results.AddRange(GetUrls(id, match.Groups[1].Value));

        //    return results;
        //}

        //private List<Episode> GetUrls(string id, string reload)
        //{
        //    List<Episode> results = new List<Episode>();

        //    string src = Http.Get(string.Format("http://list.youku.com/show/episode?id={0}&stage={1}", id, reload));

        //    // get episode links in reload
        //    MatchCollection matches = Regex.Matches(src, @"<a class=\\""c555\\"" href=\\""(.+?)\\"".+?>(\d+)<\\/a>");

        //    foreach (Match match in matches)
        //    {
        //        int episodeNumber = int.Parse(match.Groups[2].Value);

        //        // only add episode if list doesn't already include episode
        //        if (results.Count(info => info.Number == episodeNumber) == 0)
        //        {
        //            string url = "http:" + match.Groups[1].Value.Replace("\\", "");
        //            results.Add(new Episode(url, episodeNumber, Title, new Youku()));
        //        }
        //    }

        //    return results;
        //}

        //===================================================================== PROPERTIES
        public override int MaxSimultaneousDownloads
        {
            get { return 2; }
        }
    }
}
