using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public class Youku : ILinkGenerator
    {
        //===================================================================== VARIABLES
        private const string SERVICE_URL = "http://play.youku.com/play/get.json?ct={0}&vid={1}";
        private const string VIDEO_URL = "http://k.youku.com/player/getFlvPath/sid/{0}_00/st/{1}/fileid/{2}?{3}";
        private const string KEY_1 = "becaf9be";
        private const string KEY_2 = "bf7e5f01";

        //===================================================================== FUNCTIONS
        public string[] GetDownloadLinks(string videoUrl, Quality targetQuality)
        {
            return GetBaseVideo(videoUrl, targetQuality);
        }

        private static string[] GetBaseVideo(string videoUrl, Quality targetQuality)
        {
            var stream = GetStream(videoUrl, targetQuality);

            if (stream.channel_type != null)
                return new string[] { };

            var security = GetSecurity(videoUrl);
            var decrypt = DecryptSecurity(security);

            Console.WriteLine("Quality found: " + stream.stream_type);

            List<string> result = new List<string>();

            for (int i = 0; i < stream.segs.Count; i++)
            {
                var seg = stream.segs[i];

                string container = GetContainer((string)stream.stream_type);
                string newstreamfileid = GetNewStreamFileId(i, (string)seg.fileid);
                //string newstreamfileid = GetNewStreamFileId(i, (string)stream.stream_fileid);

                NameValueCollection data = new NameValueCollection
                {
                    { "ctype", "12" },
                    { "ev", "1" },
                    { "K", (string)seg.key },
                    { "ep", GetEp(newstreamfileid, decrypt) },
                    { "oip", (string)security.ip },
                    { "token", decrypt.token },
                    { "yxon", "1" }
                };

                // get link
                string url = string.Format(VIDEO_URL, decrypt.sid, container, newstreamfileid, Http.BuildQuery(data));
                string src = Http.Get(url, Encoding.ASCII);

                string dlUrl = Regex.Match(src, "\"server\":\"(.+?)\"").Groups[1].Value;

                result.Add(dlUrl);
            }

            return result.ToArray();
        }


        //---------------------------------------------------------------------
        private static dynamic GetStream(string videoUrl, Quality targetQuality)
        {
            string service10 = GetService(videoUrl, 10);
            var streams = JsonConvert.DeserializeObject<dynamic>(service10).data.stream;

            string[] types = GetTypes(targetQuality);

            // find highest quality up to requested
            foreach (string type in types)
                foreach (var stream in streams)
                    if (stream.stream_type == type)
                        return stream;

            return null;
        }

        private static dynamic GetSecurity(string videoUrl)
        {
            string service12 = GetService(videoUrl, 12);
            return JsonConvert.DeserializeObject<dynamic>(service12).data.security;
        }


        //---------------------------------------------------------------------
        private static string[] GetTypes(Quality quality)
        {
            string[][] types = new string[][]
            {
                new string[] { "mp4hd3", "hd3" },
                new string[] { "mp4hd2", "hd2" },
                new string[] { "mp4hd", "mp4" },
                new string[] { "flvhd", "flv" }
            };

            List<string> result = new List<string>();

            for (int i = (int)quality; i < types.Length; i++)
                result.AddRange(types[i]);

            return result.ToArray();
        }

        private static string GetContainer(string type)
        {
            switch (type)
            {
                case "mp4hd3":
                case "hd3": return "flv";
                case "mp4hd2":
                case "hd2": return "flv";
                case "mp4hd":
                case "mp4": return "mp4";
                case "flvhd": return "flv";
                case "flv": return "flv";
                case "3gphd": return "3gp";
            }

            throw new Exception("Invalid type: " + type);
        }


        private static string GetService(string url, int ct)
        {
            string id = Regex.Match(url, @"http://v\.youku\.com/v_show/id_(.+?)\.html").Groups[1].Value;
            return Http.Get(string.Format(SERVICE_URL, ct, id), Encoding.ASCII, "http://static.youku.com/", "__ysuid=" + EpochSeconds());
        }

        private static dynamic DecryptSecurity(dynamic security)
        {
            byte[] decryptBytes = Convert.FromBase64String((string)security.encrypt_string);
            string decrypt = RC4(decryptBytes, KEY_1);
            string[] splitDecrypt = decrypt.Split(new char[] { '_' });
            
            return new { sid = splitDecrypt[0], token = splitDecrypt[1] };
        }

        private static string GetEp(string newstreamfileid, dynamic decrypt)
        {
            string ep = RC4(decrypt.sid + "_" + newstreamfileid + "_" + decrypt.token, KEY_2);
            ep = Base64Encode(ep);

            return ep;
        }

        private static string GetNewStreamFileId(int index, string streamfileid)
        {
            string hex = index.ToString("X").PadLeft(2, '0').ToUpper();
            streamfileid = streamfileid.Substring(0, 8) + hex + streamfileid.Substring(10);

            return streamfileid;
        }


        //---------------------------------------------------------------------
        private static string RC4(string input, string key)
        {
            return RC4(Encoding.ASCII.GetBytes(input), key);
        }
        private static string RC4(byte[] input, string key)
        {
            int[] s = new int[256];
            for (int i = 0; i < 256; i++)
                s[i] = i;

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % key.Length]) % 256;
                int x = s[i];
                s[i] = s[j];
                s[j] = x;
            }

            int k = 0;
            j = 0;
            string res = "";
            for (int y = 0; y < input.Length; y++)
            {
                k = (k + 1) % 256;
                j = (j + s[k]) % 256;
                int x = s[k];
                s[k] = s[j];
                s[j] = x;
                res += (char)(input[y] ^ s[(s[k] + s[j]) % 256]);
            }

            return res;
        }

        private static string Base64Encode(string input)
        {
            // convert to bytes
            byte[] bytes = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
                bytes[i] = (byte)input[i];

            return Convert.ToBase64String(bytes);
        }

        private static double EpochSeconds()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return ts.TotalSeconds;
        }
    }
}
