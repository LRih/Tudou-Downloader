using System;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    [Obsolete]
    public class Onevis : ILinkGenerator
    {
        //===================================================================== VARIABLES
        private const string URL = "http://onevis.net/sub/flv/?key=authentication&quality={0}&url={1}";

        //===================================================================== FUNCTIONS
        public string[] GetDownloadLinks(string videoUrl, Quality targetQuality)
        {
            string url = string.Format(URL, (int)targetQuality, Uri.EscapeDataString(videoUrl));
            Console.WriteLine(url);

            string src = Http.Get(url);

            // find download link
            MatchCollection matches = Regex.Matches(src, "<div>(.+?)</div>");
            foreach (Match m in matches)
                Console.WriteLine("Url: " + m.Groups[1].Value);

            return new string[] { "" };
        }
    }
}
