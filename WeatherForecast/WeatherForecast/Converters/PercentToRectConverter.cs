using System.Globalization;
using Microsoft.Maui.Controls;

namespace WeatherForecast;

/// <summary>
/// Converts a 0–100 percentage value to a Rect used by AbsoluteLayout.LayoutBounds,
/// placing a fixed-size element horizontally at the proportional X with centered Y.
/// </summary>
public class PercentToRectConverter : IValueConverter
{
    /// <summary>
    /// Converts a numeric percentage value to a normalized rectangle representing a progress bar footprint, with optional customization of size and vertical alignment.
    /// </summary>
    /// <param name="value">The value to convert, representing the percentage to display.</param>
    /// <param name="targetType">The type of the binding target property. This parameter is not used.</param>
    /// <param name="parameter">An optional string specifying the rectangle's width, height, and vertical center.</param>
    /// <param name="culture">The culture to use in the converter. This parameter is used for parsing numeric values in the parameter string.</param>
    /// <returns>A Microsoft.Maui.Graphics.Rect representing the normalized progress bar footprint, with the width, height, and vertical alignment specified by the parameter or default values.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double percent = 0.0;
        if (value is double d) percent = d;
        else if (value is float f) percent = f;
        else if (value is int i) percent = i;

        var norm = Math.Clamp(percent / 100.0, 0.0, 1.0);

        double width = 18.0;  // default footprint width
        double height = 18.0; // default footprint height
        double y = 0.5;       // default vertical center

        if (parameter is string s)
        {
            // Supported formats (backward compatible):
            //  - "36"                          => width=36,  height=36,  y=0.5
            //  - "36,0.1" or "36|0.1"        => width=36,  height=36,  y=0.1
            //  - "60,18" or "60x18"          => width=60,  height=18,  y=0.5
            //  - "60,18,0.1" or "60x18|0.1" => width=60,  height=18,  y=0.1
            var parts = s.Split(new[] { ',', '|', ' ', 'x', 'X' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 1 && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var p0))
                width = p0;

            if (parts.Length >= 2 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var p1))
            {
                if (p1 <= 1.0)
                {
                    // Treat as Y when <= 1; keep height same as width
                    y = Math.Clamp(p1, 0.0, 1.0);
                }
                else
                {
                    height = p1;
                }
            }

            if (parts.Length >= 3 && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var p2))
            {
                y = Math.Clamp(p2, 0.0, 1.0);
            }
        }

        return new Microsoft.Maui.Graphics.Rect(norm, y, width, height);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}