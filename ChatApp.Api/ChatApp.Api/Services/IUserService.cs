using ChatApp.Api.Dtos;
using ChatApp.Api.RequestsEntities;

namespace ChatApp.Api.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<List<UserDto>> SearchUsersAsync(string query);
        Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<List<UserDto>> GetOnlineUsersAsync();
    }
}
