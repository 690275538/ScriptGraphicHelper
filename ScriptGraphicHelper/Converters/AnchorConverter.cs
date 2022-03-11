using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScriptGraphicHelper.Converters
{
    public enum AnchorMode
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
            var anchor = (AnchorMode)value;
            return anchor switch
            {
                AnchorMode.None => -1,
                AnchorMode.Left => 0,
                AnchorMode.Center => 1,
                AnchorMode.Right => 2,
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
                -1 => AnchorMode.None,
                0 => AnchorMode.Left,
                1 => AnchorMode.Center,
                2 => AnchorMode.Right,
                _ => AnchorMode.None,
            };
        }
    }
}
