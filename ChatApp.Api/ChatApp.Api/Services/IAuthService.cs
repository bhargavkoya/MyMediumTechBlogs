using ChatApp.Api.Dtos;
using ChatApp.Api.RequestsEntities;
using Microsoft.AspNetCore.Identity.Data;
using LoginRequest = ChatApp.Api.RequestsEntities.LoginRequest;
using RegisterRequest = ChatApp.Api.RequestsEntities.RegisterRequest;

namespace ChatApp.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterRequest request);
        Task<AuthResultDto> LoginAsync(LoginRequest request);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
    }
}
