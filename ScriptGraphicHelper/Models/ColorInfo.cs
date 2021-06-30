
using Avalonia;
using Avalonia.Media;
using ScriptGraphicHelper.Converters;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Models
{
    public class ColorInfo
    {
        public static double Width { get; set; } = 0;

        public static double Height { get; set; } = 0;

        public static List<string> AnchorItems => new() { "N", "L", "C", "R" };

        public int Index { get; set; }

        public AnchorType Anchor { get; set; } = AnchorType.None;

        public Point Point { get; set; }

        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                if (value.R < 0x40 && value.G < 0x40 && value.B < 0x40)
                {
                    MarkBrush = Color.FromRgb(0xe8, 0xe8, 0xe8);
                }
            }
        }

        public Color MarkBrush { get; set; } = Colors.Black;

        public bool IsChecked { get; set; } = false;

        public ColorInfo() { }

        public ColorInfo(int index, int x, int y, byte[] color)
        {
            Index = index;
            Point = new Point(x, y);
            Color = Color.FromRgb(color[0], color[1], color[2]);
            IsChecked = true;
            Anchor = AnchorType.None;
        }

        public ColorInfo(int index, int x, int y, Color color)
        {
            Index = index;
            Point = new Point(x, y);
            Color = color;
            IsChecked = true;
            Anchor = AnchorType.None;
        }

        public ColorInfo(int index, AnchorType anchor, int x, int y, byte[] color)
        {
            Index = index;
            Anchor = anchor;
            Point = new Point(x, y);
            Color = Color.FromRgb(color[0], color[1], color[2]);
            IsChecked = true;
        }

    }
}
