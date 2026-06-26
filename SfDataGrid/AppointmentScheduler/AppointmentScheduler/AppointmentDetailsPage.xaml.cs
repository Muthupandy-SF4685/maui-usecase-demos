using AppointmentScheduler.Services;
using AppointmentScheduler.ViewModels;

namespace AppointmentScheduler;

public partial class AppointmentDetailsPage : ContentPage
{
    public AppointmentDetailsPage(AppointmentService service)
    {
        InitializeComponent();
        BindingContext = new AppointmentDetailsViewModel(service);
    }
}
