using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    //#========================================================================
    //# * QQAlbumInfo
    //#     v.qq.com (drama only and must use manual source entry)
    //#========================================================================
    //public class QQAlbumInfo : AlbumInfo
    //{
    //    //===================================================================== INITIALIZE
    //    public QQAlbumInfo(string albumURL, string source) : base(albumURL, source)
    //    {
    //    }
    //    //===================================================================== FUNCTIONS
    //    protected override void GenerateInfo(string source)
    //    {
    //        CoverURL = Regex.Match(source, @"""img_cover_pic"" src=""(.+?)""").Groups[1].Value;
    //        Title = Regex.Match(source, "coverTitle:\"(.+?)\"").Groups[1].Value;
    //        TotalEpisodes = Regex.Match(source, @"<li>剧集：(全\d+?集)</li>").Groups[1].Value;

    //        Region = Regex.Match(source, @"地区：<a.+?>(.+?)</a>").Groups[1].Value;
    //        Year = Regex.Match(source, @"年份：<a.+?>(\d+?)</a>").Groups[1].Value;
    //    }
    //    protected override List<EpisodeInfo> GetEpisodeInfo(string source)
    //    {
    //        List<EpisodeInfo> result = new List<EpisodeInfo>();
    //        // get link and episode number
    //        MatchCollection matches = Regex.Matches(source, @"<a href=""(.+?)"" id="".+?"" title="".+? 第(\d+)集"" class=""video_img"">"); // drama
    //        foreach (Match match in matches)
    //        {
    //            int episodeNumber = int.Parse(match.Groups[2].Value);
    //            result.Add(new EpisodeInfo("http://v.qq.com" + match.Groups[1].Value, episodeNumber, Title));
    //        }
    //        return result;
    //    }
    //    protected List<EpisodeInfo> GetEpisodeInfoAjax(string source)
    //    {
    //        List<EpisodeInfo> result = new List<EpisodeInfo>();
    //        // use ajax to find links
    //        string sourceID = Regex.Match(source, @"sourceid=""(.+?)""").Groups[1].Value;
    //        string vap = Regex.Match(source, @"\svap=""(.+?)""").Groups[1].Value;
    //        source = NetUtils.GetSource(string.Format("http://s.video.qq.com/loadplaylist?vkey={0}&vtype={1}&otype=json", sourceID, vap), Encoding.ASCII);
    //        MatchCollection matches = Regex.Matches(source, @"""episode_number"":""(\d+)"".+?""url"":""(.+?)""");
    //        foreach (Match match in matches)
    //        {
    //            int episodeNumber = int.Parse(match.Groups[1].Value);
    //            // only add episode if list doesn't already include episode
    //            if (result.Count(info => info.EpisodeNumber == episodeNumber) == 0) result.Add(new EpisodeInfo(match.Groups[2].Value, episodeNumber, Title));
    //        }
    //        return result;
    //    }
    //    //===================================================================== PROPERTIES
    //    public override Encoding Encoding
    //    {
    //        get { return Encoding.UTF8; }
    //    }
    //}
}
