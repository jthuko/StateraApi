namespace Statera.Api.Models.Contracts.Licenses
{
    public class UpdateStaffLicenseDto : CreateStaffLicenseDto
    {
        public DateOnly? LastVerifiedOn { get; init; }
    }
}
