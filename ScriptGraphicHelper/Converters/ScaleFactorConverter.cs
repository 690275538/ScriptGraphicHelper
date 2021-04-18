using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScriptGraphicHelper.Converters
{
    class ScaleFactorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var scaleFactor = (double)value;
            return scaleFactor switch
            {
                0.4 => 0,
                0.6 => 1,
                0.8 => 2,
                1.0 => 3,
                1.5 => 4,
                2.0 => 5,
                2.5 => 6,
                3.0 => 7,
                _ => 3
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var scaleFactor = (int)value;
            return scaleFactor switch
            {
                0 => 0.4,
                1 => 0.6,
                2 => 0.8,
                3 => 1.0,
                4 => 1.5,
                5 => 2.0,
                6 => 2.5,
                7 => 3.0,
                _ => 1.0
            };
        }
    }
}
