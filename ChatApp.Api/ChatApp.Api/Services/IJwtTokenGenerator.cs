using ChatApp.Api.Models;

namespace ChatApp.Api.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
