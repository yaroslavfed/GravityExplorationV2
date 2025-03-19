using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Client.Core.Enums;

namespace Client.Avalonia.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true
            && parameter is string enumString
            && Enum.TryParse(typeof(EProjection), enumString, out var result))
        {
            return result;
        }

        return BindingOperations.DoNothing; // Используем Avalonia-эквивалент для "ничего не менять"
    }
}