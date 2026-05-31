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
public sealed class ProfileController(AppDbContext dbContext) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var student = await dbContext.Students.FirstOrDefaultAsync(value => value.Id == studentId);
        if (student is null)
        {
            return NotFound();
        }

        student.AvatarKey = string.IsNullOrWhiteSpace(request.AvatarKey) ? student.AvatarKey : request.AvatarKey;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
