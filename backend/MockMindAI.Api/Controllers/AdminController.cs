using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockMindAI.Api.Data;
using MockMindAI.Api.Dtos;

namespace MockMindAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class AdminController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        if (!await IsAdminAsync())
        {
            return Forbid();
        }

        var users = await dbContext.Students
            .Include(value => value.InterviewAttempts)
            .OrderBy(value => value.FullName)
            .Select(value => new AdminUserDto(
                value.Id,
                value.FullName,
                value.Email,
                value.Department,
                value.IsDisabled,
                value.InterviewAttempts.Count,
                value.InterviewAttempts.Count == 0 ? 0 : Math.Round(value.InterviewAttempts.Average(attempt => attempt.Score), 1)))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPatch("users/{id:int}/disable")]
    public async Task<IActionResult> SetDisabled(int id, [FromQuery] bool disabled)
    {
        if (!await IsAdminAsync())
        {
            return Forbid();
        }

        var student = await dbContext.Students.FirstOrDefaultAsync(value => value.Id == id);
        if (student is null)
        {
            return NotFound();
        }

        student.IsDisabled = disabled;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> IsAdminAsync()
    {
        var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        return await dbContext.Students.AnyAsync(value => value.Id == studentId && value.IsAdmin && !value.IsDisabled);
    }
}
