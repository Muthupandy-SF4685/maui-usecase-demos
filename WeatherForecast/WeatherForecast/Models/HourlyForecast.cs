namespace WeatherForecast;

/// <summary>
/// Represents a processed model for a single hourly (3-hour) forecast slot, used by the Today tab chart and horizontal list.
/// </summary>
public class HourlyForecast
{
    /// <summary>
    /// Gets or sets the date and time for this hourly forecast slot.
    /// Represents the start time of the 3-hour forecast period.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Gets or sets the short label shown on the chart x-axis (e.g., "09:00").
    /// </summary>
    public string TimeLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temperature in degrees Celsius for this slot.
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// Gets or sets the weather description for this slot (e.g., "light rain").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OpenWeatherMap icon code for this slot (e.g., "10d").
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full URL for the OpenWeatherMap icon image corresponding to the current <see cref="Icon"/> code.
    /// </summary>
    public string IconUrl => $"https://openweathermap.org/img/wn/{Icon}@2x.png";

    /// <summary>
    /// Gets or sets the humidity percentage for this hourly forecast slot (0-100).
    /// </summary>
    public int Humidity { get; set; }

    /// <summary>
    /// Gets or sets the wind speed in meters per second for this slot.
    /// </summary>
    public double WindSpeed { get; set; }

    /// <summary>
    /// Gets or sets the precipitation probability as a percentage (0-100).
    /// </summary>
    public int PrecipChance { get; set; }

    /// <summary>
    /// Gets or sets a Unicode emoji approximating the weather condition for this slot.
    /// </summary>
    public string WeatherEmoji { get; set; } = "🌤️";

    /// <summary>
    /// Gets the formatted temperature for display (e.g., "22°").
    /// </summary>
    public string TempDisplay => $"{Temperature:F0}°";

    /// <summary>
    /// Gets the formatted precipitation probability for display (e.g., "40%").
    /// </summary>
    public string PrecipDisplay => $"{PrecipChance}%";
}
