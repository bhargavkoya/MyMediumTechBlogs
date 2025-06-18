namespace AsyncWebAPIExample.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(CreateUserRequest request);
    }
}
