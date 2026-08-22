using System.Security.Claims;
using AsyncInterview.Api.Data;
using AsyncInterview.Api.Models;
using AsyncInterview.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncInterview.Api.Controllers;

public static class UserClaims
{
    public const string UserId = "app_uid";

    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(UserId);
        return value == null ? 0 : int.Parse(value);
    }
}

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AppOptions _options;

    public AuthController(AppDbContext db, AppOptions options)
    {
        _db = db;
        _options = options;
    }

    // Tells the SPA which sign-in mode is available.
    [HttpGet("config")]
    [AllowAnonymous]
    public IActionResult Config() => Ok(new
    {
        googleEnabled = _options.GoogleConfigured,
        devFakeAuth = _options.DevFakeAuth
    });

    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleSignIn([FromQuery] string? returnUrl)
    {
        if (!_options.GoogleConfigured)
            return NotFound(new { error = "Google sign-in is not configured yet." });

        // Only allow local paths so this can't be used as an open redirect.
        var target = (returnUrl != null && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//"))
            ? returnUrl : "/dashboard";
        return Challenge(new AuthenticationProperties { RedirectUri = target }, "Google");
    }

    // Local stand-in for Google sign-in so the app can be developed and demoed
    // before OAuth credentials exist. Enabled only with DEV_FAKE_AUTH=true.
    [HttpPost("dev-login")]
    [AllowAnonymous]
    public async Task<IActionResult> DevLogin(CancellationToken ct)
    {
        if (!_options.DevFakeAuth)
            return NotFound(new { error = "Dev login is disabled." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleSubject == "dev-user", ct);
        if (user == null)
        {
            user = new AppUser
            {
                GoogleSubject = "dev-user",
                Email = "demo@example.com",
                DisplayName = "Demo Candidate"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }

        await SignInUserAsync(user);
        return Ok(new { ok = true });
    }

    [HttpGet("me")]
    [AllowAnonymous]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new { authenticated = false });

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == User.GetUserId(), ct);
        if (user == null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { authenticated = false });
        }

        return Ok(new
        {
            authenticated = true,
            user.Id,
            user.Email,
            user.DisplayName,
            consentAccepted = user.ConsentAcceptedAt != null
        });
    }

    // Records acceptance of the privacy/terms/truthfulness notice (first login).
    [HttpPost("consent")]
    [Authorize]
    public async Task<IActionResult> AcceptConsent(CancellationToken ct)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == User.GetUserId(), ct);
        user.ConsentAcceptedAt ??= DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { ok = true });
    }

    private async Task SignInUserAsync(AppUser user)
    {
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(UserClaims.UserId, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.DisplayName));
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
