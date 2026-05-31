using System.Text.RegularExpressions;
using MockMindAI.Api.Dtos;
using MockMindAI.Api.Models;

namespace MockMindAI.Api.Services;

public static class PerformanceService
{
    private static readonly DomainProfile[] DomainProfiles =
    [
        new("Software Engineering", ["software", "developer", "programming", "engineer", "backend", "full stack", "api"], [
            new("OOP", ["oop", "object oriented", "class", "inheritance", "polymorphism", "interface", "encapsulation"]),
            new("System Design", ["system design", "architecture", "scalability", "scale", "cache", "load balancer", "distributed"]),
            new("Algorithms", ["algorithm", "complexity", "data structure", "array", "tree", "graph", "sorting", "searching"]),
            new("REST APIs", ["rest", "api", "endpoint", "controller", "http", "middleware"]),
            new("Testing", ["unit test", "testing", "mock", "assert", "test case"])
        ]),
        new(".NET Development", ["c#", ".net", "dotnet", "asp.net", "entity framework", "linq", "blazor"], [
            new("C#", ["c#", "csharp", ".net", "dotnet"]),
            new("ASP.NET Core", ["asp.net", "asp.net core", "controller", "middleware", "web api", "minimal api"]),
            new("Entity Framework", ["entity framework", "ef core", "dbcontext", "migration", "linq to sql"]),
            new("LINQ", ["linq", "query syntax", "lambda"]),
            new("Dependency Injection", ["dependency injection", "di", "service container", "inversion of control", "ioc"]),
            new("SQL", ["sql", "database", "query", "join", "stored procedure"])
        ]),
        new("Frontend Development", ["frontend", "front end", "react", "javascript", "ui", "web"], [
            new("React", ["react", "component", "hook", "useeffect", "usestate", "jsx"]),
            new("JavaScript", ["javascript", "typescript", "async", "promise", "closure"]),
            new("HTML/CSS", ["html", "css", "layout", "flexbox", "grid", "responsive"]),
            new("State Management", ["state", "redux", "context", "props"]),
            new("Accessibility", ["accessibility", "aria", "keyboard", "screen reader"])
        ]),
        new("Data Science", ["data science", "machine learning", "ai", "ml", "analytics", "data analyst"], [
            new("Python", ["python", "pandas", "numpy", "scikit", "jupyter"]),
            new("Statistics", ["statistics", "probability", "hypothesis", "variance", "regression"]),
            new("Machine Learning", ["machine learning", "model", "classification", "clustering", "training"]),
            new("Data Visualization", ["visualization", "chart", "dashboard", "matplotlib", "seaborn", "plot"]),
            new("SQL", ["sql", "database", "query", "join"])
        ]),
        new("Physics", ["physics", "mechanics", "thermodynamics", "electromagnetism", "optics", "waves", "quantum"], [
            new("Mechanics", ["mechanics", "force", "friction", "motion", "speed", "velocity", "acceleration", "momentum"]),
            new("Kinematics", ["kinematics", "motion", "displacement", "velocity", "acceleration", "projectile"]),
            new("Newton's Laws", ["newton", "newton's laws", "inertia", "force", "action reaction"]),
            new("Thermodynamics", ["thermodynamics", "heat", "temperature", "entropy", "enthalpy", "thermal"]),
            new("Electromagnetism", ["electromagnetism", "electric field", "magnetic field", "charge", "current", "voltage"]),
            new("Optics", ["optics", "light", "reflection", "refraction", "lens", "mirror"]),
            new("Waves", ["waves", "wave", "frequency", "wavelength", "amplitude", "oscillation"]),
            new("Quantum Physics", ["quantum", "photon", "wave function", "uncertainty", "superposition"])
        ]),
        new("Medicine", ["medicine", "medical", "doctor", "clinical", "nursing", "healthcare", "patient"], [
            new("Anatomy", ["anatomy", "organ", "muscle", "bone", "nervous system"]),
            new("Physiology", ["physiology", "homeostasis", "cardiac", "respiratory", "renal"]),
            new("Pharmacology", ["pharmacology", "drug", "dose", "medication", "side effect"]),
            new("Pathology", ["pathology", "disease", "infection", "inflammation", "diagnosis"]),
            new("Clinical Diagnosis", ["clinical", "symptom", "patient", "diagnosis", "case"])
        ]),
        new("Business", ["business", "management", "marketing", "sales", "finance", "product"], [
            new("Communication", ["communication", "stakeholder", "presentation", "clarity"]),
            new("Strategy", ["strategy", "market", "competitive", "roadmap"]),
            new("Analytics", ["analytics", "metric", "kpi", "reporting", "forecast"]),
            new("Leadership", ["leadership", "team", "conflict", "decision"]),
            new("Customer Focus", ["customer", "user", "client", "support"])
        ])
    ];

