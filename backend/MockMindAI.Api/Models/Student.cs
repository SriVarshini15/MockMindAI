namespace MockMindAI.Api.Models;

public sealed class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string AvatarKey { get; set; } = "mentor";
    public bool IsAdmin { get; set; }
    public bool IsDisabled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<InterviewAttempt> InterviewAttempts { get; set; } = [];
}
