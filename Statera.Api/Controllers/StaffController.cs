using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Statera.Api.Data;
using Statera.Api.Models;
using Statera.Api.Models.Contracts.Staff;

namespace Statera.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController(AppDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<List<StaffResponse>> Get()
            => await db.Staff.Include(s => s.Role)
                .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
                .Select(s => new StaffResponse(
                    s.Id, s.FirstName, s.LastName, s.PreferredName, s.DisplayName,
                    s.Email, s.Phone, s.RoleId, s.Role != null ? s.Role.Position : null,
                    s.EmploymentType, s.CanWorkDouble, s.CanWorkNights,
                    s.MaxDailyHours, s.MaxWeeklyHours, s.MinRestHoursBetweenShifts,
                    s.IsActive, s.IsOnLicenseHold,
                    s.RowVersion != null ? Convert.ToBase64String(s.RowVersion) : null
                ))
                .ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<StaffResponse>> GetById(int id)
        {
            var s = await db.Staff.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
            if (s is null) return NotFound();

            var resp = new StaffResponse(
                s.Id, s.FirstName, s.LastName, s.PreferredName, s.DisplayName,
                s.Email, s.Phone, s.RoleId, s.Role?.Position,
                s.EmploymentType, s.CanWorkDouble, s.CanWorkNights,
                s.MaxDailyHours, s.MaxWeeklyHours, s.MinRestHoursBetweenShifts,
                s.IsActive, s.IsOnLicenseHold,
                s.RowVersion != null ? Convert.ToBase64String(s.RowVersion) : null
            );
            return Ok(resp);
        }

        [HttpPost]
        public async Task<ActionResult<StaffResponse>> Create(CreateStaffDto dto)
        {
            var s = new Staff
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PreferredName = dto.PreferredName,
                Email = dto.Email,
                Phone = dto.Phone,
                RoleId = dto.RoleId,
                EmploymentType = dto.EmploymentType,   // ✅ same enum type as entity
                CanWorkDouble = dto.CanWorkDouble,
                CanWorkNights = dto.CanWorkNights,
                MaxDailyHours = dto.MaxDailyHours,
                MaxWeeklyHours = dto.MaxWeeklyHours,
                MinRestHoursBetweenShifts = dto.MinRestHoursBetweenShifts
            };

            db.Staff.Add(s);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = s.Id }, new StaffResponse(
                s.Id, s.FirstName, s.LastName, s.PreferredName, s.DisplayName,
                s.Email, s.Phone, s.RoleId, null,
                s.EmploymentType, s.CanWorkDouble, s.CanWorkNights,
                s.MaxDailyHours, s.MaxWeeklyHours, s.MinRestHoursBetweenShifts,
                s.IsActive, s.IsOnLicenseHold,
                s.RowVersion != null ? Convert.ToBase64String(s.RowVersion) : null
            ));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateStaffDto dto)
        {
            var s = await db.Staff.FindAsync(id);
            if (s is null) return NotFound();

            // Concurrency: require RowVersion on update
            if (!string.IsNullOrEmpty(dto.RowVersion))
            {
                var original = Convert.FromBase64String(dto.RowVersion);
                db.Entry(s).Property(x => x.RowVersion).OriginalValue = original;
            }

            s.FirstName = dto.FirstName;
            s.LastName = dto.LastName;
            s.PreferredName = dto.PreferredName;
            s.Email = dto.Email;
            s.Phone = dto.Phone;
            s.RoleId = dto.RoleId;
            s.EmploymentType = dto.EmploymentType;   // ✅ same enum type
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
    }
}
