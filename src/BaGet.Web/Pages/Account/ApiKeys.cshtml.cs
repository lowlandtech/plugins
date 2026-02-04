using System.Threading.Tasks;
using BaGet.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BaGet.Web.Pages.Account
{
    [Authorize]
    public class ApiKeysModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ApiKeysModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string? ApiKey { get; set; }
        public bool CanPushPackages { get; set; }
        public bool CanDeletePackages { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }

            ApiKey = user.ApiKey;
            CanPushPackages = user.CanPushPackages;
            CanDeletePackages = user.CanDeletePackages;

            return Page();
        }

        public async Task<IActionResult> OnPostRegenerateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }

            user.ApiKey = IdentityUtilities.GenerateApiKey();
            user.ApiKeyCreated = System.DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            StatusMessage = "Your API key has been regenerated.";
            return RedirectToPage();
        }
    }
}
