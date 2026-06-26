using System;
using System.ComponentModel;

namespace AppointmentScheduler.Models
{
    public class Appointment : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Guid Id { get; set; } = Guid.NewGuid();

        private DateTime _start;
        public DateTime Start
        {
            get => _start;
            set { _start = value; Raise(nameof(Start)); Raise(nameof(TimeSlot)); }
        }

        private DateTime _end;
        public DateTime End
        {
            get => _end;
            set { _end = value; Raise(nameof(End)); Raise(nameof(TimeSlot)); }
        }

        private string _clientName = string.Empty;
        public string ClientName
        {
            get => _clientName;
            set { _clientName = value; Raise(nameof(ClientName)); }
        }

        private string _serviceType = string.Empty;
        public string ServiceType
        {
            get => _serviceType;
            set { _serviceType = value; Raise(nameof(ServiceType)); }
        }

        private string _status = "Scheduled";
        public string Status
        {
            get => _status;
            set { _status = value; Raise(nameof(Status)); }
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

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set { _notes = value; Raise(nameof(Notes)); }
        }

        private string _provider = string.Empty;
        public string Provider
        {
            get => _provider;
            set { _provider = value; Raise(nameof(Provider)); }
        }

        private decimal _cost = 0;
        public decimal Cost
        {
            get => _cost;
            set { _cost = value; Raise(nameof(Cost)); }
        }

        // Computed property for display
        public string TimeSlot => $"{Start:HH:mm} - {End:HH:mm}";
    }
}
