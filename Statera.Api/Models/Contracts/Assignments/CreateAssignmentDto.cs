// Contracts/Assignments/CreateAssignmentDto.cs
using System.ComponentModel.DataAnnotations;

namespace Statera.Api.Contracts.Assignments;

public class CreateAssignmentDto
{
    [Range(1, int.MaxValue)] public int StaffId { get; init; }
    [Required, MinLength(2), MaxLength(2)] public string FacilityState { get; init; } = "";
    [Required] public DateTime StartUtc { get; init; }
    [Required] public DateTime EndUtc { get; init; }
    [MaxLength(100)] public string? Unit { get; init; }
    [MaxLength(256)] public string? Notes { get; init; }
}
