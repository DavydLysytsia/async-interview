using AsyncInterview.Api.Data;
using AsyncInterview.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncInterview.Api.Controllers;

// Data for the candidate preview page: profile + completed answers, in
// question order, ready for embedding.
[ApiController]
[Route("api/preview")]
[Authorize]
public class PreviewController : ControllerBase
{
    private readonly AppDbContext _db;

    public PreviewController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var profile = await _db.Profiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var answers = await _db.Responses.AsNoTracking()
            .Where(r => r.UserId == userId && r.Status == ResponseUploadStatus.Completed)
            .Join(_db.Questions, r => r.QuestionId, q => q.Id, (r, q) => new
            {
                q.Text,
                q.DisplayOrder,
                r.YouTubeVideoId,
                r.PrivacyStatus
            })
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(ct);

        return Ok(new
        {
            profile = new
            {
                fullName = profile?.FullName ?? "",
                headline = profile?.Headline ?? "",
                biography = profile?.Biography ?? "",
                skills = profile?.Skills ?? "",
                contactLinks = profile?.ContactLinks ?? ""
            },
            answers
        });
    }
}
