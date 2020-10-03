using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TudouDownloader
{
    public class HistoryComboBox : ComboBox
    {
        //===================================================================== CONSTANTS
        private const int ITEM_HEIGHT = 45;
        private const int SELECTION_RADIUS = 5;
        private const int MARGIN_X = 4;

        //===================================================================== VARIABLES
        private Bitmap _bmpSelection;

        //===================================================================== INITIALIZE
        public HistoryComboBox()
        {
            DoubleBuffered = true;
            DrawMode = DrawMode.OwnerDrawVariable;
            MaxDropDownItems = 10;
            RecreateBitmap();
        }
        private void RecreateBitmap()
        {
            if (_bmpSelection != null) _bmpSelection.Dispose();
            _bmpSelection = new Bitmap(Width - 2, ITEM_HEIGHT);
            Graphics g = Graphics.FromImage(_bmpSelection);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle selRect = new Rectangle(1, 1, _bmpSelection.Width - 2, _bmpSelection.Height - 2);
            GraphicsPath fillPath = GraphicsUtils.GetRoundedRectanglePath(selRect, SELECTION_RADIUS);
            selRect.Width--;
            selRect.Height--;
            GraphicsPath drawPath = GraphicsUtils.GetRoundedRectanglePath(selRect, SELECTION_RADIUS);
            using (Brush brush = new LinearGradientBrush(selRect, Color.FromArgb(227, 238, 253), Color.FromArgb(216, 233, 252), 90))
                g.FillPath(brush, fillPath);
            using (Pen pen = new Pen(Color.FromArgb(124, 163, 206)))
                g.DrawPath(pen, drawPath);
            g.Dispose();
        }

        //===================================================================== TERMINATE
        protected override void Dispose(bool disposing)
        {
            _bmpSelection.Dispose();
            _bmpSelection = null;
            base.Dispose(disposing);
        }

        //===================================================================== FUNCTIONS
        public void SetHistory(List<HistoryItem> history)
        {
            DataSource = history;
            DisplayMember = "Url";
            ValueMember = "Url";
            DropDownHeight = ITEM_HEIGHT * Math.Max(Items.Count, 1) + 2;
        }
        public void SetWidth(int width)
        {
            Width = width;
            RecreateBitmap();
        }

        //===================================================================== EVENTS
        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            e.ItemHeight = ITEM_HEIGHT;
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.State.HasFlag(DrawItemState.Selected)) e.Graphics.DrawImageUnscaled(_bmpSelection, e.Bounds);
            else e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            DrawHistoryItem(e.Graphics, e.Bounds, (HistoryItem)this.Items[e.Index]);
        }
        private void DrawHistoryItem(Graphics g, Rectangle itemRect, HistoryItem historyItem)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rectName = new Rectangle(itemRect.X + MARGIN_X, itemRect.Y + 1, itemRect.Width - MARGIN_X * 2, itemRect.Height / 2);
            Rectangle rectUrl = new Rectangle(itemRect.X + MARGIN_X, itemRect.Y + 1 + itemRect.Height / 2, itemRect.Width - MARGIN_X * 2, itemRect.Height / 2);
            using (Font font = new Font("Microsoft Yahei", 10))
                TextRenderer.DrawText(g, historyItem.Name, font, rectName, this.ForeColor, TextFormatFlags.Bottom);
            using (Font font = new Font("Microsoft Sans Serif", 9))
                TextRenderer.DrawText(g, historyItem.Url, font, rectUrl, Color.FromArgb(52, 122, 212), TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        }
    }
}
