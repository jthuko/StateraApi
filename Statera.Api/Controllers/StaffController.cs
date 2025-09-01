// Controllers/StaffController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Statera.Api.Data;
using Statera.Api.Models;
using Statera.Api.Models.Contracts.Staff;

namespace Statera.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StaffController(AppDbContext db) : ControllerBase
{
    // GET /api/staff?q=jo&onHold=false&active=true&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<object>> Get([FromQuery] string? q, [FromQuery] bool? onHold,
        [FromQuery] bool? active, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var qry = db.Staff.Include(s => s.Role).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            qry = qry.Where(s =>
                s.FirstName.Contains(term) || s.LastName.Contains(term) ||
                (s.PreferredName != null && s.PreferredName.Contains(term)) ||
                (s.Email != null && s.Email.Contains(term)));
        }
        if (onHold.HasValue) qry = qry.Where(s => s.IsOnLicenseHold == onHold);
        if (active.HasValue) qry = qry.Where(s => s.IsActive == active);

        var total = await qry.CountAsync();

        var itemsRaw = await qry
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new {
                s.Id,
                s.FirstName,
                s.LastName,
                s.PreferredName,
                s.DisplayName,
                s.Email,
                s.Phone,
                s.RoleId,
                RoleName = s.Role != null ? s.Role.Position : null,
                s.EmploymentType,
                s.CanWorkDouble,
                s.CanWorkNights,
                s.MaxDailyHours,
                s.MaxWeeklyHours,
                s.MinRestHoursBetweenShifts,
                s.IsActive,
                s.IsOnLicenseHold,
                s.RowVersion
            })
            .ToListAsync();

        var items = itemsRaw.Select(s => new StaffResponse(
            s.Id, s.FirstName, s.LastName, s.PreferredName, s.DisplayName,
            s.Email, s.Phone, s.RoleId, s.RoleName,
            s.EmploymentType, s.CanWorkDouble, s.CanWorkNights,
            s.MaxDailyHours, s.MaxWeeklyHours, s.MinRestHoursBetweenShifts,
            s.IsActive, s.IsOnLicenseHold,
            s.RowVersion != null ? Convert.ToBase64String(s.RowVersion) : null
        )).ToList();

        return Ok(new { total, page, pageSize, items });
    }

    // GET /api/staff/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StaffResponse>> GetById(int id)
    {
        var staff = await db.Staff
            .Include(x => x.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (staff is null)
            return NotFound();

        var response = new StaffResponse(
            staff.Id,
            staff.FirstName,
            staff.LastName,
            staff.PreferredName,
            staff.DisplayName,
            staff.Email,
            staff.Phone,
            staff.RoleId,
            staff.Role?.Position,
            staff.EmploymentType,
            staff.CanWorkDouble,
            staff.CanWorkNights,
            staff.MaxDailyHours,
            staff.MaxWeeklyHours,
            staff.MinRestHoursBetweenShifts,
            staff.IsActive,
            staff.IsOnLicenseHold,
            staff.RowVersion != null ? Convert.ToBase64String(staff.RowVersion) : null
        );

        return Ok(response);
    }


    // POST /api/staff
    [HttpPost]
    public async Task<ActionResult<StaffResponse>> Create(CreateStaffDto dto)
    {
        // optional: ensure role exists
        if (!await db.Roles.AnyAsync(r => r.Id == dto.RoleId))
            return BadRequest($"RoleId {dto.RoleId} does not exist.");

        var s = new Staff
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PreferredName = dto.PreferredName,
            Email = dto.Email,
            Phone = dto.Phone,
            RoleId = dto.RoleId,
            EmploymentType = dto.EmploymentType,
            CanWorkDouble = dto.CanWorkDouble,
            CanWorkNights = dto.CanWorkNights,
            MaxDailyHours = dto.MaxDailyHours,
            MaxWeeklyHours = dto.MaxWeeklyHours,
            MinRestHoursBetweenShifts = dto.MinRestHoursBetweenShifts
        };

        db.Staff.Add(s);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = s.Id }, await GetById(s.Id));
    }

    // PUT /api/staff/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateStaffDto dto)
    {
        var s = await db.Staff.FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound();

        if (!string.IsNullOrEmpty(dto.RowVersion))
            db.Entry(s).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(dto.RowVersion);

        s.FirstName = dto.FirstName;
        s.LastName = dto.LastName;
        s.PreferredName = dto.PreferredName;
        s.Email = dto.Email;
        s.Phone = dto.Phone;
        s.RoleId = dto.RoleId;
        s.EmploymentType = dto.EmploymentType;
        s.CanWorkDouble = dto.CanWorkDouble;
        s.CanWorkNights = dto.CanWorkNights;
        s.MaxDailyHours = dto.MaxDailyHours;
        s.MaxWeeklyHours = dto.MaxWeeklyHours;
        s.MinRestHoursBetweenShifts = dto.MinRestHoursBetweenShifts;

        try
        {
            await db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("This record was modified by someone else. Please refresh and try again.");
        }
    }

    // PATCH /api/staff/5/activate?active=false
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> SetActive(int id, [FromQuery] bool active = true)
    {
        var s = await db.Staff.FindAsync(id);
        if (s is null) return NotFound();
        s.IsActive = active;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // PATCH /api/staff/5/license-hold?onHold=true
    [HttpPatch("{id:int}/license-hold")]
    public async Task<IActionResult> SetLicenseHold(int id, [FromQuery] bool onHold = true)
    {
        var s = await db.Staff.FindAsync(id);
        if (s is null) return NotFound();
        s.IsOnLicenseHold = onHold;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/staff/5   (soft delete: mark inactive)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await db.Staff.FindAsync(id);
        if (s is null) return NotFound();
        s.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
