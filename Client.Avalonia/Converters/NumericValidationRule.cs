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
        {
            // Преобразуем в строку с запятой вместо точки
            return d.ToString("G", CultureInfo.GetCultureInfo("ru-RU"));
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string input)
        {
            input = input.Replace('.', ','); // Автоматически заменяем точку на запятую

            // Проверяем, является ли введённый текст числом
            if (NumericRegex.IsMatch(input) && double.TryParse(input, NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out var result))
            {
                return result;
            }
        }

        // Если ввод некорректен — возвращаем ошибку
        return new BindingNotification(new InvalidOperationException("Введите корректное число!"), BindingErrorType.DataValidationError);
    }
}