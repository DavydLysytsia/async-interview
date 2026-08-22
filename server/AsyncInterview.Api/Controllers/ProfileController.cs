using AsyncInterview.Api.Data;
using AsyncInterview.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncInterview.Api.Controllers;

public class ProfileDto
{
    public string FullName { get; set; } = "";
    public string Headline { get; set; } = "";
    public string Biography { get; set; } = "";
    public string Skills { get; set; } = "";
    public string ContactLinks { get; set; } = "";
}

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var profile = await _db.Profiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == User.GetUserId(), ct);

        return Ok(new ProfileDto
        {
            FullName = profile?.FullName ?? "",
            Headline = profile?.Headline ?? "",
            Biography = profile?.Biography ?? "",
            Skills = profile?.Skills ?? "",
            ContactLinks = profile?.ContactLinks ?? ""
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProfileDto dto, CancellationToken ct)
    {
        var errors = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(dto.FullName))
            errors["fullName"] = "Full name is required.";
        if (dto.FullName?.Length > 100) errors["fullName"] = "Full name must be 100 characters or fewer.";
        if (dto.Headline?.Length > 150) errors["headline"] = "Headline must be 150 characters or fewer.";
        if (dto.Biography?.Length > 2000) errors["biography"] = "Biography must be 2000 characters or fewer.";
        if (dto.Skills?.Length > 1000) errors["skills"] = "Skills list is too long.";
        if (dto.ContactLinks?.Length > 1000) errors["contactLinks"] = "Links list is too long.";
        if (errors.Count > 0) return BadRequest(new { errors });

        var userId = User.GetUserId();
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null)
        {
            profile = new CandidateProfile { UserId = userId };
            _db.Profiles.Add(profile);
        }

        profile.FullName = dto.FullName!.Trim();
        profile.Headline = dto.Headline?.Trim() ?? "";
        profile.Biography = dto.Biography?.Trim() ?? "";
        profile.Skills = dto.Skills?.Trim() ?? "";
        profile.ContactLinks = dto.ContactLinks?.Trim() ?? "";
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }
}
