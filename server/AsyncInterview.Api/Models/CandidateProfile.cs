namespace AsyncInterview.Api.Models;

public class CandidateProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public string FullName { get; set; } = "";
    public string Headline { get; set; } = "";
    public string Biography { get; set; } = "";
    // One skill per line; the frontend splits/joins on newlines.
    public string Skills { get; set; } = "";
    // One link per line (LinkedIn, portfolio, etc.).
    public string ContactLinks { get; set; } = "";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
