using System.ComponentModel.DataAnnotations;
using Statera.Api.Models; // for enums if you keep them here

namespace Statera.Api.Models;

public class StaffLicense
{
    public int Id { get; set; }

    // FK
    public int StaffId { get; set; }
    public Staff? Staff { get; set; }

    public ProfessionalLicenseType LicenseType { get; set; }

    [MaxLength(32)] public string LicenseNumber { get; set; } = "";
    [MaxLength(2)] public string IssuingState { get; set; } = ""; // e.g., "KS", "MO"

    public DateOnly ExpirationDate { get; set; }
    public DateOnly? LastVerifiedOn { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(256)] public string? VerificationUrl { get; set; }
}

