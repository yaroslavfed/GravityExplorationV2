using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Client.Avalonia.Converters;

public class NumericValidationRule : IValueConverter
{
    private static readonly Regex NumericRegex = new(@"^-?\d+([,.]\d+)?$", RegexOptions.Compiled);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
            return d.ToString("G", CultureInfo.GetCultureInfo("ru-RU"));

        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string input)
            return new BindingNotification(new("Введите корректное число!"), BindingErrorType.DataValidationError);

        input = input.Replace('.', ',');

        if (NumericRegex.IsMatch(input)
            && double.TryParse(input, NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out var result))
            return result;

        return new BindingNotification(new("Введите корректное число"), BindingErrorType.DataValidationError);
    }
}