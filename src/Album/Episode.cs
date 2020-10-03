using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public class Episode
    {
        //===================================================================== VARIABLES
        private ILinkGenerator _linkGenerator;

        private string _url;
        public int Number { get; private set; }
        private string _albumName;

        public string[] DownloadLinks { get; private set; }

        //===================================================================== INITIALIZE
        public Episode(string url, int number, string albumName) : this(url, number, albumName, new FLVCD())
        {
        }
        public Episode(string url, int number, string albumName, ILinkGenerator generator)
        {
            _url = url;
            Number = number;
            _albumName = albumName;

            _linkGenerator = generator;
        }

        //===================================================================== FUNCTIONS
        public void GenerateDownloadLinks(Quality targetQuality)
        {
            Console.WriteLine(string.Format("Episode link ({0}): {1}", Number, _url));

            DownloadLinks = _linkGenerator.GetDownloadLinks(_url, targetQuality);

            if (DownloadLinks.Length == 0)
                throw new Exception(Strings.NO_LINKS_FOUND);
        }

        public string GetFilePath(int index)
        {
            return DataDir + Path.DirectorySeparatorChar + GetFileName(index);
        }
        private string GetFileName(int index)
        {
            string name = Name;

            // generate part number for multipart files
            if (DownloadLinks.Length > 1)
            {
                string delim = Number == 0 ? " " : "-";
                string part = (index + 1).ToString().PadLeft(2, '0');
                name += delim + part;
            }

            return name + "." + GetExtention(DownloadLinks[index]);
        }

        private static string GetExtention(string url)
        {
            Match match = Regex.Match(url, @"(\..{3})\Z");

            if (url.Contains("/mp4/") || url.Contains(".mp4")) return "mp4";
            else if (url.Contains("/flv/")) return "flv";
            else if (url.Contains("/f4v/")) return "f4v";
            else if (match.Success) return match.Groups[1].Value;

            return "mp4";
        }

        //===================================================================== PROPERTIES
        public string Name
        {
            get
            {
                string num = Number == 0 ? "" : " " + Number.ToString().PadLeft(2, '0');
                return _albumName + num;
            }
        }
        
        public string M3UPath
        {
            get { return string.Format(@"{0}{1}{2}.m3u", BaseDir, Path.DirectorySeparatorChar, Name); }
        }

        public string BaseDir
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + _albumName; }
        }
        public string DataDir
        {
            get
            {
                if (DownloadLinks.Length > 1)
                    return BaseDir + Path.DirectorySeparatorChar + "Episodes";
                return BaseDir;
            }
        }

        public string M3UString
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < DownloadLinks.Length; i++)
                    builder.AppendLine("Episodes" + Path.DirectorySeparatorChar + GetFileName(i));

                return builder.ToString();
            }
        }
    }
}
