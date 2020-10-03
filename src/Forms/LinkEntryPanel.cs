using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TudouDownloader
{
    public class LinkEntryPanel : UserControl
    {
        //===================================================================== CONSTANTS
        private const int MARGIN = 10;

        //===================================================================== CONTROLS
        private Label _lblUrl = new Label();
        private HistoryComboBox _comAlbumUrl = new HistoryComboBox();
        private ComboBox _comQuality = new ComboBox();
        private CheckBox _chkEpisodeLimit = new CheckBox();
        private CheckBox _chkAutoShutdown = new CheckBox();
        private NumericUpDown _numEpisodeMin = new NumericUpDown();
        private NumericUpDown _numEpisodeMax = new NumericUpDown();
        private Button _btnDownload = new Button();

        private bool _isEventRunning = false;

        public event DownloadClickedHandler DownloadClicked;

        //===================================================================== INITIALIZE
        public LinkEntryPanel()
        {
            this.ForeColor = Color.White;
            this.BackColor = Colors.BODY_LINK;
            _btnDownload.FlatStyle = FlatStyle.Flat;
            _btnDownload.FlatAppearance.MouseDownBackColor = Colors.BODY_LINK_CLICKED;

            _lblUrl.AutoSize = true;
            _lblUrl.Text = "URL:";
            _comAlbumUrl.MaxDropDownItems = 10;
            _comQuality.DropDownStyle = ComboBoxStyle.DropDownList;
            _comQuality.Items.AddRange(new string[] { "LD", "SD", "HD", "Full HD" });
            _comQuality.SelectedIndex = 1;
            _chkEpisodeLimit.AutoSize = true;
            _chkEpisodeLimit.Text = "Only download episodes" + "".PadRight(17) + "to";
            _chkEpisodeLimit.CheckedChanged += chkEpisodeLimit_CheckedChanged;
            _numEpisodeMin.Enabled = false;
            _numEpisodeMin.Minimum = 1;
            _numEpisodeMin.Width = 60;
            _numEpisodeMin.ValueChanged += numEpisodeMin_ValueChanged;
            _numEpisodeMax.Enabled = false;
            _numEpisodeMax.Minimum = 1;
            _numEpisodeMax.Width = 60;
            _numEpisodeMax.ValueChanged += numEpisodeMax_ValueChanged;
            _chkAutoShutdown.AutoSize = true;
            _chkAutoShutdown.Text = "Auto Shutdown";
            _btnDownload.Text = "Download";
            _btnDownload.UseVisualStyleBackColor = true;
            _btnDownload.Size = new Size(_comQuality.Width, 30);
            _btnDownload.Click += btnOK_Click;

            this.Controls.Add(_lblUrl);
            this.Controls.Add(_comAlbumUrl);
            this.Controls.Add(_comQuality);
            this.Controls.Add(_chkEpisodeLimit);
            this.Controls.Add(_numEpisodeMin);
            this.Controls.Add(_numEpisodeMax);
            this.Controls.Add(_chkAutoShutdown);
            this.Controls.Add(_btnDownload);
        }

        protected override void OnCreateControl()
        {
            _lblUrl.Location = new Point(MARGIN, MARGIN + 3);
            _comQuality.Width = 75;
            _comQuality.Location = new Point(Width - _comQuality.Width - MARGIN, MARGIN);
            _comAlbumUrl.Location = new Point(_lblUrl.Width + MARGIN, MARGIN);
            _comAlbumUrl.SetWidth(_comQuality.Left - _lblUrl.Width - MARGIN * 2);
            _chkEpisodeLimit.Location = new Point(MARGIN, _comAlbumUrl.Bottom + 12);
            _numEpisodeMin.BringToFront();
            _numEpisodeMin.Location = new Point(_chkEpisodeLimit.Left + 173, _chkEpisodeLimit.Top);
            _numEpisodeMax.BringToFront();
            _numEpisodeMax.Location = new Point(_numEpisodeMin.Left + 85, _chkEpisodeLimit.Top);
            _chkAutoShutdown.Location = new Point(_numEpisodeMax.Right + MARGIN, _chkEpisodeLimit.Top);
            _btnDownload.Width = Width / 2;
            _btnDownload.Height = Height - MARGIN * 2 - _numEpisodeMax.Bottom;
            _btnDownload.Location = new Point((Width - _btnDownload.Width) / 2, Height - MARGIN - _btnDownload.Height);
 	        base.OnCreateControl();
        }

        //===================================================================== FUNCTIONS
        public void SetHistory(List<HistoryItem> history)
        {
            _comAlbumUrl.SetHistory(history);
        }

        public void Minimize(bool animate)
        {
            _lblUrl.Text = TudouDownloader.Version;
            if (_chkEpisodeLimit.Checked)
                _lblUrl.Text += "     |     Download ep " + _numEpisodeMin.Value + " to " + _numEpisodeMax.Value;

            _btnDownload.Visible = false; 
            _comAlbumUrl.Visible = false;
            _comQuality.Enabled = false;
            _chkEpisodeLimit.Visible = false;
            _numEpisodeMin.Visible = false;
            _numEpisodeMax.Visible = false;

            _lblUrl.Location = new Point(MARGIN / 2, MARGIN / 2 + 3);
            _comQuality.Location = new Point(Width - _comQuality.Width - MARGIN / 2, MARGIN / 2);
            _chkAutoShutdown.Location = new Point(_comQuality.Left - _chkAutoShutdown.Width - MARGIN, MARGIN / 2 + 2);

            if (animate)
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReduceHeight));
            else
                this.Height = _comQuality.Bottom + MARGIN / 2;
        }
        private void ReduceHeight(object arg)
        {
            while (this.Height != _comQuality.Bottom + MARGIN / 2)
            {
                this.Invoke((Action)(() => this.Height = Math.Max(this.Height - 15, _comQuality.Bottom + MARGIN / 2)));
                Thread.Sleep(15);
            }
        }

        //===================================================================== PROPERTIES
        public Quality Quality
        {
            get
            {
                switch (_comQuality.SelectedIndex)
                {
                    case 1: return Quality.SD;
                    case 2: return Quality.HD;
                    case 3: return Quality.FHD;
                    default: return Quality.LD;
                }
            }
        }
        public int[] EpisodeRange
        {
            get
            {
                if (_chkEpisodeLimit.Checked)
                    return new int[] { (int)_numEpisodeMin.Value, (int)_numEpisodeMax.Value };
                else
                    return new int[] { 1, int.MaxValue };
            }
        }
        public bool AutoShutdown
        {
            get { return _chkAutoShutdown.Checked; }
        }

        //===================================================================== EVENTS
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        private void chkEpisodeLimit_CheckedChanged(object sender, EventArgs e)
        {
            _numEpisodeMin.Enabled = _chkEpisodeLimit.Checked;
            _numEpisodeMax.Enabled = _chkEpisodeLimit.Checked;
        }
        private void numEpisodeMin_ValueChanged(object sender, EventArgs e)
        {
            if (!_isEventRunning)
            {
                _isEventRunning = true;
                _numEpisodeMax.Value = Math.Max(_numEpisodeMin.Value, _numEpisodeMax.Value);
                _isEventRunning = false;
            }
        }
        private void numEpisodeMax_ValueChanged(object sender, EventArgs e)
        {
            if (!_isEventRunning)
            {
                _isEventRunning = true;
                _numEpisodeMin.Value = Math.Min(_numEpisodeMin.Value, _numEpisodeMax.Value);
                _isEventRunning = false;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DownloadClicked?.Invoke(_comAlbumUrl.Text);
        }
    }


    public delegate void DownloadClickedHandler(string url);
}
