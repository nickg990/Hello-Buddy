using HelloBuddy.Application.Auth;
using HelloBuddy.Ui.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelloBuddy.Ui.Controllers;

/// <summary>
/// Handles login, logout, and forced password change flows.
/// </summary>
public sealed class AccountController : Controller
{
    private readonly ILoginService _loginService;

    public AccountController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginRequest request, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(request);

        // Honeypot: if the admin email field is filled, silently reject (bot).
        var honeypot = Request.Form["AdminEmail"].ToString();
        var honeypotTriggered = !string.IsNullOrWhiteSpace(honeypot);

        var result = await _loginService.AuthenticateAsync(request.Email, request.Password, honeypotTriggered, ct);

        switch (result.Outcome)
        {
            case LoginOutcome.Success:
                await SignInAsync(result);
                return RedirectToLocal(returnUrl);

            case LoginOutcome.MustChangePassword:
                await SignInAsync(result);
                return RedirectToAction("MustChangePassword");

            case LoginOutcome.AccountLocked:
                ModelState.AddModelError(string.Empty, "Account is locked after multiple failed attempts. Try again in 15 minutes.");
                return View(request);

            case LoginOutcome.AccountInactive:
                ModelState.AddModelError(string.Empty, "Account is inactive.");
                return View(request);

            case LoginOutcome.InvalidCredentials:
            case LoginOutcome.HoneypotTriggered:
            default:
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(request);
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult MustChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MustChangePassword([FromForm] MustChangePasswordRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View();

        var idClaim = User.FindFirst("practitioner_id")?.Value;
        if (!ulong.TryParse(idClaim, out var practitionerId) || practitionerId == 0)
        {
            return RedirectToAction("Login");
        }

        var success = await _loginService.ForceChangePasswordAsync(practitionerId, request.NewPassword, ct);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update password.");
            return View();
        }

        // Password updated; redirect to home (or a confirmation page).
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInAsync(LoginResult result)
    {
        var claims = new List<Claim>
        {
            new("practitioner_id", result.PractitionerId.ToString()),
            new(ClaimTypes.Name, result.PractitionerName),
            new("practitioner_role", result.PractitionerRole),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