    public static Dictionary<string, int> EstimateSkillScores(SubmitInterviewRequest request, int overallScore)
    {
        var text = BuildText(request.Role, request.InterviewType, request.Questions, request.Answers, [], []);
        return EstimateSkillScoreRows(text, overallScore, Normalize(request.Role))
            .GroupBy(row => row.Skill)
            .ToDictionary(
                group => group.Key,
                group => (int)Math.Round(group.Average(row => row.Score)));
    }

    public static List<SkillHeatmapGroupDto> BuildHeatmapGroups(IEnumerable<InterviewAttempt> attempts) =>
        attempts
            .GroupBy(attempt => NormalizeRole(attempt.Role))
            .OrderByDescending(roleGroup => roleGroup.Max(attempt => attempt.CreatedAt))
            .Select(roleGroup => new SkillHeatmapGroupDto(
                roleGroup.Key,
                roleGroup
                    .SelectMany(EstimateSkillScores)
                    .GroupBy(row => row.Skill)
                    .OrderByDescending(skillGroup => skillGroup.Average(row => row.Score))
                    .ThenBy(skillGroup => skillGroup.Key)
                    .ToDictionary(
                        skillGroup => skillGroup.Key,
                        skillGroup => (int)Math.Round(skillGroup.Average(row => row.Score)))))
            .Where(group => group.Skills.Count > 0)
            .ToList();

    public static Dictionary<string, int> BuildHeatmap(IEnumerable<InterviewAttempt> attempts) =>
        BuildHeatmapGroups(attempts)
            .SelectMany(group => group.Skills.Select(skill => new
            {
                Label = $"{group.Role}: {skill.Key}",
                skill.Value
            }))
            .ToDictionary(row => row.Label, row => row.Value);

    public static List<string> BuildBadges(IEnumerable<InterviewAttempt> attempts)
    {
        var list = attempts.ToList();
        var heatmapGroups = BuildHeatmapGroups(list);
        var badges = new List<string>();

        if (list.Count >= 1)
        {
            badges.Add("First Interview");
        }

        if (list.Count >= 10)
        {
            badges.Add("10 Interviews Completed");
        }

        if (list.Any(attempt => attempt.Score >= 9))
        {
            badges.Add("Score Above 90");
        }

        var topSkill = heatmapGroups
            .SelectMany(group => group.Skills.Select(skill => new { group.Role, Skill = skill.Key, Score = skill.Value }))
            .Where(skill => skill.Score >= 85)
            .OrderByDescending(skill => skill.Score)
            .FirstOrDefault();

        if (topSkill is not null)
        {
            badges.Add($"{topSkill.Skill} Expert");
        }

        return badges;
    }

    public static int CalculateStreak(IEnumerable<InterviewAttempt> attempts)
    {
        var days = attempts
            .Select(attempt => attempt.CreatedAt.Date)
            .Distinct()
            .OrderByDescending(date => date)
            .ToList();

        if (days.Count == 0)
        {
            return 0;
        }

        var streak = 0;
        var expected = days[0];
        foreach (var day in days)
        {
            if (day != expected)
            {
                break;
            }

            streak++;
            expected = expected.AddDays(-1);
        }

        return streak;
    }

    private static List<SkillScore> EstimateSkillScores(InterviewAttempt attempt)
    {
        var roleText = Normalize(attempt.Role);
        var text = BuildText(
            attempt.Role,
            attempt.InterviewType,
            InterviewAttempt.ToList(attempt.QuestionsJson),
            InterviewAttempt.ToList(attempt.AnswersJson),
            InterviewAttempt.ToList(attempt.StrengthsJson),
            InterviewAttempt.ToList(attempt.WeaknessesJson),
            attempt.ImprovedAnswer,
            attempt.Feedback);

        return EstimateSkillScoreRows(text, attempt.Score, roleText);
    }

