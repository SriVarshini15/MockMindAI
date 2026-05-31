using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MockMindAI.Api.Data;
using MockMindAI.Api.Options;
using MockMindAI.Api.Services;
using Npgsql;

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

connectionString = NormalizePostgresConnectionString(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

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
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

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

static string NormalizePostgresConnectionString(string connectionString)
{
    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        return connectionString;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = WebUtility.UrlDecode(userInfo.ElementAtOrDefault(0) ?? string.Empty),
        Password = WebUtility.UrlDecode(userInfo.ElementAtOrDefault(1) ?? string.Empty),
        Database = WebUtility.UrlDecode(uri.AbsolutePath.TrimStart('/')),
        Pooling = true
    };

    var query = ParseQuery(uri.Query);
    var sslMode = query.GetValueOrDefault("sslmode") ?? query.GetValueOrDefault("ssl");
    if (string.Equals(sslMode, "require", StringComparison.OrdinalIgnoreCase))
    {
        builder.SslMode = SslMode.Require;
        builder.TrustServerCertificate = true;
    }

    return builder.ConnectionString;
}

static Dictionary<string, string> ParseQuery(string query)
{
    return query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(part => part.Split('=', 2))
        .Where(parts => parts.Length == 2)
        .ToDictionary(
            parts => WebUtility.UrlDecode(parts[0]) ?? string.Empty,
            parts => WebUtility.UrlDecode(parts[1]) ?? string.Empty,
            StringComparer.OrdinalIgnoreCase);
}
