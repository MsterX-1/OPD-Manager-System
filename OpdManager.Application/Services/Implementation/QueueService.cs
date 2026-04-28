using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Implementations
{
    public class QueueService : IQueueService
    {
        private readonly IUnitOfWork _uow;
        public QueueService(IUnitOfWork uow) => _uow = uow;

        public async Task<IEnumerable<QueueEntryDto>> GetTodayQueueAsync()
        {
            var queue = await _uow.Queue.GetTodayQueueAsync();
            return queue.Select(q => new QueueEntryDto
            {
                Id = q.Id,
                QueueNumber = q.QueueNumber,
                PatientName = q.Appointment.Patient.FullName,
                DoctorName = q.Appointment.Doctor.Name,
                Status = q.Status,
                CheckInTime = q.CheckInTime,
                // Wait time = time from check-in until called (StartTime)
                // If not called yet = time waiting so far
                WaitingMinutes = q.StartTime.HasValue
                    ? (int)(q.StartTime.Value - q.CheckInTime).TotalMinutes
                    : (int)(DateTime.Now - q.CheckInTime).TotalMinutes
            });
        }

        public async Task<QueueEntryDto> CheckInAsync(int appointmentId)
        {
            // Prevent double check-in
            var existing = await _uow.Queue.GetWhereAsync(
                q => q.AppointmentId == appointmentId);
            if (existing.Any())
                throw new InvalidOperationException("Patient is already checked in.");

            var nextNumber = await _uow.Queue.GetNextQueueNumberAsync();
            var entry = new QueueEntry
            {
                AppointmentId = appointmentId,
                QueueNumber = nextNumber,
                Status = QueueStatus.Waiting,
                CheckInTime = DateTime.Now
            };

            await _uow.Queue.AddAsync(entry);

            // Appointment status stays Scheduled until called — correct behavior
            // CheckIn just means patient arrived, not that consultation started

            await _uow.SaveChangesAsync();

            var saved = await _uow.Queue.GetByIdAsync(entry.Id);
            return new QueueEntryDto
            {
                Id = saved!.Id,
                QueueNumber = saved.QueueNumber,
                Status = saved.Status,
                CheckInTime = saved.CheckInTime,
                WaitingMinutes = 0
            };
        }

        public async Task UpdateStatusAsync(int queueEntryId, QueueStatus status)
        {
            var entry = await _uow.Queue.GetByIdAsync(queueEntryId);
            if (entry is null) return;

            entry.Status = status;

            // Update times
            if (status == QueueStatus.InProgress)
                entry.StartTime = DateTime.Now;
            else if (status == QueueStatus.Done || status == QueueStatus.Skipped)
                entry.EndTime = DateTime.Now;

            _uow.Queue.Update(entry);

            // ← Sync appointment status based on queue status
            var appointment = await _uow.Appointments.GetByIdAsync(entry.AppointmentId);
            if (appointment != null)
            {
                appointment.Status = status switch
                {
                    QueueStatus.InProgress => AppointmentStatus.InProgress,
                    QueueStatus.Done => AppointmentStatus.Completed,
                    QueueStatus.Skipped => AppointmentStatus.NoShow,
                    _ => appointment.Status // Waiting → no change
                };
                _uow.Appointments.Update(appointment);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task<DashboardDto> GetDashboardStatsAsync()
        {
            var todayQueue = (await _uow.Queue.GetTodayQueueAsync()).ToList();
            var todayAppointments = (await _uow.Appointments.GetByDateAsync(DateTime.Today)).ToList();
            var activeDoctors = (await _uow.Doctors.GetActiveAsync()).ToList();
            var openIssues = (await _uow.Issues.GetByStatusAsync(IssueStatus.Open)).ToList();

            var activeQueue = todayQueue
                .Where(q => q.Status != QueueStatus.Skipped)
                .ToList();

            var completed = activeQueue
                .Where(q => q.Status == QueueStatus.Done && q.StartTime.HasValue)
                .ToList();

            return new DashboardDto
            {
                TotalPatientsToday = todayAppointments.Count(a => a.Status != AppointmentStatus.Cancelled),
                WaitingPatients = activeQueue.Count(q => q.Status == QueueStatus.Waiting),
                InProgressPatients = activeQueue.Count(q => q.Status == QueueStatus.InProgress),
                CompletedPatients = activeQueue.Count(q => q.Status == QueueStatus.Done),
                AverageWaitingMinutes = completed.Any()
                    ? completed.Average(q => (q.StartTime!.Value - q.CheckInTime).TotalMinutes)
                    : 0,
                ActiveDoctors = activeDoctors.Count,
                OpenIssues = openIssues.Count
            };
        }
    }
}