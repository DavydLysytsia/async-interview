namespace AsyncInterview.Api.Models;

// A candidate account. We only support "Sign in with Google", so there is no
// password hash — GoogleSubject is the external login identifier.
public class AppUser
{
    public int Id { get; set; }
    public string GoogleSubject { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public DateTime? ConsentAcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CandidateProfile? Profile { get; set; }
    public List<VideoResponse> Responses { get; set; } = new();
}
