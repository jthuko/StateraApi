using Statera.Api.Models.Contracts.Staff;

namespace Statera.Api.Models.Contracts.Roles
{
    public record RoleResponse(
     int Id,
     string Position,
     bool IsClinical,
     List<StaffBriefResponse> Staff
 );
}
