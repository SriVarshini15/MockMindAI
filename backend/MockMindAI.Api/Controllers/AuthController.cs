using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockMindAI.Api.Data;
using MockMindAI.Api.Dtos;
using MockMindAI.Api.Models;
using MockMindAI.Api.Services;

namespace MockMindAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(AppDbContext dbContext, IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Students.AnyAsync(student => student.Email == email))
        {
            return Conflict("Email is already registered.");
        }

        var student = new Student
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Department = request.Department.Trim(),
            CollegeName = request.CollegeName.Trim(),
            AvatarKey = string.IsNullOrWhiteSpace(request.AvatarKey) ? "mentor" : request.AvatarKey,
            IsAdmin = !await dbContext.Students.AnyAsync()
        };

        dbContext.Students.Add(student);
        await dbContext.SaveChangesAsync();

        return Ok(new AuthResponse(jwtTokenService.CreateToken(student), ToProfile(student, 0, 0)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var student = await dbContext.Students
            .Include(value => value.InterviewAttempts)
            .FirstOrDefaultAsync(value => value.Email == email);

        if (student is null || student.IsDisabled || !BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        var total = student.InterviewAttempts.Count;
        var average = total == 0 ? 0 : student.InterviewAttempts.Average(value => value.Score);
        return Ok(new AuthResponse(jwtTokenService.CreateToken(student), ToProfile(student, total, average)));
    }

    private static StudentProfileDto ToProfile(Student student, int total, double average) =>
        new(
            student.Id,
            student.FullName,
            student.Email,
            student.Department,
            student.CollegeName,
            student.AvatarKey,
            student.IsAdmin,
            total,
            Math.Round(average, 1));
}
