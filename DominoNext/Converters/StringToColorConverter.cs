using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace DominoNext.Converters
{
    /// <summary>
    /// 字符串颜色代码转换为Color对象的转换器
    /// </summary>
    public class StringToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string colorString && !string.IsNullOrEmpty(colorString))
            {
                try
                {
                    return Color.Parse(colorString);
                }
                catch
                {
                    // 解析失败时返回默认颜色
                    return Colors.Gray;
                }
            }
            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return color.ToString();
            }
            return "#000000";
        }
    }
}