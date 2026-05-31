namespace MockMindAI.Api.Dtos;

public sealed record InterviewSetupRequest(
    string Role,
    string Difficulty,
    string Experience,
    string InterviewType,
    bool IsTimedMode,
    int DurationMinutes);

public sealed record QuestionResponse(List<string> Questions);

public sealed record SubmitInterviewRequest(
    string Role,
    string Difficulty,
    string Experience,
    string InterviewType,
    bool IsTimedMode,
    int DurationMinutes,
    bool WasAutoSubmitted,
    List<string> Questions,
    List<string> Answers);

public sealed record EvaluationResponse(
    int Score,
    List<string> Strengths,
    List<string> Weaknesses,
    string ImprovedAnswer,
    string Feedback);

public sealed record InterviewResultDto(
    int Id,
    DateTime Date,
    string Role,
    string Difficulty,
    string Experience,
    string InterviewType,
    bool IsTimedMode,
    int DurationMinutes,
    bool WasAutoSubmitted,
    int Score,
    Dictionary<string, int> SkillScores,
    List<string> Questions,
    List<string> Answers,
    List<string> Strengths,
    List<string> Weaknesses,
    string ImprovedAnswer,
    string Feedback);

public sealed record LeaderboardEntryDto(
    int Rank,
    string FullName,
    string Department,
    string AvatarKey,
    double AverageScore,
    int InterviewStreak,
    int InterviewCount);

public sealed record AdminUserDto(
    int Id,
    string FullName,
    string Email,
    string Department,
    bool IsDisabled,
    int InterviewCount,
    double AverageScore);

public sealed record DashboardDto(
    StudentProfileDto Profile,
    List<InterviewResultDto> RecentAssessments,
    List<InterviewResultDto> AllAssessments,
    Dictionary<string, int> SkillHeatmap,
    List<SkillHeatmapGroupDto> SkillHeatmapGroups,
    List<string> AchievementBadges,
    List<LeaderboardEntryDto> Leaderboard);

public sealed record SkillHeatmapGroupDto(
    string Role,
    Dictionary<string, int> Skills);
