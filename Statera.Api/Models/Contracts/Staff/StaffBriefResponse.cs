namespace Statera.Api.Models.Contracts.Staff
{
    public record StaffBriefResponse(
     int Id,
     string FirstName,
     string LastName,
     string DisplayName,
     int RoleId   // keep int for API simplicity; cast from short in queries if needed
 );
}
