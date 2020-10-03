using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace TudouDownloader
{
    public class TudouDownloader : Form
    {
        //===================================================================== CONSTANTS
        private const int EXPANDED_HEIGHT = 325;

        //===================================================================== CONTROLS
        private IContainer _components = new Container();
        private System.Windows.Forms.Timer _timer;
        private LinkEntryPanel _pnlLinkEntry = new LinkEntryPanel();
        private DownloadManagerPanel _pnlDownloadManager = new DownloadManagerPanel();

        //===================================================================== VARIABLES
        private Font _fontMain = new Font("Segoe UI", 10);

        private Album _album;
        private DownloadManager _downloadManager;

        private bool _isClosed = false;

        //===================================================================== INITIALIZE
        public TudouDownloader()
        {
            ClientSize = new Size(480, 130);

            _timer = new System.Windows.Forms.Timer(_components);
            _timer.Interval = 2000;

            this.Font = _fontMain;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = new Icon(Assembly.GetCallingAssembly().GetManifestResourceStream("TudouDownloader.Res.icon.ico"));
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = Strings.NAME + " - " + Version;

            this.Controls.Add(_pnlLinkEntry);
            this.Controls.Add(_pnlDownloadManager);

            _pnlLinkEntry.Size = ClientSize;
            _pnlDownloadManager.Width = ClientSize.Width;
            _pnlLinkEntry.Resize += pnlLinkEntry_Resize;
            _pnlLinkEntry.DownloadClicked += pnlLinkEntry_DownloadClicked;
            _pnlDownloadManager.LinkClicked += pnlDownloadManager_LinkClicked;

            _pnlLinkEntry.SetHistory(SaveManager.LoadHistory());
            SaveManager.LoadSokuPriorities(); // create soku priority file if absent

            SleepUtils.PreventSleep();
        }
        protected override void OnLoad(EventArgs e)
        {
            string path = "";

            foreach (string arg in Environment.GetCommandLineArgs())
                if (arg.EndsWith(".txt"))
                    path = arg;

            if (path.Length > 0)
            {
                _pnlDownloadManager.HideGotoPage();
                ShowDownloadManager(false);
                InitializeDownloadManager(Quality.SD, DownloadManager.MAX_SIMULTANEOUS_DOWNLOADS);
                LoadAlbum(path);
            }

            base.OnLoad(e);
        }
        private void InitializeDownloadManager(Quality quality, int maxSimulDownloads)
        {
            _downloadManager = new DownloadManager(quality, maxSimulDownloads);
            _downloadManager.StatusChanged += downloadManager_StatusChanged;
            _downloadManager.DownloadStatusChanged += downloadManager_DownloadStatusChanged;
            _timer.Start();
            _timer.Tick += timer_Tick;
        }

        //===================================================================== TERMINATE
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _components.Dispose();
                _fontMain.Dispose();
            }

            base.Dispose(disposing);
        }

        //===================================================================== FUNCTIONS
        private void LoadAlbum(object data)
        {
            SaveManager.AddHistoryItem(_album.Url);
            List<Episode> eps;
            int retryNo = 0;
            this.BeginInvoke((Action)(() => _pnlDownloadManager.SetStatus(Strings.FETCHING_EPISODE_INFO)));

            while (true)
            {
                try
                {
                    _album.GenerateInfo();
                    SaveManager.AddHistoryItem(_album.Title, _album.Url);
                    eps = _album.Episodes;
                    this.BeginInvoke((Action)(() => _pnlDownloadManager.SetInfo(_album.CoverUrl, _album.Title, _album.Type, _album.Region, _album.Year)));

                    try
                    {
                        if (!Directory.Exists(_album.BaseDir))
                            Directory.CreateDirectory(_album.BaseDir);

                        if (!File.Exists(_album.CoverPath))
                        {
                            using (WebClient client = new WebClient())
                                client.DownloadFile(_album.CoverUrl, _album.CoverPath);
                        }
                    }
                    catch { }

                    break;
                }
                catch
                {
                    retryNo++;
                    this.BeginInvoke((Action)(() => _pnlDownloadManager.SetStatus(Strings.FETCHING_EPISODE_INFO + " unable to connect, retry " + retryNo)));
                    Thread.Sleep(1000);
                }
            }

            this.BeginInvoke((Action)(() => _pnlDownloadManager.SetStatus(Strings.EPISODE_INFO_FETCHED)));
            _downloadManager.Queue(GetSublist(eps, _pnlLinkEntry.EpisodeRange[0], _pnlLinkEntry.EpisodeRange[1]));
        }
        private void LoadAlbum(string path)
        {
            List<Episode> eps = new List<Episode>();

            using (StreamReader reader = new StreamReader(path))
            {
                string title = Path.GetFileNameWithoutExtension(path);
                int index = 1;

                _pnlDownloadManager.SetInfo("", title, "", "", "");

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Replace(" ", "");

                    if (line.Length > 0)
                    {
                        string[] split = line.Split(new char[] { ',' });
                        int episodeNumber = (split.Length == 2 ? int.Parse(split[1]) : index);
                        eps.Add(new Episode(split[0], episodeNumber, title, new Youku()));
                        index++;
                    }
                }
            }

            _pnlDownloadManager.SetStatus(Strings.EPISODE_INFO_FETCHED);
            _downloadManager.Queue(GetSublist(eps, _pnlLinkEntry.EpisodeRange[0], _pnlLinkEntry.EpisodeRange[1]));
        }

        private void ShowDownloadManager(bool animate)
        {
            _pnlLinkEntry.Minimize(animate);

            if (animate)
                ThreadPool.QueueUserWorkItem(new WaitCallback(ExpandHeight));
            else
                this.Height = EXPANDED_HEIGHT + (this.Height - ClientSize.Height);
        }
        private void ExpandHeight(object arg)
        {
            int target = EXPANDED_HEIGHT + this.Height - ClientSize.Height;
            while (this.Height != target)
            {
                this.BeginInvoke((Action)(() => this.Height = Math.Min(this.Height + 15, target)));
                Thread.Sleep(15);
            }
        }

        private static List<Episode> GetSublist(List<Episode> eps, int episodeMin, int episodeMax)
        {
            int maxCount = Math.Max(eps.Count - episodeMin + 1, 0);
            int count = Math.Min(episodeMax - episodeMin + 1, maxCount);
            return eps.GetRange(episodeMin - 1, count);
        }

        //===================================================================== PROPERTIES
        public static string Version
        {
            get
            {
                Version version = Assembly.GetEntryAssembly().GetName().Version;
                if (version.Build == 0)
                    return string.Format("{0}.{1}", version.Major, version.Minor);
                return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }

        //===================================================================== EVENTS
        protected override void OnClosing(CancelEventArgs e)
        {
            _isClosed = true;
            base.OnClosing(e);
        }

        private void pnlLinkEntry_Resize(object sender, EventArgs e)
        {
            _pnlDownloadManager.Top = _pnlLinkEntry.Bottom;
            _pnlDownloadManager.Height = EXPANDED_HEIGHT - _pnlLinkEntry.Height;
        }
        private void pnlLinkEntry_DownloadClicked(string url)
        {
            try
            {
                _album = Album.FromUrl(url);
                ShowDownloadManager(true);
                InitializeDownloadManager(_pnlLinkEntry.Quality, _album.MaxSimultaneousDownloads);
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadAlbum));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void pnlDownloadManager_LinkClicked()
        {
            Process.Start(_album.Url.Split(new char[] { '|' })[0]);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.Text = string.Format("{0} - {1} kB/s", Strings.SHORT_NAME, Math.Round(_downloadManager.ResetDownloadedBytes() / 1024f / 2, 1));
        }

        private void downloadManager_StatusChanged(string status)
        {
            if (_isClosed)
                return;

            this.BeginInvoke((Action)(() => _pnlDownloadManager.SetStatus(status)));
            this.BeginInvoke((Action)(() =>
            {
                if (status == Strings.DOWNLOADS_COMPLETED && _pnlLinkEntry.AutoShutdown)
                    using (ShutdownCountdown dialog = new ShutdownCountdown())
                        dialog.ShowDialog(this);
            }));
        }
        private void downloadManager_DownloadStatusChanged(int id, string episodeName, string status, float fraction)
        {
            if (_isClosed)
                return;

            this.BeginInvoke((Action)(() =>
            {
                string text = (episodeName == "" && status == "" ? "" : episodeName + ": " + status);
                _pnlDownloadManager.SetDownloadStatus(id, text, fraction);
            }));
        }
    }
}
