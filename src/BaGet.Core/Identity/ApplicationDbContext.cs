using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaGet.Core.Identity
{
    /// <summary>
    /// The database context for ASP.NET Identity.
    /// This is separate from the package database context.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the Identity schema if needed
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.DisplayName).HasMaxLength(256);
                entity.Property(u => u.ApiKey).HasMaxLength(64);
                entity.HasIndex(u => u.ApiKey).IsUnique();
            });
        }
    }
}
