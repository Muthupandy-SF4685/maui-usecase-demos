namespace AppointmentScheduler;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // If running on a phone or small tablet, replace the desktop root navigation
        // with a tabbed layout for a better mobile experience.
        try
        {
            var idiom = Microsoft.Maui.Devices.DeviceInfo.Idiom;
            if (idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone || idiom == Microsoft.Maui.Devices.DeviceIdiom.Tablet)
            {
                // Clear any existing items (desktop layout) and build a TabBar
                Items.Clear();

                var tabBar = new Microsoft.Maui.Controls.TabBar();

                var appointmentsTab = new Microsoft.Maui.Controls.Tab { Title = "Appointments" };
                appointmentsTab.Items.Add(new Microsoft.Maui.Controls.ShellContent
                {
                    ContentTemplate = new DataTemplate(typeof(MainPage)),
                    Route = "MainPage"
                });

                var bookingTab = new Microsoft.Maui.Controls.Tab { Title = "New Booking" };
                bookingTab.Items.Add(new Microsoft.Maui.Controls.ShellContent
                {
                    ContentTemplate = new DataTemplate(typeof(BookingPage)),
                    Route = "BookingPage"
                });

                var upcomingTab = new Microsoft.Maui.Controls.Tab { Title = "Upcoming" };
                upcomingTab.Items.Add(new Microsoft.Maui.Controls.ShellContent
                {
                    ContentTemplate = new DataTemplate(() =>
                    {
                        var root = new RootPage();
                        _ = root.ShowUpcoming();
                        return root;
                    }),
                    Route = "Upcoming"
                });

                tabBar.Items.Add(appointmentsTab);
                tabBar.Items.Add(bookingTab);
                tabBar.Items.Add(upcomingTab);

                Items.Add(tabBar);
            }
        }
        catch
        {
            // If platform detection fails, fall back to existing shell items
        }
    }
}
