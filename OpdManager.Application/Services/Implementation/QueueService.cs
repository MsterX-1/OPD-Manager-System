using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Implementations;

public class QueueService : IQueueService
{
    private readonly IUnitOfWork _uow;
    public QueueService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<QueueEntryDto>> GetTodayQueueAsync()
    {
        var queue = await _uow.Queue.GetTodayQueueAsync();
        return queue.Select(MapToDto);
    }

    public async Task<QueueEntryDto> CheckInAsync(int appointmentId)
    {
        // Prevent double check-in
        var existing = await _uow.Queue.GetWhereAsync(
            q => q.AppointmentId == appointmentId);
        if (existing.Any())
            throw new InvalidOperationException("Patient is already checked in.");

        var appointment = await _uow.Appointments.GetByIdAsync(appointmentId);
        if (appointment == null) throw new InvalidOperationException("Appointment not found.");

        var nextNumber = await _uow.Queue.GetNextQueueNumberAsync(appointment.DoctorId);
        var entry = new QueueEntry
        {
            AppointmentId = appointmentId,
            QueueNumber = nextNumber,
            Status = QueueStatus.Waiting,
            CheckInTime = DateTime.UtcNow
        };

        await _uow.Queue.AddAsync(entry);

        // Appointment status stays Scheduled until called — correct behavior
        // CheckIn just means patient arrived, not that consultation started

        await _uow.SaveChangesAsync();

        var saved = await _uow.Queue.GetByIdAsync(entry.Id);
        return MapToDto(saved!);
    }

    public async Task UpdateStatusAsync(int queueEntryId, QueueStatus status)
    {
        var entry = await _uow.Queue.GetByIdAsync(queueEntryId);
        if (entry is null) return;

        entry.Status = status;

        // Update times
        if (status == QueueStatus.InProgress)
            entry.StartTime = DateTime.UtcNow;
        else if (status == QueueStatus.Done || status == QueueStatus.Skipped)
            entry.EndTime = DateTime.UtcNow;

        _uow.Queue.Update(entry);

        // Sync appointment status based on queue status
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

        // Today's doctor performance
        var performance = await GetDoctorPerformanceAsync(DateTime.Today, DateTime.Today);

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
            OpenIssues = openIssues.Count,
            DoctorPerformance = performance.ToList()
        };
    }

    public async Task<IEnumerable<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime from, DateTime to, int? doctorId = null)
    {
        var queueList = await _uow.Queue.GetPerformanceQueueAsync(from, to);

        // Group by doctor (via Appointment navigation)
        var grouped = queueList
            .Where(q => q.Appointment?.Doctor != null && (!doctorId.HasValue || q.Appointment.DoctorId == doctorId.Value))
            .GroupBy(q => new { q.Appointment.DoctorId, q.Appointment.Doctor.Name });

        return grouped.Select(g =>
        {
            var examined = g.Where(q => q.ExaminationMinutes.HasValue).ToList();
            var waited = g.Where(q => q.WaitingMinutes.HasValue).ToList();
            
            return new DoctorPerformanceDto
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.Name,
                PatientsExamined = g.Count(),
                TotalExaminationMinutes = examined.Sum(q => q.ExaminationMinutes!.Value),
                AverageExaminationMinutes = examined.Any()
                    ? examined.Average(q => q.ExaminationMinutes!.Value)
                    : null,
                TotalWaitingMinutes = waited.Sum(q => q.WaitingMinutes!.Value),
                AverageWaitingMinutes = waited.Any()
                    ? waited.Average(q => q.WaitingMinutes!.Value)
                    : null
            };
        });
    }

    public async Task<IEnumerable<QueueEntryDto>> GetDoctorPatientDetailsAsync(DateTime from, DateTime to, int doctorId)
    {
        var queueList = await _uow.Queue.GetPerformanceQueueAsync(from, to);
        var doctorQueue = queueList.Where(q => q.Appointment?.DoctorId == doctorId).ToList();
        return doctorQueue.Select(MapToDto);
    }

    private static QueueEntryDto MapToDto(QueueEntry q) => new()
    {
        Id = q.Id,
        QueueNumber = q.QueueNumber,
        DoctorId = q.Appointment?.DoctorId ?? 0,
        AppointmentId = q.AppointmentId,
        PatientName = q.Appointment?.Patient?.FullName ?? string.Empty,
        DoctorName = q.Appointment?.Doctor?.Name ?? string.Empty,
        DoctorSpecialty = q.Appointment?.Doctor?.Specialty ?? string.Empty,
        Status = q.Status,
        CheckInTime = q.CheckInTime,
        StartTime = q.StartTime,
        EndTime = q.EndTime,
        ExaminationMinutes = q.ExaminationMinutes,
        WaitingMinutes = q.StartTime.HasValue
            ? (int)(q.StartTime.Value - q.CheckInTime).TotalMinutes
            : (int)(DateTime.UtcNow - q.CheckInTime).TotalMinutes
    };
}