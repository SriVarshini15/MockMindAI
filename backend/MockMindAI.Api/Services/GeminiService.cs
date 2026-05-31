using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using MockMindAI.Api.Dtos;
using MockMindAI.Api.Options;

namespace MockMindAI.Api.Services;

public sealed class GeminiService(HttpClient httpClient, IOptions<GeminiOptions> options) : IGeminiService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly GeminiOptions _options = options.Value;

    public async Task<List<string>> GenerateQuestionsAsync(InterviewSetupRequest request, CancellationToken cancellationToken)
    {
        var prompt = $$"""
        Generate exactly 5 interview questions for MockMind AI.
        Role: {{request.Role}}
        Difficulty: {{request.Difficulty}}
        Experience: {{request.Experience}}
        Interview Type: {{request.InterviewType}}

        Distribution:
        - 2 easy questions
        - 2 intermediate questions
        - 1 scenario-based question

        Return only valid JSON in this exact shape:
        {"questions":["Question 1","Question 2","Question 3","Question 4","Question 5"]}
        """;

        var json = await AskGeminiAsync(prompt, cancellationToken);
        var parsed = DeserializeOrDefault<QuestionResponse>(ExtractJson(json));
        return parsed?.Questions?.Where(question => !string.IsNullOrWhiteSpace(question)).Take(5).ToList() ?? [];
    }

    public async Task<EvaluationResponse> EvaluateAnswersAsync(SubmitInterviewRequest request, CancellationToken cancellationToken)
    {
        var prompt = BuildEvaluationPrompt(request, "Evaluate the interview and return only valid compact JSON.");
        var json = await AskGeminiAsync(prompt, cancellationToken);
        var evaluation = DeserializeOrDefault<EvaluationResponse>(ExtractJson(json));
        if (evaluation is not null)
        {
            return NormalizeEvaluation(evaluation)!;
        }

        var retryPrompt = BuildEvaluationPrompt(
            request,
            "Your previous response was invalid JSON. Try again. Return one compact JSON object only, with no markdown, no extra text, and keep improvedAnswer under 800 characters.");

        var retryJson = await AskGeminiAsync(retryPrompt, cancellationToken);
        return NormalizeEvaluation(DeserializeOrDefault<EvaluationResponse>(ExtractJson(retryJson)))
            ?? CreateFallbackEvaluation();
    }

    private static string BuildEvaluationPrompt(SubmitInterviewRequest request, string instruction) =>
        $$"""
        {{instruction}}
        Evaluate this interview. Score from 1 to 10.
        Role: {{request.Role}}
        Difficulty: {{request.Difficulty}}
        Experience: {{request.Experience}}
        Interview Type: {{request.InterviewType}}

        Questions and answers:
        {{JsonSerializer.Serialize(request.Questions.Zip(request.Answers, (q, a) => new { question = q, answer = a }))}}

        Evaluate technical correctness, clarity, communication skills, completeness, and problem solving ability.
        Return only valid JSON in this exact shape:
        {"score":8,"strengths":["Good explanation"],"weaknesses":["Needs practical example"],"improvedAnswer":"Better answer","feedback":"Strong answer overall."}
        Keep every string concise. Do not include line breaks inside string values.
        """;

    private async Task<string> AskGeminiAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is missing. Add Gemini:ApiKey in appsettings.json or user secrets.");
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent";
        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.2,
                maxOutputTokens = 2048,
                responseMimeType = "application/json"
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("x-goog-api-key", _options.ApiKey);

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Gemini request failed with {(int)response.StatusCode} {response.ReasonPhrase}. {error}");
        }

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        return document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";
    }

    private static string ExtractJson(string text)
    {
        var match = Regex.Match(text, "```json\\s*(.*?)\\s*```", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }

    private static T? DeserializeOrDefault<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static EvaluationResponse? NormalizeEvaluation(EvaluationResponse? evaluation)
    {
        if (evaluation is null)
        {
            return null;
        }

        return new EvaluationResponse(
            Math.Clamp(evaluation.Score, 1, 10),
            CleanList(evaluation.Strengths, "Submitted the interview answers."),
            CleanList(evaluation.Weaknesses, "Needs a clearer, more complete answer."),
            string.IsNullOrWhiteSpace(evaluation.ImprovedAnswer)
                ? "Add a structured answer with the main concept, a concrete example, and the tradeoffs."
                : evaluation.ImprovedAnswer.Trim(),
            string.IsNullOrWhiteSpace(evaluation.Feedback)
                ? "Good effort. Improve by giving more specific examples and explaining your reasoning step by step."
                : evaluation.Feedback.Trim());
    }

    private static List<string> CleanList(List<string>? values, string fallback)
    {
        var cleaned = values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Take(5)
            .ToList();

        return cleaned is { Count: > 0 } ? cleaned : [fallback];
    }

    private static EvaluationResponse CreateFallbackEvaluation() =>
        new(
            5,
            ["Your answers were submitted successfully."],
            ["The AI response could not be parsed into structured feedback."],
            "Please retry the evaluation to generate an improved answer.",
            "The interview was saved, but Gemini returned an incomplete response. Retrying usually fixes this.");
}
