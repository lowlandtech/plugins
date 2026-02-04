namespace BaGet.Core.Identity
{
    /// <summary>
    /// Utility methods for Identity.
    /// </summary>
    public static class IdentityUtilities
    {
        /// <summary>
        /// Generates a new API key for the user.
        /// </summary>
        public static string GenerateApiKey()
        {
            var bytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
