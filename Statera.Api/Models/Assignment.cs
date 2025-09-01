// Models/Assignment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Statera.Api.Models;

public class Assignment
{
    public int Id { get; set; }

    [Required] public int StaffId { get; set; }
    public Staff? Staff { get; set; }

    // 2-letter state for license matching (e.g., "KS", "MO")
    [MaxLength(2)] public string FacilityState { get; set; } = "";

    // Store in UTC to avoid TZ headaches
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    [MaxLength(100)] public string? Unit { get; set; }
    [MaxLength(256)] public string? Notes { get; set; }

    [Timestamp] public byte[]? RowVersion { get; set; }

    [NotMapped] public double Hours => (EndUtc - StartUtc).TotalHours;
}
