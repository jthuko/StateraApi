using Microsoft.EntityFrameworkCore;
using Statera.Api.Models;

namespace Statera.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Staff> Staff => Set<Staff>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(e =>
            {
                e.Property(x => x.Position).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Position).IsUnique();
            });

            // Data/AppDbContext.cs  (inside OnModelCreating)
            modelBuilder.Entity<Staff>(e =>
            {
                // ...your existing config...
                e.HasIndex(x => x.IsOnLicenseHold);
            });

            modelBuilder.Entity<Staff>(e =>
            {
                e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
                e.Property(x => x.PreferredName).HasMaxLength(100);
                e.Property(x => x.Email).HasMaxLength(256);
                e.Property(x => x.Phone).HasMaxLength(30);

                // store enums as strings for readability (optional)
                e.Property(x => x.EmploymentType).HasConversion<string>().HasMaxLength(20);

                // basic check constraints to catch bad data early
                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Staff_MaxDailyHours", "[MaxDailyHours] BETWEEN 1 AND 16");
                    tb.HasCheckConstraint("CK_Staff_MinRestHours", "[MinRestHoursBetweenShifts] BETWEEN 0 AND 24");
                });

                e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            });

            // Seed a few common roles (idempotent in migrations)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Position = "RN", IsClinical = true },
                new Role { Id = 2, Position = "LPN", IsClinical = true },
                new Role { Id = 3, Position = "CMA", IsClinical = true },
                new Role { Id = 4, Position = "CNA", IsClinical = true },
                new Role { Id = 5, Position = "Scheduler", IsClinical = false }
            );
        }
    }
}
