namespace WeatherForecast;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Dashboard",
            Route = "WeatherDashboard",
            ContentTemplate = new DataTemplate(
#if WINDOWS || MACCATALYST
                () => services.GetRequiredService<WeatherDashboardPageDesktop>())
#else
                () => services.GetRequiredService<WeatherDashboardPageMobile>())
#endif
        });
    }
}
