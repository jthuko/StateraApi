using Statera.Api.Models;
/// <summary>Employment relationship for a staff member.</summary>
public enum EmploymentType
    {
        FullTime,
        PartTime,
        PerDiem,
        Contractor
    }

    /// <summary>Preferred shift pattern for a given day.</summary>
    public enum ShiftPreference
    {
        /// <summary>No preference / available as needed.</summary>
        None = 0,

        /// <summary>1st shift (8 hours).</summary>
        First8 = 1,

        /// <summary>2nd shift (8 hours).</summary>
        Second8 = 2,

        /// <summary>3rd shift (8 hours).</summary>
        Third8 = 3,

        /// <summary>12-hour day shift.</summary>
        Day12 = 4,

        /// <summary>12-hour night shift.</summary>
        Night12 = 5
    }

    /// <summary>Professional license classification.</summary>
    public enum ProfessionalLicenseType
    {
        RN,
        LPN,
        CNA,
        NP,
        PA,
        MD,
        Other
    }


