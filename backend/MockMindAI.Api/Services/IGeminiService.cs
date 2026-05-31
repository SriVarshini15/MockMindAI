using MockMindAI.Api.Dtos;

namespace MockMindAI.Api.Services;

public interface IGeminiService
{
    Task<List<string>> GenerateQuestionsAsync(InterviewSetupRequest request, CancellationToken cancellationToken);
    Task<EvaluationResponse> EvaluateAnswersAsync(SubmitInterviewRequest request, CancellationToken cancellationToken);
}
