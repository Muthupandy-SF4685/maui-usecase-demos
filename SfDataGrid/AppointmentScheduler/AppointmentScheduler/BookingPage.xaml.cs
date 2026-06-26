using AppointmentScheduler.Services;
using AppointmentScheduler.ViewModels;

namespace AppointmentScheduler;

public partial class BookingPage : ContentPage
{
    public BookingPage(AppointmentService service)
    {
        InitializeComponent();
        BindingContext = new BookingViewModel(service);
        // Ensure page background follows theme resources at runtime (Light `Color.Background`, fallback to dark `md_sys_color_background`).
        try
        {
            object bg = null;
            if (Application.Current.Resources.ContainsKey("Color.Background"))
                bg = Application.Current.Resources["Color.Background"];
            else if (Application.Current.Resources.ContainsKey("md_sys_color_background"))
                bg = Application.Current.Resources["md_sys_color_background"];

            if (bg is Microsoft.Maui.Graphics.Color c)
                this.BackgroundColor = c;
        }
        catch
        {
            // ignore resource lookup failures
        }
    }
}
