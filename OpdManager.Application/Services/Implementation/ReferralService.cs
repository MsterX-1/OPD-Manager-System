using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Implementations;

public class ReferralService : IReferralService
{
    private readonly IUnitOfWork _uow;
    public ReferralService(IUnitOfWork uow) => _uow = uow;

    public async Task<ReferralDto> CreateAsync(int sourceAppointmentId, int referringDoctorId,
        ReferralType type, int? targetDoctorId, int? targetDepartmentId,
        int? targetDepartmentServiceId, string? reason)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(sourceAppointmentId)
            ?? throw new InvalidOperationException("Source appointment not found.");

        var referral = new Referral
        {
            PatientId = appointment.PatientId,
            ReferringDoctorId = referringDoctorId,
            SourceAppointmentId = sourceAppointmentId,
            ReferralType = type,
            TargetDoctorId = targetDoctorId,
            TargetDepartmentId = targetDepartmentId,
            TargetDepartmentServiceId = targetDepartmentServiceId,
            Reason = reason,
            Status = ReferralStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Referrals.AddAsync(referral);
        await _uow.SaveChangesAsync();

        // If referring to a department, create a DepartmentQueueEntry
        if (type == ReferralType.ToDepartment && targetDepartmentId.HasValue)
        {
            var nextNumber = await _uow.DepartmentQueue.GetNextQueueNumberAsync(targetDepartmentId.Value);
            var queueEntry = new DepartmentQueueEntry
            {
                QueueNumber = nextNumber,
                Status = QueueStatus.Waiting,
                CheckInTime = DateTime.UtcNow,
                ReferralId = referral.Id,
                DepartmentId = targetDepartmentId.Value
            };

            await _uow.DepartmentQueue.AddAsync(queueEntry);
            await _uow.SaveChangesAsync();
        }

        return await GetByIdAsync(referral.Id) ?? throw new InvalidOperationException("Failed to create referral.");
    }

    public async Task<IEnumerable<ReferralDto>> GetByReferringDoctorAsync(int doctorId)
    {
        var referrals = await _uow.Referrals.GetByReferringDoctorAsync(doctorId);
        return referrals.Select(MapToDto);
    }

    public async Task<IEnumerable<ReferralDto>> GetByTargetDoctorAsync(int doctorId)
    {
        var referrals = await _uow.Referrals.GetByTargetDoctorAsync(doctorId);
        return referrals.Select(MapToDto);
    }

    public async Task<IEnumerable<ReferralDto>> GetByPatientAsync(int patientId)
    {
        var referrals = await _uow.Referrals.GetByPatientAsync(patientId);
        return referrals.Select(MapToDto);
    }

    public async Task<IEnumerable<DepartmentQueueEntryDto>> GetDepartmentQueueAsync(int departmentId)
    {
        var entries = await _uow.DepartmentQueue.GetByDepartmentAsync(departmentId);
        var result = new List<DepartmentQueueEntryDto>();

        foreach (var entry in entries)
        {
            var referral = await _uow.Referrals.GetWithDetailsAsync(entry.ReferralId);
            result.Add(new DepartmentQueueEntryDto
            {
                Id = entry.Id,
                QueueNumber = entry.QueueNumber,
                Status = entry.Status,
                CheckInTime = entry.CheckInTime,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                ReferralId = entry.ReferralId,
                DepartmentId = entry.DepartmentId,
                PatientName = referral?.Patient?.FullName ?? string.Empty,
                Reason = referral?.Reason,
                ServiceName = referral?.TargetDepartmentService?.Name,
                ReferringDoctorName = referral?.ReferringDoctor?.Name ?? string.Empty
            });
        }

        return result;
    }

    public async Task<ReferralDto?> UpdateStatusAsync(int referralId, ReferralStatus status)
    {
        var referral = await _uow.Referrals.GetByIdAsync(referralId);
        if (referral is null) return null;

        referral.Status = status;
        if (status == ReferralStatus.Completed)
            referral.CompletedAt = DateTime.UtcNow;

        _uow.Referrals.Update(referral);

        // Also update the department queue entry status if applicable
        if (referral.ReferralType == ReferralType.ToDepartment)
        {
            var queueEntries = await _uow.DepartmentQueue.GetWhereAsync(q => q.ReferralId == referralId);
            var queueEntry = queueEntries.FirstOrDefault();
            if (queueEntry is not null)
            {
                queueEntry.Status = status switch
                {
                    ReferralStatus.InProgress => QueueStatus.InProgress,
                    ReferralStatus.Completed => QueueStatus.Done,
                    ReferralStatus.Cancelled => QueueStatus.Skipped,
                    _ => queueEntry.Status
                };

                if (status == ReferralStatus.InProgress && queueEntry.StartTime is null)
                    queueEntry.StartTime = DateTime.UtcNow;
                if (status is ReferralStatus.Completed or ReferralStatus.Cancelled)
                    queueEntry.EndTime = DateTime.UtcNow;

                _uow.DepartmentQueue.Update(queueEntry);
            }
        }

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(referralId);
    }

    public async Task<ReferralDto?> AttachResultAsync(int referralId, string? resultNotes)
    {
        var referral = await _uow.Referrals.GetByIdAsync(referralId);
        if (referral is null) return null;

        referral.ResultNotes = resultNotes;
        referral.Status = ReferralStatus.Completed;
        referral.CompletedAt = DateTime.UtcNow;

        _uow.Referrals.Update(referral);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(referralId);
    }

    public async Task<ReferralDto?> GetByIdAsync(int id)
    {
        var referral = await _uow.Referrals.GetWithDetailsAsync(id);
        return referral is null ? null : MapToDto(referral);
    }

    private static ReferralDto MapToDto(Referral r) => new()
    {
        Id = r.Id,
        PatientId = r.PatientId,
        PatientName = r.Patient?.FullName ?? string.Empty,
        ReferringDoctorId = r.ReferringDoctorId,
        ReferringDoctorName = r.ReferringDoctor?.Name ?? string.Empty,
        SourceAppointmentId = r.SourceAppointmentId,
        ReferralType = r.ReferralType,
        TargetDoctorId = r.TargetDoctorId,
        TargetDoctorName = r.TargetDoctor?.Name,
        TargetDepartmentId = r.TargetDepartmentId,
        TargetDepartmentName = r.TargetDepartment?.Name,
        TargetDepartmentServiceId = r.TargetDepartmentServiceId,
        TargetServiceName = r.TargetDepartmentService?.Name,
        Reason = r.Reason,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        CompletedAt = r.CompletedAt,
        ResultNotes = r.ResultNotes,
        Attachments = r.Attachments?.Select(a => new AttachmentDto
        {
            Id = a.Id,
            MedicalRecordId = a.MedicalRecordId,
            ReferralId = a.ReferralId,
            FileName = a.FileName,
            FileType = a.FileType,
            FileSize = a.FileSize,
            Description = a.Description,
            UploadedByUserId = a.UploadedByUserId,
            UploadedAt = a.UploadedAt
        }).ToList() ?? new()
    };
}
