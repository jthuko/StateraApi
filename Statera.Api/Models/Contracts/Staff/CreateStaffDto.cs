using System.ComponentModel.DataAnnotations;

namespace Statera.Api.Models.Contracts.Staff
{
    public record CreateStaffDto(
    [param: Required, MaxLength(100)] string FirstName,
    [param: Required, MaxLength(100)] string LastName,
    [param: MaxLength(100)] string? PreferredName,
    [param: EmailAddress, MaxLength(256)] string? Email,
    [param: Phone, MaxLength(30)] string? Phone,
    [param: Range(1, short.MaxValue)] short RoleId,
    EmploymentType EmploymentType,
    bool CanWorkDouble,
    bool CanWorkNights,
    [param: Range(1, 16)] int MaxDailyHours,
    [param: Range(0, 84)] int MaxWeeklyHours,
    [param: Range(0, 24)] int MinRestHoursBetweenShifts
);
}
