namespace AsyncInterview.Api.Models;

// Backing row for the Google API library's IDataStore. Holds the serialized
// OAuth TokenResponse (access + refresh token) per user, server-side only.
public class YouTubeToken
{
    public string Key { get; set; } = "";
    public string Json { get; set; } = "";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
