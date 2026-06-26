using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AppointmentScheduler.Helpers;
using AppointmentScheduler.Models;
using AppointmentScheduler.Services;

namespace AppointmentScheduler.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _service;
        private List<Appointment> _all = new();

        public ObservableCollection<Appointment> Appointments { get; } = new();
        public ObservableCollection<Appointment> AllAppointments { get; } = new();
        public ObservableCollection<Appointment> UpcomingAppointments { get; } = new();

        private Appointment? _selectedAppointment;
        public Appointment? SelectedAppointment
        {
            get => _selectedAppointment;
            set { _selectedAppointment = value; Raise(nameof(SelectedAppointment)); }
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; Raise(nameof(SelectedDate)); FilterForSelectedDate(); }
        }

        private int _totalAppointmentsCount;
        public int TotalAppointmentsCount
        {
            get => _totalAppointmentsCount;
            set { _totalAppointmentsCount = value; Raise(nameof(TotalAppointmentsCount)); }
        }

        private int _confirmedCount;
        public int ConfirmedCount
        {
            get => _confirmedCount;
            set { _confirmedCount = value; Raise(nameof(ConfirmedCount)); }
        }

        private int _pendingCount;
        public int PendingCount
        {
            get => _pendingCount;
            set { _pendingCount = value; Raise(nameof(PendingCount)); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand TodayCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand RescheduleCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainViewModel(AppointmentService service)
        {
            _service = service;
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            TodayCommand = new RelayCommand(_ => SelectedDate = DateTime.Today);
            FilterCommand = new RelayCommand(_ => ApplyFilter());
            RescheduleCommand = new RelayCommand(async apptObj => await RescheduleAsync(apptObj as Appointment));
            DeleteCommand = new RelayCommand(async apptObj => await DeleteAsync(apptObj as Appointment));
            ViewDetailsCommand = new RelayCommand(async apptObj => await ViewDetailsAsync(apptObj as Appointment));

            _ = LoadAsync();
        }

        private void ApplyFilter()
        {
            AllAppointments.Clear();
            var filtered = _all.Where(a => a.Start.Date == SelectedDate.Date).OrderBy(a => a.Start);
            foreach (var a in filtered) AllAppointments.Add(a);
        }

        public async Task LoadAsync()
        {
            try
            {
                var items = await _service.GetAppointmentsAsync();
                _all = items;

                // Populate AllAppointments collection (for DataGrid full view)
                AllAppointments.Clear();
                foreach (var a in _all.OrderBy(a => a.Start))
                    AllAppointments.Add(a);

                UpdateStatistics();
                FilterForSelectedDate();
                await LoadUpcomingAppointments();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading appointments: {ex.Message}");
            }
        }

        private void FilterForSelectedDate()
        {
            Appointments.Clear();
            var filtered = _all.Where(a => a.Start.Date == SelectedDate.Date).OrderBy(a => a.Start);
            foreach (var a in filtered) Appointments.Add(a);
        }

        private async Task LoadUpcomingAppointments()
        {
            try
            {
                UpcomingAppointments.Clear();
                var upcoming = await _service.GetUpcomingAppointmentsAsync(7);
                foreach (var a in upcoming.Take(5))
                {
                    UpcomingAppointments.Add(a);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading upcoming appointments: {ex.Message}");
            }
        }

        private void UpdateStatistics()
        {
            TotalAppointmentsCount = _all.Count;
            ConfirmedCount = _all.Count(a => a.Status == "Confirmed");
            PendingCount = _all.Count(a => a.Status == "Pending");
        }

        private async Task RescheduleAsync(Appointment? appt)
        {
            if (appt == null) return;

            try
            {
                var time = appt.Start.TimeOfDay;
                appt.Start = SelectedDate.Date + time;
                appt.End = appt.Start.Add(appt.End.Subtract(appt.Start.Date));
                
                await _service.UpdateAppointmentAsync(appt);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error rescheduling appointment: {ex.Message}");
            }
        }

        public async Task RescheduleAppointmentAsync(Appointment? appt, DateTime newStart, DateTime newEnd)
        {
            if (appt == null) return;
            try
            {
                await _service.RescheduleAppointmentAsync(appt.Id, newStart, newEnd);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error rescheduling appointment: {ex.Message}");
            }
        }

        private async Task DeleteAsync(Appointment? appt)
        {
            if (appt == null) return;

            try
            {
                await _service.DeleteAppointmentAsync(appt);
                _all.RemoveAll(a => a.Id == appt.Id);
                UpdateStatistics();
                FilterForSelectedDate();
                await LoadUpcomingAppointments();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting appointment: {ex.Message}");
            }
        }

        private async Task ViewDetailsAsync(Appointment? appt)
        {
            if (appt != null)
            {
                await Shell.Current.GoToAsync($"details?id={appt.Id}");
            }
        }
    }
}
