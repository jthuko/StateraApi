// Contracts/Assignments/AssignmentResponse.cs
using Statera.Api.Models;

namespace Statera.Api.Contracts.Assignments;

public record AssignmentResponse(
    int Id, int StaffId, string StaffName, string FacilityState,
    DateTime StartUtc, DateTime EndUtc, string? Unit, string? Notes, double Hours, string? RowVersion
);

