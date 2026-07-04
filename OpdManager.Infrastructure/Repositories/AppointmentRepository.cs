using Microsoft.EntityFrameworkCore;
using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context) { }

    public override async Task<Appointment?> GetByIdAsync(int id) =>
        await DbSet
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.QueueEntry)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date) =>
        await DbSet
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.QueueEntry)
            .Where(a => a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime date) =>
        await DbSet
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

    public async Task<int> CountByDoctorAndDateAsync(int doctorId, DateTime date) =>
        await DbSet.CountAsync(a =>
            a.DoctorId == doctorId &&
            a.AppointmentDate.Date == date.Date &&
            a.Status != Domain.Enums.AppointmentStatus.Cancelled);

    public async Task<PagedResult<Appointment>> GetByDatePagedAsync(
        DateTime date, int page, int pageSize, int doctorId = 0)
    {
        var query = DbSet
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.QueueEntry)
            .Where(a => a.AppointmentDate.Date == date.Date);

        if (doctorId > 0)
            query = query.Where(a => a.DoctorId == doctorId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Appointment>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Appointment>> SearchPagedAsync(
        string? patientName, int doctorId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize)
    {
        var query = DbSet
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.QueueEntry)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(patientName))
            query = query.Where(a => a.Patient.FullName.Contains(patientName));

        if (doctorId > 0)
            query = query.Where(a => a.DoctorId == doctorId);

        if (fromDate.HasValue)
            query = query.Where(a => a.AppointmentDate.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(a => a.AppointmentDate.Date <= toDate.Value.Date);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Appointment>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Appointment>> GetByPatientPagedAsync(
        int patientId, int page, int pageSize)
    {
        var query = DbSet
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.QueueEntry)
            .Where(a => a.PatientId == patientId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Appointment>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<Appointment>> GetAllByDoctorAsync(int doctorId) =>
        await DbSet.Where(a => a.DoctorId == doctorId).ToListAsync();
}