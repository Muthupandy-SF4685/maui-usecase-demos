using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using AppointmentScheduler.Services;
using AppointmentScheduler;

namespace AppointmentScheduler
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .RegisterPages()
                .RegisterServices();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static MauiAppBuilder RegisterPages(this MauiAppBuilder builder)
        {
            builder.Services.AddTransient<AppointmentDetailsPage>();
            builder.Services.AddTransient<BookingPage>();

            return builder;
        }

        public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<AppointmentService>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppointmentDetailsPage>();
            builder.Services.AddSingleton<BookingPage>();

            return builder;
        }
    }
}
