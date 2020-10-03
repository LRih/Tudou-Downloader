using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TudouDownloader
{
    public class ShutdownCountdown : Form
    {
        //===================================================================== CONSTANTS
        private const int MARGIN = 10;

        //===================================================================== CONTROLS
        private IContainer _components = new Container();
        private Label _lblMessage = new Label();
        private Button _btnCancel = new Button();
        private Timer _timer;

        //===================================================================== VARIABLES
        private int _countdown = 20;

        //===================================================================== INITIALIZE
        public ShutdownCountdown()
        {
            this.ClientSize = new Size(300, 100);
            _lblMessage.Location = new Point(MARGIN, MARGIN);
            _lblMessage.Size = new Size(ClientSize.Width - MARGIN / 2, 50);
            _btnCancel.Text = "Cancel";
            _btnCancel.Location = new Point((ClientSize.Width - _btnCancel.Width) / 2, ClientSize.Height - MARGIN - _btnCancel.Height);
            _btnCancel.Click += btnCancel_Click;
            _timer = new Timer(_components);
            _timer.Interval = 1000;
            _timer.Start();
            _timer.Tick += timer_Tick;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Shutdown Dialog";
            this.Controls.Add(_lblMessage);
            this.Controls.Add(_btnCancel);
            UpdateCountdown();
        }

        //===================================================================== TERMINATE
        protected override void Dispose(bool disposing)
        {
            if (disposing && _components != null)
                _components.Dispose();
            base.Dispose(disposing);
        }

        //===================================================================== FUNCTIONS
        private void UpdateCountdown()
        {
            _lblMessage.Text = "The computer will shut down in:\r\n\r\n" + "".PadRight(5) + _countdown.ToString() + " seconds";
        }

        //===================================================================== EVENTS
        private void timer_Tick(object sender, EventArgs e)
        {
            _countdown--;

            if (_countdown < 0)
            {
                _timer.Stop();
                System.Diagnostics.Process.Start("shutdown", "/s /t 0");
            }
            else
                UpdateCountdown();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
