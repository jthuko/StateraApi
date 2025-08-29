namespace Statera.Api.Models.Contracts.Staff
{
    public class StaffResponse
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = "";
        public string LastName { get; init; } = "";
        public string? PreferredName { get; init; }
        public string DisplayName { get; init; } = "";
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public short RoleId { get; init; }
        public string? RoleName { get; init; }
        public EmploymentType EmploymentType { get; init; }
        public bool CanWorkDouble { get; init; }
        public bool CanWorkNights { get; init; }
        public int MaxDailyHours { get; init; }
        public int MaxWeeklyHours { get; init; }
        public int MinRestHoursBetweenShifts { get; init; }
        public bool IsActive { get; init; }
        public bool IsOnLicenseHold { get; init; }
        public string? RowVersion { get; init; } // base64 of rowversion

        public StaffResponse() { }

        public StaffResponse(
            int id,
            string firstName,
            string lastName,
            string? preferredName,
            string displayName,
            string? email,
            string? phone,
            short roleId,
            string? roleName,
            EmploymentType employmentType,
            bool canWorkDouble,
            bool canWorkNights,
            int maxDailyHours,
            int maxWeeklyHours,
            int minRestHoursBetweenShifts,
            bool isActive,
            bool isOnLicenseHold,
            string? rowVersion)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            PreferredName = preferredName;
            DisplayName = displayName;
            Email = email;
            Phone = phone;
            RoleId = roleId;
            RoleName = roleName;
            EmploymentType = employmentType;
            CanWorkDouble = canWorkDouble;
            CanWorkNights = canWorkNights;
            MaxDailyHours = maxDailyHours;
            MaxWeeklyHours = maxWeeklyHours;
            MinRestHoursBetweenShifts = minRestHoursBetweenShifts;
            IsActive = isActive;
            IsOnLicenseHold = isOnLicenseHold;
            RowVersion = rowVersion;
        }
    }
}
