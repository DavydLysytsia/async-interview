using AsyncInterview.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncInterview.Api.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public QuestionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var questions = await _db.Questions.AsNoTracking()
            .Where(q => q.IsActive)
            .OrderBy(q => q.DisplayOrder)
            .Select(q => new { q.Id, q.Text, q.DisplayOrder })
            .ToListAsync(ct);
        return Ok(questions);
    }
}
