using System.Net.Http.Json;
using System.Text.Json;

namespace WeatherForecast;

/// <summary>
/// Fetches weather data from Open-Meteo (https://open-meteo.com).
/// Completely free – no API key required.
///
/// Flow:
///   1. Geocoding API  → resolve city name to lat/lon
///       GET https://geocoding-api.open-meteo.com/v1/search
///   2. Forecast API   → current + hourly + daily weather
///       GET https://api.open-meteo.com/v1/forecast
/// </summary>
public class WeatherService
{
    /// <summary>
    /// Represents the base URL for the Open-Meteo geocoding API search endpoint.
    /// </summary>
    private const string GeocodingBaseUrl = "https://geocoding-api.open-meteo.com/v1/search";

    /// <summary>
    /// The base URL for the Open-Meteo forecast API endpoint.
    /// </summary>
    private const string ForecastBaseUrl  = "https://api.open-meteo.com/v1/forecast";

    /// <summary>
    /// Provides default JSON serializer options with case-insensitive property name matching.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Field to hold the HttpClient instance used for making API requests.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherService"/> class using the specified HTTP client.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> instance used to send HTTP requests to weather data providers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="httpClient"/> is <c>null</c>.</exception>
    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.Timeout = TimeSpan.FromSeconds(20);
    }

    // ── Public API ───────────────────────────────────────────────────────────
    
    /// <summary>
    /// Asynchronously retrieves the current weather and a 7-day forecast for the specified city.
    /// </summary>
    /// <param name="city">The name of the city for which to retrieve the weather forecast. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="WeatherResult"/> object with current conditions and forecast data for the specified city.</returns>
    /// <exception cref="ArgumentException"><paramref name="city"/> is null, empty, or consists only of white-space characters.</exception>
    /// <exception cref="WeatherServiceException">A network, timeout, or parsing error occurred while fetching weather data.</exception>
    /// <exception cref="InvalidOperationException">The upstream API returned an empty or unexpected response.</exception>
    public async Task<WeatherResult> GetForecastAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City name must not be empty.", nameof(city));

        // Step 1: geocode city name → lat/lon ────────────────────────────────
        var geoUrl = $"{GeocodingBaseUrl}?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
        GeocodingResponse geoResponse;
        try
        {
            geoResponse = await _httpClient
                              .GetFromJsonAsync<GeocodingResponse>(geoUrl, _jsonOptions)
                          ?? throw new InvalidOperationException("Empty geocoding response.");
        }
        catch (HttpRequestException ex)
        {
            throw new WeatherServiceException(
                $"Network error while geocoding '{city}': {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new WeatherServiceException(
                "Request timed out. Check your internet connection.", ex);
        }

        if (geoResponse.Results is not { Count: > 0 })
            throw new WeatherServiceException($"City '{city}' not found.");

        var geo       = geoResponse.Results[0];
        var cityLabel = string.IsNullOrEmpty(geo.CountryCode)
            ? geo.Name
            : $"{geo.Name}, {geo.CountryCode}";

        // Step 2: fetch forecast ──────────────────────────────────────────────
        var forecastUrl =
            $"{ForecastBaseUrl}" +
            $"?latitude={geo.Latitude:F4}&longitude={geo.Longitude:F4}" +
            "&current=temperature_2m,apparent_temperature,weather_code,wind_speed_10m,relative_humidity_2m" +
            "&hourly=temperature_2m,weather_code,precipitation_probability" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
            "&timezone=auto&forecast_days=7";

        OpenMeteoForecastResponse forecast;
        try
        {
            forecast = await _httpClient
                           .GetFromJsonAsync<OpenMeteoForecastResponse>(forecastUrl, _jsonOptions)
                       ?? throw new InvalidOperationException("Empty forecast response.");
        }
        catch (HttpRequestException ex)
        {
            throw new WeatherServiceException(
                $"Network error while fetching weather for '{city}': {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new WeatherServiceException(
                "Request timed out. Check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new WeatherServiceException("Failed to parse weather data.", ex);
        }

        var current = forecast.Current;
        var daily   = forecast.Daily;
        double high = daily.Temperature2mMax.Count > 0 ? Math.Round(daily.Temperature2mMax[0], 1) : current.Temperature2m;
        double low  = daily.Temperature2mMin.Count > 0 ? Math.Round(daily.Temperature2mMin[0], 1) : current.Temperature2m;

        return new WeatherResult
        {
            CityName           = cityLabel,
            CurrentTemperature = Math.Round(current.Temperature2m, 1),
            FeelsLike          = Math.Round(current.ApparentTemperature, 1),
            Description        = WmoDescription(current.WeatherCode),
            CurrentIcon        = WmoIcon(current.WeatherCode),
            Humidity           = current.RelativeHumidity2m,
            WindSpeed          = Math.Round(current.WindSpeed10m, 1),
            HighTemperature    = high,
            LowTemperature     = low,
            HourlyForecasts    = BuildHourlyForecasts(forecast),
            DailyForecasts     = BuildDailyForecasts(forecast)
        };
    }

    // ── Hourly ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a list of up to eight upcoming hourly weather forecasts from the provided forecast data.
    /// </summary>
    /// <param name="forecast">The forecast response containing hourly weather data to extract forecast information from. Cannot be null.</param>
    /// <returns>A list of up to eight <see cref="HourlyForecast"/> objects representing the next available hourly forecasts.</returns>
    private static List<HourlyForecast> BuildHourlyForecasts(OpenMeteoForecastResponse forecast)
    {
        var hourly = forecast.Hourly;
        var now    = DateTime.Now;
        var result = new List<HourlyForecast>();

        for (int i = 0; i < hourly.Time.Count && result.Count < 8; i++)
        {
            if (!DateTime.TryParse(hourly.Time[i], out var dt)) continue;
            if (dt < now) continue;  // skip past slots

            result.Add(new HourlyForecast
            {
                DateTime     = dt,
                TimeLabel    = dt.ToString("HH:mm"),
                Temperature  = Math.Round(hourly.Temperature2m[i], 1),
                Description  = WmoDescription(hourly.WeatherCode[i]),
                Icon         = WmoIcon(hourly.WeatherCode[i]),
                PrecipChance = i < hourly.PrecipitationProbability.Count
                               ? hourly.PrecipitationProbability[i] : 0,
                WeatherEmoji = WmoEmoji(hourly.WeatherCode[i])
            });
        }

        // Fallback: first 8 slots if all times were in the past
        if (result.Count == 0)
        {
            for (int i = 0; i < Math.Min(8, hourly.Time.Count); i++)
            {
                if (!DateTime.TryParse(hourly.Time[i], out var dt)) continue;
                result.Add(new HourlyForecast
                {
                    DateTime     = dt,
                    TimeLabel    = dt.ToString("HH:mm"),
                    Temperature  = Math.Round(hourly.Temperature2m[i], 1),
                    Description  = WmoDescription(hourly.WeatherCode[i]),
                    Icon         = WmoIcon(hourly.WeatherCode[i]),
                    PrecipChance = i < hourly.PrecipitationProbability.Count
                                   ? hourly.PrecipitationProbability[i] : 0,
                    WeatherEmoji = WmoEmoji(hourly.WeatherCode[i])
                });
            }
        }

        return result;
    }

    // ── Daily ────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Builds a list of daily weather forecasts from the specified OpenMeteo forecast response.
    /// </summary>
    /// <param name="forecast">The OpenMeteo forecast response containing daily weather data to be transformed into forecast objects. Cannot be null.</param>
    /// <returns>A list of <see cref="DailyForecast"/> objects representing the daily weather forecasts extracted from the input data. The list will be empty if no valid daily entries are found.</returns>
    private static List<DailyForecast> BuildDailyForecasts(OpenMeteoForecastResponse forecast)
    {
        var daily  = forecast.Daily;
        var today  = DateTime.Today;
        var result = new List<DailyForecast>();

        for (int i = 0; i < daily.Time.Count; i++)
        {
            if (!DateOnly.TryParse(daily.Time[i], out var date)) continue;
            var dt     = date.ToDateTime(TimeOnly.MinValue);
            int code   = i < daily.WeatherCode.Count        ? daily.WeatherCode[i]             : 0;
            double max = i < daily.Temperature2mMax.Count   ? Math.Round(daily.Temperature2mMax[i], 1) : 0;
            double min = i < daily.Temperature2mMin.Count   ? Math.Round(daily.Temperature2mMin[i], 1) : 0;
            double avg = Math.Round((max + min) / 2.0, 1);

            result.Add(new DailyForecast
            {
                Date           = dt,
                DayName        = dt.Date == today ? "Today" : dt.ToString("ddd"),
                DateLabel      = dt.ToString("MMM d"),
                AvgTemperature = avg,
                MaxTemperature = max,
                MinTemperature = min,
                Description    = WmoDescription(code),
                Icon           = WmoIcon(code),
                WeatherEmoji   = WmoEmoji(code)
            });
        }

        if (result.Count > 0)
            result.OrderByDescending(d => d.MaxTemperature).First().IsHottestDay = true;

        return result;
    }

    // ── WMO weather-code helpers ─────────────────────────────────────────────

    /// <summary>
    /// Returns a Unicode emoji representing the weather condition for the specified WMO weather code.
    /// </summary>
    /// <param name="code">The WMO weather code indicating the current weather condition.</param>
    /// <returns>A string containing a Unicode emoji that visually represents the specified weather code. Returns a default emoji if the code is not recognized.</returns>
    private static string WmoEmoji(int code) => code switch
    {
        0                                     => "☀️",
        1                                     => "🌤️",
        2                                     => "⛅",
        3                                     => "☁️",
        45 or 48                              => "🌫️",
        51 or 53 or 55 or 56 or 57            => "🌦️",
        61 or 63 or 65 or 66 or 67            => "🌧️",
        71 or 73 or 75 or 77 or 85 or 86      => "❄️",
        80 or 81 or 82                        => "🌧️",
        95 or 96 or 99                        => "⛈️",
        _                                     => "🌤️"
    };

    /// <summary>
    /// Returns a human-readable weather description corresponding to the specified WMO weather code.
    /// </summary>
    /// <param name="code">The WMO weather code to translate. Valid codes are defined by the World Meteorological Organization and represent specific weather conditions.</param>
    /// <returns>A string containing the weather description for the specified code. Returns "Unknown" if the code does not match a known WMO weather code.</returns>
    private static string WmoDescription(int code) => code switch
    {
        0        => "Clear sky",
        1        => "Mainly clear",
        2        => "Partly cloudy",
        3        => "Overcast",
        45       => "Foggy",
        48       => "Icy fog",
        51       => "Light drizzle",
        53       => "Moderate drizzle",
        55       => "Dense drizzle",
        56 or 57 => "Freezing drizzle",
        61       => "Slight rain",
        63       => "Moderate rain",
        65       => "Heavy rain",
        66 or 67 => "Freezing rain",
        71       => "Slight snow",
        73       => "Moderate snow",
        75       => "Heavy snow",
        77       => "Snow grains",
        80       => "Slight showers",
        81       => "Moderate showers",
        82       => "Violent showers",
        85 or 86 => "Snow showers",
        95       => "Thunderstorm",
        96 or 99 => "Thunderstorm with hail",
        _        => "Unknown"
    };

    /// <summary>
    /// Maps WMO codes to OWM-style icon strings so the existing
    /// icon→emoji switch in WeatherViewModel continues to work unchanged.
    /// </summary>
    /// <param name="code">The WMO weather code to map.</param>
    /// <returns>An OpenWeather-style icon code string (e.g. "01d").</returns>
    private static string WmoIcon(int code) => code switch
    {
        0                                => "01d",
        1                                => "02d",
        2                                => "03d",
        3                                => "04d",
        45 or 48                         => "50d",
        51 or 53 or 55 or 56 or 57       => "09d",
        61 or 63 or 65 or 66 or 67       => "10d",
        71 or 73 or 75 or 77 or 85 or 86 => "13d",
        80 or 81 or 82                   => "09d",
        95 or 96 or 99                   => "11d",
        _                                => "02d"
    };
}