using MockMindAI.Api.Models;

namespace MockMindAI.Api.Services;

public interface IJwtTokenService
{
    string CreateToken(Student student);
}
