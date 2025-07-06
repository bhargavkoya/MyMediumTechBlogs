using ECommerceWithRoutingAndMiddleware.Entities;

namespace ECommerceWithRoutingAndMiddleware.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string userId);
    }

    public class UserService : IUserService
    {
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            throw new NotImplementedException(); // Simulate async operation
        }
    }
}
