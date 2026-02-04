using Microsoft.AspNetCore.Identity;

namespace BaGet.Core.Identity
{
    /// <summary>
    /// The application user entity for BaGet authentication.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The user's display name.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// API key for package operations.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// When the API key was generated.
        /// </summary>
        public DateTime? ApiKeyCreated { get; set; }

        /// <summary>
        /// Whether this user can push packages.
        /// </summary>
        public bool CanPushPackages { get; set; } = true;

        /// <summary>
        /// Whether this user can delete packages.
        /// </summary>
        public bool CanDeletePackages { get; set; } = false;
    }
}
