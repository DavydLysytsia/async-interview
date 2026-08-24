using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using AsyncInterview.Api.Controllers;
using AsyncInterview.Api.Data;
using AsyncInterview.Api.Models;
using AsyncInterview.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// Load server/.env (or any .env up the tree) for local development.
// On Azure the same settings come from App Service configuration.
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var options = new AppOptions
{
    // Trim() guards against stray whitespace from copy-pasting credentials.
    GoogleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")?.Trim(),
    GoogleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")?.Trim(),
    AppBaseUrl = (Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:5240").TrimEnd('/'),
    DevFakeAuth = string.Equals(Environment.GetEnvironmentVariable("DEV_FAKE_AUTH"), "true",
        StringComparison.OrdinalIgnoreCase)
};
builder.Services.AddSingleton(options);

var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "app.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<EfDataStore>();
builder.Services.AddScoped<YouTubeVideoService>();

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

var auth = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
auth.AddCookie(cookie =>
{
    cookie.Cookie.Name = "async_interview_auth";
    cookie.Cookie.HttpOnly = true;
    cookie.Cookie.SameSite = SameSiteMode.Lax;
    cookie.ExpireTimeSpan = TimeSpan.FromDays(7);
    cookie.SlidingExpiration = true;
    // This is an API for a SPA: return status codes instead of redirecting
    // to a login page.
    cookie.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    cookie.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

if (options.GoogleConfigured)
{
    auth.AddGoogle(google =>
    {
        google.ClientId = options.GoogleClientId!;
        google.ClientSecret = options.GoogleClientSecret!;
        google.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // Sign-in only asks for identity (openid/email/profile). YouTube
        // upload permissions are a separate, explicit consent step.
        google.Events.OnTicketReceived = async ctx =>
        {
            var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var principal = ctx.Principal!;
            var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var name = principal.FindFirstValue(ClaimTypes.Name) ?? email;

            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleSubject == subject);
            if (user == null)
            {
                user = new AppUser { GoogleSubject = subject, Email = email, DisplayName = name };
                db.Users.Add(user);
            }
            else
            {
                user.Email = email;
                user.DisplayName = name;
            }
            await db.SaveChangesAsync();

            // The app identifies the user by its own id, not the Google subject.
            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim(UserClaims.UserId, user.Id.ToString()));
        };
    });
}

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    AppDbContext.Seed(db);
}

// Users get a generic message; details stay in the server log.
app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await ctx.Response.WriteAsJsonAsync(new { error = "Something went wrong on the server." });
}));

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { ok = true }));
app.MapControllers();

// Unknown /api routes are JSON 404s; everything else is the React app.
app.MapFallback("/api/{**path}", () => Results.NotFound(new { error = "Not found." }));
app.MapFallbackToFile("index.html");

app.Run();
