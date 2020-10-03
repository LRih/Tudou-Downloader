using System.Drawing;
namespace TudouDownloader
{
    public static class Colors
    {
        //===================================================================== Link Entry Panel
        public static readonly Color BODY_LINK = Color.SteelBlue;
        public static readonly Color BODY_LINK_CLICKED = Color.FromArgb(57, 107, 148);
        public static readonly Pen BODY_BORDER_LINK = Pens.PowderBlue;

        //===================================================================== Download Manager Panel
        public static readonly Color TEXT = Color.DarkSlateGray;
        public static readonly Brush TEXT_BRUSH = Brushes.DarkSlateGray;

        public static readonly Color LINK = Color.FromArgb(0, 136, 204);
        public static readonly Color LINK_ACTIVE = Color.FromArgb(65, 72, 80);

        public static readonly Brush STATUS = Brushes.AliceBlue;
        public static readonly Pen STATUS_BORDER = Pens.SkyBlue;
    }
}
