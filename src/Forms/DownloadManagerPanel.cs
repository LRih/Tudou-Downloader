using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TudouDownloader
{
    public class DownloadManagerPanel : UserControl
    {
        //===================================================================== CONSTANTS
        private const int MARGIN = 10;

        //===================================================================== CONTROLS
        private PictureBox _picCover = new PictureBox();
        private Label _lblTitle = new Label();
        private Label _lblType = new Label();
        private Label _lblRegion = new Label();
        private Label _lblYear = new Label();
        private LinkLabel _lblUrl = new LinkLabel();
        private Label _lblStatus = new Label();

        //===================================================================== VARIABLES
        private Font _fontTitle = new Font("Microsoft Yahei", 14);
        private Font _fontInfo = new Font("Microsoft Yahei", 10);

        private string[] _downloadStatus = new string[DownloadManager.MAX_SIMULTANEOUS_DOWNLOADS];
        private float[] _downloadFraction = new float[DownloadManager.MAX_SIMULTANEOUS_DOWNLOADS];

        public event OnLinkClickedHandler LinkClicked;

        //===================================================================== INITIALIZE
        public DownloadManagerPanel()
        {
            _picCover.BackColor = Color.Transparent;
            _lblTitle.BackColor = _lblType.BackColor = _lblRegion.BackColor = _lblYear.BackColor = _lblUrl.BackColor = _lblStatus.BackColor = Color.Transparent;

            _picCover.Size = new Size(80, 120);
            _picCover.SizeMode = PictureBoxSizeMode.Zoom;

            _lblTitle.Font = _fontTitle;
            _lblType.Font = _lblRegion.Font = _lblYear.Font = _lblUrl.Font = _fontInfo;
            _lblTitle.ForeColor = _lblType.ForeColor = _lblRegion.ForeColor = _lblYear.ForeColor = _lblStatus.ForeColor = Colors.TEXT;
            _lblTitle.AutoSize = _lblType.AutoSize = _lblRegion.AutoSize = _lblYear.AutoSize = _lblUrl.AutoSize = _lblStatus.AutoSize = true;

            _lblUrl.ActiveLinkColor = Colors.LINK_ACTIVE;
            _lblUrl.LinkBehavior = LinkBehavior.HoverUnderline;
            _lblUrl.LinkColor = Colors.LINK;
            _lblUrl.Text = "Go to page";
            _lblUrl.LinkClicked += lblUrl_LinkClicked;

            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            this.Controls.Add(_picCover);
            this.Controls.Add(_lblTitle);
            this.Controls.Add(_lblType);
            this.Controls.Add(_lblRegion);
            this.Controls.Add(_lblYear);
            this.Controls.Add(_lblUrl);
            this.Controls.Add(_lblStatus);

            // initialize data
            for (int i = 0; i < _downloadStatus.Length; i++)
            {
                _downloadStatus[i] = "";
                _downloadFraction[i] = -1;
            }
        }

        //===================================================================== TERMINATE
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_picCover.Image != null)
                    _picCover.Image.Dispose();
                _fontTitle.Dispose();
                _fontInfo.Dispose();
            }

            base.Dispose(disposing);
        }

        //===================================================================== FUNCTIONS
        public void HideGotoPage()
        {
            _lblUrl.Visible = false;
        }

        public void SetInfo(string coverUrl, string title, string type, string region, string year)
        {
            _picCover.ImageLocation = coverUrl;
            _lblTitle.Text = title;
            _lblType.Text = type;
            _lblRegion.Text = region.Length > 0 ? "地区： " + region : "";
            _lblYear.Text = year.Length > 0 ? "年代： " + year : "";
        }
        public void SetDownloadStatus(int id, string status, float fraction)
        {
            _downloadStatus[id] = status;
            _downloadFraction[id] = fraction;
            this.Invalidate();
        }
        public void SetStatus(string status)
        {
            if (_lblStatus.Text != status)
                _lblStatus.Text = status;
        }

        private static void DrawRoundedRectangle(Graphics g, Rectangle rect, int radius, Color color)
        {
            using (Pen pen = new Pen(color))
            using (GraphicsPath path = GraphicsUtils.GetRoundedRectanglePath(rect, radius))
                g.DrawPath(pen, path);
        }

        //===================================================================== EVENTS
        protected override void OnPaint(PaintEventArgs e)
        {
            int albumBoxHeight = _picCover.Height + MARGIN * 2;
            int downloadBoxY = albumBoxHeight + MARGIN;
            int statusBoxHeight = _lblStatus.Font.Height + MARGIN;
            int statusBoxY = ClientSize.Height - statusBoxHeight;

            e.Graphics.FillRectangle(Colors.STATUS, 0, 0, ClientSize.Width, albumBoxHeight);
            e.Graphics.DrawLine(Colors.BODY_BORDER_LINK, 0, 0, Width, 0);
            e.Graphics.DrawLine(Colors.STATUS_BORDER, 0, albumBoxHeight, ClientSize.Width, albumBoxHeight);

            e.Graphics.FillRectangle(Colors.STATUS, 0, statusBoxY, ClientSize.Width, statusBoxHeight);
            e.Graphics.DrawLine(Colors.STATUS_BORDER, 0, statusBoxY, ClientSize.Width, statusBoxY);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 0; i < _downloadStatus.Length; i++)
            {
                int y = downloadBoxY + MARGIN / 2 + i * (this.Font.Height + 6);
                string status = _downloadStatus[i];
                e.Graphics.DrawString(status, _fontInfo, Colors.TEXT_BRUSH, MARGIN, y);

                if (_downloadFraction[i] >= 0)
                {
                    int totalWidth = 150;
                    float width = totalWidth * _downloadFraction[i];
                    int x = ClientSize.Width - totalWidth - MARGIN;
                    e.Graphics.FillRectangle(Brushes.YellowGreen, x, y, width, this.Font.Height);
                    e.Graphics.DrawRectangle(Pens.OliveDrab, x, y, totalWidth, this.Font.Height);

                    string percentage = Math.Round(_downloadFraction[i] * 100, 2).ToString("N2") + " %";
                    e.Graphics.DrawString(percentage, _fontInfo, Colors.TEXT_BRUSH, x + totalWidth / (float)2, y, new StringFormat { Alignment = StringAlignment.Center });
                }
            }

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            _picCover.Location = new Point(MARGIN, MARGIN);

            _lblTitle.Location = new Point(_picCover.Right + 6, _picCover.Top);
            _lblType.Location = new Point(_picCover.Right + 6, _lblTitle.Bottom + 6);
            _lblRegion.Location = new Point(_picCover.Right + 6, _lblType.Bottom + 6);
            _lblYear.Location = new Point(_picCover.Right + 6, _lblRegion.Bottom);
            _lblUrl.Location = new Point(ClientSize.Width - 92, _lblRegion.Bottom + 22);

            _lblStatus.Location = new Point(MARGIN, ClientSize.Height - (_lblStatus.Font.Height + MARGIN) + 4);

            this.Invalidate();
            base.OnResize(e);
        }

        private void lblUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left && LinkClicked != null)
                LinkClicked();
        }
    }


    public delegate void OnLinkClickedHandler();
}
