using System;
using System.IO;
using System.Net;

namespace TudouDownloader
{
    public class Downloader
    {
        //===================================================================== VARIABLES
        private const int BYTE_RATE = 4096;

        private readonly string _link;
        public readonly string FilePath;
        public readonly int Id;
        private readonly int _kbpsLimit;

        public event ProgressChangedHandler ProgressChanged;

        //===================================================================== INITIALIZE
        public Downloader(string link, string filePath, int id, int kbpsLimit)
        {
            _link = link;
            FilePath = filePath;
            Id = id;
            _kbpsLimit = kbpsLimit;
        }

        //===================================================================== FUNCTIONS
        public void Download()
        {
            // retrieve position of broken download
            long start;
            using (FileStream fileStream = new FileStream(PartialFilePath, FileMode.Append))
                start = fileStream.Length;

            // create http request
            using (HttpWebResponse response = GetWebResponse(_link, start))
            using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
            using (FileStream writer = new FileStream(PartialFilePath, FileMode.Append, FileAccess.Write))
            {
                reader.BaseStream.ReadTimeout = 60000;
                long fileSize = response.ContentLength + start;
                while (fileSize > writer.Length)
                {
                    int read = (int)Math.Min(fileSize - writer.Length, BYTE_RATE);
                    try { writer.Write(reader.ReadBytes(read), 0, read); }
                    catch { throw new Exception("timeout error"); }

                    ProgressChanged?.Invoke(this, read, writer.Length, fileSize);

                    System.Threading.Thread.Sleep(GetSleepTime(_kbpsLimit));
                }
            }

            File.Move(PartialFilePath, FilePath);
        }
        private HttpWebResponse GetWebResponse(string link, long start = 0)
        {
            HttpWebRequest request = Http.CreateRequest(link, 5000);

            if (start > 0)
                request.AddRange(start);

            try { return (HttpWebResponse)request.GetResponse(); }
            catch { throw new Exception("unable to connect to filestream"); }
        }

        private int GetSleepTime(int targetKbps)
        {
            float kbps = BYTE_RATE / (float)1024;
            int sleep = (int)(kbps / targetKbps * 1000);

            return sleep;
        }

        //===================================================================== PROPERTIES
        private string PartialFilePath
        {
            get { return FilePath + "PART"; }
        }
    }


    public delegate void ProgressChangedHandler(Downloader downloader, long bytesDownloaded, long totalBytesDownloaded, long fileSize);
}
