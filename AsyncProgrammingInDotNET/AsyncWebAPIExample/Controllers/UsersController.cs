using AsyncWebAPIExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsyncWebAPIExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public UsersController(HttpClient httpClient, IUserService userService)
        {
            _httpClient = httpClient;
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                //Asynchronous database operation
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                    return NotFound();

                //Asynchronous external API call
                var additionalInfo = await _httpClient.GetFromJsonAsync<UserInfo>(
                    $"https://external-api.com/users/{id}");

                user.AdditionalInfo = additionalInfo;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            //Validate and create user asynchronously
            var user = await _userService.CreateUserAsync(request);

            //Send welcome email asynchronously without blocking
            _ = Task.Run(async () => await SendWelcomeEmailAsync(user.Email));

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        private async Task SendWelcomeEmailAsync(string email)
        {
            //Simulate email sending
            await Task.Delay(2000);
            Console.WriteLine($"Welcome email sent to {email}");
        }
    }

}
