using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockMindAI.Api.Data;
using MockMindAI.Api.Dtos;
using MockMindAI.Api.Models;
using MockMindAI.Api.Services;

namespace MockMindAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class DashboardController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var studentId = GetStudentId();
        var student = await dbContext.Students
            .Include(value => value.InterviewAttempts.OrderByDescending(attempt => attempt.CreatedAt))
            .FirstOrDefaultAsync(value => value.Id == studentId);

        if (student is null)
        {
            return NotFound();
        }

        var attempts = student.InterviewAttempts
            .OrderByDescending(value => value.CreatedAt)
            .Select(ToResult)
            .ToList();

        var average = attempts.Count == 0 ? 0 : attempts.Average(value => value.Score);
        var profile = new StudentProfileDto(
            student.Id,
            student.FullName,
            student.Email,
            student.Department,
            student.CollegeName,
            student.AvatarKey,
            student.IsAdmin,
            attempts.Count,
            Math.Round(average, 1));

        var leaderboard = await dbContext.Students
            .Where(value => !value.IsDisabled)
            .Include(value => value.InterviewAttempts)
            .ToListAsync();

        var leaderboardRows = leaderboard
            .Where(value => value.InterviewAttempts.Count > 0)
            .Select(value => new
            {
                Student = value,
                Average = Math.Round(value.InterviewAttempts.Average(attempt => attempt.Score), 1),
                Streak = PerformanceService.CalculateStreak(value.InterviewAttempts),
                Count = value.InterviewAttempts.Count
            })
            .OrderByDescending(value => value.Average)
            .ThenByDescending(value => value.Streak)
            .ThenByDescending(value => value.Count)
            .Take(10)
            .Select((value, index) => new LeaderboardEntryDto(
                index + 1,
                value.Student.FullName,
                value.Student.Department,
                value.Student.AvatarKey,
                value.Average,
                value.Streak,
                value.Count))
            .ToList();

        var heatmapGroups = PerformanceService.BuildHeatmapGroups(student.InterviewAttempts);

        return Ok(new DashboardDto(
            profile,
            attempts.Take(5).ToList(),
            attempts,
            heatmapGroups.SelectMany(group => group.Skills.Select(skill => new
                {
                    Label = $"{group.Role}: {skill.Key}",
                    skill.Value
                }))
                .ToDictionary(row => row.Label, row => row.Value),
            heatmapGroups,
            PerformanceService.BuildBadges(student.InterviewAttempts),
            leaderboardRows));
    }

    private int GetStudentId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private static InterviewResultDto ToResult(InterviewAttempt attempt) =>
        new(
            attempt.Id,
            attempt.CreatedAt,
            attempt.Role,
            attempt.Difficulty,
            attempt.Experience,
            attempt.InterviewType,
            attempt.IsTimedMode,
            attempt.DurationMinutes,
            attempt.WasAutoSubmitted,
            attempt.Score,
            InterviewAttempt.ToDictionary(attempt.SkillScoresJson),
            InterviewAttempt.ToList(attempt.QuestionsJson),
            InterviewAttempt.ToList(attempt.AnswersJson),
            InterviewAttempt.ToList(attempt.StrengthsJson),
            InterviewAttempt.ToList(attempt.WeaknessesJson),
            attempt.ImprovedAnswer,
            attempt.Feedback);
}
