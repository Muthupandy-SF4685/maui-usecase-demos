using AppointmentScheduler.Services;
using Syncfusion.Maui.Themes;
using System;
using System.Threading.Tasks;

namespace AppointmentScheduler;

public partial class RootPage : ContentPage
{
    readonly AppointmentService _service;

    public RootPage()
    {
        InitializeComponent();

        // On phones hide the left navigation panel and collapse its column
        try
        {
            var idiom = Microsoft.Maui.Devices.DeviceInfo.Idiom;
            if (idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone)
            {
                if (NavPanel != null)
                    NavPanel.IsVisible = false;

                if (RootGrid != null)
                {
                    RootGrid.ColumnDefinitions.Clear();
                    RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
                    RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
            }
        }
        catch
        {
            // ignore platform detection failures
        }

        // If DI is available, prefer injecting. For now create service if missing.
        _service = new AppointmentService();

        // Load appointments view by default
        ShowAppointments();
    }

    // Helper to resolve themed colors: prefer Light `Color.*` keys, fall back to `md_sys_*` dark keys.
    Color ResolveColor(string lightKey, string darkKey, Color fallback)
    {
        try
        {
            if (Application.Current?.Resources?.ContainsKey(lightKey) == true && Application.Current.Resources[lightKey] is Color c1)
                return c1;
            if (Application.Current?.Resources?.ContainsKey(darkKey) == true && Application.Current.Resources[darkKey] is Color c2)
                return c2;
        }
        catch
        {
            // ignore and return fallback
        }
        return fallback;
    }

    //void OnAppointmentsClicked(object sender, EventArgs e)
    //{
    //    ShowAppointments();
    //    BtnAppointments.BackgroundColor = ResolveColor("Color.Primary", "md_sys_color_primary", Color.FromArgb("#6750A4"));
    //    BtnAppointments.TextColor = ResolveColor("Color.OnPrimary", "md_sys_color_onprimary", Colors.White);

    //    BtnNewBooking.BackgroundColor = Colors.Transparent;
    //    BtnNewBooking.TextColor = ResolveColor("Color.OnBackground", "md_sys_color_onbackground", Colors.Black);

    //    // Reset Upcoming button style
    //    if (BtnUpcoming != null)
    //    {
    //        BtnUpcoming.BackgroundColor = Colors.Transparent;
    //        BtnUpcoming.TextColor = ResolveColor("Color.OnBackground", "md_sys_color_onbackground", Colors.Black);
    //    }
    //}

    void OnAppointmentsClicked(object sender, EventArgs e)
    {
        ShowAppointments();

        BtnAppointments.BackgroundColor = ResolveColor("Color.Primary", "md_sys_color_primary", Color.FromArgb("#6750A4"));
        BtnAppointments.TextColor = ResolveColor("Color.OnPrimary", "md_sys_color_onprimary", Colors.White);

        BtnNewBooking.BackgroundColor = Colors.Transparent;
        SetThemedTextColor(BtnNewBooking);   // ✅ FIX

        if (BtnUpcoming != null)
        {
            BtnUpcoming.BackgroundColor = Colors.Transparent;
            SetThemedTextColor(BtnUpcoming); // ✅ FIX
        }
    }


    void OnNewBookingClicked(object sender, EventArgs e)
    {
        ShowBooking();

        BtnNewBooking.BackgroundColor = ResolveColor("Color.Primary", "md_sys_color_primary", Color.FromArgb("#6750A4"));
        BtnNewBooking.TextColor = ResolveColor("Color.OnPrimary", "md_sys_color_onprimary", Colors.White);

        BtnAppointments.BackgroundColor = Colors.Transparent;
        SetThemedTextColor(BtnAppointments);  // ✅ FIX

        if (BtnUpcoming != null)
        {
            BtnUpcoming.BackgroundColor = Colors.Transparent;
            SetThemedTextColor(BtnUpcoming);  // ✅ FIX
        }
    }

    void ShowAppointments()
    {
        var page = new MainPage(_service);
        // Copy the page Content into ContentHost
        ContentHost.Content = page.Content;
        // Set BindingContext so bindings in injected content still work
        ContentHost.BindingContext = page.BindingContext;
    }

    void ShowBooking()
    {
        var page = new BookingPage(_service);
        ContentHost.Content = page.Content;
        ContentHost.BindingContext = page.BindingContext;
    }


    void SetThemedTextColor(Button button)
    {
        button.SetAppThemeColor(Button.TextColorProperty,
            Colors.Black,   // Light
            Colors.White);  // Dark ✅
    }


    async void OnUpcomingClicked(object sender, EventArgs e)
    {
        BtnUpcoming.BackgroundColor = ResolveColor("Color.Primary", "md_sys_color_primary", Color.FromArgb("#6750A4"));
        BtnUpcoming.TextColor = ResolveColor("Color.OnPrimary", "md_sys_color_onprimary", Colors.White);

        BtnAppointments.BackgroundColor = Colors.Transparent;
        SetThemedTextColor(BtnAppointments); // ✅ FIX

        BtnNewBooking.BackgroundColor = Colors.Transparent;
        SetThemedTextColor(BtnNewBooking);   // ✅ FIX

        await ShowUpcoming();
    }


    public async Task ShowUpcoming()
    {
        var upcoming = await _service.GetUpcomingAppointmentsAsync(7);

        var surfaceVariant = ResolveColor("Color.SurfaceVariant", "md_sys_color_surface_variant", Color.FromArgb("#EADDFF"));
        var primary = ResolveColor("Color.Primary", "md_sys_color_primary", Color.FromArgb("#6750A4"));
        var onPrimary = ResolveColor("Color.OnPrimary", "md_sys_color_onprimary", Colors.White);

        var collection = new CollectionView
        {
            ItemsSource = upcoming,
            SelectionMode = SelectionMode.Single,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 10
            },
            Margin = new Thickness(10, 0),

            ItemTemplate = new DataTemplate(() =>
            {
                // ✅ CARD
                var card = new Frame
                {
                    CornerRadius = 16,
                    Padding = 14,
                    Margin = new Thickness(0, 6),
                    BorderColor = surfaceVariant,
                    HasShadow = false
                };

                // ✅ Theme background
                card.SetAppThemeColor(VisualElement.BackgroundColorProperty,
                    Colors.White,                 // Light
                    Color.FromArgb("#5B566B"));   // Dark (matched your image)

                var grid = new Grid
                {
                    ColumnDefinitions =
                {
                    new ColumnDefinition{ Width = 70 },
                    new ColumnDefinition{ Width = GridLength.Star },
                    new ColumnDefinition{ Width = 110 }
                },
                    ColumnSpacing = 12
                };

                // ✅ DATE BADGE
                var dateFrame = new Frame
                {
                    CornerRadius = 12,
                    Padding = 8,
                    HasShadow = false,
                    VerticalOptions = LayoutOptions.Center
                };

                dateFrame.SetAppThemeColor(VisualElement.BackgroundColorProperty,
                    surfaceVariant,
                    Color.FromArgb("#6C6680")); // soft dark variant

                var dateStack = new VerticalStackLayout { Spacing = 2 };

                var day = new Label
                {
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center
                };

                day.SetAppThemeColor(Label.TextColorProperty,
                    primary,
                    Color.FromArgb("#E0DDF8"));

                day.SetBinding(Label.TextProperty, new Binding("Start", stringFormat: "{0:dd}"));

                var month = new Label
                {
                    FontSize = 11,
                    HorizontalTextAlignment = TextAlignment.Center
                };

                month.SetAppThemeColor(Label.TextColorProperty,
                    primary,
                    Color.FromArgb("#E0DDF8"));

                month.SetBinding(Label.TextProperty, new Binding("Start", stringFormat: "{0:MMM}"));

                dateStack.Add(day);
                dateStack.Add(month);
                dateFrame.Content = dateStack;

                // ✅ CONTENT
                var contentStack = new VerticalStackLayout { Spacing = 3 };

                var name = new Label
                {
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold
                };

                // ✅ WHITE in dark mode
                name.SetAppThemeColor(Label.TextColorProperty,
                    Colors.Black,
                    Colors.White);

                name.SetBinding(Label.TextProperty, "ClientName");

                var serviceLbl = new Label { FontSize = 13 };

                // ✅ Light gray in dark mode
                serviceLbl.SetAppThemeColor(Label.TextColorProperty,
                    Color.FromArgb("#6B6B6B"),
                    Color.FromArgb("#D1CFE2"));

                serviceLbl.SetBinding(Label.TextProperty, "ServiceType");

                var timeLbl = new Label { FontSize = 12 };

                // ✅ Purple accent
                timeLbl.SetAppThemeColor(Label.TextColorProperty,
                    primary,
                    Color.FromArgb("#B69DF6"));

                timeLbl.SetBinding(Label.TextProperty, "TimeSlot");

                contentStack.Add(name);
                contentStack.Add(serviceLbl);
                contentStack.Add(timeLbl);

                // ✅ STATUS CHIP
                var statusLabel = new Label
                {
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                statusLabel.SetBinding(Label.TextProperty, "Status");

                var statusChip = new Frame
                {
                    CornerRadius = 10,
                    Padding = new Thickness(14, 6),
                    HasShadow = false,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    MinimumHeightRequest = 34,
                    Content = statusLabel
                };

                // ✅ Status colors
                statusChip.BindingContextChanged += (s, e) =>
                {
                    if (statusChip.BindingContext is Models.Appointment appt)
                    {
                        statusChip.BackgroundColor = GetStatusColor(appt.Status, primary);
                    }
                };

                grid.Add(dateFrame, 0);
                grid.Add(contentStack, 1);
                grid.Add(statusChip, 2);

                card.Content = grid;

                // ✅ Tap action
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) =>
                {
                    if (card.BindingContext is Models.Appointment appt)
                    {
                        ShowDetails(appt.Id);
                    }
                };
                card.GestureRecognizers.Add(tap);

                return card;
            })
        };

        // ✅ Selection fallback
        collection.SelectionChanged += (s, e) =>
        {
            if (e.CurrentSelection?.Count > 0)
            {
                var appt = e.CurrentSelection[0] as Models.Appointment;
                if (appt != null)
                    ShowDetails(appt.Id);

                ((CollectionView)s).SelectedItem = null;
            }
        };

        // ✅ HEADER
        var title = new Label
        {
            Text = "Upcoming Appointments",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold
        };

        title.SetAppThemeColor(Label.TextColorProperty,
            Colors.Black,
            Colors.White);


        var subtitle = new Label
        {
            Text = "Next 7 Days",
            FontSize = 13
        };

        subtitle.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#6B6B6B"),
            Color.FromArgb("#D1CFE2"));

        var hostLayout = new VerticalStackLayout
        {
            Padding = 16,
            Spacing = 10
        };

        hostLayout.Add(title);
        hostLayout.Add(subtitle);
        hostLayout.Add(collection);

        ContentHost.Content = new ScrollView
        {
            Content = hostLayout
        };
    }

    Color GetStatusColor(string status, Color primary)
    {
        return status switch
        {
            "Pending" => Color.FromArgb("#F59E0B"),
            "Confirmed" => Color.FromArgb("#10B981"),
            "Scheduled" => primary,
            _ => primary
        };
    }

    void ShowDetails(Guid appointmentId)
    {
        var page = new AppointmentDetailsPage(_service);
        // page.BindingContext is AppointmentDetailsViewModel
        if (page.BindingContext is ViewModels.AppointmentDetailsViewModel vm)
        {
            vm.Id = appointmentId;

            // Subscribe so when details VM requests returning to list, we show upcoming content.
            vm.ReturnToListRequested += async () =>
            {
                await ShowUpcoming();
            };
        }

        ContentHost.Content = page.Content;
        ContentHost.BindingContext = page.BindingContext;
    }
}
