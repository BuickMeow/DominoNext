using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DominoNext.Converters
{
    public class VelocityToOpacityConverter : IValueConverter
    {
        public static readonly VelocityToOpacityConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int velocity)
            {
                // 将MIDI力度值 (0-127) 转换为透明度 (0.3-1.0)
                return Math.Max(0.3, velocity / 127.0);
            }
            return 0.8;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}