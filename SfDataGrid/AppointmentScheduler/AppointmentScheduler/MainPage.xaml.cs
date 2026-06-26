using AppointmentScheduler.Models;
using AppointmentScheduler.Services;
using AppointmentScheduler.ViewModels;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Themes;

namespace AppointmentScheduler;

public partial class MainPage : ContentPage
{
    private Appointment? _rescheduleAppointment;

    public MainPage(AppointmentService service)
    {
        InitializeComponent();
        BindingContext = new MainViewModel(service);
        // Initialize theme toggle button text based on Syncfusion theme (preferred), fallback to app theme
        try
        {
            if (ThemeToggleButton != null && Application.Current != null)
            {
                var mergedDictionaries = Application.Current.Resources?.MergedDictionaries;
                var sfTheme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
                if (sfTheme != null)
                {
                    // If current visual theme is dark, show sun (indicating pressing will switch to light)
                    ThemeToggleButton.Text = sfTheme.VisualTheme is SfVisuals.MaterialDark ? "☀️" : "🌙";
                }
                else
                {
                    var cur = Application.Current.UserAppTheme;
                    ThemeToggleButton.Text = cur == AppTheme.Dark ? "☀️" : "🌙";
                }
            }
        }
        catch
        {
            // ignore init errors
        }
    }

    void OnRescheduleRowClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Appointment appt)
        {
            _rescheduleAppointment = appt;
            // handle nullable or unexpected values defensively
            var start = appt.Start;
            // if Start were nullable in another version, fall back to today
            var safeStart = start == default ? DateTime.Today : start;
            PopupDatePicker.Date = safeStart.Date;
            PopupTimePicker.Time = safeStart.TimeOfDay;
            ReschedulePopup.IsVisible = true;
        }
    }

    async void OnPopupSaveClicked(object sender, EventArgs e)
    {
        if (_rescheduleAppointment == null) return;

        // compute new start/end from popup controls
        var date = PopupDatePicker.Date; // DateTime
        var time = PopupTimePicker.Time; // TimeSpan
        var newStart = date + time;
        var duration = _rescheduleAppointment.End - _rescheduleAppointment.Start;
        var newEnd = newStart + duration;

        if (BindingContext is ViewModels.MainViewModel vm)
        {
            await vm.RescheduleAppointmentAsync(_rescheduleAppointment, newStart.Value, newEnd.Value);
        }

        ReschedulePopup.IsVisible = false;
        _rescheduleAppointment = null;
    }

    void OnPopupCancelClicked(object sender, EventArgs e)
    {
        ReschedulePopup.IsVisible = false;
        _rescheduleAppointment = null;
    }

    private void ThemeToggleButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (Application.Current == null)
                return;

            var mergedDictionaries = Application.Current.Resources?.MergedDictionaries;
            var sfTheme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();

            if (sfTheme != null)
            {
                // Toggle Syncfusion visual theme and keep Application theme in sync
                if (sfTheme.VisualTheme is SfVisuals.MaterialDark)
                {
                    sfTheme.VisualTheme = SfVisuals.MaterialLight;
                    (Application.Current as App)?.SetAppTheme(AppTheme.Light);
                    if (ThemeToggleButton != null)
                        ThemeToggleButton.Text = "🌙"; // show moon to indicate pressing will go to Dark
                }
                else
                {
                    sfTheme.VisualTheme = SfVisuals.MaterialDark;
                    (Application.Current as App)?.SetAppTheme(AppTheme.Dark);
                    if (ThemeToggleButton != null)
                        ThemeToggleButton.Text = "☀️"; // show sun to indicate pressing will go to Light
                }
            }
            else
            {
                // No Syncfusion theme found — fallback to toggling the app theme
                var cur = Application.Current.UserAppTheme;
                if (cur == AppTheme.Dark)
                {
                    (Application.Current as App)?.SetAppTheme(AppTheme.Light);
                    if (ThemeToggleButton != null)
                        ThemeToggleButton.Text = "🌙";
                }
                else
                {
                    (Application.Current as App)?.SetAppTheme(AppTheme.Dark);
                    if (ThemeToggleButton != null)
                        ThemeToggleButton.Text = "☀️";
                }
            }
        }
        catch
        {
            // swallow any errors to avoid crashing the UI
        }
    }
}
