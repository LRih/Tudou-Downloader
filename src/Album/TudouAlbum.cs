using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    //#========================================================================
    //# * TudouAlbumInfo
    //#========================================================================
    //public class TudouAlbumInfo : AlbumInfo
    //{
    //    //===================================================================== INITIALIZE
    //    public TudouAlbumInfo(string albumURL, string source) : base(albumURL, source)
    //    {
    //    }
    //    //===================================================================== FUNCTIONS
    //    protected override void GenerateInfo(string source)
    //    {
    //        CoverURL = Regex.Match(source, @"<img  class=""quic""  width=""160"" height=""240""  src=""(.+?)""").Groups[1].Value;
    //        Title = Regex.Match(source, "<h1>(.+?)</h1>").Groups[1].Value;
    //        TotalEpisodes = Regex.Match(source, @"共\d+集").Groups[0].Value;
    //        Region = Regex.Match(source, @"<b>地区： </b>\r\n.+?<a.+?>(.+?)</a>").Groups[1].Value;
    //        Year = Regex.Match(source, @"<b>年代：</b>\r\n.+?<a.+?>(\d+?)</a>").Groups[1].Value;
    //    }
    //    protected override List<EpisodeInfo> GetEpisodeInfo(string source)
    //    {
    //        List<EpisodeInfo> result = new List<EpisodeInfo>();
    //        // get link and episode number
    //        MatchCollection matches;
    //        if (source.Contains("共1集")) matches = Regex.Matches(source, @"<a  href=""(.+?)""  title=");  // if movie 
    //        else matches = Regex.Matches(source, @"href=""(.+?)"">第(\d+)集</a>"); // if drama
    //        foreach (Match match in matches)
    //        {
    //            int episodeNumber = 0;
    //            if (match.Groups[2].Value != string.Empty) episodeNumber = int.Parse(match.Groups[2].Value);
    //            result.Add(new EpisodeInfo(match.Groups[1].Value, episodeNumber, Title));
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
