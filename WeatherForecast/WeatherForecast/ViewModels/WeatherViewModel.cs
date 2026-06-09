using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WeatherForecast;

/// <summary>
/// ViewModel for WeatherDashboardPage.
/// Implements INotifyPropertyChanged manually for full transparency.
/// </summary>
public class WeatherViewModel : INotifyPropertyChanged
{
    private string _selectedCity = "London";
    private List<string> _citySuggestions = [];
    private bool _isLoading;
    private bool _isRefreshing;
    private string _errorMessage = string.Empty;
    private string _cityName = "—";
    private double _currentTemperature;
    private double _feelsLike;
    private string _description = string.Empty;
    private string _currentWeatherEmoji = "🌤️";
    private double _highTemperature;
    private double _lowTemperature;
    private double _humidity;
    private double _windSpeed;
    private ObservableCollection<HourlyForecast> _hourlyForecasts = [];
    private ObservableCollection<DailyForecast> _dailyForecasts = [];
    private readonly WeatherService _weatherService;

    /// <summary>
    /// Initializes a new instance of the WeatherViewModel class with the specified weather service dependency.
    /// </summary>
    /// <param name="weatherService">The WeatherService instance used to retrieve weather data for the view model.</param>
    public WeatherViewModel(WeatherService weatherService)
    {
        _weatherService = weatherService;

        SearchCommand = new Command(async () => await FetchWeatherAsync(), CanSearch);
        LoadWeatherCommand = new Command<string>(async city => await FetchWeatherAsync(city));
        PullToRefreshCommand = new Command(async () =>
        {
            // Ensure the control shows refreshing state on the main thread
            await RunOnMainThread(() => IsRefreshing = true);
            try
            {
                // When invoked via pull-to-refresh, avoid showing the full-page
                // IsLoading overlay so the native pull indicator is the only one visible.
                await FetchWeatherAsync(showOverlay: false);
            }
            catch (Exception ex)
            {
                // Swallow and record any exception to avoid leaving the UI stuck.
                await RunOnMainThread(() => ErrorMessage = $"Refresh failed: {ex.Message}");
            }
            finally
            {
                // Ensure the pull-to-refresh control is always cleared even if
                // FetchWeatherAsync fails or throws.
                await RunOnMainThread(() => IsRefreshing = false);
            }
        });

        // Seed popular city suggestions
        CitySuggestions = new List<string>
        {
            "London", "New York", "Tokyo", "Sydney", "Paris",
            "Dubai", "Singapore", "Toronto", "Berlin", "Mumbai",
            "Los Angeles", "Chicago", "Seoul", "Amsterdam", "Madrid"
        };

        // Note: the initial weather load is intentionally NOT started here.
        // Kicking off a network call via Task.Run inside a constructor is
        // unsafe on Android – the MAUI activity may not be fully initialized,
        // which can deadlock the main-thread dispatcher and freeze the splash
        // screen.  Call InitializeAsync() from the page's OnAppearing instead.
    }

    /// <summary>
    /// Command to execute a search for weather data based on the currently selected city. The command is enabled only when a valid city is selected and a search is not already in progress.
    /// </summary>
    public ICommand SearchCommand { get; }

    /// <summary>
    /// Command to load weather data for a specified city. This command can be executed with a city name parameter to fetch weather data for that city, and is typically used for loading weather data when a city is selected from suggestions or other UI elements.
    /// </summary>
    public ICommand LoadWeatherCommand { get; }

    /// <summary>
    /// Command to refresh the current weather data, typically triggered by a pull-to-refresh gesture in the UI. This command re-fetches the weather data for the currently selected city and updates the view model properties accordingly. The IsRefreshing property is set to false after the refresh operation completes to signal the end of the refresh state in the UI.
    /// </summary>
    public ICommand PullToRefreshCommand { get; }

    /// <summary>
    /// Gets or sets the name of the currently selected city.
    /// </summary>
    public string SelectedCity
    {
        get => _selectedCity;
        set
        {
            if (SetProperty(ref _selectedCity, value))
                ((Command)SearchCommand).ChangeCanExecute();
        }
    }

