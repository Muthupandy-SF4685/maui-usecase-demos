namespace WeatherForecast;

/// <summary>
/// Represents the current weather conditions and forecast data for a specific city.
/// </summary>
public class WeatherResult
{
    /// <summary>
    /// The city or location name for the weather result.
    /// </summary>
    public string CityName { get; init; } = string.Empty;

    /// <summary>
    /// The current air temperature in degrees Celsius.
    /// </summary>
    public double CurrentTemperature { get; init; }

    /// <summary>
    /// The apparent ("feels like") temperature in degrees Celsius.
    /// </summary>
    public double FeelsLike { get; init; }

    /// <summary>
    /// A short human-readable description of the current weather.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The icon code representing the current weather condition.
    /// </summary>
    public string CurrentIcon { get; init; } = "01d";

    /// <summary>
    /// The relative humidity as a percentage.
    /// </summary>
    public int Humidity { get; init; }

    /// <summary>
    /// The wind speed (typically in meters per second).
    /// </summary>
    public double WindSpeed { get; init; }

    /// <summary>
    /// The expected high temperature for the current day in degrees Celsius.
    /// </summary>
    public double HighTemperature { get; init; }

    /// <summary>
    /// The expected low temperature for the current day in degrees Celsius.
    /// </summary>
    public double LowTemperature { get; init; }

    /// <summary>
    /// Hourly forecast entries aligned by hour.
    /// </summary>
    public List<HourlyForecast> HourlyForecasts { get; init; } = [];

    /// <summary>
    /// Daily forecast entries aligned by day.
    /// </summary>
    public List<DailyForecast> DailyForecasts { get; init; } = [];
}

/// <summary>
/// Represents errors that occur when interacting with the weather service API.
/// </summary>
public class WeatherServiceException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="WeatherServiceException"/> with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public WeatherServiceException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="WeatherServiceException"/> with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public WeatherServiceException(string message, Exception inner) : base(message, inner) { }
}
