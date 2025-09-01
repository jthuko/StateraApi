using System.ComponentModel.DataAnnotations;

namespace Statera.Api.Models.Contracts.Licenses
{
    public class CreateStaffLicenseDto
    {
        public ProfessionalLicenseType LicenseType { get; init; }
        [MaxLength(32)] public string LicenseNumber { get; init; } = "";
        [MaxLength(2)] public string IssuingState { get; init; } = "";
        [Required] public DateOnly ExpirationDate { get; init; }
        public bool IsActive { get; init; } = true;
        [MaxLength(256)] public string? VerificationUrl { get; init; }
    }
}
