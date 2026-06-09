using System.Text.Json.Serialization;

namespace WeatherForecast;

// ────────────────────────────────────────────────────────────────────────────
// Open-Meteo response models (https://open-meteo.com)
// No API key required.
// Geocoding:  GET https://geocoding-api.open-meteo.com/v1/search
// Forecast:   GET https://api.open-meteo.com/v1/forecast
// ────────────────────────────────────────────────────────────────────────────

// ── Geocoding ────────────────────────────────────────────────────────────────

/// <summary>
/// Response container for the Open-Meteo geocoding API.
/// </summary>
public class GeocodingResponse
{
    /// <summary>
    /// The list of geocoding results returned by the API. May be null if no results were found.
    /// </summary>
    [JsonPropertyName("results")]
    public List<GeocodingResult>? Results { get; set; }
}

/// <summary>
/// A single geocoding result returned by the Open-Meteo geocoding API.
/// Contains the location name, coordinates and country information.
/// </summary>
public class GeocodingResult
{
    /// <summary>
    /// The name of the location (e.g., city or place).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The latitude of the location in decimal degrees.
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// The longitude of the location in decimal degrees.
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// The country name for the location.
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// The ISO 3166-1 alpha-2 country code for the location.
    /// </summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = string.Empty;
}

// ── Forecast ─────────────────────────────────────────────────────────────────

/// <summary>
/// Root object for the Open-Meteo forecast API response,
/// including coordinates, timezone, current conditions, hourly and daily data.
/// </summary>
public class OpenMeteoForecastResponse
{
    /// <summary>
    /// The latitude of the forecast location.
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// The longitude of the forecast location.
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// The timezone identifier for the forecast data (e.g., "Europe/Berlin").
    /// </summary>
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    /// <summary>
    /// The current weather conditions.
    /// </summary>
    [JsonPropertyName("current")]
    public OpenMeteoCurrent Current { get; set; } = new();

    /// <summary>
    /// The hourly forecast data arrays.
    /// </summary>
    [JsonPropertyName("hourly")]
    public OpenMeteoHourly Hourly { get; set; } = new();

    /// <summary>
    /// The daily forecast data arrays.
    /// </summary>
    [JsonPropertyName("daily")]
    public OpenMeteoDaily Daily { get; set; } = new();
}

/// <summary>
/// Current weather data returned by the Open-Meteo API.
/// Fields map to the "current" section of the forecast response.
/// </summary>
public class OpenMeteoCurrent
{
    /// <summary>
    /// The timestamp of the current observation in ISO 8601 format.
    /// </summary>
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// The current air temperature at 2 meters above ground in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }

    /// <summary>
    /// The apparent (feels-like) temperature in degrees Celsius.
    /// </summary>
    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    /// <summary>
    /// The weather condition code as defined by Open-Meteo.
    /// </summary>
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    /// <summary>
    /// The wind speed at 10 meters above ground in the API's units (typically m/s).
    /// </summary>
    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed10m { get; set; }

    /// <summary>
    /// The relative humidity at 2 meters as a percentage.
    /// </summary>
    [JsonPropertyName("relative_humidity_2m")]
    public int RelativeHumidity2m { get; set; }
}

/// <summary>
/// Hourly forecast arrays returned by the Open-Meteo API.
/// Each list contains values aligned by index (time, temperature, weather code, precipitation probability).
/// </summary>
public class OpenMeteoHourly
{
    /// <summary>
    /// The list of hourly timestamps in ISO 8601 format.
    /// </summary>
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = [];

    /// <summary>
    /// The list of hourly temperatures at 2 meters in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperature_2m")]
    public List<double> Temperature2m { get; set; } = [];

    /// <summary>
    /// The list of hourly weather condition codes.
    /// </summary>
    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; } = [];

    /// <summary>
    /// The list of hourly precipitation probabilities as percentages.
    /// </summary>
    [JsonPropertyName("precipitation_probability")]
    public List<int> PrecipitationProbability { get; set; } = [];
}

/// <summary>
/// Daily forecast arrays returned by the Open-Meteo API.
/// Each list contains values aligned by index (date, weather code, daily max/min temperatures, max precipitation probability).
/// </summary>
public class OpenMeteoDaily
{
    /// <summary>
    /// The list of daily dates in ISO 8601 format.
    /// </summary>
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = [];

    /// <summary>
    /// The list of daily weather condition codes.
    /// </summary>
    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; } = [];

    /// <summary>
    /// The list of daily maximum temperatures at 2 meters in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperature_2m_max")]
    public List<double> Temperature2mMax { get; set; } = [];

    /// <summary>
    /// The list of daily minimum temperatures at 2 meters in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperature_2m_min")]
    public List<double> Temperature2mMin { get; set; } = [];

    /// <summary>
    /// The list of daily maximum precipitation probabilities as percentages.
    /// </summary>
    [JsonPropertyName("precipitation_probability_max")]
    public List<int> PrecipitationProbabilityMax { get; set; } = [];
}