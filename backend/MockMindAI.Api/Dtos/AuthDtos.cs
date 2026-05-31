namespace MockMindAI.Api.Dtos;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Department,
    string CollegeName,
    string? AvatarKey);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, StudentProfileDto Student);

public sealed record StudentProfileDto(
    int Id,
    string FullName,
    string Email,
    string Department,
    string CollegeName,
    string AvatarKey,
    bool IsAdmin,
    int TotalInterviewsAttended,
    double AverageScore);

public sealed record UpdateProfileRequest(string AvatarKey);
