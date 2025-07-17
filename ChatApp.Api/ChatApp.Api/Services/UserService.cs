using AutoMapper;
using ChatApp.Api.Data;
using ChatApp.Api.Dtos;
using ChatApp.Api.RequestsEntities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ChatDbContext _context;
        private readonly IMapper _mapper;

        public UserService(ChatDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<List<UserDto>> SearchUsersAsync(string query)
        {
            var users = await _context.Users
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                .ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.ProfileImageUrl = request.ProfileImageUrl;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserDto>> GetOnlineUsersAsync()
        {
            var users = await _context.Users.Where(u => u.IsOnline).ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }
    }
}
