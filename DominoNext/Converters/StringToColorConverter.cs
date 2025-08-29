using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace DominoNext.Converters
{
    /// <summary>
    /// �ַ�����ɫ����ת��ΪColor�����ת����
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
                    // ����ʧ��ʱ����Ĭ����ɫ
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