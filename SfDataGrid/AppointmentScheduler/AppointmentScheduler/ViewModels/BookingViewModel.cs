using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using AppointmentScheduler.Helpers;
using AppointmentScheduler.Models;
using AppointmentScheduler.Services;

namespace AppointmentScheduler.ViewModels
{
    public class BookingViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _service;

        public ObservableCollection<string> ServiceTypes { get; } = new()
        {
            "Consultation",
            "Follow-up",
            "Therapy",
            "Haircut",
            "Dental Checkup",
            "Massage",
            "Physical Therapy",
            "Eye Exam",
            "Skin Treatment",
            "Nail Service"
        };

        public ObservableCollection<string> Statuses { get; } = new()
        {
            "Pending",
            "Scheduled",
            "Confirmed"
        };

        private string _clientName = string.Empty;
        public string ClientName
        {
            get => _clientName;
            set { _clientName = value; Raise(nameof(ClientName)); }
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; Raise(nameof(PhoneNumber)); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; Raise(nameof(Email)); }
        }

        private string _selectedServiceType = "Consultation";
        public string SelectedServiceType
        {
            get => _selectedServiceType;
            set { _selectedServiceType = value; Raise(nameof(SelectedServiceType)); }
        }

        private string _provider = string.Empty;
        public string Provider
        {
            get => _provider;
            set { _provider = value; Raise(nameof(Provider)); }
        }

        private DateTime _appointmentDate = DateTime.Today;
        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set { _appointmentDate = value; Raise(nameof(AppointmentDate)); }
        }

        private TimeSpan _appointmentTime = new TimeSpan(9, 0, 0);
        public TimeSpan AppointmentTime
        {
            get => _appointmentTime;
            set { _appointmentTime = value; Raise(nameof(AppointmentTime)); }
        }

        private TimeSpan _durationTime = new TimeSpan(1, 0, 0);
        public TimeSpan DurationTime
        {
            get => _durationTime;
            set { _durationTime = value; Raise(nameof(DurationTime)); }
        }

        private decimal _cost = 0;
        public decimal Cost
        {
            get => _cost;
            set { _cost = value; Raise(nameof(Cost)); }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set { _notes = value; Raise(nameof(Notes)); }
        }

        private string _selectedStatus = "Pending";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; Raise(nameof(SelectedStatus)); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; Raise(nameof(IsLoading)); }
        }

        public ICommand BookCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public BookingViewModel(AppointmentService service)
        {
            _service = service;
            BookCommand = new RelayCommand(async _ => await BookAsync());
            CancelCommand = new RelayCommand(async _ => await CancelAsync());
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ClientName))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.Windows.Count > 0)
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync("Validation Error", "Client name is required", "OK");
                });
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.Windows.Count > 0)
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync("Validation Error", "Phone number is required", "OK");
                });
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.Windows.Count > 0)
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync("Validation Error", "Email is required", "OK");
                });
                return false;
            }

            if (AppointmentDate < DateTime.Today)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.Windows.Count > 0)
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync("Validation Error", "Appointment date cannot be in the past", "OK");
                });
                return false;
            }

            return true;
        }

        private async Task BookAsync()
        {
            if (!ValidateInput()) return;

            try
            {
                IsLoading = true;

                var startTime = AppointmentDate.Add(AppointmentTime);
                var endTime = startTime.Add(DurationTime);

                var appointment = new Appointment
                {
                    ClientName = ClientName,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    ServiceType = SelectedServiceType,
                    Provider = Provider,
                    Start = startTime,
                    End = endTime,
                    Cost = Cost,
                    Notes = Notes,
                    Status = SelectedStatus
                };

                await _service.CreateAppointmentAsync(appointment);

                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync(
                        "Success",
                        "Appointment booked successfully!",
                        "OK");
                }

                ClearForm();
               // await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error booking appointment: {ex.Message}");
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", "Failed to book appointment", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearForm()
        {
            ClientName = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            SelectedServiceType = "Consultation";
            Provider = string.Empty;
            AppointmentDate = DateTime.Today;
            AppointmentTime = new TimeSpan(9, 0, 0);
            DurationTime = new TimeSpan(1, 0, 0);
            Cost = 0;
            Notes = string.Empty;
            SelectedStatus = "Pending";
        }

        private Task CancelAsync()
        {
            // Do not navigate — just clear the form fields
            ClearForm();
            return Task.CompletedTask;
        }
    }
}
