namespace WeatherForecast;

/// <summary>
/// Represents the mobile dashboard page for displaying weather information, providing data binding and lifecycle management for the weather view model.
/// </summary>
public partial class WeatherDashboardPageMobile : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the WeatherDashboardPageMobile class with the specified view model.
    /// </summary>
    /// <param name="viewModel">The WeatherViewModel that provides data and commands for the dashboard page. Cannot be null.</param>
    public WeatherDashboardPageMobile(WeatherViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
   
    /// <summary>
    /// Method to handle the appearing of the page.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is WeatherViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;

            // Trigger the initial data load here, after the Android activity is
            // fully set up.  InitializeAsync is a no-op if data is already loaded.
            await vm.InitializeAsync();
        }
    }

    /// <summary>
    /// Method to handle the disappearing of the page.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is WeatherViewModel vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
    
    /// <summary>
    /// Method to handle the property changed event of the view model, used to trigger animations when certain properties change.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnViewModelPropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Soft fade-in when weather data arrives (IsLoading transitions to false)
        if (e.PropertyName is nameof(WeatherViewModel.IsLoading)
            && sender is WeatherViewModel vm
            && !vm.IsLoading)
        {
            _ = FadeInContentAsync();
        }
    }

    /// <summary>
    /// Method to perform a soft fade-in animation for the page content, used to enhance the user experience when new weather data is loaded and displayed.
    /// </summary>
    /// <returns>The task</returns>
    private async Task FadeInContentAsync()
    {
        this.Opacity = 0.7;
        await this.FadeToAsync(1.0, 350, Easing.CubicOut);
    }

    /// <summary>
    /// Method to notify the selection changing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SfAutocomplete_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        if (BindingContext is WeatherViewModel vm)
        {
            vm.SearchCommand.Execute(null);
        }
    }
}