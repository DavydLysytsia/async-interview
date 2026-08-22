namespace AsyncInterview.Api.Models;

public enum ResponseUploadStatus
{
    Uploading,
    Completed,
    Failed
}

// Links a candidate + question to the video uploaded on the candidate's own
// YouTube channel. We store only the YouTube video id, never the video itself.
public class VideoResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int QuestionId { get; set; }
    public InterviewQuestion? Question { get; set; }

    public string? YouTubeVideoId { get; set; }
    // Privacy status YouTube actually applied (may differ from the "unlisted"
    // we request: unverified API projects are forced to "private").
    public string? PrivacyStatus { get; set; }
    public ResponseUploadStatus Status { get; set; } = ResponseUploadStatus.Uploading;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
