using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WinLogAnalyzer.App.Infrastructure;

/// <summary>
/// Non-null -> Visible, null -> Collapsed. Parametre "invert" pour inverser.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasValue = value is not null;
        if (string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase))
            hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
