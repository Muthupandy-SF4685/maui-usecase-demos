namespace WeatherForecast;

/// <summary>
/// Represents aggregated daily forecast data for a single calendar day,
/// produced by grouping multiple 3-hourly API forecast entries.
/// </summary>
public class DailyForecast
{
    /// <summary>
    /// Gets or sets the calendar date for this daily forecast.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the abbreviated day name for list display (e.g., "Mon").
    /// </summary>
    public string DayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted date label for display (e.g., "Mar 3").
    /// </summary>
    public string DateLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the average temperature for the day, calculated across all 3-hour forecast slots.
    /// </summary>
    public double AvgTemperature { get; set; }

    /// <summary>
    /// Gets or sets the maximum temperature observed for the day, taken from the highest 3-hour slot.
    /// </summary>
    public double MaxTemperature { get; set; }

    /// <summary>
    /// Gets or sets the minimum temperature observed for the day, taken from the lowest 3-hour slot.
    /// </summary>
    public double MinTemperature { get; set; }

    /// <summary>
    /// Gets or sets the most common weather description for the day (e.g., "light rain").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OpenWeatherMap icon code representing the day's dominant weather condition.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full URL for the OpenWeatherMap icon image corresponding to the current <see cref="Icon"/> code.
    /// </summary>
    public string IconUrl => $"https://openweathermap.org/img/wn/{Icon}@2x.png";

    /// <summary>
    /// Gets or sets an emoji representing the dominant weather condition for the day.
    /// </summary>
    public string WeatherEmoji { get; set; } = "🌤️";

    /// <summary>
    /// Gets or sets a value indicating whether this day is the hottest day in the forecast range.
    /// </summary>
    public bool IsHottestDay { get; set; }

    /// <summary>
    /// Gets the formatted average temperature for display (e.g., "22°").
    /// </summary>
    public string AvgTempDisplay => $"{AvgTemperature:F0}°";

    /// <summary>
    /// Gets the formatted high/low temperature string for display (e.g., "H: 25° / L: 17°").
    /// </summary>
    public string HighLowDisplay => $"H:{MaxTemperature:F0}° / L:{MinTemperature:F0}°";

    /// <summary>
    /// Gets the badge text shown on the hottest day card; returns an empty string when this is not the hottest day.
    /// </summary>
    public string BadgeText => IsHottestDay ? "🔥 HOT" : string.Empty;
}
