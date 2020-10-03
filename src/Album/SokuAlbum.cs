using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TudouDownloader
{
    public class SokuAlbum : Album
    {
        //===================================================================== INITIALIZE
        public SokuAlbum(string url) : base(url)
        {
        }

        //===================================================================== FUNCTIONS
        protected override void GenerateInfo(string src)
        {
            CoverUrl = Regex.Match(src, @"<li class=""p_thumb""><img class="""" src=""(.+?)""").Groups[1].Value;
            Title = Regex.Match(src, "<h1>(.+?)</h1>").Groups[1].Value;
            string type = Regex.Match(src, @"<li class=""base_what"">[\n\s]*?([^\n\t]+?)[\n\s]*?</li>").Groups[1].Value;
            IsMovie = (type == "电影");
            TotalEpisodes = (IsMovie ? "1" : Regex.Match(type, @"\d+").Groups[0].Value);
            Region = Regex.Match(src, @"<label>地区:</label><span>(.+?)</span>").Groups[1].Value;
            Year = Regex.Match(src, @"<li class=""base_pub"">(\d+)").Groups[1].Value;
        }

        protected override List<Episode> GetMovie(string src)
        {
            List<Episode> results = new List<Episode>();
            MatchCollection matches = Regex.Matches(src, @"<ul class=""linkpanel panel_15"">.+?<li><a href=""(.+?)""></a></li>", RegexOptions.Singleline);

            int index = 0;
            if (Url.Contains("|")) int.TryParse(GetSourceNames()[0], out index);
            index = Math.Min(index, matches.Count - 1);

            results.Add(new Episode(matches[index].Groups[1].Value, 0, Title));
            return results;
        }
        protected override List<Episode> GetDrama(string src)
        {
            List<Episode> results = new List<Episode>();

            foreach (string sourceName in GetSourceNames())
            {
                MatchCollection matches = Regex.Matches(src, @"<a href=""(.+?)"" target='_blank' site=""" + sourceName + @""".+?>(\d+)</a>");
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                        results.Add(new Episode(match.Groups[1].Value, int.Parse(match.Groups[2].Value), Title));
                    break;
                }
            }

            return results;
        }

        private string[] GetSourceNames()
        {
            if (Url.Contains("|"))
                return new string[] { Regex.Match(Url, @"\|(.+)").Groups[1].Value };
            return SaveManager.LoadSokuPriorities();
        }
    }
}