    /// <summary>
    /// Gets or sets the list of suggested city names based on the current input or search criteria.
    /// </summary>
    public List<string> CitySuggestions
    {
        get => _citySuggestions;
        set => SetProperty(ref _citySuggestions, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a loading operation is in progress.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
                OnPropertyChanged(nameof(IsContentVisible));
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a refresh operation is currently in progress.
    /// </summary>
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    /// <summary>
    /// Gets or sets the error message to display when a weather data fetch operation fails. An empty or null value indicates no error.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    /// Gets a value indicating whether an error condition is present.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Gets a value indicating whether the content is currently visible to the user.
    /// </summary>
    public bool IsContentVisible => !IsLoading && !HasError;

    /// <summary>
    /// Determines whether a search operation can be initiated based on the current state.
    /// </summary>
    /// <returns>true if a city is selected and a search is not currently in progress; otherwise, false.</returns>
    private bool CanSearch() => !string.IsNullOrWhiteSpace(SelectedCity) && !IsLoading;

    /// <summary>
    /// Gets or sets the name of the city associated with this instance.
    /// </summary>
    public string CityName
    {
        get => _cityName;
        set => SetProperty(ref _cityName, value);
    }

    /// <summary>
    /// Gets or sets the current temperature value.
    /// </summary>
    public double CurrentTemperature
    {
        get => _currentTemperature;
        set
        {
            if (SetProperty(ref _currentTemperature, value))
                OnPropertyChanged(nameof(CurrentTempDisplay));
        }
    }

    /// <summary>
    /// Gets the current temperature formatted as a whole number followed by the degree symbol.
    /// </summary>
    public string CurrentTempDisplay => $"{CurrentTemperature:F0}°";

    /// <summary>
    /// Gets or sets the apparent temperature, representing how hot or cold it feels to a person.
    /// </summary>
    public double FeelsLike
    {
        get => _feelsLike;
        set
        {
            if (SetProperty(ref _feelsLike, value))
                OnPropertyChanged(nameof(FeelsLikeDisplay));
        }
    }

    /// <summary>
    /// Gets a formatted string representing the current 'feels like' temperature in degrees Celsius.
    /// </summary>
    public string FeelsLikeDisplay => $"Feels like {FeelsLike:F0}°C";

    /// <summary>
    /// Gets or sets the description associated with the current object.
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// Gets or sets the emoji representing the current weather condition.
    /// </summary>
    public string CurrentWeatherEmoji
    {
        get => _currentWeatherEmoji;
        set => SetProperty(ref _currentWeatherEmoji, value);
    }

    /// <summary>
    /// Gets or sets the recorded high temperature value.
    /// </summary>
    public double HighTemperature
    {
        get => _highTemperature;
        set
        {
            if (SetProperty(ref _highTemperature, value))
                OnPropertyChanged(nameof(HighTempDisplay));
        }
    }

    /// <summary>
    /// Gets the formatted string representation of the high temperature for display purposes.
    /// </summary>
    public string HighTempDisplay => $"H: {HighTemperature:F0}°";

    /// <summary>
    /// Gets or sets the lowest recorded temperature value.
    /// </summary>
    public double LowTemperature
    {
        get => _lowTemperature;
        set
        {
            if (SetProperty(ref _lowTemperature, value))
            {
                OnPropertyChanged(nameof(LowTempDisplay));
                OnPropertyChanged(nameof(TempProgressValue));
            }
        }
    }

    /// <summary>
    /// Gets the formatted display string for the low temperature value.
    /// </summary>
    public string LowTempDisplay => $"L: {LowTemperature:F0}°";

    /// <summary>
    /// Gets the current progress as a percentage based on the current temperature within the configured temperature range.
    /// </summary>
    public double TempProgressValue
    {
        get
        {
            double range = HighTemperature - LowTemperature;
            if (range <= 0) return 50;
            double progress = (CurrentTemperature - LowTemperature) / range * 100.0;
            return Math.Clamp(progress, 0, 100);
        }
    }

    /// <summary>
    /// Gets or sets the current relative humidity value.
    /// </summary>
    public double Humidity
    {
        get => _humidity;
        set
        {
            if (SetProperty(ref _humidity, value))
                OnPropertyChanged(nameof(HumidityDisplay));
        }
    }

    /// <summary>
    /// Gets the humidity value formatted as a percentage string with no decimal places.
    /// </summary>
    public string HumidityDisplay => $"{Humidity:F0}%";

    /// <summary>
    /// Gets or sets the wind speed value.
    /// </summary>
    public double WindSpeed
    {
        get => _windSpeed;
        set
        {
            if (SetProperty(ref _windSpeed, value))
                OnPropertyChanged(nameof(WindSpeedDisplay));
        }
    }

    /// <summary>
    /// Gets the wind speed formatted as a string with one decimal place, followed by the unit of measurement.
    /// </summary>
    public string WindSpeedDisplay => $"{WindSpeed:F1} m/s";

    /// <summary>
    /// Gets or sets the collection of hourly weather forecasts for the current location or time period.
    /// </summary>
    public ObservableCollection<HourlyForecast> HourlyForecasts
    {
        get => _hourlyForecasts;
        set => SetProperty(ref _hourlyForecasts, value);
    }

    /// <summary>
    /// Gets or sets the collection of daily weather forecasts for the current location or time period.
    /// </summary>
    public ObservableCollection<DailyForecast> DailyForecasts
    {
        get => _dailyForecasts;
        set => SetProperty(ref _dailyForecasts, value);
    }

    /// <summary>
    /// Call once from the page's OnAppearing to load the default city.
    /// Safe to call again on subsequent appearances – the guard on IsLoading
    /// prevents redundant fetches while data is already present.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Only load once – skip if we already have data or a fetch is in progress.
        if (IsLoading || CityName != "—") return;
        await FetchWeatherAsync(SelectedCity);
    }

    /// <summary>
    /// Method to fetch weather data for a given city (or the currently selected city if null).
    /// </summary>
    /// <param name="city">The city.</param>
    /// <returns></returns>
    /// <param name="showOverlay">If true (default), the page-level loading overlay is shown via `IsLoading`.
    /// If false, only local UI indicators (e.g., pull-to-refresh) should be used.</param>
    public async Task FetchWeatherAsync(string? city = null, bool showOverlay = true)
    {
        var targetCity = city ?? SelectedCity;
        if (string.IsNullOrWhiteSpace(targetCity)) return;

        // Only set the global IsLoading overlay when requested and when
        // a pull-to-refresh isn't already active.
        if (showOverlay && !IsRefreshing)
        {
            await RunOnMainThread(() =>
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
            });
        }
        else
        {
            // If overlay is suppressed, at least clear any previous error message
            await RunOnMainThread(() => ErrorMessage = string.Empty);
        }

        try
        {
            var result = await _weatherService.GetForecastAsync(targetCity);
            await ApplyResultAsync(result);
        }
        catch (WeatherServiceException ex)
        {
            await RunOnMainThread(() => ErrorMessage = ex.Message);
        }
        catch (Exception ex)
        {
            await RunOnMainThread(() =>
                ErrorMessage = $"Unexpected error: {ex.Message}");
        }
        finally
        {
            // Only clear the global overlay if we set it earlier.
            if (showOverlay && !IsRefreshing)
                await RunOnMainThread(() => IsLoading = false);
        }
    }

