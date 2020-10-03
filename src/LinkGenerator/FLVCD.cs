using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public class FLVCD : ILinkGenerator
    {
        //===================================================================== VARIABLES
        private const string URL = "http://www.flvcd.com/parse.php?format=&kw=";

        //===================================================================== FUNCTIONS
        public string[] GetDownloadLinks(string videoUrl, Quality targetQuality)
        {
            List<string> links = new List<string>();

            string source = GetSource(videoUrl, targetQuality);

            MatchCollection matches = Regex.Matches(source, @"<a href=""(.+?)"".+?;return false;");
            foreach (Match match in matches)
                links.Add(match.Groups[1].Value);

            Console.WriteLine("FLVCD parts found: " + links.Count);
            return links.ToArray();
        }

        private static string GetSource(string videoUrl, Quality targetQuality)
        {
            string flvCDUrl = URL + Uri.EscapeDataString(videoUrl);

            // get LD source
            Console.WriteLine("Launching FLVCD: " + flvCDUrl);
            string source = GetSource(flvCDUrl);

            // get HD source if specified
            string[] formats = { "real", "super", "high" };
            for (int id = (int)targetQuality; id < (int)Quality.LD; id++)
            {
                Match match = Regex.Match(source, @"<a href=""(.+?&format=" + formats[id] + @")"">");
                if (match.Success)
                {
                    Console.WriteLine("FLVCD found quality: " + formats[id]);
                    return GetSource("http://www.flvcd.com/" + match.Groups[1].Value);
                }
                else
                    Console.WriteLine("FLV quality '" + formats[id] + "' not found");
            }
            return source;
        }
        private static string GetSource(string videoUrl)
        {
            return Http.Get(videoUrl);

            string source = Http.Get(videoUrl, Encoding.GetEncoding(936));
            if (source.Contains("请选择一个广告"))
            {
                string cookie = GenerateCookie(source);
                Console.WriteLine("Generating cookie: " + cookie);
                source = Http.Get(videoUrl, Encoding.GetEncoding(936), "", cookie);
            }
            else
                Console.WriteLine("No cookie required");
            return source;
        }

        [Obsolete]
        private static string GenerateCookie(string source)
        {
            string b = Regex.Match(source, @"\|([^\|]+?)\|createSc\|length").Groups[1].Value;
            string a = Regex.Match(source, "var avdGgggg='(.+?)';").Groups[1].Value;
            long t = long.Parse(Regex.Match(source, "var avdGggggtt=(.+?);").Groups[1].Value);

            DateTime time = DateTime.UtcNow;
            time = time.AddMilliseconds(300 * 1000);

            return "go=" + CreateSc(b, a, t) + ";expires=" + time.ToString("ddd, dd MMM yyyy HH:mm:ss G'M'T") + ";avdGggggtt=" + t.ToString();
        }
        private static string CreateSc(string b, string a, long t)
        {
            string result = "";

	        t = (long)Math.Floor(t / (float)(600 * 1000));

	        for (int i = 0; i < a.Length; i++)
            {
		        long j = a[i] ^ b[i] ^ t;
		        j = j % 'z';
		        char c;
		        if (j < '0')
			        c = (char)('0' + j % 9);
		        else if (j >= '0' && j <= '9')
			        c = (char)j;
		        else if (j > '9' && j < 'A')
			        c = '9';
		        else if (j >= 'A' && j <= 'Z')
			        c = (char)j;
		        else if (j > 'Z' && j < 'a')
			        c = 'Z';
		        else if (j >= 'z' && j <= 'z')
			        c = (char)j;
		        else 
			        c = 'z';
		        result += c;
	        }
            return result;
        }
    }
}
