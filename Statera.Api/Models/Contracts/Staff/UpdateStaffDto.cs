using System.ComponentModel.DataAnnotations;

namespace Statera.Api.Models.Contracts.Staff
{
    public record UpdateStaffDto(
    [property: Required, MaxLength(100)] string FirstName,
    [property: Required, MaxLength(100)] string LastName,
    [property: MaxLength(100)] string? PreferredName,
    [property: EmailAddress, MaxLength(256)] string? Email,
    [property: Phone, MaxLength(30)] string? Phone,
    [property: Range(1, short.MaxValue)] short RoleId,
    EmploymentType EmploymentType,
    bool CanWorkDouble,
    bool CanWorkNights,
    [property: Range(1, 16)] int MaxDailyHours,
    [property: Range(0, 84)] int MaxWeeklyHours,
    [property: Range(0, 24)] int MinRestHoursBetweenShifts,
    string? RowVersion // base64 string from the entity’s byte[] RowVersion
);
}
