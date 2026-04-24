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
                WaitingMinutes = q.WaitingMinutes ?? 0
            });
        }

        public async Task<QueueEntryDto> CheckInAsync(int appointmentId)
        {
            var nextNumber = await _uow.Queue.GetNextQueueNumberAsync();
            var entry = new QueueEntry
            {
                AppointmentId = appointmentId,
                QueueNumber = nextNumber,
                Status = QueueStatus.Waiting,
                CheckInTime = DateTime.Now
            };

            await _uow.Queue.AddAsync(entry);
            await _uow.SaveChangesAsync();

            // reload with navigation properties
            var saved = await _uow.Queue.GetByIdAsync(entry.Id);
            return new QueueEntryDto
            {
                Id = saved!.Id,
                QueueNumber = saved.QueueNumber,
                Status = saved.Status,
                CheckInTime = saved.CheckInTime,
                WaitingMinutes = saved.WaitingMinutes ?? 0
            };
        }

        public async Task UpdateStatusAsync(int queueEntryId, QueueStatus status)
        {
            var entry = await _uow.Queue.GetByIdAsync(queueEntryId);
            if (entry is null) return;

            entry.Status = status;

            if (status == QueueStatus.InProgress)
                entry.StartTime = DateTime.Now;
            else if (status == QueueStatus.Done)
                entry.EndTime = DateTime.Now;

            _uow.Queue.Update(entry);
            await _uow.SaveChangesAsync();
        }

        public async Task<DashboardDto> GetDashboardStatsAsync()
        {
            var todayQueue = (await _uow.Queue.GetTodayQueueAsync()).ToList();
            var todayAppointments = (await _uow.Appointments.GetByDateAsync(DateTime.Today)).ToList();
            var activeDoctors = (await _uow.Doctors.GetActiveAsync()).ToList();
            var openIssues = (await _uow.Issues.GetByStatusAsync(IssueStatus.Open)).ToList();

            // Exclude skipped (cancelled) queue entries from stats
            var activeQueue = todayQueue
                .Where(q => q.Status != QueueStatus.Skipped)
                .ToList();

            var completed = activeQueue
                .Where(q => q.Status == QueueStatus.Done && q.StartTime.HasValue)
                .ToList();

            return new DashboardDto
            {
                // Only count non-cancelled appointments
                TotalPatientsToday = todayAppointments
                    .Count(a => a.Status != AppointmentStatus.Cancelled),
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