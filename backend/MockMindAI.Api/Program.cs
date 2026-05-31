using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MockMindAI.Api.Data;
using MockMindAI.Api.Options;
using MockMindAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is missing. Set it with user-secrets, appsettings.Development.json, or a deployment environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(45);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Frontend:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:5173"];

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
if (string.IsNullOrWhiteSpace(jwt.Key))
{
    throw new InvalidOperationException(
        "Jwt:Key is missing. Set a strong secret with user-secrets, appsettings.Development.json, or a deployment environment variable.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await EnsureDevelopmentSchemaAsync(dbContext);
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

static async Task EnsureDevelopmentSchemaAsync(AppDbContext dbContext)
{
    await dbContext.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('Students', 'AvatarKey') IS NULL
        BEGIN
            ALTER TABLE Students
            ADD AvatarKey nvarchar(max) NOT NULL
                CONSTRAINT DF_Students_AvatarKey DEFAULT 'mentor';
        END;
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('Students', 'IsAdmin') IS NULL
        BEGIN
            ALTER TABLE Students
            ADD IsAdmin bit NOT NULL
                CONSTRAINT DF_Students_IsAdmin DEFAULT 0;
        END;
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('Students', 'IsDisabled') IS NULL
        BEGIN
            ALTER TABLE Students
            ADD IsDisabled bit NOT NULL
                CONSTRAINT DF_Students_IsDisabled DEFAULT 0;
        END;
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('InterviewAttempts', 'DurationMinutes') IS NULL
        BEGIN
            ALTER TABLE InterviewAttempts
            ADD DurationMinutes int NOT NULL
                CONSTRAINT DF_InterviewAttempts_DurationMinutes DEFAULT 0;
        END;
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('InterviewAttempts', 'IsTimedMode') IS NULL
        BEGIN
            ALTER TABLE InterviewAttempts
            ADD IsTimedMode bit NOT NULL
                CONSTRAINT DF_InterviewAttempts_IsTimedMode DEFAULT 0;
        END;
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('InterviewAttempts', 'SkillScoresJson') IS NULL
        BEGIN
            ALTER TABLE InterviewAttempts
            ADD SkillScoresJson nvarchar(max) NOT NULL
                CONSTRAINT DF_InterviewAttempts_SkillScoresJson DEFAULT '{{}}';
        END;
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('InterviewAttempts', 'WasAutoSubmitted') IS NULL
        BEGIN
            ALTER TABLE InterviewAttempts
            ADD WasAutoSubmitted bit NOT NULL
                CONSTRAINT DF_InterviewAttempts_WasAutoSubmitted DEFAULT 0;
        END;
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF EXISTS (SELECT 1 FROM Students)
        BEGIN
            UPDATE Students
            SET IsAdmin = 1
            WHERE Id = (SELECT MIN(Id) FROM Students);
        END;
        """);
}
