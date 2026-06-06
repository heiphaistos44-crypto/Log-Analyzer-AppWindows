using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WinLogAnalyzer.App.Infrastructure;

/// <summary>Severite / niveau -> couleur (badge, bordure). Retourne un Brush.</summary>
public sealed class SeverityToBrushConverter : IValueConverter
{
    public static readonly SolidColorBrush Critical = New("#FF5C5C");
    public static readonly SolidColorBrush Error = New("#FF9F40");
    public static readonly SolidColorBrush Warning = New("#FFD24C");
    public static readonly SolidColorBrush Info = New("#4CC4FF");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value as string)?.ToLowerInvariant() switch
        {
            "critical" => Critical,
            "error" => Error,
            "warning" => Warning,
            _ => Info
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static SolidColorBrush New(string hex)
    {
        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        b.Freeze();
        return b;
    }
}
