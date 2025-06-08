using System.ComponentModel.DataAnnotations;

namespace ExceptionHandlingInNET.ExceptionHandler
{
    public class UserService
    {
        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            var validationErrors = new Dictionary<string, string[]>();

            if (string.IsNullOrEmpty(request.Email))
                validationErrors.Add("Email", new[] { "Email is required" });

            if (validationErrors.Any())
                throw new ValidationException(validationErrors);

            //continue with user creation
            return await Task.FromResult(new User
            {
                Id = new Random().Next(1, 1000),
                Email = request.Email,
                Name = request.Name
            });
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class CreateUserRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class BusinessException : Exception
    {
        public string ErrorCode { get; }

        public BusinessException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class ValidationException : BusinessException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("VALIDATION_FAILED", "One or more validation errors occurred")
        {
            Errors = errors;
        }
    }
}

