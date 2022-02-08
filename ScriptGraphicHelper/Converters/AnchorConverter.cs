using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScriptGraphicHelper.Converters
{
    public enum AnchorType
    {
        None = -1,
        Left = 0,
        Center = 1,
        Right = 2,
    }
    class AnchorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var anchor = (AnchorType)value;
            return anchor switch
            {
                AnchorType.None => 0,
                AnchorType.Left => 1,
                AnchorType.Center => 2,
                AnchorType.Right => 3,
                _ => 0
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var anchor = (int)value;
            return anchor switch
            {
                0 => AnchorType.None,
                1 => AnchorType.Left,
                2 => AnchorType.Center,
                3 => AnchorType.Right,
                _ => AnchorType.None,
            };
        }
    }
}