    /// <summary>
    /// Applies the specified weather result to update the current weather and forecast properties asynchronously on the main thread.
    /// </summary>
    /// <param name="result">The weather data to apply to the current view model. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ApplyResultAsync(WeatherResult result)
    {
        await RunOnMainThread(() =>
        {
            CityName = result.CityName;
            CurrentTemperature = Math.Round(result.CurrentTemperature, 1);
            FeelsLike = Math.Round(result.FeelsLike, 1);
            Description = result.Description;
            HighTemperature = result.HighTemperature;
            LowTemperature = result.LowTemperature;
            Humidity = result.Humidity;
            WindSpeed = result.WindSpeed;

            // Map icon to emoji for the current condition
            CurrentWeatherEmoji = GetEmojiForIcon(result.CurrentIcon);

            // Refresh collections
            HourlyForecasts = new ObservableCollection<HourlyForecast>(result.HourlyForecasts);
            DailyForecasts = new ObservableCollection<DailyForecast>(result.DailyForecasts);

            // Notify derived properties that depend on multiple fields
            OnPropertyChanged(nameof(TempProgressValue));
            OnPropertyChanged(nameof(CurrentTempDisplay));
            OnPropertyChanged(nameof(FeelsLikeDisplay));
            OnPropertyChanged(nameof(HighTempDisplay));
            OnPropertyChanged(nameof(LowTempDisplay));
        });
    }

    /// <summary>
    /// Returns the corresponding emoji for a given weather icon code.
    /// </summary>
    /// <param name="iconCode">The weather icon code to convert to an emoji.</param>
    /// <returns>A string containing the emoji that represents the specified weather icon code. Returns a default emoji if the code is unrecognized.</returns>
    private static string GetEmojiForIcon(string iconCode) => iconCode switch
    {
        "01d" or "01n" => "☀️",
        "02d" or "02n" => "🌤️",
        "03d" or "03n" => "⛅",
        "04d" or "04n" => "☁️",
        "09d" or "09n" => "🌧️",
        "10d" or "10n" => "🌦️",
        "11d" or "11n" => "⛈️",
        "13d" or "13n" => "❄️",
        "50d" or "50n" => "🌫️",
        _ => "🌤️"
    };

    /// <summary>
    /// Executes the specified action on the main thread asynchronously.
    /// </summary>
    /// <param name="action">The action to execute on the main thread. Cannot be null.</param>
    /// <returns>A task that represents the completion of the action on the main thread.</returns>
    private static Task RunOnMainThread(Action action)
    {
        if (MainThread.IsMainThread)
        {
            action();
            return Task.CompletedTask;
        }
        return MainThread.InvokeOnMainThreadAsync(action);
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event to notify listeners that a property value has changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. This value is optional and is automatically provided when called from a property setter.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Sets the specified backing field to the given value and raises a property changed notification if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property being set.</typeparam>
    /// <param name="backingStore">A reference to the field that stores the property's current value. This value will be updated if it differs from
    /// <paramref name="value"/>.</param>
    /// <param name="value">The new value to assign to the property.</param>
    /// <param name="propertyName">The name of the property. This is automatically provided by the compiler and is used to raise the property changed notification. Can be null.</param>
    /// <returns>true if the value was changed and the property changed notification was raised; otherwise, false.</returns>
    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}