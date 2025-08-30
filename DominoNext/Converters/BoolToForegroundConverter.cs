using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DominoNext.Converters
{
    public class BoolToForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.White : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}