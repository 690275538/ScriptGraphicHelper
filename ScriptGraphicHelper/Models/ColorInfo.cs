
using Avalonia;
using Avalonia.Media;
using System;

namespace ScriptGraphicHelper.Models
{
    public class ColorInfo
    {
        public static double Width { get; set; } = 0;
        public static double Height { get; set; } = 0;


        public int Index { get; set; }
        public Point Point { get; set; }
        public Color Color { get; set; }
        public bool IsChecked { get; set; }
        public string Anchors { get; set; }
        


        public ColorInfo(int index, int x, int y, byte[] color)
        {
            Index = index;
            Point = new Point(x, y);
            Color = Color.FromRgb(color[0], color[1], color[2]);
            IsChecked = true;
        }

        public ColorInfo(int index, string anchors, Point point, System.Drawing.Color color)
        {

        }

        

    }
}
