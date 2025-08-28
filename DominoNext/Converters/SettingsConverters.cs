using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace DominoNext.Converters
{
    /// <summary>
    /// ���������ת����
    /// </summary>
    public class ObjectEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return parameter;
            }
            return BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// ö�ٵ��ַ���ת����
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue && targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, stringValue);
                }
                catch
                {
                    return BindingOperations.DoNothing;
                }
            }
            return BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// ˫���ȸ�������ʽ��ת����
    /// </summary>
    public class DoubleFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string format)
            {
                return doubleValue.ToString(format, culture);
            }
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue && double.TryParse(stringValue, NumberStyles.Float, culture, out double result))
            {
                return result;
            }
            return BindingOperations.DoNothing;
        }
    }
}