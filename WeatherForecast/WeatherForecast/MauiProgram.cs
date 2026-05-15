using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using Syncfusion.Maui.Core.Hosting;

namespace WeatherForecast;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Enter the License");
        }
        catch (Exception)
        {
            // License validation failed – controls will show a trial watermark
            // but the app will still launch.  Replace the key above to silence this.
        }

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            // ── Register all Syncfusion MAUI control handlers ───────────────
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
            });

        // ── HTTP client for Open-Meteo (no API key required) ─────────────────
        builder.Services.AddHttpClient<WeatherService>(client =>
        {
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
                    "application/json"));
        });

        // ── Application services ──────────────────────────────────────────────
        builder.Services.AddTransient<WeatherViewModel>();
        builder.Services.AddTransient<WeatherDashboardPageDesktop>();
        builder.Services.AddTransient<WeatherDashboardPageMobile>();
        builder.Services.AddTransient<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
