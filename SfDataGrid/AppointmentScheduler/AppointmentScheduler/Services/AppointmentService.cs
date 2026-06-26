using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentScheduler.Models;

namespace AppointmentScheduler.Services
{
    public class AppointmentService
    {
        private List<Appointment> _appointments = new();

        public AppointmentService()
        {
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            var now = DateTime.Now.Date;

            var names = new[] {
                "Alice Johnson","Bob Smith","Carol Lee","Daniel Kim","Eva Green",
                "Frank Martinez","Grace Hall","Hank Miller","Ivy Chen","Jack Turner",
                "Kara Patel","Liam Brown","Maya Singh","Noah Davis","Olivia Clark",
                "Paul Young","Quinn Rivera","Rita Gomez","Sam Walker","Tina Brooks"
            };

            var services = new[] { "Consultation", "Follow-up", "Therapy", "Haircut", "Dental Checkup", "Massage", "Vaccination", "Physical", "Grooming", "Styling" };
            var providers = new[] { "Dr. Smith", "Dr. Johnson", "Dr. Williams", "Maria Salon", "Dr. Brown", "Wellness Center", "Health Clinic", "Clinic A", "Salon B", "Therapy Center" };
            var statuses = new[] { "Scheduled", "Confirmed", "Pending" };

            _appointments = new List<Appointment>();

            for (int i = 0; i < 20; i++)
            {
                var dayOffset = i % 7; // spread across a week
                var hour = 8 + (i % 9); // slots between 8 and 16
                var start = now.AddDays(dayOffset).AddHours(hour);
                var end = start.AddHours(1);

                var client = names[i % names.Length];
                var service = services[i % services.Length];
                var provider = providers[i % providers.Length];
                var status = statuses[i % statuses.Length];

                var appt = new Appointment
                {
                    Start = start,
                    End = end,
                    ClientName = client,
                    ServiceType = service,
                    Status = status,
                    Provider = provider,
                    PhoneNumber = $"555-{1000 + i:D4}",
                    Email = client.ToLower().Replace(' ', '.') + "@example.com",
                    Notes = i % 3 == 0 ? "Bring previous records" : (i % 3 == 1 ? "Allergies: none" : "Prefers morning"),
                    Cost = 50 + (i % 5) * 25
                };

                _appointments.Add(appt);
            }
        }

        public Task<List<Appointment>> GetAppointmentsAsync()
        {
            return Task.FromResult(_appointments.OrderBy(a => a.Start).ToList());
        }

        public Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            var filtered = _appointments.Where(a => a.Start.Date == date.Date).OrderBy(a => a.Start).ToList();
            return Task.FromResult(filtered);
        }

        public Task<List<Appointment>> GetUpcomingAppointmentsAsync(int daysAhead = 7)
        {
            var now = DateTime.Now;
            var futureDate = now.AddDays(daysAhead);
            var filtered = _appointments.Where(a => a.Start >= now && a.Start <= futureDate).OrderBy(a => a.Start).ToList();
            return Task.FromResult(filtered);
        }

        public Task<Appointment?> GetAppointmentByIdAsync(Guid id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            return Task.FromResult(appointment);
        }

        public Task<bool> CreateAppointmentAsync(Appointment appointment)
        {
            if (appointment.Id == Guid.Empty)
                appointment.Id = Guid.NewGuid();
            
            _appointments.Add(appointment);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAppointmentAsync(Appointment appointment)
        {
            var existing = _appointments.FirstOrDefault(a => a.Id == appointment.Id);
            if (existing != null)
            {
                existing.Start = appointment.Start;
                existing.End = appointment.End;
                existing.ClientName = appointment.ClientName;
                existing.ServiceType = appointment.ServiceType;
                existing.Status = appointment.Status;
                existing.Provider = appointment.Provider;
                existing.PhoneNumber = appointment.PhoneNumber;
                existing.Email = appointment.Email;
                existing.Notes = appointment.Notes;
                existing.Cost = appointment.Cost;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteAppointmentAsync(Appointment appointment)
        {
            return Task.FromResult(_appointments.Remove(appointment));
        }

        public Task<bool> RescheduleAppointmentAsync(Guid id, DateTime newStart, DateTime newEnd)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            if (appointment != null)
            {
                appointment.Start = newStart;
                appointment.End = newEnd;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
