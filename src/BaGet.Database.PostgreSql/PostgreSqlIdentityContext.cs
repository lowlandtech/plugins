using BaGet.Core.Identity;
using Microsoft.EntityFrameworkCore;

namespace BaGet.Database.PostgreSql
{
    /// <summary>
    /// PostgreSQL-specific Identity context.
    /// </summary>
    public class PostgreSqlIdentityContext : ApplicationDbContext
    {
        public PostgreSqlIdentityContext(DbContextOptions<PostgreSqlIdentityContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Use citext for case-insensitive email and username
            builder.HasPostgresExtension("citext");

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.Email).HasColumnType("citext");
                entity.Property(u => u.NormalizedEmail).HasColumnType("citext");
                entity.Property(u => u.UserName).HasColumnType("citext");
                entity.Property(u => u.NormalizedUserName).HasColumnType("citext");
            });
        }
    }
}
