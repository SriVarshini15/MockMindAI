using System.Text.Json;

namespace MockMindAI.Api.Models;

public sealed class InterviewAttempt
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string InterviewType { get; set; } = string.Empty;
    public bool IsTimedMode { get; set; }
    public int DurationMinutes { get; set; }
    public bool WasAutoSubmitted { get; set; }
    public int Score { get; set; }
    public string QuestionsJson { get; set; } = "[]";
    public string AnswersJson { get; set; } = "[]";
    public string SkillScoresJson { get; set; } = "{}";
    public string StrengthsJson { get; set; } = "[]";
    public string WeaknessesJson { get; set; } = "[]";
    public string ImprovedAnswer { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static string ToJson<T>(T value) => JsonSerializer.Serialize(value);
    public static List<string> ToList(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? [];
    public static Dictionary<string, int> ToDictionary(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
}
