using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace TudouDownloader
{
    public static class Http
    {
        //===================================================================== FUNCTIONS
        public static string Get(string url)
        {
            return Get(url, "", "");
        }
        public static string Get(string url, string referer, string cookie)
        {
            return Get(url, Encoding.UTF8, "", "");
        }
        public static string Get(string url, Encoding encoding)
        {
            return Get(url, encoding, "", "");
        }
        public static string Get(string url, Encoding encoding, string referer, string cookie)
        {
            try
            {
                HttpWebRequest request = CreateRequest(url, 10000, referer, cookie);
                using (WebResponse response = request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static HttpWebRequest CreateRequest(string url, int timeout)
        {
            return CreateRequest(url, timeout, "", "");
        }
        private static HttpWebRequest CreateRequest(string url, int timeout, string referer, string cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Timeout = timeout;
            request.Referer = referer;
            request.Headers["Cookie"] = cookie;

            return request;
        }


        public static string BuildQuery(NameValueCollection items)
        {
            string data = "";

            foreach (string key in items.AllKeys)
            {
                if (data.Length > 0)
                    data += '&';
                data += key + "=" + Uri.EscapeDataString(items[key]);
            }

            return data;
        }
    }
}
