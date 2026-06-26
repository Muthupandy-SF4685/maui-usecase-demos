using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using AppointmentScheduler.Helpers;
using AppointmentScheduler.Models;
using AppointmentScheduler.Services;

namespace AppointmentScheduler.ViewModels
{
    [QueryProperty(nameof(Id), "id")]
    public class AppointmentDetailsViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _service;
        private Guid _id;

        private Appointment? _appointment;
        public Appointment? Appointment
        {
            get => _appointment;
            set { _appointment = value; Raise(nameof(Appointment)); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; Raise(nameof(IsLoading)); }
        }

        public Guid Id
        {
            get => _id;
            set { _id = value; LoadAppointment(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        // Raised when the viewmodel requests returning to the list/upcoming view
        public event Func<Task>? ReturnToListRequested;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public AppointmentDetailsViewModel(AppointmentService service)
        {
            _service = service;
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(async _ => await CancelAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync());
        }

        private async void LoadAppointment()
        {
            if (_id == Guid.Empty) return;

            try
            {
                IsLoading = true;
                Appointment = await _service.GetAppointmentByIdAsync(_id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading appointment: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (Appointment == null) return;

            try
            {
                IsLoading = true;
                await _service.UpdateAppointmentAsync(Appointment);

                // Show success feedback
                if (Application.Current?.Windows?.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Success", "Appointment saved successfully", "OK");
                }

                // If a host (like RootPage) subscribed, ask it to show upcoming content.
                if (ReturnToListRequested != null)
                {
                    await ReturnToListRequested.Invoke();
                    return;
                }

                // Fallback to platform/shell navigation
                if (Microsoft.Maui.Controls.Shell.Current != null)
                {
                    await Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
                    return;
                }

                if (Application.Current?.Windows?.Count > 0)
                {
                    var page = Application.Current.Windows[0].Page;
                    if (page is global::AppointmentScheduler.RootPage rootPage)
                    {
                        await rootPage.ShowUpcoming();
                        return;
                    }
                    if (page?.Navigation != null && page.Navigation.NavigationStack.Count > 0)
                    {
                        await page.Navigation.PopAsync();
                        return;
                    }
                }

                if (Application.Current?.MainPage is global::AppointmentScheduler.RootPage mainRoot)
                {
                    await mainRoot.ShowUpcoming();
                    return;
                }

                if (Application.Current?.MainPage?.Navigation != null && Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
                {
                    await Application.Current.MainPage.Navigation.PopAsync();
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving appointment: {ex.Message}");
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", "Failed to save appointment", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelAsync()
        {
            // If a host (like RootPage) subscribed, ask it to show upcoming content.
            if (ReturnToListRequested != null)
            {
                await ReturnToListRequested.Invoke();
                return;
            }

            // If the app uses the desktop RootPage (Content injection), call its ShowUpcoming
            if (Application.Current?.Windows?.Count > 0)
            {
                var page = Application.Current.Windows[0].Page;

                if (page is global::AppointmentScheduler.RootPage rootPage)
                {
                    await rootPage.ShowUpcoming();
                    return;
                }

                if (page?.Navigation != null && page.Navigation.NavigationStack.Count > 0)
                {
                    await page.Navigation.PopAsync();
                    return;
                }
            }
        }

        private async Task DeleteAsync()
        {
            if (Appointment == null) return;

            var result = false;
            if (Application.Current?.Windows.Count > 0)
            {
                result = await Application.Current.Windows[0].Page!.DisplayAlertAsync(
                    "Confirm Delete",
                    "Are you sure you want to delete this appointment?",
                    "Yes", "No");
            }

            if (!result) return;

            try
            {
                IsLoading = true;
                await _service.DeleteAppointmentAsync(Appointment);
                // Show success feedback
                if (Application.Current?.Windows?.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Success", "Appointment deleted", "OK");
                }
                // If a host (like RootPage) subscribed, ask it to show upcoming content first
                if (ReturnToListRequested != null)
                {
                    await ReturnToListRequested.Invoke();
                    return;
                }

                // Prefer RootPage.ShowUpcoming when running desktop/content-host flow
                if (Application.Current?.Windows?.Count > 0)
                {
                    var page = Application.Current.Windows[0].Page;
                    if (page is global::AppointmentScheduler.RootPage rootPage)
                    {
                        await rootPage.ShowUpcoming();
                        return;
                    }
                }

                if (Application.Current?.MainPage is global::AppointmentScheduler.RootPage mainRoot)
                {
                    await mainRoot.ShowUpcoming();
                    return;
                }

                // Fall back to Shell or navigation stack
                if (Microsoft.Maui.Controls.Shell.Current != null)
                {
                    await Microsoft.Maui.Controls.Shell.Current.GoToAsync("..");
                }
                else if (Application.Current?.Windows?.Count > 0)
                {
                    var page = Application.Current.Windows[0].Page;
                    if (page?.Navigation != null && page.Navigation.NavigationStack.Count > 0)
                    {
                        await page.Navigation.PopAsync();
                    }
                }
                else if (Application.Current?.MainPage?.Navigation != null && Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
                {
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting appointment: {ex.Message}");
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", "Failed to delete appointment", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
