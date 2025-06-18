
namespace AsyncWebAPIExample.Services
{
    public class UserService : IUserService
    {
        public Task<User> CreateUserAsync(CreateUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
