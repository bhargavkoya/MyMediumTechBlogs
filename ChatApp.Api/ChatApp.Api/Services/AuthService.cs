using ChatApp.Api.Data;
using ChatApp.Api.Dtos;
using ChatApp.Api.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ChatDbContext _context;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthService(ChatDbContext context, IJwtTokenGenerator tokenGenerator)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthResultDto> RegisterAsync(ChatApp.Api.RequestsEntities.RegisterRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenGenerator.GenerateToken(user);
            return new AuthResultDto { User = user, Token = token };
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = _tokenGenerator.GenerateToken(user);
            return new AuthResultDto { User = user, Token = token };
        }

        public Task<bool> ChangePasswordAsync(Guid userId, RequestsEntities.ChangePasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}
