using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public abstract class Album
    {
        //===================================================================== VARIABLES
        public string Url { get; }

        public List<Episode> Episodes { get; private set; }

        public string CoverUrl { get; protected set; }
        public string Title { get; protected set; }
        protected bool IsMovie { get; set; }
        protected string TotalEpisodes { get; set; }
        public string Region { get; protected set; }
        public string Year { get; protected set; }

        //===================================================================== INITIALIZE
        protected Album(string url)
        {
            Url = url;
        }
        public static Album FromUrl(string url)
        {
            if (Regex.IsMatch(url, @"\Ahttp://tv\.sohu\.com/.+\Z")) return new SohuAlbum(url);
            else if (Regex.IsMatch(url, @"\Ahttp://www\.youku\.com/show_page/id_.+?\.html.*\Z")) return new YoukuAlbum(url);
            else if (Regex.IsMatch(url, @"\Ahttp://list\.youku\.com/show/id_.+?\.html.*\Z")) return new NewYoukuAlbum(url);
            else if (Regex.IsMatch(url, @"\Ahttp://www\.soku\.com/detail/show/.+?\Z")) return new SokuAlbum(url);
            else if (Regex.IsMatch(url, @"\Ahttp://v\.qq\.com/detail/.+?\.html\Z")) throw new Exception("QQ link generator not functional.");
            else if (Regex.IsMatch(url, @"\Ahttp://v\.pps\.tv/splay_.+?\.html\Z")) throw new Exception("PPS link generator not functional.");
            else if (Regex.IsMatch(url, @"\Ahttp://www\.tudou\.com/albumcover/.+?\.html\Z")) throw new Exception("Tudou link generator not functional.");

            throw new Exception("URL is invalid. Press enter a valid URL.");
        }

        //===================================================================== FUNCTIONS
        public void GenerateInfo()
        {
            string src = Http.Get(Url.Split(new char[] { '|' })[0], Encoding);

            GenerateInfo(src);
            Episodes = GetEpisode(src).OrderBy(info => info.Number).ToList();

            Console.WriteLine("Link count: " + Episodes.Count);
        }
        protected abstract void GenerateInfo(string src);

        protected List<Episode> GetEpisode(string src)
        {
            return IsMovie ? GetMovie(src) : GetDrama(src);
        }
        protected abstract List<Episode> GetMovie(string src);
        protected abstract List<Episode> GetDrama(string src);

        //===================================================================== PROPERTIES
        public string CoverPath
        {
            get { return BaseDir + Path.DirectorySeparatorChar + "Cover.jpg"; }
        }
        public string BaseDir
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + Title; }
        }

        public string Type
        {
            get
            {
                if (IsMovie)
                    return "电影";
                return TotalEpisodes + "集电视剧";
            }
        }
        public virtual Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
        public virtual int MaxSimultaneousDownloads
        {
            get { return 4; }
        }
    }
}
