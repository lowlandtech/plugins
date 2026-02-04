using System;
using BaGet.Core;
using BaGet.Core.Identity;
using BaGet.Database.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BaGet
{
    public static class PostgreSqlApplicationExtensions
    {
        public static BaGetApplication AddPostgreSqlDatabase(this BaGetApplication app)
        {
            app.Services.AddBaGetDbContextProvider<PostgreSqlContext>("PostgreSql", (provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                options.UseNpgsql(databaseOptions.Value.ConnectionString);
            });

            return app;
        }

        public static BaGetApplication AddPostgreSqlDatabase(
            this BaGetApplication app,
            Action<DatabaseOptions> configure)
        {
            app.AddPostgreSqlDatabase();
            app.Services.Configure(configure);
            return app;
        }

        /// <summary>
        /// Adds PostgreSQL-backed ASP.NET Identity to BaGet.
        /// Call this after AddPostgreSqlDatabase.
        /// </summary>
        public static BaGetApplication AddPostgreSqlIdentity(this BaGetApplication app)
        {
            app.Services.AddDbContext<PostgreSqlIdentityContext>((provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();
                options.UseNpgsql(databaseOptions.Value.ConnectionString);
            });

            return app;
        }

        /// <summary>
        /// Configures ASP.NET Identity with PostgreSQL storage.
        /// Must be called after AddPostgreSqlIdentity.
        /// </summary>
        public static IServiceCollection AddBaGetIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<PostgreSqlIdentityContext>()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}
