namespace AsyncInterview.Api.Models;

public class InterviewQuestion
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
