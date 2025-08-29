using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Statera.Api.Models
{
    public enum EmploymentType { FullTime, PartTime, PerDiem, Contractor }
    public class Staff
    {
        public int Id { get; set; }                        // PK

        [MaxLength(100)] public string FirstName { get; set; } = "";
        [MaxLength(100)] public string LastName { get; set; } = "";
        [MaxLength(100)] public string? PreferredName { get; set; } // optional

        [MaxLength(256)] public string? Email { get; set; }
        [MaxLength(30)] public string? Phone { get; set; }

        // Primary role
        public short RoleId { get; set; }
        public Role? Role { get; set; }

        public EmploymentType EmploymentType { get; set; } = EmploymentType.PerDiem;

        // Scheduling constraints (v1)
        public bool CanWorkDouble { get; set; }            // allow 16-hour doubles
        public bool CanWorkNights { get; set; }
        public int MaxDailyHours { get; set; } = 16;      // hard cap per policy
        public int MaxWeeklyHours { get; set; } = 60;     // adjust to your rules
        public int MinRestHoursBetweenShifts { get; set; } = 8;

        public bool IsActive { get; set; } = true;
        public bool IsOnLicenseHold { get; set; } = false; // 🚫 blocks shift pickup
        [Timestamp] public byte[]? RowVersion { get; set; } // optimistic concurrency

        [NotMapped]
        public string DisplayName =>
            string.IsNullOrWhiteSpace(PreferredName)
                ? $"{FirstName} {LastName}"
                : $"{PreferredName} {LastName}";
    }
}
