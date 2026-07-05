using OpdManager.Application.DTOs;

namespace OpdManager.Application.Services.Interfaces;

/// <summary>
/// Manages persistent clinical data (vitals, diagnoses, prescriptions).
/// Serves as the source of truth for patient encounter history.
/// </summary>
public interface IMedicalRecordService
{
    /// <summary>
    /// Create a new medical record for an appointment. Only the assigned doctor can create.
    /// </summary>
    Task<MedicalRecordDto> CreateAsync(int appointmentId, int doctorId);

    /// <summary>
    /// Update vitals — editable by Staff or the assigned Doctor.
    /// </summary>
    Task<MedicalRecordDto?> UpdateVitalsAsync(int id, int? bpSystolic, int? bpDiastolic,
        decimal? temperatureC, int? pulseRate, int? respiratoryRate,
        decimal? weightKg, decimal? heightCm, int? oxygenSaturation);

    /// <summary>
    /// Update clinical fields — Doctor-only, and only the assigned doctor.
    /// </summary>
    Task<MedicalRecordDto?> UpdateDiagnosisAsync(int id, string? chiefComplaint,
        string? diagnosis, string? clinicalNotes);

    Task<MedicalRecordDto?> GetByIdAsync(int id);
    Task<MedicalRecordDto?> GetByAppointmentIdAsync(int appointmentId);

    /// <summary>
    /// Full patient chart — all records + prescriptions + attachments, ordered chronologically.
    /// Any doctor can read any patient's chart (continuity of care).
    /// </summary>
    Task<PatientChartDto> GetPatientChartAsync(int patientId);

    // Prescriptions
    Task<PrescriptionDto> AddPrescriptionAsync(int medicalRecordId, string medicationName,
        string? dosage, string? frequency, int? durationDays, string? instructions);
    Task<PrescriptionDto?> UpdatePrescriptionAsync(int id, string medicationName,
        string? dosage, string? frequency, int? durationDays, string? instructions);
    Task<bool> DeletePrescriptionAsync(int id);
}
