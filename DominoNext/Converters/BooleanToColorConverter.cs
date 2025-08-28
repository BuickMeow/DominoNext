using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DominoNext.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public static readonly BooleanToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isBlackKey)
            {
                return isBlackKey ? Brushes.Black : Brushes.White;
            }
            return Brushes.White;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanToTextColorConverter : IValueConverter
    {
        public static readonly BooleanToTextColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isBlackKey)
            {
                return isBlackKey ? Brushes.White : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}