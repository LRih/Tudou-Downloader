using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TudouDownloader
{
    public class DownloadManager
    {
        //===================================================================== CONSTANTS
        public const int MAX_SIMULTANEOUS_DOWNLOADS = 4;

        //===================================================================== EVENTS
        public event StatusChangedHandler StatusChanged;
        public event DownloadStatusChangedHandler DownloadStatusChanged;

        //===================================================================== VARIABLES
        private Quality _quality;
        private int _maxSimulDownloads;

        private Stack<Episode> _eps = new Stack<Episode>();
        private Episode[] _currentDownloads;
        private long[] _downloadedBytes;

        private object _lock = new object();

        //===================================================================== INITIALIZE
        public DownloadManager(Quality quality, int maxSimulDownloads)
        {
            if (maxSimulDownloads > MAX_SIMULTANEOUS_DOWNLOADS)
                throw new Exception("Too many simultaneous downloads, max: " + MAX_SIMULTANEOUS_DOWNLOADS);

            _quality = quality;
            _maxSimulDownloads = maxSimulDownloads;

            _currentDownloads = new Episode[_maxSimulDownloads];
            _downloadedBytes = new long[_maxSimulDownloads];
        }

        //===================================================================== FUNCTIONS
        private void CheckEmptySlot()
        {
            lock (_lock)
            {
                // no more episodes to download
                if (_eps.Count == 0 && _currentDownloads.Count(download => download == null) == _maxSimulDownloads)
                    ChangeStatus(Strings.DOWNLOADS_COMPLETED);
                else
                {
                    // remaining episode to download and empty slot exists
                    while (_eps.Count > 0 && _currentDownloads.Contains(null))
                    {
                        ChangeStatus(Strings.DOWNLOADING);

                        int id = Array.IndexOf(_currentDownloads, null);
                        _currentDownloads[id] = _eps.Pop();
                        ThreadPool.QueueUserWorkItem(new WaitCallback(Download), new object[] { id, _currentDownloads[id] });
                    }
                }
            }
        }

        public void Queue(List<Episode> eps)
        {
            eps.Reverse();

            lock (_lock)
                foreach (Episode ep in eps)
                    _eps.Push(ep);

            CheckEmptySlot();
        }
        private void Download(object data)
        {
            int id = (int)((object[])data)[0];
            Episode ep = (Episode)((object[])data)[1];

            ChangeDownloadStatus(id, ep.Name, Strings.INITIALIZING);

            string completeMessage = Strings.COMPLETE;
            try
            {
                Download(id, ep);
            }
            catch (Exception ex)
            {
                completeMessage = ex.Message;
                if (ex.Message != Strings.NO_LINKS_FOUND)
                    lock (_lock)
                        _eps.Push(ep);
            }

            ChangeDownloadStatus(id, ep.Name, completeMessage);
            Thread.Sleep(1000); // so user can see the changed download status message

            lock (_lock)
                _currentDownloads[id] = null;

            ClearDownloadStatus(id);

            CheckEmptySlot();
        }
        private void Download(int id, Episode ep)
        {
            // check episode already downloaded
            if (File.Exists(ep.M3UPath))
                return;

            try
            {
                ep.GenerateDownloadLinks(_quality);
            }
            catch (Exception ex)
            {
                if (ex.Message == Strings.NO_LINKS_FOUND)
                    throw ex;
                else
                    throw new Exception("unable to get download links");
            }

            // create data folder
            if (!Directory.Exists(ep.DataDir))
                Directory.CreateDirectory(ep.DataDir);

            // iterate links
            for (int i = 0; i < ep.DownloadLinks.Length; i++)
            {
                // check part already downloaded
                if (!File.Exists(ep.GetFilePath(i)))
                {
                    int kbpsLimit = SaveManager.LoadSpeedLimit() / _maxSimulDownloads;

                    Downloader downloader = new Downloader(ep.DownloadLinks[i], ep.GetFilePath(i), id, kbpsLimit);
                    downloader.ProgressChanged += downloader_ProgressChanged;
                    downloader.Download();
                }
            }

            MakeM3U(ep);
        }
        private void MakeM3U(Episode ep)
        {
            if (ep.DownloadLinks.Length > 1)
            {
                using (StreamWriter writer = new StreamWriter(ep.M3UPath))
                    writer.Write(ep.M3UString);
            }
        }


        /**
         * Get bytes downloaded since last call and reset bytes to zero.
         */
        public long ResetDownloadedBytes()
        {
            long totalBytes = 0;

            lock (_downloadedBytes)
            {
                for (int i = 0; i < _downloadedBytes.Length; i++)
                {
                    totalBytes += _downloadedBytes[i];
                    _downloadedBytes[i] = 0;
                }
            }

            return totalBytes;
        }


        private void ChangeStatus(string status)
        {
            StatusChanged?.Invoke(status);
        }

        private void ChangeDownloadStatus(int id, string episodeName, string status)
        {
            DownloadStatusChanged?.Invoke(id, episodeName, status, -1);
        }
        private void ChangeDownloadStatus(int id, string episodeName, float fraction)
        {
            DownloadStatusChanged?.Invoke(id, episodeName, "", fraction);
        }
        private void ClearDownloadStatus(int id)
        {
            DownloadStatusChanged?.Invoke(id, "", "", -1);
        }

        //===================================================================== EVENTS
        private void downloader_ProgressChanged(Downloader downloader, long bytesDownloaded, long totalBytesDownloaded, long fileSize)
        {
            float fraction = totalBytesDownloaded / (float)fileSize;
            ChangeDownloadStatus(downloader.Id, Path.GetFileName(downloader.FilePath), fraction);

            lock (_downloadedBytes)
                _downloadedBytes[downloader.Id] += bytesDownloaded;
        }
    }


    public delegate void StatusChangedHandler(string status);
    public delegate void DownloadStatusChangedHandler(int id, string episodeName, string status, float fraction);
}
