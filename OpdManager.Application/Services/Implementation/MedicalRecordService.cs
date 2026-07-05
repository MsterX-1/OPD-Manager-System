using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Implementations;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IUnitOfWork _uow;
    public MedicalRecordService(IUnitOfWork uow) => _uow = uow;

    public async Task<MedicalRecordDto> CreateAsync(int appointmentId, int doctorId)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(appointmentId)
            ?? throw new InvalidOperationException("Appointment not found.");

        // Check if record already exists
        var existing = await _uow.MedicalRecords.GetByAppointmentIdAsync(appointmentId);
        if (existing is not null)
            throw new InvalidOperationException("A medical record already exists for this appointment.");

        var record = new MedicalRecord
        {
            AppointmentId = appointmentId,
            DoctorId = doctorId,
            PatientId = appointment.PatientId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.MedicalRecords.AddAsync(record);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(record.Id) ?? throw new InvalidOperationException("Failed to create record.");
    }

    public async Task<MedicalRecordDto?> UpdateVitalsAsync(int id, int? bpSystolic, int? bpDiastolic,
        decimal? temperatureC, int? pulseRate, int? respiratoryRate,
        decimal? weightKg, decimal? heightCm, int? oxygenSaturation)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(id);
        if (record is null) return null;

        record.BloodPressureSystolic = bpSystolic;
        record.BloodPressureDiastolic = bpDiastolic;
        record.TemperatureC = temperatureC;
        record.PulseRate = pulseRate;
        record.RespiratoryRate = respiratoryRate;
        record.WeightKg = weightKg;
        record.HeightCm = heightCm;
        record.OxygenSaturation = oxygenSaturation;
        record.UpdatedAt = DateTime.UtcNow;

        _uow.MedicalRecords.Update(record);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<MedicalRecordDto?> UpdateDiagnosisAsync(int id, string? chiefComplaint,
        string? diagnosis, string? clinicalNotes)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(id);
        if (record is null) return null;

        record.ChiefComplaint = chiefComplaint;
        record.Diagnosis = diagnosis;
        record.ClinicalNotes = clinicalNotes;
        record.UpdatedAt = DateTime.UtcNow;

        _uow.MedicalRecords.Update(record);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<MedicalRecordDto?> GetByIdAsync(int id)
    {
        var record = await _uow.MedicalRecords.GetWithDetailsAsync(id);
        if (record is null) return null;
        return MapToDto(record);
    }

    public async Task<MedicalRecordDto?> GetByAppointmentIdAsync(int appointmentId)
    {
        var record = await _uow.MedicalRecords.GetByAppointmentIdAsync(appointmentId);
        if (record is null) return null;
        // Re-fetch with full details
        return await GetByIdAsync(record.Id);
    }

    public async Task<PatientChartDto> GetPatientChartAsync(int patientId)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId)
            ?? throw new InvalidOperationException("Patient not found.");

        var records = await _uow.MedicalRecords.GetByPatientIdAsync(patientId);
        var referrals = await _uow.Referrals.GetByPatientAsync(patientId);

        return new PatientChartDto
        {
            PatientId = patient.Id,
            PatientName = patient.FullName,
            KnownAllergies = patient.KnownAllergies,
            MedicalRecords = records.OrderByDescending(r => r.CreatedAt).Select(MapToDto).ToList(),
            Referrals = referrals.OrderByDescending(r => r.CreatedAt).Select(MapReferralToDto).ToList()
        };
    }

    // ── Prescriptions ───────────────────────────────────────────────────────

    public async Task<PrescriptionDto> AddPrescriptionAsync(int medicalRecordId, string medicationName,
        string? dosage, string? frequency, int? durationDays, string? instructions)
    {
        var prescription = new Prescription
        {
            MedicalRecordId = medicalRecordId,
            MedicationName = medicationName,
            Dosage = dosage,
            Frequency = frequency,
            DurationDays = durationDays,
            Instructions = instructions
        };

        // Use the base repository through the query
        var record = await _uow.MedicalRecords.GetWithDetailsAsync(medicalRecordId)
            ?? throw new InvalidOperationException("Medical record not found.");

        record.Prescriptions.Add(prescription);
        record.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        return new PrescriptionDto
        {
            Id = prescription.Id,
            MedicalRecordId = prescription.MedicalRecordId,
            MedicationName = prescription.MedicationName,
            Dosage = prescription.Dosage,
            Frequency = prescription.Frequency,
            DurationDays = prescription.DurationDays,
            Instructions = prescription.Instructions
        };
    }

    public async Task<PrescriptionDto?> UpdatePrescriptionAsync(int id, string medicationName,
        string? dosage, string? frequency, int? durationDays, string? instructions)
    {
        // Find the prescription through medical records
        var records = await _uow.MedicalRecords.GetAllAsync();
        Prescription? prescription = null;
        MedicalRecord? parentRecord = null;

        foreach (var record in records)
        {
            var fullRecord = await _uow.MedicalRecords.GetWithDetailsAsync(record.Id);
            if (fullRecord is null) continue;
            prescription = fullRecord.Prescriptions.FirstOrDefault(p => p.Id == id);
            if (prescription is not null)
            {
                parentRecord = fullRecord;
                break;
            }
        }

        if (prescription is null || parentRecord is null) return null;

        prescription.MedicationName = medicationName;
        prescription.Dosage = dosage;
        prescription.Frequency = frequency;
        prescription.DurationDays = durationDays;
        prescription.Instructions = instructions;
        parentRecord.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync();

        return new PrescriptionDto
        {
            Id = prescription.Id,
            MedicalRecordId = prescription.MedicalRecordId,
            MedicationName = prescription.MedicationName,
            Dosage = prescription.Dosage,
            Frequency = prescription.Frequency,
            DurationDays = prescription.DurationDays,
            Instructions = prescription.Instructions
        };
    }

    public async Task<bool> DeletePrescriptionAsync(int id)
    {
        var records = await _uow.MedicalRecords.GetAllAsync();
        foreach (var record in records)
        {
            var fullRecord = await _uow.MedicalRecords.GetWithDetailsAsync(record.Id);
            if (fullRecord is null) continue;
            var prescription = fullRecord.Prescriptions.FirstOrDefault(p => p.Id == id);
            if (prescription is not null)
            {
                fullRecord.Prescriptions.Remove(prescription);
                fullRecord.UpdatedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

    // ── Mapping ─────────────────────────────────────────────────────────────

    private static MedicalRecordDto MapToDto(MedicalRecord r) => new()
    {
        Id = r.Id,
        AppointmentId = r.AppointmentId,
        DoctorId = r.DoctorId,
        DoctorName = r.Doctor?.Name ?? string.Empty,
        PatientId = r.PatientId,
        PatientName = r.Patient?.FullName ?? string.Empty,
        BloodPressureSystolic = r.BloodPressureSystolic,
        BloodPressureDiastolic = r.BloodPressureDiastolic,
        TemperatureC = r.TemperatureC,
        PulseRate = r.PulseRate,
        RespiratoryRate = r.RespiratoryRate,
        WeightKg = r.WeightKg,
        HeightCm = r.HeightCm,
        OxygenSaturation = r.OxygenSaturation,
        ChiefComplaint = r.ChiefComplaint,
        Diagnosis = r.Diagnosis,
        ClinicalNotes = r.ClinicalNotes,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Prescriptions = r.Prescriptions?.Select(p => new PrescriptionDto
        {
            Id = p.Id,
            MedicalRecordId = p.MedicalRecordId,
            MedicationName = p.MedicationName,
            Dosage = p.Dosage,
            Frequency = p.Frequency,
            DurationDays = p.DurationDays,
            Instructions = p.Instructions
        }).ToList() ?? new(),
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

    private static ReferralDto MapReferralToDto(Referral r) => new()
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
