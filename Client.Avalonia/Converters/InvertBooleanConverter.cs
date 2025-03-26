using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Client.Avalonia.Converters;

public class InvertBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value is bool b
                   ? !b
                   : value)
               ?? throw new InvalidOperationException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}