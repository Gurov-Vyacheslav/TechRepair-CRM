using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.Auth;

namespace TechRepair_CRM.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public record LoginInput
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; init; } = string.Empty;

        public bool RememberMe { get; init; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/Orders";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= "/Orders";

        if (!ModelState.IsValid)
            return Page();

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user is not null)
            {
                var isTechnician = await _userManager.IsInRoleAsync(user, "Technician");
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isManager = await _userManager.IsInRoleAsync(user, "Manager");

                if (isTechnician && !isAdmin && !isManager)
                    return RedirectToPage("/Technicians/MyWork");
            }

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectToPage("/Orders/Index");
        }

        ModelState.AddModelError(string.Empty, "Неверный email или пароль");
        return Page();
    }
}