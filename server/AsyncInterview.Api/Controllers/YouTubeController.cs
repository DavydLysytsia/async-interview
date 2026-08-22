using AsyncInterview.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsyncInterview.Api.Controllers;

[ApiController]
[Route("api/youtube")]
[Authorize]
public class YouTubeController : ControllerBase
{
    private const string StateCookie = "yt_oauth_state";

    private readonly YouTubeVideoService _youtube;
    private readonly ILogger<YouTubeController> _logger;

    public YouTubeController(YouTubeVideoService youtube, ILogger<YouTubeController> logger)
    {
        _youtube = youtube;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        try
        {
            var status = await _youtube.GetStatusAsync(User.GetUserId(), ct);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YouTube status check failed");
            return Ok(new YouTubeStatus { Configured = true, Connected = false });
        }
    }

    // Starts the "Connect YouTube" authorization (upload + readonly scopes).
    [HttpGet("connect")]
    public IActionResult Connect()
    {
        try
        {
            var state = Guid.NewGuid().ToString("N");
            Response.Cookies.Append(StateCookie, state, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(10)
            });
            return Redirect(_youtube.BuildAuthorizationUrl(state));
        }
        catch (YouTubeFriendlyException ex)
        {
            return BadRequest(new { error = ex.Message, reason = ex.Reason });
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error, CancellationToken ct)
    {
        var expectedState = Request.Cookies[StateCookie];
        Response.Cookies.Delete(StateCookie);

        if (!string.IsNullOrEmpty(error))
            return Redirect("/connect?error=denied");
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state) || state != expectedState)
            return Redirect("/connect?error=state");

        try
        {
            await _youtube.ExchangeCodeAsync(User.GetUserId(), code, ct);
            return Redirect("/connect?connected=1");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YouTube token exchange failed");
            return Redirect("/connect?error=exchange");
        }
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect(CancellationToken ct)
    {
        await _youtube.DisconnectAsync(User.GetUserId(), ct);
        return Ok(new { ok = true });
    }
}
