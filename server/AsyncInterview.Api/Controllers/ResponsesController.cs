using AsyncInterview.Api.Data;
using AsyncInterview.Api.Models;
using AsyncInterview.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncInterview.Api.Controllers;

[ApiController]
[Route("api/responses")]
[Authorize]
public class ResponsesController : ControllerBase
{
    // Generous prototype cap; YouTube itself allows far larger files.
    private const long MaxUploadBytes = 200 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
        { ".mp4", ".webm", ".mov", ".m4v", ".avi", ".mkv" };

    private readonly AppDbContext _db;
    private readonly YouTubeVideoService _youtube;
    private readonly ILogger<ResponsesController> _logger;

    public ResponsesController(AppDbContext db, YouTubeVideoService youtube, ILogger<ResponsesController> logger)
    {
        _db = db;
        _youtube = youtube;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var responses = await _db.Responses.AsNoTracking()
            .Where(r => r.UserId == User.GetUserId())
            .Select(r => new
            {
                r.Id,
                r.QuestionId,
                r.YouTubeVideoId,
                r.PrivacyStatus,
                status = r.Status,
                r.ErrorMessage,
                r.UpdatedAt
            })
            .ToListAsync(ct);
        return Ok(responses);
    }

    // Upload (or replace) the video answer for one question. The file goes to
    // the candidate's own YouTube channel; we keep only the returned video id.
    [HttpPost("{questionId:int}/video")]
    [RequestSizeLimit(MaxUploadBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes + 1024 * 1024)]
    public async Task<IActionResult> Upload(int questionId, IFormFile? file, CancellationToken ct)
    {
        var question = await _db.Questions.AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == questionId && q.IsActive, ct);
        if (question == null)
            return NotFound(new { error = "Unknown interview question." });

        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Choose a video file to upload." });
        if (file.Length > MaxUploadBytes)
            return BadRequest(new { error = "The file is larger than the 200 MB prototype limit." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var looksLikeVideo = file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
        if (!looksLikeVideo && !AllowedExtensions.Contains(extension))
            return BadRequest(new { error = "Unsupported file type. Upload a video file (mp4, webm, mov...)." });

        var userId = User.GetUserId();
        var response = await _db.Responses
            .FirstOrDefaultAsync(r => r.UserId == userId && r.QuestionId == questionId, ct);
        if (response == null)
        {
            response = new VideoResponse { UserId = userId, QuestionId = questionId };
            _db.Responses.Add(response);
        }
        response.Status = ResponseUploadStatus.Uploading;
        response.ErrorMessage = null;
        response.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _youtube.UploadAsync(
                userId,
                stream,
                string.IsNullOrWhiteSpace(file.ContentType) ? "video/*" : file.ContentType,
                title: $"Interview answer: {question.Text}",
                description: "Asynchronous interview response uploaded by the Async Interview Profile student project.",
                ct);

            response.YouTubeVideoId = result.VideoId;
            response.PrivacyStatus = result.PrivacyStatus;
            response.Status = ResponseUploadStatus.Completed;
            response.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                response.Id,
                response.QuestionId,
                response.YouTubeVideoId,
                response.PrivacyStatus,
                status = response.Status
            });
        }
        catch (YouTubeFriendlyException ex)
        {
            await MarkFailedAsync(response, ex.Message);
            return ex.Reason == "youtube_not_connected"
                ? Conflict(new { error = ex.Message, reason = ex.Reason })
                : BadRequest(new { error = ex.Message, reason = ex.Reason });
        }
        catch (OperationCanceledException)
        {
            await MarkFailedAsync(response, "The upload was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YouTube upload failed for user {UserId}, question {QuestionId}", userId, questionId);
            await MarkFailedAsync(response, "The upload failed unexpectedly. Please try again.");
            return StatusCode(502, new { error = "The upload failed unexpectedly. Please try again." });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var response = await _db.Responses
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == User.GetUserId(), ct);
        if (response == null) return NotFound(new { error = "Response not found." });

        // Requirement: removing the record is enough; deleting the actual
        // YouTube video is optional and left to the owner's YouTube Studio.
        _db.Responses.Remove(response);
        await _db.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }

    private async Task MarkFailedAsync(VideoResponse response, string message)
    {
        response.Status = ResponseUploadStatus.Failed;
        response.ErrorMessage = message;
        response.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(CancellationToken.None);
    }
}