    private static List<SkillScore> EstimateSkillScoreRows(string text, int overallScore, string roleText)
    {
        var normalized = Normalize(text);
        var baseScore = Math.Clamp(overallScore * 10, 0, 100);

        var relevantProfiles = DomainProfiles
            .Where(profile => CountHits(roleText, profile.Keywords) > 0
                || profile.Skills.Any(skill => CountHits(roleText, skill.Keywords) > 0))
            .ToList();

        if (relevantProfiles.Count == 0)
        {
            relevantProfiles = DomainProfiles
                .Where(profile => CountHits(normalized, profile.Keywords) > 0)
                .ToList();
        }

        var scoredProfiles = relevantProfiles
            .Select(profile => new ScoredProfile(
                profile,
                CountHits(normalized, profile.Keywords),
                profile.Skills
                    .Select(skill => new
                    {
                        Skill = skill,
                        Hits = CountHits(normalized, skill.Keywords)
                    })
                    .Where(skill => skill.Hits > 0)
                    .Select(skill => new ScoredSkill(skill.Skill, skill.Hits))
                    .ToList()))
            .Where(profile => profile.DomainHits > 0 || profile.Skills.Count > 0)
            .ToList();

        var rows = scoredProfiles
            .SelectMany(profile => profile.Skills.Select(skill => new SkillScore(
                profile.Profile.Domain,
                skill.Skill.Name,
                Math.Clamp(baseScore + Math.Min(15, skill.Hits * 5) + Math.Min(10, profile.DomainHits * 2), 0, 100))))
            .ToList();

        return AddRelatedSubtopics(rows, scoredProfiles, baseScore);
    }

    private static List<SkillScore> AddRelatedSubtopics(
        List<SkillScore> rows,
        IEnumerable<ScoredProfile> scoredProfiles,
        int baseScore)
    {
        const int minimumTopics = 3;
        const int maximumTopics = 5;

        if (rows.Count >= minimumTopics)
        {
            return rows
                .OrderByDescending(row => row.Score)
                .ThenBy(row => row.Skill)
                .Take(maximumTopics)
                .ToList();
        }

        var existingSkills = rows
            .Select(row => row.Skill)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var relatedSkills = scoredProfiles
            .OrderByDescending(profile => profile.DomainHits)
            .SelectMany(profile => profile.Profile.Skills.Select((skill, index) => new
            {
                profile.Profile.Domain,
                Skill = skill.Name,
                Index = index
            }))
            .Where(skill => !existingSkills.Contains(skill.Skill))
            .ToList();

        foreach (var relatedSkill in relatedSkills)
        {
            rows.Add(new SkillScore(
                relatedSkill.Domain,
                relatedSkill.Skill,
                Math.Clamp(baseScore - 5, 0, 100)));
            existingSkills.Add(relatedSkill.Skill);

            if (rows.Count >= maximumTopics)
            {
                break;
            }
        }

        return rows
            .OrderByDescending(row => row.Score)
            .ThenBy(row => row.Skill)
            .Take(maximumTopics)
            .ToList();
    }

    private static int CountHits(string text, IEnumerable<string> keywords) =>
        keywords.Count(keyword => ContainsKeyword(text, keyword));

    private static bool ContainsKeyword(string normalizedText, string keyword)
    {
        var normalizedKeyword = Normalize(keyword);

        if (normalizedKeyword.Contains('#') || normalizedKeyword.Contains('.'))
        {
            return normalizedText.Contains(normalizedKeyword);
        }

        return Regex.IsMatch(
            normalizedText,
            $@"(?<![a-z0-9+#.]){Regex.Escape(normalizedKeyword)}(?![a-z0-9+#.])",
            RegexOptions.CultureInvariant);
    }

    private static string BuildText(
        string role,
        string interviewType,
        IEnumerable<string> questions,
        IEnumerable<string> answers,
        IEnumerable<string> strengths,
        IEnumerable<string> weaknesses,
        params string[] extra) =>
        string.Join(' ', [role, interviewType, .. questions, .. answers, .. strengths, .. weaknesses, .. extra]);

    private static string Normalize(string value) =>
        value.ToLowerInvariant().Replace("c sharp", "c#");

    private static string NormalizeRole(string role) =>
        string.IsNullOrWhiteSpace(role) ? "Interview Role" : role.Trim();

    private sealed record DomainProfile(string Domain, string[] Keywords, SkillProfile[] Skills);
    private sealed record SkillProfile(string Name, string[] Keywords);
    private sealed record ScoredProfile(DomainProfile Profile, int DomainHits, List<ScoredSkill> Skills);
    private sealed record ScoredSkill(SkillProfile Skill, int Hits);
    private sealed record SkillScore(string Domain, string Skill, int Score);
}
