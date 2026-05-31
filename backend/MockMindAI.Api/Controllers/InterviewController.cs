using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MockMindAI.Api.Data;
using MockMindAI.Api.Dtos;
using MockMindAI.Api.Models;
using MockMindAI.Api.Services;

namespace MockMindAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class InterviewController(AppDbContext dbContext, IGeminiService geminiService) : ControllerBase
{
    [HttpPost("generate-questions")]
    public async Task<ActionResult<QuestionResponse>> GenerateQuestions(
        InterviewSetupRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var questions = await geminiService.GenerateQuestionsAsync(request, cancellationToken);
            return Ok(new QuestionResponse(questions));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("submit")]
    public async Task<ActionResult<InterviewResultDto>> Submit(
        SubmitInterviewRequest request,
        CancellationToken cancellationToken)
    {
        EvaluationResponse evaluation;
        try
        {
            evaluation = await geminiService.EvaluateAnswersAsync(request, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        var attempt = new InterviewAttempt
        {
            StudentId = GetStudentId(),
            Role = request.Role,
            Difficulty = request.Difficulty,
            Experience = request.Experience,
            InterviewType = request.InterviewType,
            IsTimedMode = request.IsTimedMode,
            DurationMinutes = request.DurationMinutes,
            WasAutoSubmitted = request.WasAutoSubmitted,
            Score = evaluation.Score,
            QuestionsJson = InterviewAttempt.ToJson(request.Questions),
            AnswersJson = InterviewAttempt.ToJson(request.Answers),
            SkillScoresJson = InterviewAttempt.ToJson(PerformanceService.EstimateSkillScores(request, evaluation.Score)),
            StrengthsJson = InterviewAttempt.ToJson(evaluation.Strengths),
            WeaknessesJson = InterviewAttempt.ToJson(evaluation.Weaknesses),
            ImprovedAnswer = evaluation.ImprovedAnswer,
            Feedback = evaluation.Feedback
        };

        dbContext.InterviewAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new InterviewResultDto(
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
            request.Questions,
            request.Answers,
            evaluation.Strengths,
            evaluation.Weaknesses,
            evaluation.ImprovedAnswer,
            evaluation.Feedback));
    }

    private int GetStudentId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}
