using Microsoft.EntityFrameworkCore;
using Statera.Api.Data;
using Statera.Api.Models;

namespace Statera.Api.Services;

public sealed class LicensePolicyService
{
    private readonly AppDbContext _db;
    public LicensePolicyService(AppDbContext db) => _db = db;

    /// <summary>
    /// True if the staff member can accept a shift in the given state:
    /// - Staff exists, is active, not on license hold
    /// - Has at least one active license for the facility state that is not expired
    /// </summary>
    public async Task<(bool ok, string? reason)> CanAcceptShiftsAsync(int staffId, string facilityState)
    {
        var state = (facilityState ?? "").Trim().ToUpperInvariant();

        var staff = await _db.Staff
            .AsNoTracking()
            .Select(s => new { s.Id, s.IsActive, s.IsOnLicenseHold })
            .FirstOrDefaultAsync(s => s.Id == staffId);

        if (staff is null) return (false, "Staff not found.");
        if (!staff.IsActive) return (false, "Staff is inactive.");
        if (staff.IsOnLicenseHold) return (false, "Staff is on license hold.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var hasValid = await _db.StaffLicenses
            .AsNoTracking()
            .AnyAsync(l =>
                l.StaffId == staffId &&
                l.IsActive &&
                l.IssuingState == state &&
                l.ExpirationDate >= today);

        return hasValid
            ? (true, null)
            : (false, "No valid, unexpired license for the facility state.");
    }

    /// <summary>Convenience helper for reminders.</summary>
    public async Task<List<ExpiringLicenseSummary>> GetExpiringAsync(int days = 30)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(days));
        return await _db.StaffLicenses
            .Include(l => l.Staff)
            .Where(l => l.IsActive && l.ExpirationDate <= cutoff)
            .OrderBy(l => l.ExpirationDate)
            .Select(l => new ExpiringLicenseSummary(
                l.StaffId,
                (l.Staff!.PreferredName ?? l.Staff.FirstName) + " " + l.Staff.LastName,
                l.LicenseType,
                l.IssuingState,
                l.ExpirationDate,
                (l.ExpirationDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days
            ))
            .ToListAsync();
    }
}

public record ExpiringLicenseSummary(
    int StaffId,
    string StaffName,
    ProfessionalLicenseType LicenseType,
    string IssuingState,
    DateOnly ExpirationDate,
    int DaysUntilExpiry);

