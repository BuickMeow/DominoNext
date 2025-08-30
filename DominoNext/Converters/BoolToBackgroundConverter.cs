using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DominoNext.Converters
{
    public class BoolToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? new SolidColorBrush(Color.Parse("#4CAF50")) : new SolidColorBrush(Color.Parse("#F0F0F0"));
            }
            return new SolidColorBrush(Color.Parse("#F0F0F0"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}