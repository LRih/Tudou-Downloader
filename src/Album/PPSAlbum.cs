using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    //#========================================================================
    //# * PPSAlbumInfo
    //#========================================================================
    //public class PPSAlbumInfo : AlbumInfo
    //{
    //    //===================================================================== INITIALIZE
    //    public PPSAlbumInfo(string albumURL, string source) : base(albumURL, source)
    //    {
    //    }
    //    //===================================================================== FUNCTIONS
    //    protected override void GenerateInfo(string source)
    //    {
    //        CoverURL = string.Empty;
    //        Title = Regex.Match(source, "<title>《(.+?)》").Groups[1].Value;
    //        TotalEpisodes = "集数：" + Regex.Match(source, @"集数：\n\s{20}</span>\n\s{20}(\d+)").Groups[1].Value;
    //        Region = "unknown";
    //        Year = Regex.Match(source, @"发行日期：.+?(\d+)\s{20}</a>", RegexOptions.Singleline).Groups[1].Value;
    //    }
    //    protected override List<EpisodeInfo> GetEpisodeInfo(string source)
    //    {
    //        List<EpisodeInfo> result = new List<EpisodeInfo>();
    //        // get link and episode number
    //        if (Regex.IsMatch(source, "电影版本：")) // if movie
    //        {
    //            Match match = Regex.Match(source, @"<a href=""(.+?)"" class=""thumb-outer"" title=""" + Title);
    //            result.Add(new EpisodeInfo(match.Groups[1].Value, 0, Title));
    //        }
    //        else // if drama
    //        {
    //            // use ajax to find links
    //            string id = Regex.Match(source, @"sid: (\d+?),").Groups[1].Value;
    //            source = NetUtils.GetSource(string.Format("http://v.pps.tv/ugc/ajax/aj_newlongvideo.php?&sid={0}&type=splay", id), Encoding.ASCII);
    //            MatchCollection matches = Regex.Matches(source, @"""url_key"":""(.+?)"".+?""order"":""(\d+)""");
    //            foreach (Match match in matches)
    //            {
    //                string link = string.Format("http://v.pps.tv/play_{0}.html", match.Groups[1].Value);
    //                int episodeNumber = int.Parse(match.Groups[2].Value);
    //                // only add episode if list doesn't already include episode
    //                if (result.Count(info => info.EpisodeNumber == episodeNumber) == 0) result.Add(new EpisodeInfo(link, episodeNumber, Title));
    //            }
    //        }
    //        return result;
    //    }
    //    //===================================================================== PROPERTIES
    //    public override Encoding Encoding
    //    {
    //        get { return Encoding.GetEncoding(936); }
    //    }
    //}
}
