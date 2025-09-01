// Controllers/AssignmentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Statera.Api.Contracts.Assignments;
using Statera.Api.Data;
using Statera.Api.Models;
using Statera.Api.Services;

namespace Statera.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController(AppDbContext db, LicensePolicyService licensePolicy) : ControllerBase
{
    // GET /api/assignments?staffId=1&from=2025-08-01&to=2025-09-01
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentResponse>>> List(
        [FromQuery] int? staffId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var q = db.Assignments.Include(a => a.Staff).AsNoTracking();

        if (staffId is int sid) q = q.Where(a => a.StaffId == sid);
        if (from.HasValue) q = q.Where(a => a.EndUtc > from.Value);
        if (to.HasValue) q = q.Where(a => a.StartUtc < to.Value);

        var rows = await q
            .OrderBy(a => a.StartUtc)
            .Select(a => new {
                a.Id,
                a.StaffId,
                StaffName = a.Staff!.DisplayName,
                a.FacilityState,
                a.StartUtc,
                a.EndUtc,
                a.Unit,
                a.Notes,
                a.RowVersion
            })
            .ToListAsync();

        var result = rows.Select(a => new AssignmentResponse(
            a.Id, a.StaffId, a.StaffName, a.FacilityState,
            a.StartUtc, a.EndUtc, a.Unit, a.Notes,
            (a.EndUtc - a.StartUtc).TotalHours,
            a.RowVersion != null ? Convert.ToBase64String(a.RowVersion) : null
        ));

        return Ok(result);
    }

    // GET /api/assignments/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(int id)
    {
        var a = await db.Assignments.Include(x => x.Staff).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound();

        return new AssignmentResponse(
            a.Id, a.StaffId, a.Staff!.DisplayName, a.FacilityState,
            a.StartUtc, a.EndUtc, a.Unit, a.Notes, (a.EndUtc - a.StartUtc).TotalHours,
            a.RowVersion != null ? Convert.ToBase64String(a.RowVersion) : null
        );
    }

    // POST /api/assignments
    [HttpPost]
    public async Task<ActionResult<AssignmentResponse>> Create(CreateAssignmentDto dto)
    {
        if (dto.EndUtc <= dto.StartUtc)
            return BadRequest("EndUtc must be after StartUtc.");

        var staff = await db.Staff.Include(s => s.Role).FirstOrDefaultAsync(s => s.Id == dto.StaffId);
        if (staff is null) return BadRequest("Staff not found.");

        // 1) License eligibility
        var state = dto.FacilityState.Trim().ToUpperInvariant();  // <-- MISSING ; FIXED

        var (ok, reason) = await licensePolicy.CanAcceptShiftsAsync(dto.StaffId, state);
        if (!ok)
            return BadRequest(reason ?? "Staff is not eligible to accept shifts (license hold or no valid license for the facility state).");

        // 2) Overlap check
        var overlaps = await db.Assignments.AnyAsync(a =>
            a.StaffId == dto.StaffId &&
            a.StartUtc < dto.EndUtc &&
            a.EndUtc > dto.StartUtc
        );
        if (overlaps) return Conflict("Assignment overlaps an existing assignment.");

        // 3) Optional: min rest hours
        var prev = await db.Assignments
            .Where(a => a.StaffId == dto.StaffId && a.EndUtc <= dto.StartUtc)
            .OrderByDescending(a => a.EndUtc)
            .FirstOrDefaultAsync();

        if (prev is not null)
        {
            var rest = (dto.StartUtc - prev.EndUtc).TotalHours;
            if (rest < staff.MinRestHoursBetweenShifts)
                return BadRequest($"Insufficient rest between shifts. Required: {staff.MinRestHoursBetweenShifts}h.");
        }

        var entity = new Assignment
        {
            StaffId = dto.StaffId,
            FacilityState = state,
            StartUtc = DateTime.SpecifyKind(dto.StartUtc, DateTimeKind.Utc),
            EndUtc = DateTime.SpecifyKind(dto.EndUtc, DateTimeKind.Utc),
            Unit = dto.Unit,
            Notes = dto.Notes
        };

        db.Assignments.Add(entity);
        await db.SaveChangesAsync();

        // Re-read with Staff for response payload
        var created = await db.Assignments.Include(a => a.Staff).AsNoTracking().FirstAsync(a => a.Id == entity.Id);
        var payload = new AssignmentResponse(
            created.Id, created.StaffId, created.Staff!.DisplayName, created.FacilityState,
            created.StartUtc, created.EndUtc, created.Unit, created.Notes,
            (created.EndUtc - created.StartUtc).TotalHours,
            created.RowVersion != null ? Convert.ToBase64String(created.RowVersion) : null
        );

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, payload); // <-- pass DTO, not ActionResult
    }

    // DELETE /api/assignments/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await db.Assignments.FindAsync(id);
        if (a is null) return NotFound();
        db.Assignments.Remove(a);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
