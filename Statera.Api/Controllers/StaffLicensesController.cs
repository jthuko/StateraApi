// Controllers/RolesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Statera.Api.Data;
using Statera.Api.Models;                       // EF entities
using Statera.Api.Models.Contracts.Roles;      // RoleResponse
using Statera.Api.Models.Contracts.Staff;      // StaffBriefResponse

namespace Statera.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(AppDbContext db) : ControllerBase
{
    // GET /api/roles
    [HttpGet]
    public async Task<ActionResult<List<RoleResponse>>> Get()
    {
        var roles = await db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Position)
            .Select(r => new RoleResponse(
                (int)r.Id,                       // short -> int for API
                r.Position,
                r.IsClinical,
                r.Staff.Select(s => new StaffBriefResponse(
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.DisplayName,
                    (int)s.RoleId                // short -> int for API
                )).ToList()
            ))
            .ToListAsync();

        return Ok(roles);
    }

    public record UpsertRoleDto(string Position, bool IsClinical);

    // POST /api/roles
    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(UpsertRoleDto dto)
    {
        var role = new Role { Position = dto.Position, IsClinical = dto.IsClinical };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var response = new RoleResponse(
            (int)role.Id,
            role.Position,
            role.IsClinical,
            new List<StaffBriefResponse>()
        );

        return CreatedAtAction(nameof(GetById), new { id = (int)role.Id }, response);
    }

    // GET /api/roles/{id}
    [HttpGet("{id:int}", Name = "GetRoleById")]
    public async Task<ActionResult<RoleResponse>> GetById(int id)
    {
        // guard against values outside SQL smallint range
        if (id < short.MinValue || id > short.MaxValue)
            return BadRequest($"id must be between {short.MinValue} and {short.MaxValue}.");

        var role = await db.Roles
            .AsNoTracking()
            .Where(r => r.Id == (short)id)
            .Select(r => new RoleResponse(
                (int)r.Id,
                r.Position,
                r.IsClinical,
                r.Staff.Select(s => new StaffBriefResponse(
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.DisplayName,
                    (int)s.RoleId
                )).ToList()
            ))
            .FirstOrDefaultAsync();

        return role is null ? NotFound() : Ok(role);
    }

    // PUT /api/roles/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpsertRoleDto dto)
    {
        if (id < short.MinValue || id > short.MaxValue)
            return BadRequest($"id must be between {short.MinValue} and {short.MaxValue}.");

        var role = await db.Roles.FindAsync((short)id);
        if (role is null) return NotFound();

        role.Position = dto.Position;
        role.IsClinical = dto.IsClinical;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/roles/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id < short.MinValue || id > short.MaxValue)
            return BadRequest($"id must be between {short.MinValue} and {short.MaxValue}.");

        var used = await db.Staff.AnyAsync(s => s.RoleId == (short)id);
        if (used) return Conflict("Role is in use by one or more staff.");

        var role = await db.Roles.FindAsync((short)id);
        if (role is null) return NotFound();

        db.Roles.Remove(role);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
